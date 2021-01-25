using Force.DeepCloner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace LazyStack
{
    public class SolutionModel
    {
        public SolutionModel(string solutionFilePath, ILogger logger)
        {
            this.logger = logger;

            SolutionRootFolderPath = Path.GetDirectoryName(solutionFilePath); // may be empty
            AppName = Path.GetFileNameWithoutExtension(solutionFilePath); // may be empty

            // use current directory if no directory was specified           
            if (string.IsNullOrEmpty(SolutionRootFolderPath))
                SolutionRootFolderPath = Directory.GetCurrentDirectory();

            // find sln file if none was specified
            if (string.IsNullOrEmpty(AppName))
            { 
                // Find Visual Studio solution file
                var solFiles = Directory.GetFiles(SolutionRootFolderPath, "*.sln");
                if (solFiles.Length > 1)
                    throw new System.Exception("More than one .sln file in folder");

                if (solFiles.Length == 0)
                    throw new System.Exception("Solution file not found");

                AppName = Path.GetFileNameWithoutExtension(solFiles[0]);
            }

            SolutionFilePath = Path.Combine(SolutionRootFolderPath, $"{AppName}.sln");

            if (!File.Exists(SolutionFilePath))
                throw new Exception($"Error: \"{SolutionFilePath}\" does not exist");

            OpenApiFilePath = Path.Combine(SolutionRootFolderPath, $"{AppName}.yaml");

            SAMTemplateFilePath = Path.Combine(SolutionRootFolderPath, "template.yaml");

            if (!File.Exists(OpenApiFilePath))
                throw new System.Exception($"{AppName}.yaml, api file missing");

            LambdaFolderPath = Path.Combine(SolutionRootFolderPath, "Lambdas");
            if (!Directory.Exists(LambdaFolderPath))
            Directory.CreateDirectory(LambdaFolderPath);

            ControllersFolderPath = Path.Combine(SolutionRootFolderPath, "Controllers");
            if (!Directory.Exists(ControllersFolderPath))
            Directory.CreateDirectory(ControllersFolderPath);

            // Initialize paths to LazyStack assets
            var executingAssemblyFilePath = Assembly.GetExecutingAssembly().Location;
            var executingAssemblyFolderPath = Path.GetDirectoryName(executingAssemblyFilePath);

            LazyStackTemplateFolderPath = Path.Combine(executingAssemblyFolderPath, "Templates");
            if (!Directory.Exists(LazyStackTemplateFolderPath))
                throw new System.Exception("LazyStack Templates folder Missing. Check installation.");

            SrcTemplateFilePath = Path.Combine(LazyStackTemplateFolderPath, "template.yaml");
            if (!File.Exists(SrcTemplateFilePath))
                throw new System.Exception("Lazy Stack template.yaml Missing. Check installation.");

            InitDefaults();
        }

        #region Properties
        public string SolutionFilePath { get; private set; }
        public string SolutionRootFolderPath { get; private set; }
        public string AppName { get; private set; }
        public string OpenApiFilePath { get; private set; }

        public string LazyStackTemplateFolderPath { get; private set; }
        public string SrcTemplateFilePath { get; private set; }

        public string SAMTemplateFilePath { get; private set; }

        public string LambdaFolderPath { get; private set; }
        public string ControllersFolderPath { get; private set; }
        
        public Dictionary<string, AwsResource> Resources { get; } = new Dictionary<string, AwsResource>(); // Key is resource name, value is Resource 
        public Dictionary<string, AwsApi> Apis { get; } = new Dictionary<string, AwsApi>(); // Key is Api resource name, Contain Api and/or HttpApi resource references
        public Dictionary<string, AWSLambda> Lambdas { get; } = new Dictionary<string, AWSLambda>();  // key is LambdaName
        public List<string> Tags { get; } = new List<string>(); // List of tags
        public Dictionary<string, string> LambdaNameByTagName { get; } = new Dictionary<string, string>(); // key is TagName, value is LambdaName
        public Dictionary<string, string> TagNameByLambdaName { get; } = new Dictionary<string, string>(); // key is LambdaName, value is Tagname
        public Dictionary<string, string> ApiNameByTagName { get; } = new Dictionary<string, string>(); // Key is TagName, value is ApiName
        public Dictionary<string, EndPoint> EndPoints { get; } = new Dictionary<string, EndPoint>(); // Key is OperationId

        public Dictionary<string, ProjectInfo> Projects { get; } = new Dictionary<string, ProjectInfo>(); // key is projectName
        public ProjectInfo ClientSDK { get; set; }
        public string ClientSDKProjectName { get; set; }

        public List<string> SolutionFolders { get; } = new List<string>();

        public string DefaultApi { get; set; } = "HttpApiUnsecure";
        public string UsersAwsTemplate { get; set; } = string.Empty;
        public string DefaultTag { get; set; } = string.Empty;

        public bool IsCognitoConfigured { get; set; }
        public string UserPoolName { get; set; }
        public string UserPoolClientName { get; set; }
        public string IdentityPoolName { get; set; }

        public bool IsIAMConfigured { get; set; }

        public YamlMappingNode DefaultResourceConfigurations { get; set; }

        public string CodeUriTarget { get; set; }


        /// <summary>
        /// LazyStack default template values keyed by Resource Type.
        /// </summary>
        public Dictionary<string, YamlMappingNode> LzDefaultTemplate = new Dictionary<string, YamlMappingNode>();
        public YamlMappingNode ProjectGenerationOptions;
        public YamlMappingNode OpenApiSpecRootNode;
        public string OpenApiSpecText;

        public Dictionary<string, Environment> Environments { get; set; } = new Dictionary<string, Environment>();
        public Dictionary<string, AwsSettings.LocalApi> LocalApis { get; set; } = new Dictionary<string, AwsSettings.LocalApi>();

        #endregion Properties

        #region Variables
        YamlMappingNode lzConfigRootNode;

        // LazyStackVersions earliest to latest
        List<string> LazyStackVersions = new List<string> { "1.0.0" }; 
        string LazyStackDirectivesVersion = string.Empty;
        YamlMappingNode samRootNode;
        readonly ILogger logger;
        bool IsConfigurationParsed;
        #endregion Variables

        #region Methods

        /// <summary>
        /// Init Stack with LazyStack defaults
        /// </summary>
        public void InitDefaults()
        {
            // Load default resource configurations
            var fileName = Path.Combine(LazyStackTemplateFolderPath, "aws_defaults.yaml");
            if (!File.Exists(fileName))
                throw new Exception("Error: aws_defaults.yaml file missing from LazyStack templates folder");
            DefaultResourceConfigurations = ReadAndParseYamlFile(fileName);

            // Read the LazyStack.yaml from the template directory to get project generation defaults
            fileName = Path.Combine(LazyStackTemplateFolderPath, "LazyStack.yaml");
            if (!File.Exists(fileName))
                throw new Exception("Error: LazyStack.yaml file missing from LazyStack templates folder");

 
            var lazyStackNode = ReadAndParseYamlFile(fileName);
            if (!lazyStackNode.Children.TryGetValue("ProjectOptions", out YamlNode node))
                throw new Exception("Error: ProjectOptions missing from LazyStack.yaml file in LazyStack templates folder ");
            ProjectGenerationOptions = node as YamlMappingNode;
        }

       public async Task LoadLazyStackDirectives()
        {
            var earliestLazyStackVersion = LazyStackVersions[0];
            var latestLazyStackVersion = LazyStackVersions[LazyStackVersions.Count - 1];

            var lzConfigFilePath = Path.Combine(SolutionRootFolderPath, "LazyStack.yaml");
            if (!File.Exists(lzConfigFilePath))
            {
                string fileText =
@"# LazyStack Version __Version__
Stacks:
  Dev:
    ProfileName: default
    RegionName: us-east-1
    Stackname: __AppName__Dev 
    Stage: Dev
    IncludeLocalApis: true";
                fileText = fileText.Replace("__Version__", latestLazyStackVersion);
                fileText = fileText.Replace("__AppName__", AppName);
                File.WriteAllText(lzConfigFilePath, fileText);
            }

            // Load Users LazyStack.ymal
            await logger.InfoAsync($"\nLoading LazyStack.yaml configuration file");
            lzConfigRootNode = ReadAndParseYamlFile(lzConfigFilePath);
            Debug.WriteLine($"lzConfigRootNode \n{new SerializerBuilder().Build().Serialize(lzConfigRootNode)}");

            // Check version of LazyStack.yaml -- must be first line of file
            var versionErrorString = $"Error: LazyStack.yaml firstline must start with \"# LazyStack Version {latestLazyStackVersion}\" -- where version is {earliestLazyStackVersion} to {latestLazyStackVersion}";
            var firstLine = string.Empty;
            using (StreamReader reader = new StreamReader(lzConfigFilePath))
            {
                firstLine = reader.ReadLine() ?? string.Empty;
            }
            if (string.IsNullOrEmpty(firstLine))
                throw new Exception(versionErrorString);

            var firstLineParts = firstLine.Split(' ');
            if(firstLineParts.Length != 4)
                throw new Exception(versionErrorString);

            if(!firstLineParts[0].Equals("#") || !firstLineParts[1].Equals("LazyStack") || !firstLineParts[2].Equals("Version"))
                throw new Exception(versionErrorString);

            LazyStackDirectivesVersion = firstLineParts[3];
            var found = false;
            foreach (var version in LazyStackVersions)
                if (found = version.Equals(LazyStackDirectivesVersion))
                    break;

            if (!found)
                throw new Exception($"Error: LazyStack.yaml file version {LazyStackDirectivesVersion} is not a recognized version.");

            // TODO - later on we will check if the directives file (and processed projects etc.) need to be upgraded
            // We will add a LazyStack -- Update Generated Projects to Latest Version command

            // Parse top-level configuration directives from Users LazyStack.yaml file
            // DefaultApi, AwsTemplate, ProjectOptions, Environments, LocalApis
            // Note: ProjectOptions add/override existing ProjectOptions that were loaded in InitDefaults processing of template\LazyStack.yaml
            await ParseLzConfigurationAsync();

            IsConfigurationParsed = true;

        }


        /// <summary>
        /// Read the solutions OpenApi specification file, and directives in
        /// the LazyStack.yaml to write the solution's SAM serverless.template file.
        /// </summary>
        public async Task ProcessOpenApiAsync()
        {
            // Load Users OpenApi specification file
            await logger.InfoAsync($"\nLoading OpenApi Specification {OpenApiFilePath}");
            OpenApiSpecText = File.ReadAllText(OpenApiFilePath);
            OpenApiSpecRootNode = ParseYamlText(OpenApiSpecText);

            if (!IsConfigurationParsed)
                await LoadLazyStackDirectives();

            // Create folders for each environment
            await CreateEnvironmentFolders();

            // HttpApiUnsecure, HttpApiSecure, ApiUnsecure, ApiSecure, 
            // UserPool, UserPoolClient, IdentityPool, CognitoIdentityPoolRoles, AuthRole, UnAuthRole
            await CreateDefaultResourcesAsync();

            // At this point we have default resources provided by LazyStack templates. We also have
            // the final merged directives information from both the default template LazyStack.yaml and 
            // any user LazyStack.yaml. Remember that the user can use the AwsResources directive to
            // add AWS resources here.

            // Read user 
            // UPDATES Resources dictionary, samRootNode, Apis dictionary
            // The users resource specifications override LazyStack template defaults.
            await LoadSAMAsync(); // creates samRootNode

            // Parse OpenApi tags object to generate list of LambdaNames we will generate
            // UPDATES Tags list, Lambdas dictionary, LambdaByName dictionary, ApiNameByTagName dictionary
            await ParseOpenApiTagsObjectForLambdaNamesAsync();

            // Parse AwsResources diective loaded from LazyStack.yaml
            // UPDATES Resources dictionary, Apis dictionary
            // Skips lambda resources - see ParseAwsReosurcesForLambdaAsync()
            await ParseAwsResourcesAsync();

            // Parse ApiTagMap
            // Updates ApiNameByTagName dictionary
            // Uses Default Api when tag is not mapped!
            await ParseApiTagMapAsync();

            // Create/Updates solutionModel.Resources, solutionModel.Apis.Lambdas and solutionModel.Lambdas 
            await ParseOpenApiTagsObjectAsync();

            // Overwrite/Extend Lambda properties
            await ParseAwsResourcesForLambdasAsync();

            // Add Lambda.Events for each Path/Operation (route) in OpenApi specification
            // Updates EndPoints dictionary, updates Lambda event node(s)
            await ParseOpenApiPathObjectAsync();

            // Prune -- remove default resources not referenced
            await PruneResourcesAsync();

            // Identify ApiGateway Security level and set SecurityLevel property
            DiscoverSecurityLevel();

        }

        /// <summary>
        /// Examine each Api and set security level in asset to facilitate project generation.
        /// </summary>
        private void DiscoverSecurityLevel()
        {
            // Set the security levels of the Api
            foreach (var api in Apis.Values)
                api.DiscoverSecurityLevel();

        }

        /// <summary>
        /// Validate LazyStack.yaml Directives and process initilizers
        /// DefaultApi: - initializer
        /// AwsTemplate: - initializer
        /// AwsResoures: - just verify good directive name
        /// ApiTagMap: - just verify good directive name
        /// TagLambdas: - just verify good directive name
        /// </summary>
        private async Task ParseLzConfigurationAsync()
        {
            if (lzConfigRootNode == null)
                return;


            await logger.InfoAsync($"\nLoading LazyStack Configuration");

            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();

            // Parse LazyStack Directives in config file
            foreach (KeyValuePair<YamlNode, YamlNode> kvp in lzConfigRootNode)
                switch (kvp.Key.ToString())
                {
                    case "DefaultApi": // Initializer
                        DefaultApi = kvp.Value.ToString();
                        await logger.InfoAsync($"  Default-Api = {DefaultApi}");
                        break;

                    case "AwsTemplate": // Initializer
                        // Don't load it, just make sure it exists
                        if (!string.IsNullOrEmpty(UsersAwsTemplate))
                            throw new Exception($"Error: More than AwsTemplate directive found.");

                        if (string.IsNullOrEmpty(kvp.Value.ToString()))
                            throw new Exception($"AwsTemplate value can't be empty");

                        UsersAwsTemplate = kvp.Value.ToString();
                        var path = Path.Combine(SolutionRootFolderPath, UsersAwsTemplate);
                        if (!File.Exists(path))
                            throw new Exception($"AwsTemplate file not found {path}");

                        await logger.InfoAsync($"  AwsTempalte = {UsersAwsTemplate}");

                        break;

                    case "AwsResources":
                        // skip - this directive is handled in ParseLzAwsResources()
                        break;

                    case "TagLambdas":
                        // skip - is directive is handled in ParseTagLambdas()
                        break;

                    case "ApiTagMap":
                        // skip - this directive is handled by ParseLzApiTagMap
                        break;

                    case "ProjectOptions":
                        // ProjectGenerationOptions are also loaded from template\LazyStack.yaml so we merge here - right into left
                        ProjectGenerationOptions= MergeNode( leftNode: ProjectGenerationOptions, rightNode: kvp.Value) as YamlMappingNode;
                        Debug.WriteLine($"ProjectGenerationOptions\n {new SerializerBuilder().Build().Serialize(ProjectGenerationOptions)}");
                        break;

                    case "Stacks": 
                        // Load values - environments are used by the Generate Settings process
                        var envYamlStr = SolutionModel.YamlNodeToText(kvp.Value as YamlMappingNode);
                        try
                        {
                            Environments = deserializer.Deserialize<Dictionary<string, Environment>>(SolutionModel.YamlNodeToText(kvp.Value as YamlMappingNode));
                            if (Environments.Count == 0)
                                throw new Exception("No Stacks defined");
                            if (!Environments.TryGetValue("Dev", out Environment devEnv))
                                throw new Exception("No \"Dev\" stack defined");
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Error: Could not parse Stacks section. {e.InnerException.Message}");
                        }
                        break;

                    case "LocalApis":
                        // Load values - LocalApis are used by the Generate Settings process
                        var localApisYamlStr = SolutionModel.YamlNodeToText(kvp.Value as YamlMappingNode);
                        try
                        {
                            LocalApis = deserializer.Deserialize<Dictionary<string, AwsSettings.LocalApi>>(SolutionModel.YamlNodeToText(kvp.Value as YamlMappingNode));
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Error: Could not parse LocalApis section. {e.InnerException.Message}");
                        }
                        break;

                    default: // stop on any unrecognized directives
                        throw new Exception($"Unknown directive {kvp.Key}");
                }
        }


        private async Task CreateEnvironmentFolders()
        {
            await Task.Delay(0); // avoid no await warning 
            try
            {
                var envFoldersPath = Path.Combine(SolutionRootFolderPath, "Stacks");
                if (!Directory.Exists(envFoldersPath))
                    Directory.CreateDirectory(envFoldersPath);
                foreach (var kvp in Environments)
                {
                    var envFolderPath = Path.Combine(envFoldersPath, kvp.Key);
                    if (!Directory.Exists(envFolderPath))
                        Directory.CreateDirectory(envFolderPath);
                }
                var existingEnvFolders = Directory.GetDirectories(envFoldersPath);
                foreach(var folder in existingEnvFolders)
                {
                    
                    var dirParts = folder.Split('\\');
                    var dir = dirParts[dirParts.Length - 1]; 

                    // remove any obsolete env folders
                    if (!Environments.ContainsKey(dir))
                        Directory.Delete(folder);
                }
            }
            catch
            {
                throw new Exception($"Error: Could not create stack folder(s)");
            }
        }

        /// <summary>
        /// Merge in LazyStack default resources
        /// HttpApiUnsecure
        /// HttpApiSecure
        /// ApiUnsecure
        /// ApiSecure
        /// </summary>
        private async Task CreateDefaultResourcesAsync()
        {
            await logger.InfoAsync($"\nCreating default resources");
            Debug.WriteLine("Creating default resources");
            // Load default resources
            var defaultDoc = ReadAndParseYamlFile(Path.Combine(LazyStackTemplateFolderPath, "default_resources.yaml"));
            if (defaultDoc.Children.TryGetValue("Resources", out YamlNode resources))
                foreach (KeyValuePair<YamlNode, YamlNode> kvp in resources as YamlMappingNode)
                {
                    await logger.InfoAsync($"  Loading default resource: {kvp.Key}");
                    var resource =AwsResource.MakeGeneralResource(kvp.Key.ToString(), kvp.Value as YamlMappingNode, solutionModel: this, isDefault: true);
                    Debug.WriteLine($"Created Resource: {kvp.Key}\n{new SerializerBuilder().Build().Serialize(resource.RootNode)}");
                }
        }

        /// <summary>
        /// Load in users AwsTemplate - creates samRootNode
        /// UPDATES Resources dictionary, samRootNode, Apis dictionary
        /// </summary>
        private async Task LoadSAMAsync()
        {
          
            if(!string.IsNullOrEmpty(UsersAwsTemplate))
                await logger.InfoAsync($"\nLoading SAM Template {UsersAwsTemplate}");

            // Load user supplied SAM template (or minimal template from templates folder if no user template provided)
            var tpl = new YamlStream();
            var source = !string.IsNullOrEmpty(UsersAwsTemplate)
                ? SAMTemplateFilePath
                : Path.Combine(LazyStackTemplateFolderPath, "template.yaml");

            var text = File.ReadAllText(source);
            tpl.Load(new StringReader(text));
            samRootNode = tpl.Documents[0].RootNode as YamlMappingNode;
            if (samRootNode.Children.TryGetValue("Resources", out YamlNode node))
            {
                if (node.NodeType != YamlNodeType.Mapping)
                    throw new Exception($"Error: Sam Template has invalid Resources Node");

                foreach (KeyValuePair<YamlNode, YamlNode> kvp in node as YamlMappingNode)
                {
                    await logger.InfoAsync($"  Loading SAM Template resource: {kvp.Key}");
                    AwsResource.MakeGeneralResource(kvp.Key.ToString(), kvp.Value as YamlMappingNode, this, isDefault: false);
                }

                // Remove Resources Node as we will add it back in later from the solutionModel.Resources array
                samRootNode.Children.Remove("Resources");
            }
        }

        /// <summary>
        /// Parse the OpenApi Tags Object to get tag names and generate LambdaNames.
        /// UPDATES Tags list, Lambdas dictionary, LambdaByName dictionary, ApiNameByTagName dictionary
        /// </summary>
        private async Task ParseOpenApiTagsObjectForLambdaNamesAsync()
        {
            await logger.InfoAsync($"\nParsing OpenApi Specification tags");
            if (!OpenApiSpecRootNode.Children.TryGetValue("tags", out YamlNode node))
                throw new Exception($"Error: Your OpenApi specification contains no tags. LazyStack can't generate an Api.");
            else
                foreach (YamlMappingNode tagsNodeItem in node as YamlSequenceNode)
                {
                    var tagName = tagsNodeItem["name"].ToString();
                    if (Tags.Contains(tagName))
                        throw new Exception($"Error: Tag \"{tagName}\" defined twice");
                    Tags.Add(tagName);
                    var lambdaName = TagToFunctionName(tagName);
                    Lambdas.Add(lambdaName, null);
                    LambdaNameByTagName.Add(tagName, lambdaName);
                    TagNameByLambdaName.Add(lambdaName, tagName);
                    ApiNameByTagName.Add(tagName, string.Empty);
                    await logger.InfoAsync($"  {tagName} will create lambda {lambdaName}");
                }
        }

        /// <summary>
        /// Parse AwsResource directive in LazyStack.yaml file.
        /// UPDATES Resources dictionary, Apis dictionary
        /// Throws error if a Lambda resource is specified that LazyStack will generate!
        /// </summary>
        private async Task ParseAwsResourcesAsync()
        {
            if (lzConfigRootNode == null)
                return;

            await logger.InfoAsync($"\nParsing AwsResource (if any) in LazyStack configuration");
            Debug.WriteLine("\nParsing AwsREsources (if any) in LazyStack configuration");
            // Parse LazyStack Resources
            if (lzConfigRootNode.Children.TryGetValue("AwsResources", out YamlNode node))
                foreach (KeyValuePair<YamlNode, YamlNode> kvp in node as YamlMappingNode)
                {
                    if (!Lambdas.ContainsKey(kvp.Key.ToString()))
                    {
                        await logger.InfoAsync($"  Loading resource {kvp.Key}");
                        AwsResource.MakeGeneralResource(kvp.Key.ToString(), kvp.Value as YamlMappingNode, this, isDefault: false); // Added to solutionModel.Resources
                    }
                }
        }

        /// <summary>
        /// Parse AwsResource directive in LazyStack.yaml file.
        /// UPDATES Resources dictionary, Apis dictionary
        /// Throws error if a Lambda resource is specified that LazyStack will generate!
        /// </summary>
        private async Task ParseAwsResourcesForLambdasAsync()
        {
            if (lzConfigRootNode == null)
                return;

            await logger.InfoAsync($"\nParsing AwsResource (if any) for Lambda properties in LazyStack configuration");
            Debug.WriteLine("\nParsing AwsResources (if any) for Lamnbda properties in LazyStack configuration");
            // Parse LazyStack Resources
            if (lzConfigRootNode.Children.TryGetValue("AwsResources", out YamlNode node))
                foreach (KeyValuePair<YamlNode, YamlNode> kvp in node as YamlMappingNode)
                {
                    if (Lambdas.ContainsKey(kvp.Key.ToString()))
                    {
                        var lambda = Lambdas[kvp.Key.ToString()];
                        var lambdaResource = lambda.AwsResource;
                        var lambdaResourceTxt = YamlNodeToText(lambdaResource.RootNode);
                        await logger.InfoAsync($"  Updating Lambda resource {kvp.Key}");

                        // Check for illegal properties - thse are properties that LazyStack manages
                        YamlNode outNode;
                        if (GetNamedProperty(kvp.Value, "Properties/FunctionName", out outNode))
                            throw new Exception($"    Error: Property FunctionName not allowed in AwsResources AWS::Serverless::Function resource. LazyStack manages this property.");

                        if (GetNamedProperty(kvp.Value, "Properties/CodeUri", out outNode))
                            throw new Exception($"    Error: Property CodeUri not allowed in AwsResources AWS::Serverless::Function resource. LazyStack manages this property.");

                        if (GetNamedProperty(kvp.Value, "Properties/Handler", out outNode))
                            throw new Exception($"    Error: Property Handler not allowed in AwsResources AWS::Serverless::Function resource. LazyStack manages this property.");

                        if (GetNamedProperty(kvp.Value, "Properties/Events", out outNode))
                            throw new Exception($"    Error: Property Events not allowed in AwsResources AWS::Serverless::Function resource. LazyStack manages this property.");

                        lambdaResource.RootNode = MergeNode(lambdaResource.RootNode as YamlNode, kvp.Value) as YamlMappingNode;
                    }
                }
        }

        /// <summary>
        /// Parse the ApiTagMap
        /// UPDATES ApiNameByTagName dictionary
        /// Uses Default Api when tag is not mapped!
        /// </summary>
        private async Task ParseApiTagMapAsync()
        {
            if (lzConfigRootNode != null)
                if (lzConfigRootNode.Children.TryGetValue("ApiTagMap", out YamlNode node))
                {
                    await logger.InfoAsync($"\nParsing ApiTagMap directive in LazyStack configuration");
                    Debug.WriteLine($"\nParsing ApiTagMap directive in LazyStack configuration");
                    foreach (KeyValuePair<YamlNode, YamlNode> apiKvp in node as YamlMappingNode)
                    {
                        var apiName = apiKvp.Key.ToString();
                        if (!Apis.ContainsKey(apiName))
                            throw new Exception($"Error: ApiTagMap contains reference to \"{apiName}\" which does not exist.");

                        string tagName = string.Empty;
                        foreach (YamlNode lambdaNode in apiKvp.Value as YamlSequenceNode)
                            if (ApiNameByTagName.ContainsKey(tagName = lambdaNode.ToString()))
                            {
                                await logger.InfoAsync($"  {tagName}  {apiName}");
                                ApiNameByTagName[tagName] = apiName;
                            }
                            else
                                throw new Exception($"Error: Unknown tag \"{tagName}\" under API \"{apiName}\"");
                    }
                }
            var first = false;
            foreach (var tagName in Tags)
                if (string.IsNullOrEmpty(ApiNameByTagName[tagName]))
                {
                    if(first)
                        await logger.InfoAsync($"Api Tag Map Default Assignments");

                    await logger.InfoAsync($"  {tagName}  {DefaultApi}");
                    ApiNameByTagName[tagName] = DefaultApi;
                }
        }

        /// <summary>
        /// Parse the Tag Object in the openApi specification
        /// </summary>
        private async Task ParseOpenApiTagsObjectAsync()
        {
            await logger.InfoAsync($"\nParsing OpenApi Specification tags to generate Lambdas");
            Debug.WriteLine("\nParsing OpenApi Specification tags to generate Lambdas");
            
            if (!OpenApiSpecRootNode.Children.TryGetValue("tags", out YamlNode tagsNode))
                throw new Exception($"Error: Could not find a tags field. An OpenApi tags field is required for LazyStack to generate an api.");

            if (tagsNode.NodeType != YamlNodeType.Sequence)
                throw new Exception($"Error: tags field is not a sequence node.");

            if (!OpenApiSpecRootNode.Children.TryGetValue("paths", out YamlNode pathsNode))
                throw new Exception($"Error: Could not find a paths field. An OpenApi paths field is required for LazyStack to generate an api.");

            if (pathsNode.NodeType != YamlNodeType.Mapping)
                throw new Exception($"Error: paths field is not a mapping node.");

            foreach (YamlMappingNode tagsNodeItem in tagsNode as YamlSequenceNode)
            {
                var tagName = tagsNodeItem["name"].ToString();
                var lambdaName = TagToFunctionName(tagName);
                var apiName = ApiNameByTagName[tagName];
                await logger.InfoAsync($"  Creating resource \"{lambdaName}\" for tag \"{tagName}\"");
                Debug.WriteLine($"  Creating resource \"{lambdaName}\" for tag \"{tagName}\"");
                var resource = AwsResource.MakeLambdaResource(this, tagName, Apis[apiName]);
                if(lzConfigRootNode != null &&  GetNamedProperty(lzConfigRootNode, "ProjectOptions/LambdaProjects/Properties", out YamlNode propertiesNode))
                {
                    if (!GetNamedProperty(resource.RootNode, "Properties", out YamlNode projectPropertiesNode))
                        throw new Exception("Error: Lambda project missing Properties");
                    var mergedNode = MergeNode(projectPropertiesNode, propertiesNode) as YamlMappingNode;
                    resource.RootNode.Children["Properties"] = mergedNode;
                }

                // Initialize OpenApiSpecification for Lambda
                var lambda = Lambdas[lambdaName];
                lambda.OpenApiSpec = OpenApiSpecRootNode.DeepClone();
                lambda.OpenApiSpec.Children["paths"] = new YamlMappingNode(); // we add paths to this spec in ParseOpenApiaPathObject()
                lambda.OpenApiSpec.Children["tags"] = new YamlSequenceNode(new YamlMappingNode("name", tagName)); // a tag generates a lambda
                Debug.WriteLine($"Lamnda OpenApi\n{new SerializerBuilder().Build().Serialize(lambda.OpenApiSpec)}");
                Debug.WriteLine($"Lambda SAM\n {new SerializerBuilder().Build().Serialize(resource.RootNode)}");
            }
        }

        /// <summary>
        /// Parse the Path Object in the OpenApi specification
        /// We look for and handle the following properties
        /// tags:
        /// - string   # tag identifying the AWSLambda to handle this path
        /// 
        /// For each Path we add an Event to the Lambda associated with the tag.
        /// 
        /// </summary>
        private async Task ParseOpenApiPathObjectAsync()
        {
            await logger.InfoAsync($"\nParsing OpenApi Specifications paths to generate Lambda events");
            Debug.WriteLine($"\nParsing OpenApi Specifications paths to generate Lambda events");

            string[] validOperations = { "GET", "PUT", "POST", "DELETE", "UPDATE" };

            if (!OpenApiSpecRootNode.Children.TryGetValue("paths", out YamlNode pathsNode))
                throw new Exception($"Error: OpenApi specification missing \"Paths\" Object");

            // foreach Path in Paths
            foreach (KeyValuePair<YamlNode, YamlNode> pathsNodeChild in pathsNode as YamlMappingNode)
            {
                var path = pathsNodeChild.Key.ToString();
                var pathNodeValue = (YamlMappingNode)pathsNodeChild.Value;

                await logger.InfoAsync($"  {path}");
                Debug.WriteLine($"{path}");
                // foreach Operation in Path
                foreach (KeyValuePair<YamlNode, YamlNode> apiPathNodeChild in pathNodeValue)
                {
                    var httpOperation = apiPathNodeChild.Key.ToString().ToUpper(); // ex: get, post, put, delete
                    var apiOpNode = apiPathNodeChild.Value as YamlMappingNode; // ex: tags, summary, operationId, responses, requestBody etc.

                    if (!validOperations.Contains(httpOperation))
                        throw new Exception($"Error: Invalid http operation \"{httpOperation}\" specified in \"{path}\"");

                    // Get the tag
                    var tagName = string.Empty;
                    if (apiOpNode.Children.TryGetValue("tags", out YamlNode apiOpTagsNode))
                    {
                        if (apiOpTagsNode.NodeType != YamlNodeType.Sequence)
                            throw new Exception($"Error: Path \"{path}\" Tag Object must be a sequence node");
                        tagName = ((YamlSequenceNode)apiOpTagsNode).Children[0].ToString();
                    }
                    else
                        tagName = DefaultTag;

                    await logger.InfoAsync($"    {httpOperation} {tagName}");
                    Debug.WriteLine($" {httpOperation} {tagName}");

                    // Find Lambda in global index of Lambdas (indexed by tag) 
                    if (!Lambdas.TryGetValue(LambdaNameByTagName[tagName], out AWSLambda awsLambda))
                        throw new Exception($"Error: Tag \"{tagName}\" specified for path \"{path}\" not found.");

                    // Add path/operation to lamnda.OpenApiSpec
                    YamlMappingNode paths = awsLambda.OpenApiSpec.Children["paths"] as YamlMappingNode;
                    YamlMappingNode pathMappingNode;
                    if (!paths.Children.TryGetValue(path, out YamlNode pathNode))
                        paths.Children.Add(path, new YamlMappingNode());

                    pathMappingNode = paths.Children[path] as YamlMappingNode;
                    pathMappingNode.Add(httpOperation, (apiPathNodeChild.Value as YamlMappingNode).DeepClone());

                    // Generate eventName
                    // ex: GET /order/{orderId} => GetOrderOrderId
                    GetNamedProperty(apiOpNode, "operationId", out YamlNode operationIdNode);
                    var operationId = operationIdNode == null ? string.Empty : operationIdNode.ToString();
                    var eventName = RouteToEventName(httpOperation, path, operationId);
                    if (awsLambda.AwsResource.RootNode.Children.TryGetValue("Events", out YamlNode eventsNode)
                        && ((YamlMappingNode)eventsNode).Children.ContainsKey(eventName))
                        throw new Exception($"Error: Duplicate event name \"{eventName}\" in Lambda");
                    if (EndPoints.ContainsKey(eventName))
                        throw new Exception($"Error: Duplicate event name \"{eventName}\" in Stack");

                    // Add event into the global EndPoints Dictionary
                    EndPoints.Add(eventName, new EndPoint(eventName, awsLambda.AwsApi));

                    // The api is responsible for creating the event node because Event nodes are different based on Api
                    var eventNode = awsLambda.AwsApi.EventNode(path, httpOperation);

                    var inspectAwsResource = new SerializerBuilder().Build().Serialize(awsLambda.AwsResource.RootNode);
                    Debug.WriteLine($"{inspectAwsResource}");

                    YamlMappingNode properties = awsLambda.AwsResource.RootNode.Children["Properties"] as YamlMappingNode;

                    YamlMappingNode eventsMappingNode;
                    if (properties.Children.TryGetValue("Events", out YamlNode node))
                        eventsMappingNode = node as YamlMappingNode;
                    else
                    {
                        eventsMappingNode = new YamlMappingNode();
                        properties.Children.Add("Events", eventsMappingNode);
                    }
                    eventsMappingNode.Children.Add(eventName, eventNode);
                }
            }
             
            // Show LambdaOpenApiSpecs
            foreach(var lambda in Lambdas.Values)
                Debug.WriteLine($"Lambnda OpenApiSpec\n{new SerializerBuilder().Build().Serialize(lambda.OpenApiSpec)}");
        }

        private async Task PruneResourcesAsync()
        {
            await Task.Delay(0);
            // Prune un-referenced default resources
            // UserPool
            // UserPoolClient
            // IdentityPool
            // CognitoIdentityPoolRoles
            // AuthRole
            // UnAuthRole

            var pruneList = new HashSet<string>
            {
                { "UserPool"},
                { "UserPoolClient"},
                { "IdentityPool"},
                { "CognitoIdentityPoolRoles"},
                { "AuthRole"},
                { "UnauthRole"}
            };

            foreach(var resource in Resources)
                switch(resource.Value.AwsType)
                {
                    case "AWS::Serverless::HttpApi":
                        if (Apis[resource.Key].Lambdas.Count > 0  && resource.Value.RootNode.AllNodes.Contains("UserPoolClient"))
                        {
                            pruneList.Remove("UserPool");
                            pruneList.Remove("UserPoolClient");
                            pruneList.Remove("IdentityPool");
                            pruneList.Remove("CognitoIdentityPoolRoles");
                        }

                        if (Apis[resource.Key].Lambdas.Count == 0)
                            pruneList.Add(resource.Key);

                        break;
                    case "AWS::Serverless::Api":
                        if (Apis[resource.Key].Lambdas.Count > 0)
                        {
                            if (resource.Value.RootNode.AllNodes.Contains("AWS_IAM"))
                            {
                                pruneList.Remove("UserPool");
                                pruneList.Remove("UserPoolClient");
                                pruneList.Remove("IdentityPool");
                                pruneList.Remove("CognitoIdentityPoolRoles");
                                pruneList.Remove("AuthRole");
                            }
                            else
                            {
                                pruneList.Remove("UnauthRole");
                            }
                        }
                        else
                            pruneList.Add(resource.Key);
                        break;
                    default:
                        break;
                }

            foreach(var item in pruneList)
                    Resources.Remove(item);
        }

        // 
        public async Task WriteSAMAsync()
        {
            await logger.InfoAsync($"\nWriting SAM File(s)");

            var resources = new YamlMappingNode();
            foreach (var resource in Resources)
                resources.Add(resource.Key, resource.Value.RootNode);

            samRootNode.Add("Resources", resources);
            var fileText = new YamlDotNet.Serialization.Serializer().Serialize(samRootNode);

            var samReviewMsg =
                "# SAM Template Review - DO NOT ATTEMPT TO PUBLISH THIS SAM TEMPLATE\n" +
                "# Publish for a specific stack instead (ex: Stacks/Dev/serverless.template).\n" +
                "# This file is generated for genral review of generated template. Environment specific content targets\n" +
                "# like __codeUriTarget__ and __StageName__ are not resolved.\n\n";

            File.WriteAllText(Path.Combine(SolutionRootFolderPath, "SAM_Review.yaml"), samReviewMsg + fileText);
             
            foreach(var env in Environments)
            {
                var envFileText = fileText.Replace("__codeUriTarget__", env.Value.UriCodeTarget);
                envFileText = envFileText.Replace("__StageName__", env.Value.Stage);
                File.WriteAllText(Path.Combine(SolutionRootFolderPath,"Stacks",env.Key,"serverless.template"), envFileText);
            }
        }

        #endregion

        #region Utility Methods

        public YamlNode NewNode(YamlNode node)
        {
            YamlNode targetNode = null;
            switch (node.NodeType)
            {
                case YamlNodeType.Alias:
                    throw new Exception("I don't know how to process Alias nodes");
                case YamlNodeType.Mapping:
                    targetNode = new YamlMappingNode();
                    break;
                case YamlNodeType.Scalar:
                    targetNode = new YamlScalarNode(null);
                    break;
                case YamlNodeType.Sequence:
                    targetNode = new YamlSequenceNode();
                    break;
            }
            return targetNode;
        }

        public YamlNode CopyNode(YamlNode sourceNode)
        {
            var targetNode = NewNode(sourceNode);
            CopyNode(sourceNode, targetNode);
            return targetNode;
        }

        public YamlNode CopyNode(YamlNode sourceNode, YamlNode targetNode)
        {
            if (sourceNode == null)
                throw new Exception("sourcenode is null");

            switch (sourceNode.NodeType)
            {
                case YamlNodeType.Alias:
                    throw new Exception("I don't know how to process Alias nodes");

                case YamlNodeType.Mapping:
                    var sourceMappingNode = sourceNode as YamlMappingNode;
                    var targetMappingNode = targetNode as YamlMappingNode;
                    foreach (KeyValuePair<YamlNode, YamlNode> childNode in sourceMappingNode.Children)
                    {
                        var newChildNode = NewNode(childNode.Value);
                        CopyNode(childNode.Value, newChildNode);
                        targetMappingNode.Children.Add(childNode.Key, newChildNode);
                    }
                    break;
                case YamlNodeType.Sequence:
                    var sourceSequenceNode = sourceNode as YamlSequenceNode;
                    var targetSequenceNode = targetNode as YamlSequenceNode;
                    foreach (YamlNode childNode in sourceSequenceNode.Children)
                    {
                        var newChildNode = NewNode(childNode);
                        CopyNode(childNode, newChildNode);
                        targetSequenceNode.Children.Add(newChildNode);
                    }
                    break;
                case YamlNodeType.Scalar:
                    var targetScalerNode = targetNode as YamlScalarNode;
                    targetScalerNode.Value = ((YamlScalarNode)sourceNode).Value;
                    break;
            }
            return targetNode;
        }

        public bool FindComponentReferences(YamlNode node, List<string> references)
        {
            switch (node.NodeType)
            {
                case YamlNodeType.Alias:
                    throw new Exception("I don't know how to handle Alias nodes");
                case YamlNodeType.Mapping:
                    foreach (KeyValuePair<YamlNode, YamlNode> childNode in ((YamlMappingNode)node).Children)
                        if (childNode.Key.ToString().Equals(@"$ref") && childNode.Value.NodeType == YamlNodeType.Scalar)
                        {
                            var scalarNode = childNode.Value as YamlScalarNode;
                            if (!references.Contains(scalarNode.Value))
                                references.Add(scalarNode.Value);
                        }
                        else
                            FindComponentReferences(childNode.Value, references);
                    break;
                case YamlNodeType.Scalar:
                    break;
                case YamlNodeType.Sequence:
                    foreach (YamlNode childNode in ((YamlSequenceNode)node).Children)
                        FindComponentReferences(childNode, references);
                    break;
            }

            return references.Count > 0;
        }

        public string TagToFunctionName(string tag)
        {
            return ToUpperFirstChar(tag);
        }

        /// <summary>
        /// Create readbale Event Name from Route
        /// ex: GET /order/{orderId} => GetOrderOrderId
        /// </summary>
        /// <param name="httpOperation"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string RouteToEventName(string httpOperation, string path, string operationId)
        {
            if (string.IsNullOrEmpty(operationId))
            {
                var name = ToUpperFirstChar(httpOperation.ToLower()); //ex: GET to Get
                var parts = path.Split('/');
                foreach (var part in parts)
                {
                    var namePart = part;
                    namePart = namePart.Replace("{", "");
                    namePart = namePart.Replace("}", "");
                    name += ToUpperFirstChar(namePart);
                }
                return name;
            }
            else
                return ToUpperFirstChar(operationId);
        }

        public string GetMappingNodeStringValue(YamlMappingNode node, string key)
        {
            var value = node.Children.TryGetValue(key, out YamlNode childNode)
                ? childNode.ToString()
                : string.Empty;
            return value;
        }
        #endregion Methods

        #region Utility Methods
        public static YamlMappingNode ReadAndParseYamlFile(string path)
        {
            var api = new YamlStream();
            var text = File.ReadAllText(path, Encoding.UTF8);
            api.Load(new StringReader(text));
            YamlDocument doc = api.Documents[0];
            return doc.RootNode as YamlMappingNode;
        }

        public static YamlMappingNode ParseYamlText(string text)
        {
            var api = new YamlStream();
            api.Load(new StringReader(text));
            return (YamlMappingNode)api.Documents[0].RootNode;
        }

        public static string YamlNodeToText(YamlMappingNode node)
        {
            var serializer = new YamlDotNet.Serialization.Serializer();
            return serializer.Serialize(node);
        }

        public static YamlNode MergeNode(YamlNode leftNode, YamlNode rightNode)
        {
            var leftNodeYaml = new StringReader(new YamlDotNet.Serialization.SerializerBuilder().Build().Serialize(leftNode));
            var leftNodeYamlObject = new YamlDotNet.Serialization.DeserializerBuilder().Build().Deserialize(leftNodeYaml);
            var leftNodeJson = new YamlDotNet.Serialization.SerializerBuilder().JsonCompatible().Build().Serialize(leftNodeYamlObject);

            var rightNodeYaml = new StringReader(new YamlDotNet.Serialization.SerializerBuilder().Build().Serialize(rightNode));
            var rightNodeYamlObject = new YamlDotNet.Serialization.DeserializerBuilder().Build().Deserialize(rightNodeYaml);
            var rightNodeJson = new YamlDotNet.Serialization.SerializerBuilder().JsonCompatible().Build().Serialize(rightNodeYamlObject);

            // Using Json.NET merge
            var leftNodeJObject = JObject.Parse(leftNodeJson);
            var rightNodeJObject = JObject.Parse(rightNodeJson);
            leftNodeJObject.Merge(rightNodeJObject, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
            var jsonResult = leftNodeJObject.ToString();
            var resultDoc = ConvertJTokenToObject(JsonConvert.DeserializeObject<JToken>(jsonResult));

            var resultYamlStr = new SerializerBuilder().Build().Serialize(resultDoc);
            var resultYaml = ParseYamlText(resultYamlStr);

            return resultYaml;
        }

        static object ConvertJTokenToObject(JToken token)
        {
            if (token is JValue value)
                return value.Value;
            if (token is JArray)
                return token.AsEnumerable().Select(ConvertJTokenToObject).ToList();
            if (token is JObject)
                return token.AsEnumerable().Cast<JProperty>().ToDictionary(x => x.Name, x => ConvertJTokenToObject(x.Value));
            throw new InvalidOperationException("Unexpected token: " + token);
        }

        public static string ReplaceTargets(string source, Dictionary<string, string> replacements)
        {
            foreach (KeyValuePair<string, string> kvp in replacements)
                source = source.Replace(kvp.Key, kvp.Value);
            return source;
        }

        public static string ToUpperFirstChar(string str)
        {
            if (str.Length == 0)
                return str;
            else 
            if (str.Length == 1)
                return str.ToUpper();
            else
                return str[0].ToString().ToUpper() + str.Substring(1);
        }

        public static string ToLowerFirstChar(string input)
        {
            string newString = input;
            if (!String.IsNullOrEmpty(newString) && Char.IsUpper(newString[0]))
                newString = Char.ToLower(newString[0]) + newString.Substring(1);
            return newString;
        }

        public static bool NamedPropertyExists(YamlNode node, string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath))
                return false;

            string[] nodeElement = propertyPath.Split('/');
            int el = 0; // 

            if (node.NodeType != YamlNodeType.Mapping)
                return false;

            var mappingNode = node as YamlMappingNode;
            while (
                el < nodeElement.Length
                && mappingNode.Children.TryGetValue(nodeElement[el++], out node)
                && node.NodeType == YamlNodeType.Mapping)
                mappingNode = node as YamlMappingNode;

            if (el != nodeElement.Length)
                return false;

            return node != null;
        }

        public static bool GetNamedProperty(YamlNode node, string propertyPath, out YamlNode outNode)
        {
            outNode = null;

            if (string.IsNullOrEmpty(propertyPath))
                return false;

            string[] nodeElement = propertyPath.Split('/');
            int el = 0; // 

            if (node.NodeType != YamlNodeType.Mapping)
                return false;

            var mappingNode = node as YamlMappingNode;
            while (
                el < nodeElement.Length
                && mappingNode.Children.TryGetValue(nodeElement[el++], out node)
                && node.NodeType == YamlNodeType.Mapping)
                mappingNode = node as YamlMappingNode;

            if (el != nodeElement.Length)
                return false;

            outNode = node;
            return outNode != null;
        }

        public string GetConfigProperty(string path, bool errorIfMissing = true)
        {
            GetNamedProperty(
                ProjectGenerationOptions,
                path,
                out YamlNode node
                );
            if (node == null && errorIfMissing)
                throw new Exception($"Error: Can't find value for {path} configuration property");
            if (node == null)
                return string.Empty;
            return node.ToString();
        }

        public Dictionary<string, string> GetConfigProperties(string path, bool errorIfMissing = true)
        {
            var result = new Dictionary<string, string>();

            GetNamedProperty(
                ProjectGenerationOptions,
                path,
                out YamlNode node
                );

            if (node == null && errorIfMissing)
                throw new Exception($"Error: Can't find value for {path} configuration property");

            if (node == null)
                return result;


            if (node.NodeType != YamlNodeType.Mapping && node.NodeType != YamlNodeType.Scalar)
                throw new Exception($"Error: Nodetype for {path} configuration property is not mapping or scalar node");

            if (node.NodeType == YamlNodeType.Mapping)
            {
                var mappingNode = node as YamlMappingNode;
                foreach (var kvp in mappingNode.Children)
                    result.Add(kvp.Key.ToString(), kvp.Value.ToString());
            }

            if (node.NodeType == YamlNodeType.Scalar)
                result.Add((node as YamlScalarNode).ToString(), string.Empty);

            return result;
        }

        public List<string> GetConfigPropertyItems(string path, bool errorIfMissing = true)
        {
            var result = new List<string>();

            // var text = YamlNodeToText(ProjectGenerationOptions); // handy for debugging

            GetNamedProperty(
                ProjectGenerationOptions,
                path,
                out YamlNode node
                );

            if (node == null && errorIfMissing)
                throw new Exception($"Error: Can't find value for {path} configuration property");

            if (node == null)
                return result;

            if (node.NodeType != YamlNodeType.Sequence)
                throw new Exception($"Error: Nodetype for {path} configuration property is not a sequence node");

            var itemsNode = node as YamlSequenceNode;
            foreach (var item in itemsNode.Children)
                result.Add(item.ToString());
            return result;
        }
        #endregion
    }
}
