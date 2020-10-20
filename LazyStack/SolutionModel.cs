using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using YamlDotNet.RepresentationModel;
using Force.DeepCloner;
using System.Diagnostics;
using CommandLine;

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

            OpenApiGeneratorFilePath = Path.Combine(executingAssemblyFolderPath, "OpenAPIGenerator", "openapi-generator-cli.jar");
            if (!File.Exists(OpenApiGeneratorFilePath))
                throw new System.Exception("Lazy Stack version of openapi-generator-cli.jar not found. Check installation");

            InitDefaults();

        }

        #region Properties
        public string SolutionRootFolderPath { get; private set; }
        public string AppName { get; private set; }
        public string OpenApiFilePath { get; private set; }

        public string OpenApiGeneratorFilePath { get; private set; }
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

        public Dictionary<string, ProjectInfo> Projects { get; } = new Dictionary<string, ProjectInfo>(); // key is projectName
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

        /// <summary>
        /// LazyStack default template values keyed by Resource Type.
        /// </summary>
        public Dictionary<string, YamlMappingNode> LzDefaultTemplate = new Dictionary<string, YamlMappingNode>();
        public YamlMappingNode ProjectGenerationOptions;

        #endregion Properties

        #region Variables
        YamlMappingNode openApiSpecRootNode;
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

            if (!lazyStackNode.Children.TryGetValue("ProjectGenerationOptions", out YamlNode node))
                throw new Exception("Error: ProjectGenerationOptions missing from LazyStack.yaml file in LazyStack templates folder ");

            ProjectGenerationOptions = node as YamlMappingNode;

        }

        /// <summary>
        /// Read the solutions OpenApi specification file, an
        /// optional x-lz-AwsTemplate file and then create/update
        /// resources to write the solution's SAM serverless.template file.
        /// </summary>
        public void ProcessOpenApi()
        {
            if (!File.Exists(OpenApiGeneratorFilePath))
                throw new Exception($"Error: Missing OpenApi specificaiton file {OpenApiGeneratorFilePath}");

            logger.Info($"\nLoading OpenApi Specification {OpenApiFilePath}");
            openApiSpecRootNode = ReadAndParseYamlFile(OpenApiFilePath); // Read OpenApi specification

            var lzConfigFilePath = Path.Combine(SolutionRootFolderPath, "LazyStack.yaml");
            if (File.Exists(lzConfigFilePath))
            {
                logger.Info($"\nLoading LazyStack.yaml configuration file");
                lzConfigRootNode = ReadAndParseYamlFile(lzConfigFilePath);
            }

            // Grab top-level configuration directives
            ParseLzConfiguration();

            // HttpApiUnsecure, HttpApiSecure, ApiUnsecure, ApiSecure, 
            // UserPool, UserPoolClient, IdentityPool, CognitoIdentityPoolRoles, AuthRole, UnAuthRole
            CreateDefaultResources();

            // Read AwsTemplate or default template. Update Resources.
            LoadSAM();

            // Parse OpenApi tags object to generate list of LambdaNames we will generate later
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

            WriteSAM(); // Write serverless.template file

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

                    case "ProjectGenerationOptions":
                        //ProcessProjectGenerationOptions(kvp.Value as YamlMappingNode, "LazyStack.yaml");
                        var msg = MergeNode(ProjectGenerationOptions, kvp.Value);
                        if (!string.IsNullOrEmpty(msg))
                            throw new Exception($"Error: Parsing LazyStack.yaml file: {msg}");
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
            // Load default resources
            var defaultDoc = ReadAndParseYamlFile(Path.Combine(LazyStackTemplateFolderPath, "default_resources.yaml"));
            if (defaultDoc.Children.TryGetValue("Resources", out YamlNode resources))
                foreach (KeyValuePair<YamlNode, YamlNode> kvp in resources as YamlMappingNode)
                {
                    logger.Info($"  Loading default resource: {kvp.Key}");
                    new AwsResource(kvp.Key.ToString(), kvp.Value as YamlMappingNode, solutionModel: this, isDefault: true);
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
            if (!openApiSpecRootNode.Children.TryGetValue("tags", out YamlNode node))
                throw new Exception($"Error: Your OpenApi specification contains no tags. LazyStack can't generate an Api.");
            else
                foreach (YamlMappingNode tagsNodeItem in node as YamlSequenceNode)
                {
                    var tagName = tagsNodeItem["name"].ToString();
                    if (Tags.Contains(tagName))
                        throw new Exception($"Error: Tag \"{tagName}\" defined twice");
                    Tags.Add(tagName);
                    var lambdaName = TagToFunctionName(AppName,tagName);
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

            // Parse LazyStack Resources
            if (lzConfigRootNode.Children.TryGetValue("AwsResources", out YamlNode node))
                foreach (KeyValuePair<YamlNode, YamlNode> kvp in node as YamlMappingNode)
                {
                    if (Lambdas.ContainsKey(kvp.Key.ToString()))
                        throw new Exception($"Error: AwsResources file contains a resource name \"{kvp.Key}\" that conflicts with generated lambda name");
                    logger.Info($"  Loading resource {kvp.Key}");
                    new AwsResource(kvp.Key.ToString(), kvp.Value as YamlMappingNode, this, isDefault: false); // Added to solutionModel.Resources
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

            if (!openApiSpecRootNode.Children.TryGetValue("tags", out YamlNode tagsNode))
                throw new Exception($"Error: Could not find a tags field. An OpenApi tags field is required for LazyStack to generate an api.");

            if (tagsNode.NodeType != YamlNodeType.Sequence)
                throw new Exception($"Error: tags node is not a sequence node.");

            foreach (YamlMappingNode tagsNodeItem in tagsNode as YamlSequenceNode)
            {
                var tagName = tagsNodeItem["name"].ToString();
                var lambdaName = TagToFunctionName(AppName,tagName);
                var apiName = ApiNameByTagName[tagName];
                logger.Info($"  Creating resource \"{lambdaName}\" for tag \"{tagName}\"");
                new AwsResource(this, tagName, Apis[apiName]);
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

            string[] validOperations = { "GET", "PUT", "POST", "DELETE", "UPDATE" };

            if (!openApiSpecRootNode.Children.TryGetValue("paths", out YamlNode pathsNode))
                throw new Exception($"Error: OpenApi specification missing \"Paths\" Object");

            // foreach Path in Paths
            foreach (KeyValuePair<YamlNode, YamlNode> pathsNodeChild in pathsNode as YamlMappingNode)
            {
                var path = pathsNodeChild.Key.ToString();
                var pathNodeValue = (YamlMappingNode)pathsNodeChild.Value;

                logger.Info($"  {path}");
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

                    // Find Lambda in global index of Lambdas (indexed by tag) 
                    if (!Lambdas.TryGetValue(LambdaNameByTagName[tagName], out AWSLambda awsLambda))
                        throw new Exception($"Error: Tag \"{tagName}\" specified for path \"{path}\" not found.");

                    // Generate eventName
                    // ex: GET /order/{orderId} => GetOrderOrderId
                    var eventName = RouteToEventName(httpOperation, path);
                    if (awsLambda.AwsResource.RootNode.Children.TryGetValue("Events", out YamlNode eventsNode)
                        && ((YamlMappingNode)eventsNode).Children.ContainsKey(eventName))
                        throw new Exception($"Error: Duplicate event name \"{eventName}\" in Lambda");

                    // The api is responsible for creating the event node because Event nodes are different based on Api
                    var eventNode = awsLambda.AwsApi.EventNode(path, httpOperation);

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
        }

        /// <summary>
        /// Overwrite default properties in Lambda Resources generated by LazyStack 
        /// </summary>
        private void ParseTagLambdas()
        {
            if (lzConfigRootNode != null && lzConfigRootNode.Children.TryGetValue("TagLambdas", out YamlNode node))
            {
                logger.Info($"\nOverwriting/Extending Lambda properties based on TagLambdas directives in LazyStack configuration");
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
                    var lambda = Lambdas[lambdaName];

                    MergeNode(lambda.AwsResource.RootNode, rootNode);
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
            {
                if (item.Value)
                    Resources.Remove(item.Key);
            }

        }


        private void WriteSAM()
        {
            logger.Info($"\nWriting SAM serverless.template file");
            var resources = new YamlMappingNode();
            foreach (var resource in Resources)
                if (resource.Value.IsHttpApi || resource.Value.IsRestApi)
                {   // Emit the Api only if it services a lambda
                    if (Apis[resource.Key].Lambdas.Count > 0)
                        resources.Add(resource.Key, resource.Value.RootNode);
                }
                else
                    resources.Add(resource.Key, resource.Value.RootNode);

            samRootNode.Add("Resources", resources);

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

        public string TagToFunctionName(string apiName, string tag)
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
        public string RouteToEventName(string httpOperation, string path)
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
                var text = File.ReadAllText(path);
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

        // Recursive Merge rightNode into leftNode. rightNode value has higher priority.
        public static string MergeNode(YamlNode leftNode, YamlNode rightNode)
        {
            if (leftNode == null)
                return "Can't merge into null node";

            if (rightNode == null)
                return string.Empty;

            if (leftNode.NodeType != rightNode.NodeType)
                return "Can't merge nodes of different types";

            switch (leftNode.NodeType)
            {
                case YamlNodeType.Alias:
                    return "Don't know how to merge Alias nodes";

                case YamlNodeType.Scalar:
                    // todo - do we need to make the leftNode a ref to support this case? test
                    leftNode = rightNode.DeepClone();
                    break;

                case YamlNodeType.Sequence:
                    return "Don't know how to merge sequence nodes";

                case YamlNodeType.Mapping:
                    var leftMappingNode = leftNode as YamlMappingNode;
                    var rightMappingNode = rightNode as YamlMappingNode;
                    foreach (KeyValuePair<YamlNode, YamlNode> kvp in rightMappingNode.Children)
                        if (leftMappingNode.Children.ContainsKey(kvp.Key))
                        {
                            if (leftMappingNode[kvp.Key].NodeType == YamlNodeType.Scalar)
                                leftMappingNode.Children[kvp.Key] = kvp.Value.DeepClone();
                            else
                            {
                                var innerMsg = MergeNode(leftMappingNode[kvp.Key], kvp.Value);
                                if (!string.IsNullOrEmpty(innerMsg))
                                    return innerMsg;
                            }
                        }
                        else
                            leftMappingNode.Children[kvp.Key] = kvp.Value.DeepClone();
                    break;
            }
            return string.Empty;
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
            else if (str.Length == 1)
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
            while (el < nodeElement.Length
                && mappingNode.Children.TryGetValue(nodeElement[el++], out node)
                && node.NodeType == YamlNodeType.Mapping)
                mappingNode = node as YamlMappingNode;

            if (el != nodeElement.Length)
                return false;

            outNode = node;
            return true;
        }
        #endregion

    }

}
