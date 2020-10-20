using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization.ObjectGraphTraversalStrategies;

namespace LazyStack
{
    /// <summary>
    /// Process the solution projects.
    /// ProcessClientProject -- openApi generation of client Api
    /// ProcessAppApiProject -- openApi generation of core application Api
    /// For Each tag
    ///     ProcessLambdaProject -- create if it doesn't exist
    ///     ProcessControllerProject -- create or update
    /// ProcessLocalWebServer -- update references when new AppApiGroupApi projects are added
    /// </summary>
    public class ProcessProjects
    { 
        public ProcessProjects(SolutionModel solutionModel, ILogger logger)
        {
            this.solutionModel = solutionModel;
            this.logger = logger;
        }

        private readonly SolutionModel solutionModel;
        private readonly ILogger logger;

        public void Run()
        {
            ProcessClientSDKProject();

            ProcessReferenceApiProject();

            // ApiS and FUNCTIONS
            // We generate Controller and Lambda function project for each
            // tag, in the OpenAPI specification. 
            foreach (KeyValuePair<string,AWSLambda> lambda in solutionModel.Lambdas)
            {
                var lambdaName = lambda.Key; // ex: Order
                // ex: Lambdas/OrderLambda project
                ProcessLambdaProject(lambdaName, lambda.Value.AwsApi); // api passed because project needs to know type of Api calling it
                // ex: Controllers/OrderController project
                ProcessControllerProject(lambdaName);
            }

            ProcessLocalWebApiProject();

            // Update solutionModel.SolutionFolders
            foreach (var projInfo in solutionModel.Projects.Values)
                if (!string.IsNullOrEmpty(projInfo.SolutionFolder)
                    && !solutionModel.SolutionFolders.Contains(projInfo.SolutionFolder))
                    solutionModel.SolutionFolders.Add(projInfo.SolutionFolder);

        }

        /// <summary>
        /// Generate the <AppName>ClientSDK project.
        /// </summary>
        private void ProcessClientSDKProject()
        {
            var appName = solutionModel.AppName; // PetStore
            var projName = $"{appName}ClientSDK";  // PetStoreClientSDK
            var projFileName = $"{projName}.csproj"; // PetStoreClientSDK.csproj
            var projFileRelativePath = Path.Combine(projName, projFileName); // PetStoreClientSDK/PetStoreClientSDK.csproj
            var projFolderPath = Path.Combine(solutionModel.SolutionRootFolderPath, projName); 
            var projFilePath = Path.Combine(projFolderPath, projFileName); 

            solutionModel.Projects.Add(projName,
                new ProjectInfo(
                    solutionFolder: string.Empty,
                    path: projFilePath,
                    relativePath: projFileRelativePath
                    ));

            logger.Info($"Generating/updating {projName}");

            var client = new GenerateProject(solutionModel)
            {
                Input = solutionModel.OpenApiFilePath,
                Generator = "csharp-netcore",
                Output = solutionModel.SolutionRootFolderPath,
                SkipValidateSpec = true,
                ProjectName = projName
            };

            client.AdditionalProperites.Add("aspnetCoreVersion", 
                GetConfigProperty("ClientSDKProjectOpenApiGenerationOptions/aspnetcoreVersion"));
            client.AdditionalProperites.Add("netCoreProjectFile", "true");
            client.AdditionalProperites.Add("packageName", projName);
            client.AdditionalProperites.Add("targetFramework", 
                GetConfigProperty("ClientSDKProjectOpenApiGenerationOptions/targetFramework"));
            client.Run();

            // Replace generated ApiClient.cs with our version - replacing RestSharp with MS classes
            // and implementing AwsSignerVersion4
            // ApiClient.cs
            var filePath = Path.Combine(projFolderPath, "Client", "ApiClient.cs");
             File.Copy(Path.Combine(solutionModel.LazyStackTemplateFolderPath, "ApiClient.cs"),
                filePath, true);

            var text = File.ReadAllText(filePath);
            text = text.Replace("__AppName__", appName);
            File.WriteAllText(filePath, text);

            // Modify csproj
            XElement xmlDoc = XElement.Load(projFilePath);

            // Remove Package references used by original OpenAPI-Genereator project
            // We replaced the use of RestSharp with Microsoft HTTP classes so we could
            // use AwsSignatureVersion4 library.
            foreach (var itemGroup in xmlDoc.Elements("ItemGroup"))
                foreach (var pkgRef in itemGroup.Elements("PackageReference"))
                    if (pkgRef.Attribute("Include").Value.Equals("RestSharp"))
                        pkgRef.Remove();

            xmlDoc.Save(projFilePath);

            UpdatePackageReferences(projFilePath, "ClientSDKProject");

        }

        /// <summary>
        /// Generate the <AppName>Api project.
        /// </summary>
        private void ProcessReferenceApiProject()
        {
            var projName = $"{solutionModel.AppName}Api"; // PetStoreApi
            var projFileName = $"{projName}.csproj"; // PetStoreApi.csproj
            var projFileRelativePath = Path.Combine(projName, projFileName); // PetStoreApi/PetStoreApi.csproj
            var projFolderPath = Path.Combine(solutionModel.SolutionRootFolderPath, projName);
            var projFilePath = Path.Combine(projFolderPath, projFileName);

            solutionModel.Projects.Add(projName,
                new ProjectInfo(
                    solutionFolder: string.Empty,
                    path: projFilePath,
                    projFileRelativePath
                    ));

            logger.Info($"Generating/updating project {projName}");
            var projectLib = new GenerateProject(solutionModel)
            {
                Input = solutionModel.OpenApiFilePath,
                Generator = "aspnetcore",
                Output = solutionModel.SolutionRootFolderPath,
                SkipValidateSpec = true,
                ProjectName = projName
            };
            projectLib.AdditionalProperites.Add("aspnetCoreVersion", 
               GetConfigProperty("ApiProjectOpenApiGenerationOptions/aspnetCoreVersion"));
            projectLib.AdditionalProperites.Add("buildTarget", "library");
            projectLib.AdditionalProperites.Add("isLibrary", "true");
            projectLib.AdditionalProperites.Add("packageName", projName);
            projectLib.Run();
        }

        /// <summary>
        /// Generate the Lambdas/<LambdaName>Lambda project.
        /// </summary>
        /// <param name="lambdaName"></param>
        private void ProcessLambdaProject(string lambdaName, AwsApi api)
        {
            // Create Lambda project
            // LazyStack Templates contains a simple project fileset that we copy and 
            // modify to create the Func project. The Func project will reference the
            // <LambdaName>Api project. Programmers do not need to add anything to
            // these projects.
            var projName = $"{lambdaName}";
            var projFileName = $"{projName}.csproj";
            var projFileRelativePath = Path.Combine("Lambdas", projName, projFileName);
            var projFolderPath = Path.Combine(solutionModel.SolutionRootFolderPath, "Lambdas", projName);
            var projFilePath = Path.Combine(projFolderPath, $"{lambdaName}.csproj");

            solutionModel.Projects.Add(projName,
                new ProjectInfo(
                    solutionFolder: "Lambdas",
                    path: projFilePath,
                    relativePath: projFileRelativePath
                    ));

            logger.Info($"Generating/updating project {projName}");

            if (!Directory.Exists(projFolderPath))
            {
                Utilities.DirectoryCopy(
                    Path.Combine(solutionModel.LazyStackTemplateFolderPath, "Lambda"),
                    projFolderPath,
                    copySubDirs: true,
                    overwrite: true);

                File.Move( // Rename csproj file. ex Lambda.csproj to PetStoreOrderLambda.csproj
                    Path.Combine(projFolderPath, "Lambda.csproj"),
                    projFilePath);

                var configureSvcsFilePath = Path.Combine(projFolderPath, "ConfigureSvcs.cs");
                var configureSvcsText = File.ReadAllText(configureSvcsFilePath);
                configureSvcsText = configureSvcsText.Replace(
                    "__ServiceRegistrations__",
                    $"\t\t\tservices.AddSingleton<{lambdaName}Controller.{lambdaName}Controller>();\n"
                    );
                File.WriteAllText(configureSvcsFilePath, configureSvcsText);
            }

            UpdateProjectReferences(
                projFilePath,
                new List<string>
                {
                            //< ProjectReference Include = "..\..\Controllers\OrderController\OrderController.csproj" />
                            Path.Combine("..","..","Controllers",$"{lambdaName}Controller", $"{lambdaName}Controller.csproj"),
                });


            // Update LambdaEntryPoint class to inherit correct base class for specified HttpApi or Api
            // We do this every time because it is not unusual to change the tag/api mapping
            var text = File.ReadAllText(Path.Combine(solutionModel.LazyStackTemplateFolderPath, "Lambda","LambdaEntryPoint.cs"));
            var baseClass = api.ProxyFunctionName;
            text = text.Replace("__APIGatewayProxyFunction__", baseClass);
            var lamdbaEntryPointFilePath = Path.Combine(projFolderPath, "LambdaEntryPoint.cs");
            File.WriteAllText(lamdbaEntryPointFilePath, text);

            UpdatePackageReferences(projFilePath, "LambdaProject");

        }

        /// <summary>
        /// Generate a <LambdaName></LambdaName>Controller project.
        /// </summary>
        /// <param name="lambdaName"></param>
        private void ProcessControllerProject(string lambdaName)
        {
            var appName = solutionModel.AppName; // PetStore
            var projName = $"{lambdaName}Controller"; // OrderController
            var projFileName = $"{projName}.csproj"; // OrderController.csproj
            var projFileRelativePath = Path.Combine("Controllers", projName, projFileName); // Controllers/OrderController/OrderController.csproj
            var projFolderPath = Path.Combine(solutionModel.SolutionRootFolderPath, "Controllers", projName);
            var projFilePath = Path.Combine(projFolderPath, projFileName);

            solutionModel.Projects.Add(projName,
                new ProjectInfo(
                    solutionFolder: "Controllers",
                    path: projFilePath,
                    relativePath: projFileRelativePath
                    ));

            var classFilePath = Path.Combine(projFolderPath, $"{lambdaName}Controller.cs");

            logger.Info($"Generating/updating project {lambdaName}Controller");

            // Get the apiMethods from Api Reference project <lambdaName>ApiController class in <lambdaName>Api.cs
            var apiMethods = GetApiControllerMethods(lambdaName);

            // Create new project if it doesn't exist
            if (!Directory.Exists(projFolderPath))
            {
                Utilities.DirectoryCopy(
                    Path.Combine(solutionModel.LazyStackTemplateFolderPath, "Controllers"),
                    projFolderPath,
                    copySubDirs: true,
                    overwrite: true);

                File.Move( // Rename csproj file. ex: ApiController.csproj to OrderController.csproj
                    Path.Combine(projFolderPath, "Controller.csproj"),
                    projFilePath);

                UpdateProjectReferences(
                    projFilePath,
                    new List<string> {
                            // ex  <ProjectReference Include= "..\..\PetStoreApi\PetStoreApi.csproj"/>
                            Path.Combine("..","..", $"{appName}Api", $"{appName}Api.csproj")
                    });

                // Create Class file
                // ex: Rename Controller.cs to .cs ex: OrderController.cs
                File.Move(
                    Path.Combine(projFolderPath, "Controller.cs"),
                    classFilePath
                    );

                var text = File.ReadAllText(classFilePath);
                var usingStatements = $"using {appName}Api.Models;\n" // ex: using PetStoreApi.Models;
                    + $"using {appName}Api.Controllers;\n"; // ex: using PetStoreApi.Controllers

                text = text.Replace("__UsingStatements__", usingStatements);
                text = text.Replace("__LambdaNameController__", $"{lambdaName}Controller"); // ex: OrderController
                text = text.Replace("__LambdaNameApiController__", $"{lambdaName}ApiController"); //ex: OrderApiController
                text = text.Replace("__Methods__", GenerateControllerMethods(apiMethods));
                File.WriteAllText(classFilePath, text);
            }
            else
            { // Update methods

                var existingMethods = GetControllerMethods(lambdaName);
                var insertMethods = new List<Method>();
                foreach (var m in apiMethods)
                {
                    bool found = false;
                    foreach(var em in existingMethods)
                        if(m.MethodName.Equals(em.MethodName) && m.Arguments.Equals(em.Arguments))
                        {
                            found = true;
                            break;
                        }
                    if (!found)
                        insertMethods.Add(m);
                }
                if (insertMethods.Count > 0)
                    InsertMethods($"{lambdaName}Controller",$"{lambdaName}Controller", classFilePath, GenerateControllerMethods(insertMethods));
            }
        }

        private void ProcessLocalWebApiProject()
        {
            // APP PROJECT -- local web server
            var appName = solutionModel.AppName;
            var projFolderPath = Path.Combine(solutionModel.SolutionRootFolderPath, appName);

            logger.Info($"Updating project {appName}");

            // Update csproj file
            var csprojFilePath = Path.Combine(projFolderPath, $"{appName}.csproj");
            var references = new List<string>();
            foreach (var lambdaName in solutionModel.Lambdas.Keys)
                references.Add(Path.Combine("..", "Controllers", $"{lambdaName}Controller", $"{lambdaName}Controller.csproj"));

            UpdateProjectReferences(csprojFilePath, references);

            // Update ConfgiureSvcs.cs 
            string filePath = Path.Combine(solutionModel.SolutionRootFolderPath, solutionModel.AppName, "ConfigureSvcs.cs");
            string nameSpace = solutionModel.AppName;
            var projServices = new Dictionary<string, bool>();
            foreach (var key in solutionModel.Lambdas.Keys)
                projServices.Add(key + "Controller", false); // bool indicates if the reference has been registered in the Startup.ConfigureSvcs method

            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath)).GetCompilationUnitRoot();

            var method = root
                .DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where(x => x.Identifier.Text.Equals("Startup"))
                .FirstOrDefault()
                    ?.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    .Where(x => x.Identifier.Text.Equals("ConfigureSvcs")
                            && x.ParameterList.Parameters.Count == 1
                            && x.ParameterList.Parameters[0].Type.ToString().Equals("IServiceCollection"))
                    .First();

            if (method == null)
                throw new Exception("Error: Missing Startup.ConfigureSvcs method");

            var serviceRegistrations = method.DescendantNodes().OfType<BlockSyntax>()
                .First()
                ?.DescendantNodes().OfType<ExpressionStatementSyntax>()
                .Where( x => 
                        (((x.Expression as InvocationExpressionSyntax))?.Expression is MemberAccessExpressionSyntax)
                        && ((((x.Expression as InvocationExpressionSyntax))
                                ?.Expression as MemberAccessExpressionSyntax).Expression as IdentifierNameSyntax)
                                    .Identifier.ValueText.Equals("services")
                    );

            var allowedServiceMethods = new string[] { "AddSingleton" };
            foreach (var x in serviceRegistrations)
            {
                var m = (x.Expression as InvocationExpressionSyntax)?.Expression as MemberAccessExpressionSyntax;
                string methodName = m.Name.Identifier.ValueText.ToString();
                if (allowedServiceMethods.Contains<string>(methodName))
                {
                    var arguments = (m.Name as GenericNameSyntax).TypeArgumentList.Arguments;
                    if (arguments.Count > 0)
                    {
                        string left = (arguments[0] as QualifiedNameSyntax).Left.ToString();
                        if (projServices.ContainsKey(left))
                            projServices[left] = true;
                    }
                }
            }

            // Insert any Missing service registrations
            var newStatements = new List<StatementSyntax>();
            foreach (var kvp in projServices)
                if (!kvp.Value)
                    newStatements.Add(ParseStatement($"\t\t\tservices.AddSingleton<{kvp.Key}.{kvp.Key}>();\n")); 

            if(newStatements.Count > 0)
                root = root.ReplaceNode(method, method.AddBodyStatements(newStatements.ToArray()));

            File.WriteAllText(filePath, root.ToFullString());
        }

        private void InsertMethods(string nameSpace, string className, string fileName, string newMethodText)
        {
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(fileName)).GetCompilationUnitRoot();

            var classDecl = root
                .DescendantNodes().OfType<NamespaceDeclarationSyntax>()
                .Where(x => x.Name.ToString().Equals(nameSpace))
                .First()
                    ?.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(x => x.Identifier.ValueText.Equals(className))
                    .First();

            if (classDecl == null)
                throw new Exception($"Error: Can't find {nameSpace}.{className}");

            root = root.ReplaceNode(classDecl, 
                classDecl.AddMembers(CSharpSyntaxTree.ParseText(newMethodText).GetCompilationUnitRoot().Members.ToArray()));
            File.WriteAllText(fileName, root.ToString());
        }

        /// <summary>
        /// Get methods from AppNameApi reference project for a specified class name
        /// </summary>
        /// <param name="lambdaName"></param>
        /// <returns></returns>
        private List<Method> GetApiControllerMethods(string lambdaName)
        {

            // ex: PetStoreApi/Controllers/OrderApi.cs
            var fileText = File.ReadAllText(
                Path.Combine(solutionModel.SolutionRootFolderPath, 
                $"{solutionModel.AppName}Api", 
                "Controllers", $"{lambdaName}Api.cs"));

            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(fileText).GetCompilationUnitRoot();

            var classDecls = root
                .DescendantNodes().OfType<NamespaceDeclarationSyntax>()
                .Where(x => x.Name.ToString().Equals($"{solutionModel.AppName}Api.Controllers")) // ex: PetStoreApi.Controllers
                .First()
                    ?.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(x => x.Identifier.ValueText.Equals($"{lambdaName}ApiController"))
                    .First();

            if (classDecls == null)
                throw new Exception($"Error: Can't find {lambdaName}ApiController class");

            var methods = classDecls.DescendantNodes().OfType<MethodDeclarationSyntax>();
                
            var methodList = new List<Method>();
            foreach (var method in methods)
                methodList.Add(new Method(
                    methodSummary: method.GetLeadingTrivia().ToString(),
                    returnType: method.ReturnType.ToString(),
                    methodName: method.Identifier.ValueText,
                    arguments: method.ParameterList.ToString()
                    ));

            return methodList;
        }

        private List<Method> GetControllerMethods(string lambdaName)
        {
            // ex: Controllers/OrderApiController/OrderApiController.cs
            var fileText = File.ReadAllText(
                Path.Combine(solutionModel.SolutionRootFolderPath, 
                "Controllers", 
                $"{lambdaName}Controller", 
                $"{lambdaName}Controller.cs"));

            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(fileText).GetCompilationUnitRoot();

            var classDecls = root
                .DescendantNodes().OfType<NamespaceDeclarationSyntax>()
                .Where(x => x.Name.ToString().Equals($"{lambdaName}Controller")) // ex: OrderController
                .First()
                    ?.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(x => x.Identifier.ValueText.Equals($"{lambdaName}Controller")) // ex: OrderController
                    .First();

            if (classDecls == null)
                throw new Exception($"Error: Can't find {lambdaName}Controller");

            var methods = classDecls.DescendantNodes().OfType<MethodDeclarationSyntax>();

            var methodList = new List<Method>();
            foreach (var method in methods)
                methodList.Add(new Method(
                    methodSummary: method.GetLeadingTrivia().ToString(),
                    returnType: method.ReturnType.ToString(),
                    methodName: method.Identifier.ValueText,
                    arguments: method.ParameterList.ToString()
                    ));

            return methodList;
        }

        private string GenerateControllerMethods(List<Method> methods)
        {
            var text = string.Empty;
            foreach (var m in methods)
                text += m.GenerateControllerMethodDef();
            return text;
        }

        /// <summary>
        /// Parsed Method
        /// </summary>
        private class Method
        {
            public Method(string methodSummary, string returnType, string methodName, string arguments)
            {
                MethodSummary = methodSummary;
                ReturnType = returnType;
                MethodName = methodName;
                Arguments = arguments;

                if (!string.IsNullOrEmpty(arguments))
                {
                    var noAnnotationsRegEx = new Regex(@"\[.*?\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    ArgumentsNoAnnotations = noAnnotationsRegEx.Replace(arguments, String.Empty);
                    var argValuePairsRegEx = new Regex(@"[\w<>]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    var argValuePairs = argValuePairsRegEx.Matches(ArgumentsNoAnnotations);
                    bool argValue = false;
                    bool firstValue = true;
                    foreach (var am in argValuePairs)
                    {
                        if (argValue)
                        {
                            ArgName.Add(am.ToString());
                            ArgList += (firstValue) ? am.ToString() : ", " + am.ToString();
                            firstValue = false;
                            argValue = false;
                        }
                        else
                        {
                            argValue = true;
                            ArgType.Add(am.ToString());
                        }
                    }
                }
            }

            #region Properties
            /// <summary>
            /// Return type of method
            /// </summary>
            public string ReturnType { get; }

            /// <summary>
            /// Method Summary text
            /// </summary>
            public string MethodSummary { get; }

            /// <summary>
            ///  Method Name
            /// </summary>
            public string MethodName { get; }

            /// <summary>
            /// Arguments list with annotations. 
            /// </summary>
            public string Arguments { get; } = String.Empty;

            /// <summary>
            /// Argument list without annotations
            /// </summary>            
            public string ArgumentsNoAnnotations { get; } = String.Empty;

            /// <summary>
            /// Array of Argument types
            /// </summary>
            public List<string> ArgType { get; } = new List<string>();

            /// <summary>
            /// 
            /// Array of Argyment names
            /// </summary>
            public List<string> ArgName { get; } = new List<string>();

            /// <summary>
            /// Comma separate list of argument names
            /// </summary>
            public string ArgList { get; } = String.Empty;
            #endregion

            #region Methods
            public string GenerateControllerMethodDef()
            {
                //        //+lz DeleteOrder([FromRoute][Required]long orderId)
                //        /// <summary>
                //        /// Delete purchase order by ID
                //        /// </summary>
                //        /// <remarks>For valid response try integer IDs with positive integer value.\\ \\ Negative or non-integer values will generate API errors</remarks>
                //        /// <param name="orderId">ID of the order that needs to be deleted</param>
                //        /// <response code="400">Invalid ID supplied</response>
                //        /// <response code="404">Order not found</response>
                //        /// [HttpDelete]
                //        /// [Route("/order/{orderId}")]
                //        /// [ValidateModelState]
                //        //-lz
                //         public override IActionResult DeleteOrder([FromRoute][Required]long orderId)
                //         {
                //             throw new NotImplementedException("DeleteOrder not implemented in Svcs library");
                //         }
                string newSummary = string.Empty;
                if(!string.IsNullOrEmpty(MethodSummary))
                {
                    var lines = MethodSummary.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    // Trim leading spaces
                    int i = 0;
                    for (; i < lines.Length; i++)
                        lines[i] = lines[i].Trim();

                    var newLines = new List<string> { $"\t\t//+lz {MethodName} {Arguments}\n" };
                    foreach (var line in lines)
                        if (line.Length > 0)
                            if (line.StartsWith("/"))
                                newLines.Add($"\t\t{line}\n");
                            else
                                newLines.Add($"\t\t/// {line}\n");

                    newLines.Add($"\t\t//-lz\n");
                    newSummary = string.Concat(newLines);
                }

                var method = 
                    newSummary +
                    $"\t\tpublic override {ReturnType} {MethodName} {Arguments}\n" +
                    $"\t\t{{\n\t\t\tthrow new NotImplementedException(\"{MethodName} not implemented in Controller\");\n\t\t}}\n\n";
                return method;
            }
            #endregion
        }

        public void UpdateProjectReferences(string csprojFilePath, List<string> references)
        {
            var csprojXml = XElement.Load(csprojFilePath);
            // Remove current ItemGroup Label="LazyStackProjectReferences" section
            foreach (var itemGroup in csprojXml.Elements("ItemGroup"))
                if (itemGroup.Attribute("Label") != null && itemGroup.Attribute("Label").Value.Equals("LazyStackProjectReferences"))
                    itemGroup.Remove();

            if (references.Count > 0)
            {
                var newItemGroup = new XElement("ItemGroup", new XAttribute("Label", "LazyStackProjectReferences"));
                foreach (var reference in references)
                    newItemGroup.Add(new XElement("ProjectReference", new XAttribute("Include", reference)));

                csprojXml.Add(newItemGroup);
            }
            csprojXml.Save(csprojFilePath);
        }

        /// <summary>
        /// Update Project Package References from ProjectGenerationOptions data
        /// </summary>
        /// <param name="csprojFilePath"></param>
        /// <param name="projectType"></param>
        /// <param name="references"></param>
        public void UpdatePackageReferences(string csprojFilePath, string projectType)
        {
            var referencesPath = $"{projectType}/PackageReferences";

            var references = GetConfigPropertyItems(referencesPath, errorIfMissing: false);

            var csprojXml = XElement.Load(csprojFilePath);
            // Remove current ItemGroup Label="LazyStackPackageReferences" section
            foreach (var itemGroup in csprojXml.Elements("ItemGroup"))
                if (itemGroup.Attribute("Label") != null && itemGroup.Attribute("Label").Value.Equals("LazyStackPackageReferences"))
                    itemGroup.Remove();

            // Add item group if there are any package references managed by LazyStack
            if (references.Count > 0)
            {
                var newItemGroup = new XElement("ItemGroup", new XAttribute("Label", "LazyStackPackageReferences"));
                foreach (var reference in references)
                {
                    var projectVersion = GetConfigProperty($"{referencesPath}/{reference}", false);
                    if (string.IsNullOrEmpty(projectVersion))
                        newItemGroup.Add(new XElement("PackageReference",
                                new XAttribute("Include", reference)));
                    else
                        newItemGroup.Add(
                        new XElement("PackageReference",
                                new XAttribute("Include", reference),
                                new XAttribute("Version", projectVersion)));
                }
                csprojXml.Add(newItemGroup);
            }

            csprojXml.Save(csprojFilePath);
        }

        private string GetConfigProperty(string path, bool errorIfMissing = true)
        {
            var propertyValue = SolutionModel.GetNamedProperty(
                solutionModel.ProjectGenerationOptions,
                path,
                out YamlNode node
                );
            if (node == null && errorIfMissing)
                throw new Exception($"Error: Can't find value for {path} configuration property");
            if (node == null)
                return string.Empty;
            return node.ToString();
        }

        private List<string> GetConfigPropertyItems(string path, bool errorIfMissing = true)
        {
            var propertyValue = SolutionModel.GetNamedProperty(
                solutionModel.ProjectGenerationOptions,
                path,
                out YamlNode node
                );

            if (node == null && errorIfMissing)
                throw new Exception($"Error: Can't find value for {path} configuration property");

            if (node == null)
                return new List<string>();

            if (node.NodeType != YamlNodeType.Mapping)
                throw new Exception($"Error: Nodetype for {path} configuration property is not mapping node");

            var result = new List<string>();
            var mappingNode = node as YamlMappingNode;
            foreach (var key in mappingNode.Children.Keys)
                result.Add(key.ToString());
            return result;
        }
    }

    public class GenerateProject
    {
        public GenerateProject(SolutionModel solutionModel)
        {
            this.solutionModel = solutionModel;
        }

        public string Input { get; set; }
        public string Generator { get; set; }
        public string Output { get; set; }
        public bool SkipValidateSpec { get; set; }
        public Dictionary<String, String> AdditionalProperites { get; set; } = new Dictionary<string, string>();
        public string ProjectName { get; set; }

        readonly SolutionModel solutionModel;

        public void Run()
        {
            // We use OpenApiGenerator to generate projects
            // OpenApiGenerator creates a complete solution (which we do not need)
            // as well as the required project. So we generate into a 
            // temp folder, copy the project content we want to
            // the specified output folder and then remove the generated solution from
            // the temporary folder.

            var tempFolder = Path.Combine(Path.GetTempPath(),"LazyStack");

            // First Create LazyStack temp folder if it doesn't exist
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            var tempSolution = Path.Combine(tempFolder,Guid.NewGuid().ToString());
            if (!Directory.Exists(tempSolution))
                Directory.CreateDirectory(tempSolution);

            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                pProcess.StartInfo.FileName = @"java.exe";

                var arguments = $"-jar \"{solutionModel.OpenApiGeneratorFilePath}\" generate -i \"{Input}\" -g \"{Generator}\" -o \"{tempSolution}\"";
                if (SkipValidateSpec)
                    arguments += " --skip-validate-spec";


                // Falling back to Netstandard2.0 syntax for compatibility with Net4.7.2 (ie Visual Studio)
            
                //pProcess.StartInfo.ArgumentList.Add("-jar");
                //pProcess.StartInfo.ArgumentList.Add(solutionModel.OpenApiGeneratorFilePath);
                //pProcess.StartInfo.ArgumentList.Add($"generate");
                //pProcess.StartInfo.ArgumentList.Add("-i");
                //pProcess.StartInfo.ArgumentList.Add(Input);
                //pProcess.StartInfo.ArgumentList.Add("-g");
                //pProcess.StartInfo.ArgumentList.Add(Generator);
                //pProcess.StartInfo.ArgumentList.Add("-o");
                //pProcess.StartInfo.ArgumentList.Add(tempSolution);
                //if (SkipValidateSpec)
                //    pProcess.StartInfo.ArgumentList.Add("--skip-validate-spec");
                string props = String.Empty;
                bool first = true;
                foreach (KeyValuePair<string, string> kvp in AdditionalProperites)
                {
                    props += (first) ? $" --additional-properties={kvp.Key}={kvp.Value}" : $",{kvp.Key}={kvp.Value}";
                    first = false;
                }

                //if (!string.IsNullOrEmpty(props))
                //    pProcess.StartInfo.ArgumentList.Add(props);

                if (!string.IsNullOrEmpty(props))
                    arguments += props;

                pProcess.StartInfo.Arguments = arguments;

                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                pProcess.StartInfo.CreateNoWindow = true; //di not diplay a windows
                Debug.WriteLine($"Calling openapi-generagor {arguments}");
                pProcess.Start();
                StreamReader reader = pProcess.StandardOutput;
                string output = reader.ReadToEnd();
                pProcess.WaitForExit();
                Debug.WriteLine($"{output}");
                Debug.WriteLine($"Exit Code: {pProcess.ExitCode}");

                if(Directory.Exists(tempSolution))
                {
                    // Copy the project to the specified output folder
                    var srcProjectFolderPath = Path.Combine(tempSolution,"src", ProjectName);
                    var tarProjectFolderPath = Path.Combine(Output, ProjectName);
                    Utilities.DirectoryCopy(srcProjectFolderPath, tarProjectFolderPath, true, true);
                }
            }
            // Remove the temporary solution folder
            if(Directory.Exists(tempSolution))
                Directory.Delete(tempSolution, true);
        }


    }

}
