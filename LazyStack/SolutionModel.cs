using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Force.DeepCloner;

namespace LazyStack 
{
    public class SolutionModel
    {
        public SolutionModel(string solutionRootFolderPath, ILogger logger)
        {

            SolutionRootFolderPath = solutionRootFolderPath;
            this.logger = logger;

            // Find Visual Studio solution file
            var solFiles = Directory.GetFiles(solutionRootFolderPath, "*.sln");
            if (solFiles.Length > 1)
                throw new System.Exception("More than one .sln file in folder");

            if (solFiles.Length == 0)
                throw new System.Exception("Solution file not found");

            AppName = Path.GetFileNameWithoutExtension(solFiles[0]);

            OpenApiFilePath = Path.Combine(solutionRootFolderPath, $"{AppName}.yaml");

            SAMTemplateFilePath = Path.Combine(solutionRootFolderPath, "template.yaml");

            PrevSAMTemplateFilePath = Path.Combine(solutionRootFolderPath, "template_prev.yaml");

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
        public string SolutionRootFolderPath { get; private set; }
        public string AppName { get; private set; }
        public string OpenApiFilePath { get; private set; }

        public string LazyStackTemplateFolderPath { get; private set; }
        public string SrcTemplateFilePath { get; private set; }

        public string SAMTemplateFilePath { get; private set; }
        public string PrevSAMTemplateFilePath { get; private set; }

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
        public string XlzAwsTemplate { get; set; } = string.Empty;
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

        #endregion Properties

        #region Variables
        YamlMappingNode lzConfigRootNode;
        YamlMappingNode samRootNode;
        readonly ILogger logger;
        #endregion Variables

        #region Methods

        /// <summary>
        /// Init Stack with LazyStack defaults
        /// </summary>
        public void InitDefaults()
        {
            // Mapping node keys are Resource Types
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

        /// <summary>
        /// Read the solutions OpenApi specification file, and directives in
        /// the LazyStack.yaml to write the solution's SAM serverless.template file.
        /// </summary>
        public void ProcessOpenApi()
        {
            logger.Info($"\nLoading OpenApi Specification {OpenApiFilePath}");
            OpenApiSpecText = File.ReadAllText(OpenApiFilePath);
            OpenApiSpecRootNode = ParseYamlText(OpenApiSpecText); 

            var lzConfigFilePath = Path.Combine(SolutionRootFolderPath, "LazyStack.yaml");
            if (File.Exists(lzConfigFilePath))
            {
                logger.Info($"\nLoading LazyStack.yaml configuration file");
                lzConfigRootNode = ReadAndParseYamlFile(lzConfigFilePath);
                Debug.WriteLine($"lzConfigRootNode \n{new SerializerBuilder().Build().Serialize(lzConfigRootNode)}"); 
            }

            // Grab top-level configuration directives
            ParseLzConfiguration();

            // HttpApiUnsecure, HttpApiSecure, ApiUnsecure, ApiSecure, 
            // UserPool, UserPoolClient, IdentityPool, CognitoIdentityPoolRoles, AuthRole, UnAuthRole
            CreateDefaultResources();

            // Read AwsTemplate or default template. Update Resources.
            LoadSAM();

            // Parse OpenApi tags object to generate list of LambdaNames we will generate
            ParseOpenApiTagsObjectForLambdaNames();

            // Parse optional AwsResources in LazyStack configuration
            ParseAwsResources();

            // Parse ApiTagMap
            ParseLzApiTagMap();

            // Create/Updates solutionModel.Resources, solutionModel.Apis.Lambdas and solutionModel.Lambdas 
            ParseOpenApiTagsObject();

            // Overwrite/Extend Lambda properties
            ParseTagLambdas();

            // Add Lambda.Events for each Path/Operation (route) in OpenApi specification
            ParseOpenApiPathObject();

            // Prune -- remove default resources not referenced
            PruneResources();

            // Identify ApiGateway Security level and set SecurityLevel property
            DiscoverSecurityLevel();

            WriteSAM(); // Write serverless.template file

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
        private void ParseLzConfiguration()
        {
            if (lzConfigRootNode == null)
                return;

            logger.Info($"\nLoading LazyStack Configuration");

            // Parse LazyStack Directives in config file
            foreach (KeyValuePair<YamlNode, YamlNode> kvp in lzConfigRootNode)
                switch (kvp.Key.ToString())
                {
                    case "DefaultApi": // Initializer
                        DefaultApi = kvp.Value.ToString();
                        logger.Info($"  Default-Api = {DefaultApi}");
                        break;

                    case "AwsTemplate": // Initializer
                        if (!string.IsNullOrEmpty(XlzAwsTemplate))
                            throw new Exception($"Error: More than AwsTemplate directive found.");

                        if (string.IsNullOrEmpty(kvp.Value.ToString()))
                            throw new Exception($"AwsTemplate value can't be empty");

                        XlzAwsTemplate = kvp.Value.ToString();
                        var path = Path.Combine(SolutionRootFolderPath, XlzAwsTemplate);
                        if (!File.Exists(path))
                            throw new Exception($"AwsTemplate file not found {path}");

                        logger.Info($"  AwsTempalte = {XlzAwsTemplate}");

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
                        ProjectGenerationOptions= MergeNode( ProjectGenerationOptions, kvp.Value) as YamlMappingNode;
                        Debug.WriteLine($"ProjectGenerationOptions\n {new SerializerBuilder().Build().Serialize(ProjectGenerationOptions)}");
                        break;

                    default: // stop on any unrecognized directives
                        throw new Exception($"Unknown directive {kvp.Key}");
                }
        }

        /// <summary>
        /// Merge in LazyStack default resources
        /// HttpApiUnsecure
        /// HttpApiSecure
        /// ApiUnsecure
        /// ApiSecure
        /// </summary>
        private void CreateDefaultResources()
        {
            logger.Info($"\nCreating default resources");
            Debug.WriteLine("Creating default resources");
            // Load default resources
            var defaultDoc = ReadAndParseYamlFile(Path.Combine(LazyStackTemplateFolderPath, "default_resources.yaml"));
            if (defaultDoc.Children.TryGetValue("Resources", out YamlNode resources))
                foreach (KeyValuePair<YamlNode, YamlNode> kvp in resources as YamlMappingNode)
                {
                    logger.Info($"  Loading default resource: {kvp.Key}");
                    var resource = new AwsResource(kvp.Key.ToString(), kvp.Value as YamlMappingNode, solutionModel: this, isDefault: true);
                    Debug.WriteLine($"Created Resource: {kvp.Key}\n{new SerializerBuilder().Build().Serialize(resource.RootNode)}");
                }
        }

        /// <summary>
        /// Load in AwsTemplate or default LazyStack template.yaml
        /// </summary>
        private void LoadSAM()
        {
          
            if(!string.IsNullOrEmpty(XlzAwsTemplate))
                logger.Info($"\nLoading SAM Template {XlzAwsTemplate}");

            // Load LazyStack SAM template or user supplied SAM template
            var tpl = new YamlStream();
            var source = !string.IsNullOrEmpty(XlzAwsTemplate)
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
                    logger.Info($"  Loading SAM Template resource: {kvp.Key}");
                    new AwsResource(kvp.Key.ToString(), kvp.Value as YamlMappingNode, this, isDefault: false);
                }

                // Remove Resources Node as we will add it back in later from the solutionModel.Resources array
                samRootNode.Children.Remove("Resources");
            }
        }

        /// <summary>
        /// Parse the OpenApi Tags Object to get tag names and generate LambdaNames.
        /// </summary>
        private void ParseOpenApiTagsObjectForLambdaNames()
        {
            logger.Info($"\nParsing OpenApi Specification tags");
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
                    logger.Info($"  {tagName} will create lambda {lambdaName}");
                }
        }

        /// <summary>
        /// Parse AwsResources Directive
        /// </summary>
        private void ParseAwsResources()
        {
            if (lzConfigRootNode == null)
                return;

            logger.Info($"\nParsing AwsResource (if any) in LazyStack configuration");
            Debug.WriteLine("\nParsing AwsREsources (if any) in LazyStack configuration");
            // Parse LazyStack Resources
            if (lzConfigRootNode.Children.TryGetValue("AwsResources", out YamlNode node))
                foreach (KeyValuePair<YamlNode, YamlNode> kvp in node as YamlMappingNode)
                {
                    if (Lambdas.ContainsKey(kvp.Key.ToString()))
                        throw new Exception($"Error: AwsResources file contains a resource name \"{kvp.Key}\" that conflicts with generated lambda name");
                    logger.Info($"  Loading resource {kvp.Key}");
                    var resource = new AwsResource(kvp.Key.ToString(), kvp.Value as YamlMappingNode, this, isDefault: false); // Added to solutionModel.Resources
                    Debug.WriteLine($"Added Resource {kvp.Key}\n{new SerializerBuilder().Build().Serialize(resource.RootNode)}");
                }
        }

        /// <summary>
        /// Parse the ApiTagMap
        /// </summary>
        private void ParseLzApiTagMap()
        {
            if (lzConfigRootNode != null)
                if (lzConfigRootNode.Children.TryGetValue("ApiTagMap", out YamlNode node))
                {
                    logger.Info($"\nParsing ApiTagMap directive in LazyStack configuration");
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
                                logger.Info($"  {tagName}  {apiName}");
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
                        logger.Info($"Api Tag Map Default Assignments");

                    logger.Info($"  {tagName}  {DefaultApi}");
                    ApiNameByTagName[tagName] = DefaultApi;
                }
        }

        /// <summary>
        /// Parse the Tag Object in the openApi specification
        /// </summary>
        private void ParseOpenApiTagsObject()
        {
            logger.Info($"\nParsing OpenApi Specification tags to generate Lambdas");
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
                logger.Info($"  Creating resource \"{lambdaName}\" for tag \"{tagName}\"");
                Debug.WriteLine($"  Creating resource \"{lambdaName}\" for tag \"{tagName}\"");
                var resource = new AwsResource(this, tagName, Apis[apiName]);
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
        private void ParseOpenApiPathObject()
        {
            logger.Info($"\nParsing OpenApi Specifications paths to generate Lambda events");
            Debug.WriteLine($"\nParsing OpenApi Specifications paths to generate Lambda events");

            string[] validOperations = { "GET", "PUT", "POST", "DELETE", "UPDATE" };

            if (!OpenApiSpecRootNode.Children.TryGetValue("paths", out YamlNode pathsNode))
                throw new Exception($"Error: OpenApi specification missing \"Paths\" Object");

            // foreach Path in Paths
            foreach (KeyValuePair<YamlNode, YamlNode> pathsNodeChild in pathsNode as YamlMappingNode)
            {
                var path = pathsNodeChild.Key.ToString();
                var pathNodeValue = (YamlMappingNode)pathsNodeChild.Value;

                logger.Info($"  {path}");
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

                    logger.Info($"    {httpOperation} {tagName}");
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

        /// <summary>
        /// Overwrite default properties in Lambda Resources generated by LazyStack 
        /// </summary>
        private void ParseTagLambdas()
        {
            if (lzConfigRootNode != null && lzConfigRootNode.Children.TryGetValue("TagLambdas", out YamlNode node))
            {
                logger.Info($"\nOverwriting/Extending Lambda properties based on TagLamnbdas directives in LazyStack configuration");
                Debug.WriteLine("\nOverwriting/Extending Lambda properties based on TagLamnbdas directives in LazyStack configuration");

                Debug.WriteLine($"{new SerializerBuilder().Build().Serialize(lzConfigRootNode)}");
                foreach (KeyValuePair<YamlNode, YamlNode> kvp in node as YamlMappingNode)
                {

                    var tagName = kvp.Key.ToString();
                    if (!LambdaNameByTagName.ContainsKey(tagName))
                        throw new Exception($"Error: TagLambdas directive references unknown tag \"{tagName}\"");

                    var lambdaName = LambdaNameByTagName[tagName];
                    var rootNode = kvp.Value as YamlMappingNode;
                    var awsType = string.Empty;
                    if (rootNode.Children.TryGetValue("Type", out node))
                        awsType = node.ToString();

                    if (!string.IsNullOrEmpty(awsType) && !awsType.Equals("AWS::Serverless::Function"))
                        throw new Exception($"Error: TagLambda {tagName} can only contain AWS::Serverless::Function");

                    if (!Lambdas.ContainsKey(lambdaName))
                        throw new Exception($"Error: Lambda \"{lambdaName}\" for tag \"{tagName}\" not generated by LazyStack. Are you missing a tag?");

                    logger.Info($"  Processing tag: \"{tagName}\" lambda: \"{lambdaName}\" resource overrides");
                    Debug.WriteLine($"Processing tag: \"{tagName}\" lambda: \"{lambdaName}\" resource overrides");
                    var lambda = Lambdas[lambdaName];
                    Debug.WriteLine($"lnode\n{new SerializerBuilder().Build().Serialize(lambda.AwsResource.RootNode)}");
                    Debug.WriteLine($"rnode\n{new SerializerBuilder().Build().Serialize(rootNode)}");
                    lambda.AwsResource.RootNode = MergeNode(lambda.AwsResource.RootNode, rootNode) as YamlMappingNode;
                    Debug.WriteLine($"mergednode\n{new SerializerBuilder().Build().Serialize(lambda.AwsResource.RootNode)}");
                }
            }
        }

        private void PruneResources()
        {
            // Prune un-referenced default resources
            // UserPool
            // UserPoolClient
            // IdentityPool
            // CognitoIdentityPoolRoles
            // AuthRole
            // UnAuthRole

            var pruneList = new Dictionary<string, bool>
            {
                { "UserPool", true },
                { "UserPoolClient", true },
                { "IdentityPool", true },
                { "CognitoIdentityPoolRoles", true },
                { "AuthRole", true },
                { "UnauthRole", true}
            };

            foreach(var resource in Resources)
            {
                if (!pruneList.Keys.Contains(resource.Value.Name)) // don't look for references inside pruneList resources
                {
                    switch(resource.Value.AwsType)
                    {
                        case "AWS::Serverless::HttpApi":
                            if (Apis[resource.Key].Lambdas.Count > 0  && resource.Value.RootNode.AllNodes.Contains("UserPoolClient"))
                            {
                                pruneList["UserPool"] = false;
                                pruneList["UserPoolClient"] = false;
                                pruneList["IdentityPool"] = false;
                                pruneList["CognitoIdentityPoolRoles"] = false;
                            }
                            break;
                        case "AWS::Serverless::Api":

                            if (Apis[resource.Key].Lambdas.Count > 0)
                            {
                                if (resource.Value.RootNode.AllNodes.Contains("AWS_IAM"))
                                {
                                    pruneList["UserPool"] = false;
                                    pruneList["UserPoolClient"] = false;
                                    pruneList["IdentityPool"] = false;
                                    pruneList["CognitoIdentityPoolRoles"] = false;
                                    pruneList["AuthRole"] = false;
                                }
                                else
                                {
                                    pruneList["UnauthRole"] = false;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            foreach(var item in pruneList)
                if (item.Value)
                    Resources.Remove(item.Key);
        }

        private void WriteSAM()
        {
            logger.Info($"\nWriting SAM serverless.template file");
            var OutputSection = string.Empty;
            var resources = new YamlMappingNode();
            foreach (var resource in Resources)
                if (resource.Value.IsHttpApi || resource.Value.IsRestApi)
                {   // Emit the Api only if it services a lambda
                    if (Apis[resource.Key].Lambdas.Count > 0)
                    {
                        resources.Add(resource.Key, resource.Value.RootNode);
                        OutputSection += resource.Value.GetOutputItem();
                    }
                }
                else
                {
                    resources.Add(resource.Key, resource.Value.RootNode);
                    OutputSection += resource.Value.GetOutputItem();
                }

            samRootNode.Add("Resources", resources);
            if(!string.IsNullOrEmpty(OutputSection))
                samRootNode.Add("Outputs", ParseYamlText(OutputSection));

            // Write new serverless.template
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(SolutionRootFolderPath, "serverless.template")))
            {
                new YamlDotNet.Serialization.Serializer().Serialize(file, samRootNode);
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
                throw new Exception($"Error: Nodetype for {path} configuration property is not mapping node");

            var itemsNode = node as YamlSequenceNode;
            foreach (var item in itemsNode.Children)
                result.Add(item.ToString());
            return result;
        }
        #endregion
    }
}
