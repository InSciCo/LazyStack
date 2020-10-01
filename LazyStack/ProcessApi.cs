////using Microsoft.Build.Framework;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using YamlDotNet.RepresentationModel;
//using YamlDotNet.Serialization.ObjectGraphVisitors;

//namespace LazyStack
//{
//    /// <summary>
//    /// The ProcessApi class is responsible for processing LazyStack
//    /// application scheama files. 
//    /// Background:
//    /// 
//    /// </summary>
//    public class ProcessApi
//    {
//        public ProcessApi(SolutionModel solutionModel)
//        {
//            this.solutionModel = solutionModel;
//        }
//        private SolutionModel solutionModel;


//        #region Propeties
//        #endregion

//        #region class variables
//        // List of Apis
//        YamlMappingNode openApiSpecRootNode;
//        YamlMappingNode samRootNode;
//        #endregion

//        #region Methods
//        /// <summary>
//        /// Then update the solution's SAM file (usually named template.yaml).
//        /// </summary>
//        /// <param name="solutionModel">Class containing all solution paths necessary to process files</param>
//        public void 
//            ProcessOpenApi()
//        {
//            if (solutionModel == null)
//                throw new Exception("solutionModel not set.");

//            openApiSpecRootNode = Utilities.ReadAndParseYamlFile(solutionModel.OpenApiFilePath); // Read OpenApi specification

//            // HttpApiUnsecure, HttpApiSecure, ApiUnsecure, ApiSecure
//            CreateDefaultResources();

//            // Read x-lz-AwsTemplate or default template. Update Resources.
//            LoadSAM(); 

//            // Updates solutionModel.Apis and solutionModel.Resources
//            ParseOpenApiInfoObject();

//            // Create/Updates solutionModel.Resources, solutionModel.Apis.Lambdas and solutionModel.Lambdas
//            ParseOpenApiTagsObject();

//            // Update Lambda.Events using Path/Operation (route) specs
//            // UserPool, UserPoolClient && IdentityPool will be added if auth required and not previously defined
//            ParseOpenApiPathObject(); 
                                      
//            WriteSAM(); // Write Final SAM file
//        }
//        /// <summary>
//        /// Merge in LazyStack default resources
//        /// HttpApiUnsecure
//        /// HttpApiSecure
//        /// ApiUnsecure
//        /// ApiSecure
//        /// </summary>
//        private void CreateDefaultResources()
//        {
//            // Load default resources
//            var defaultDoc = Utilities.ReadAndParseYamlFile(Path.Combine(solutionModel.LazyStackTemplateFolderPath, "default_resources.yaml"));
//            if (defaultDoc.Children.TryGetValue("Resources", out YamlNode resources))
//                foreach (KeyValuePair<YamlNode, YamlNode> kvp in ((YamlMappingNode)resources).Children)
//                    new AwsResource(kvp.Key.ToString(), kvp.Value as YamlMappingNode, solutionModel: solutionModel, isDefault: true);
//        }

//        /// <summary>
//        /// Load in x-lz-AwsTemplate or default LazyStack template.yaml
//        /// </summary>
//        private void LoadSAM()
//        {
//            // Load LazyStack SAM template or user supplied SAM template
//            var tpl = new YamlStream();
//            var source = !string.IsNullOrEmpty(solutionModel.XlzAwsTemplate)
//                ? solutionModel.SAMTemplateFilePath
//                : Path.Combine(solutionModel.LazyStackTemplateFolderPath, "template.yaml");
//            var text = File.ReadAllText(source);
//            tpl.Load(new StringReader(File.ReadAllText(source)));
//            samRootNode = tpl.Documents[0].RootNode as YamlMappingNode;
//            if (samRootNode.Children.TryGetValue("Resources", out YamlNode node))
//            {
//                if (node.NodeType != YamlNodeType.Mapping)
//                    throw new Exception($"Error: Sam Template has invalid Resources Node");

//                foreach (KeyValuePair<YamlNode, YamlNode> kvp in node as YamlMappingNode)
//                    new AwsResource(kvp.Key.ToString(), kvp.Value as YamlMappingNode, solutionModel, isDefault: false);

//                // Remove Resources Node as we will add it back in later from the solutionModel.Resources array
//                samRootNode.Children.Remove("Resources");
//            }
//        }

//        /// <summary>
//        /// Parse the Info Object in the openApi specification
//        /// We look for and handle the following LZ directives
//        /// x-lz-default-api: string
//        /// x-lz-AwsResource:
//        ///   Name: string
//        ///   Type: string
//        ///   Properties:
//        ///     ...
//        ///   MetaData:
//        ///     ...
//        /// x-lz-AwsTemplate: string
//        /// 
//        /// Adds to
//        ///     solutionModel.Resources - Dictionary of resources defined in OpenApi spec, keyed by resource name
//        ///     solutionModel.Apis - Dictionary of Apis defined in OpenApi spec, keyed by associated resource name
//        /// 
//        /// </summary>
//        private void ParseOpenApiInfoObject()
//        {
//            // Parse LazyStack Directives in OpenApi Object Info Object
//            var apiInfoNode = openApiSpecRootNode["info"] as YamlMappingNode;
//            foreach (KeyValuePair<YamlNode, YamlNode> kvp in apiInfoNode)
//                switch (kvp.Key.ToString())
//                {
//                    case "x-lz-Default-Api":
//                        solutionModel.DefaultApi = kvp.Value.ToString();
//                        break;

//                    case "x-lz-AwsResource":
//                        new AwsResource(kvp.Value as YamlMappingNode, solutionModel); // Added to solutionModel.Resources
//                        break;

//                    case "x-lz-AwsTemplate":
//                        if (!string.IsNullOrEmpty(solutionModel.XlzAwsTemplate))
//                            throw new Exception($"Error: More than one x-lz-AwsTemplate directive found.");

//                        if (string.IsNullOrEmpty(kvp.Value.ToString()))
//                            throw new Exception($"x-lz-AwsTemplate value can't be empty");

//                        solutionModel.XlzAwsTemplate = kvp.Value.ToString();
//                        var path = Path.Combine(solutionModel.SolutionRootFolderPath, solutionModel.XlzAwsTemplate);
//                        if (!File.Exists(path))
//                            throw new Exception($"x-lz-AwsTemplate file not found {path}");
//                        break;

//                    default: // stop on any unrecognized directives
//                        if (kvp.Key.ToString().StartsWith("x-lz-", StringComparison.OrdinalIgnoreCase))
//                            throw new Exception($"Unknown directive {kvp.Key.ToString()}");
//                        break;
//                }
//        }

//        /// <summary>
//        /// Parse the Tag Object in the openApi specification
//        /// We look for and handle the following LZ directives
//        /// x-lz-api: string
//        /// x-lz-d-MemorySize: integer
//        /// x-lz-MemorySize: integer
//        /// x-lz-DefaultTag: bool
//        /// We update the solutionModel.Lambdas Dictionary and
//        /// the solutionModel.Apis Dictionary.
//        /// 
//        /// </summary>
//        private void ParseOpenApiTagsObject()
//        {
//            // Create apiSpecsByApiType and ApiGroups entries based on apiRootNode.tags
//            if (!openApiSpecRootNode.Children.TryGetValue("tags", out YamlNode tagsNode))
//                throw new Exception($"Error: Could not find a tags field. An OpenApi tags field is required for LazyStack to generate an api.");

//            if (tagsNode.NodeType != YamlNodeType.Sequence)
//                throw new Exception($"Error: tags node is not a sequence node.");

//            foreach (YamlMappingNode tagsNodeItem in tagsNode as YamlSequenceNode)
//            {
//                if (tagsNodeItem.NodeType != YamlNodeType.Mapping)
//                    throw new Exception($"Error: Object Tag {tagsNodeItem.ToString()} is not a mapping object.");

//                YamlMappingNode awsResourceNode = null;
//                string lambdaApiName = string.Empty;
//                string tagName = string.Empty;
//                bool defaultTag = false;
//                foreach (KeyValuePair<YamlNode,YamlNode> kvp in tagsNodeItem)
//                {
//                    var propertyName = kvp.Key.ToString();
//                    switch (propertyName)
//                    {
//                        case "name": // this is the tag name
//                            tagName = kvp.Value.ToString();
//                            break;

//                        case "x-lz-Api":
//                            lambdaApiName = kvp.Value.ToString();
//                            break;

//                        case "x-lz-DefaultTag":
//                            defaultTag = bool.Parse(kvp.Value.ToString());
//                            break;

//                        case "x-lz-AwsResource":
//                            awsResourceNode = kvp.Value as YamlMappingNode;
//                            break;

//                        default:
//                            break;
//                    }
//                }

//                if (string.IsNullOrEmpty(tagName))
//                    throw new Exception($"Error: tag with no name property found.");

//                if (string.IsNullOrEmpty(lambdaApiName))
//                    lambdaApiName = solutionModel.DefaultApi;

//                if (defaultTag)
//                    if (!string.IsNullOrEmpty(solutionModel.DefaultTag))
//                        throw new Exception($"Error: Attempt to set {tagName} as default tag when default tag is already set to {solutionModel.DefaultTag}");
//                    else
//                        solutionModel.DefaultTag = tagName;

//                if (!solutionModel.Apis.ContainsKey(lambdaApiName))
//                    throw new Exception($"Error: tag {tagName} references Api {lambdaApiName} which does not exist");

//                // If AwsResource creates a new instance it adds itself to :
//                // solutionModel.Resources, 
//                // solutionModel.Apis[lambdaApiName].Lambdas, 
//                // soulutionModel.LambdaByName
//                // solutionModel.LambdaByTag
//                //
//                // Do not use any returned instance here! It could be we found a reference and the returned
//                // instance is a throw away.
//                new AwsResource(awsResourceNode, solutionModel, tagName , solutionModel.Apis[lambdaApiName]);
//            }
//        }

//        /// <summary>
//        /// Parse the Path Object in the OpenApi specification
//        /// We look for and handle the following properties
//        /// tags:
//        /// - string   # tag identifying the AWSLambda to handle this path
//        /// 
//        /// For each Path we add an Event to the Lambda associated with the tag.
//        /// 
//        /// </summary>
//        private void ParseOpenApiPathObject()
//        {
//            string[] validOperations = { "GET", "PUT", "POST", "DELETE", "UPDATE" };

//            if (!openApiSpecRootNode.Children.TryGetValue("paths", out YamlNode pathsNode))
//                throw new Exception($"Error: OpenApi specification missing \"Paths\" Object");

//            // foreach Path in Paths
//            foreach (KeyValuePair<YamlNode, YamlNode> pathsNodeChild in pathsNode as YamlMappingNode)
//            {
//                var path = pathsNodeChild.Key.ToString();
//                var pathNodeValue = (YamlMappingNode)pathsNodeChild.Value;

//                // foreach Operation in Path
//                foreach (KeyValuePair<YamlNode, YamlNode> apiPathNodeChild in pathNodeValue)
//                {
//                    var httpOperation = apiPathNodeChild.Key.ToString().ToUpper(); // ex: get, post, put, delete
//                    var apiOpNode = apiPathNodeChild.Value as YamlMappingNode; // ex: tags, summary, operationId, responses, requestBody etc.

//                    if (!validOperations.Contains(httpOperation))
//                        throw new Exception($"Error: Invalid http operation {httpOperation} specified in {path}");

//                    // Get the tag
//                    var tagName = string.Empty;
//                    if(apiOpNode.Children.TryGetValue("tags", out YamlNode apiOpTagsNode))
//                    {   
//                        if (apiOpTagsNode.NodeType != YamlNodeType.Sequence)
//                            throw new Exception($"Error: Path {path} Tag Object must be a sequence node");
//                        tagName = ((YamlSequenceNode)apiOpTagsNode).Children[0].ToString();
//                    }
//                    else
//                        tagName = solutionModel.DefaultTag;

//                    // Find Lambda in global index of Lambdas (indexed by tag) 
//                    if (!solutionModel.LambdasByTag.TryGetValue(tagName, out AWSLambda awsLambda))
//                        throw new Exception($"Error: {tagName} specified for path {path} not found.");

//                    // Generate eventName
//                    // ex: GET /order/{orderId} => GetOrderOrderId
//                    var eventName = RouteToEventName(httpOperation, path);
//                    if (awsLambda.AwsResource.RootNode.Children.TryGetValue("Events", out YamlNode eventsNode)
//                        && ((YamlMappingNode)eventsNode).Children.ContainsKey(eventName))
//                        throw new Exception($"Error: Duplicate event name {eventName} in Lambda {awsLambda.Name}");

//                    // The api is responsible for creating the event node because Event nodes are different based on Api
//                    var eventNode = awsLambda.AwsApi.EventNode(path, httpOperation);

//                    YamlMappingNode eventsMappingNode;
//                    if (awsLambda.AwsResource.RootNode.Children.TryGetValue("Events", out YamlNode node))
//                        eventsMappingNode = node as YamlMappingNode;
//                    else
//                    {
//                        eventsMappingNode = new YamlMappingNode();
//                        awsLambda.AwsResource.RootNode.Children.Add("Events", eventsMappingNode);
//                    }
//                    eventsMappingNode.Children.Add(eventName, eventNode);
//                }
//            }
//        }
       
//        private void
//            WriteSAM()
//        {
//            var resources = new YamlMappingNode();
//            foreach (var resource in solutionModel.Resources)
//                if(resource.Value.IsHttpApi || resource.Value.IsRestApi)
//                {   // Emit the Api only if it services a lambda
//                    if(solutionModel.Apis[resource.Key].Lambdas.Count > 0)
//                        resources.Add(resource.Key, resource.Value.RootNode);
//                }
//                else
//                    resources.Add(resource.Key, resource.Value.RootNode);

//            samRootNode.Add("Resources", resources);

//            // Write new template.yaml
//            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(solutionModel.SolutionRootFolderPath, "serverless.template")))
//            {
//                new YamlDotNet.Serialization.Serializer().Serialize(file, samRootNode);
//            }
//        }
//        #endregion

//        #region Utility Methods

//        public YamlNode 
//            NewNode(YamlNode node)
//        {
//            YamlNode targetNode = null;
//            switch (node.NodeType)
//            {
//                case YamlNodeType.Alias:
//                    throw new Exception("I don't know how to process Alias nodes");
//                case YamlNodeType.Mapping:
//                    targetNode = new YamlMappingNode();
//                    break;
//                case YamlNodeType.Scalar:
//                    targetNode = new YamlScalarNode(null);
//                    break;
//                case YamlNodeType.Sequence:
//                    targetNode = new YamlSequenceNode();
//                    break;
//            }
//            return targetNode;
//        }

//        public YamlNode 
//            CopyNode(YamlNode sourceNode)
//        {
//            var targetNode = NewNode(sourceNode);
//            CopyNode(sourceNode, targetNode);
//            return targetNode;
//        }

//        public YamlNode 
//            CopyNode(YamlNode sourceNode, YamlNode targetNode)
//        {
//            if (sourceNode == null)
//                throw new Exception("sourcenode is null");

//            switch (sourceNode.NodeType)
//            {
//                case YamlNodeType.Alias:
//                    throw new Exception("I don't know how to process Alias nodes");

//                case YamlNodeType.Mapping:
//                    var sourceMappingNode = sourceNode as YamlMappingNode;
//                    var targetMappingNode = targetNode as YamlMappingNode;
//                    foreach (KeyValuePair<YamlNode, YamlNode> childNode in sourceMappingNode.Children)
//                    {
//                        var newChildNode = NewNode(childNode.Value);
//                        CopyNode(childNode.Value, newChildNode);
//                        targetMappingNode.Children.Add(childNode.Key, newChildNode);
//                    }
//                    break;
//                case YamlNodeType.Sequence:
//                    var sourceSequenceNode = sourceNode as YamlSequenceNode;
//                    var targetSequenceNode = targetNode as YamlSequenceNode;
//                    foreach (YamlNode childNode in sourceSequenceNode.Children)
//                    {
//                        var newChildNode = NewNode(childNode);
//                        CopyNode(childNode, newChildNode);
//                        targetSequenceNode.Children.Add(newChildNode);
//                    }
//                    break;
//                case YamlNodeType.Scalar:
//                    var targetScalerNode = targetNode as YamlScalarNode;
//                    targetScalerNode.Value = ((YamlScalarNode)sourceNode).Value;
//                    break;
//            }
//            return targetNode;
//        }

//        public bool 
//            FindComponentReferences2(YamlNode node, List<string> references, List<string> processedReferences)
//        {
//            switch (node.NodeType)
//            {
//                case YamlNodeType.Alias:
//                    throw new Exception("I don't know how to handle Alias nodes");
//                case YamlNodeType.Mapping:
//                    foreach (KeyValuePair<YamlNode, YamlNode> childNode in ((YamlMappingNode)node).Children)
//                        if (childNode.Key.ToString().Equals(@"$ref") && childNode.Value.NodeType == YamlNodeType.Scalar)
//                        {
//                            var scalarNode = childNode.Value as YamlScalarNode;
//                            if (!references.Contains(scalarNode.Value) && !processedReferences.Contains(scalarNode.Value))
//                                references.Add(scalarNode.Value);
//                        }
//                        else
//                            FindComponentReferences2(childNode.Value, references, processedReferences);
//                    break;
//                case YamlNodeType.Scalar:
//                    break;
//                case YamlNodeType.Sequence:
//                    foreach (YamlNode childNode in ((YamlSequenceNode)node).Children)
//                        FindComponentReferences2(childNode, references, processedReferences);
//                    break;
//            }

//            return references.Count > 0;
//        }

//        public string 
//            TagToFunctionName(string apiName, string tag)
//        {
//            return apiName + Utilities.ToUpperFirstChar(tag) + "Lambda";
//        }

//        /// <summary>
//        /// Create readbale Event Name from Route
//        /// ex: GET /order/{orderId} => GetOrderOrderId
//        /// </summary>
//        /// <param name="httpOperation"></param>
//        /// <param name="path"></param>
//        /// <returns></returns>
//        public string
//            RouteToEventName(string httpOperation, string path)
//        {
//            var name = Utilities.ToUpperFirstChar(httpOperation.ToLower()); //ex: GET to Get
//            var parts = path.Split("/"); 
//            foreach(var part in parts)
//            {
//                var namePart = part;
//                namePart = namePart.Replace("{", "");
//                namePart = namePart.Replace("}", "");
//                name += Utilities.ToUpperFirstChar(namePart); 
//            }
//            return name;
//        }

//        public string GetMappingNodeStringValue(YamlMappingNode node, string key)
//        {
//            var value = node.Children.TryGetValue(key, out YamlNode childNode)
//                ? childNode.ToString()
//                : string.Empty;
//            return value;
//        }

//        #endregion Methods

//        #region Obsolete Methods -- keeping in case we need to do schema collection

//        ///// <summary>
//        ///// Update the template's AWS::Serverless::Api or AWS::Serverless::HttpApi resources
//        ///// along with Function resources referenced by the api resources
//        ///// </summary>
//        ///// <param name="apiType">Api or HttpApi</param>
//        ///// <param name="apiRootNode">Root node of openapi specification</param>
//        ///// <param name="tplResourcesNode">Existing template root node</param>
//        ///// <param name="newResourcesNode">New template root node</param>
//        ///// <param name="apiGatewayLambdaFunctions">Dictionary of functions</param>
//        //private void UpdateTemplateApi(
//        //    string apiType, // HttpApi or Api
//        //    YamlMappingNode tplResourcesNode,
//        //    YamlMappingNode newResourcesNode,
//        //    Dictionary<string, LambdaFuncInfo> apiGatewayLambdaFunctions)
//        //{
//        //    string funcNodeTemplate = File.ReadAllText(Path.Combine(solutionModel.LazyStackTemplateFolderPath, "snippet_function.yaml"));
//        //    string apiNodeTemplate = File.ReadAllText(Path.Combine(solutionModel.LazyStackTemplateFolderPath, "snippet_api.yaml"));
//        //    string httpApiNodeTemplate = File.ReadAllText(Path.Combine(solutionModel.LazyStackTemplateFolderPath, "snippet_httpapi.yaml"));
//        //    string apiGatewayIntegrationTemplate = File.ReadAllText(Path.Combine(solutionModel.LazyStackTemplateFolderPath, "snippet_apigatewayintegration.yaml"));
//        //    string eventTemplate = File.ReadAllText(Path.Combine(solutionModel.LazyStackTemplateFolderPath, "snippet_event.yaml"));

//        //    // Create default AWS::Serverless::Function resources for any new API Gateway Functions
//        //    foreach (KeyValuePair<string, LambdaFuncInfo> kvp in apiGatewayLambdaFunctions)
//        //        if (!kvp.Value.Found)
//        //        {
//        //            YamlMappingNode funcNode = ParseYamlText(funcNodeTemplate.Replace("__lambdaName__", kvp.Key));
//        //            apiGatewayLambdaFunctions[kvp.Key].Properties = funcNode["Properties"] as YamlMappingNode;
//        //            tplResourcesNode.Add(kvp.Key, funcNode);
//        //            kvp.Value.Found = true;
//        //        }

//        //    // Create Api resource if it doesn't exist
//        //    YamlMappingNode apiNode;
//        //    if (tplResourcesNode.Children.ContainsKey(apiType))
//        //        apiNode = tplResourcesNode[apiType] as YamlMappingNode; // Assign from current temeplate
//        //    else
//        //    {  // Create Api Default Resources Settings if it doesn't exist
//        //        switch (apiType)
//        //        {
//        //            case "Api":
//        //                apiNode = ParseYamlText(apiNodeTemplate);
//        //                tplResourcesNode.Add("Api", apiNode);
//        //                break;
//        //            case "HttpApi":
//        //                apiNode = ParseYamlText(httpApiNodeTemplate);
//        //                tplResourcesNode.Add("HttpApi", apiNode);
//        //                break;
//        //            default:
//        //                throw new Exception($"Unsupported api type {apiType} found");
//        //        }
//        //    }
//        //    newResourcesNode.Add(apiType, apiNode);

//        //    // Assign OpenApi specification to DefinitionBody
//        //    var apiPropertiesNode = apiNode["Properties"] as YamlMappingNode;
//        //    apiPropertiesNode.Add("DefinitionBody", openApiSpecRootNode);

//        //    // go through api adding in aws x-amazon-apigateway-integration properties
//        //    // Note that this will modify the original apiRootNode object graph but this is okay as
//        //    // we no longer need it in its original form.
//        //    // We also generate new AWS::Serverless::Function sections as necessary

//        //    var pathsNode = apiPropertiesNode["DefinitionBody"]["paths"] as YamlMappingNode;
//        //    foreach (KeyValuePair<YamlNode, YamlNode> pathNode in pathsNode)
//        //    {
//        //        // PathsNodeChild ex: /pet/findByTags
//        //        var path = pathNode.Key.ToString();
//        //        foreach (KeyValuePair<YamlNode, YamlNode> operationNode in pathNode.Value as YamlMappingNode)
//        //        {
//        //            var operation = operationNode.Key.ToString();
//        //            var operationMappingNode = operationNode.Value as YamlMappingNode;

//        //            // Get tag from tags list to use to determine which function handles endpoint operation
//        //            var tagNode = operationMappingNode["tags"] as YamlSequenceNode;
//        //            var apiGroup = tagNode[0].ToString();
//        //            var functionName = TagToFunctionName(solutionModel.AppName, apiGroup);

//        //            // Add integration
//        //            operationMappingNode.Add("x-amazon-apigateway-integration",
//        //                ParseYamlText(apiGatewayIntegrationTemplate.Replace("__lambdaName__", functionName)));

//        //            // Add Event
//        //            var lambdaFuncInfo = apiGatewayLambdaFunctions[functionName];
//        //            var properties = lambdaFuncInfo.Properties;
//        //            YamlMappingNode events = new YamlMappingNode();
//        //            if (!properties.Children.ContainsKey("Events"))
//        //                properties.Add("Events", events);

//        //            if (properties["Events"] == null || !(properties["Events"].GetType() == typeof(YamlMappingNode)))
//        //            {
//        //                properties.Children.Remove("Events");
//        //                properties.Add("Events", events);
//        //            }
//        //            else
//        //                events = properties["Events"] as YamlMappingNode;


//        //            var eventText = eventTemplate.Replace("__path__", path);
//        //            eventText = eventText.Replace("__httpMethod__", operation.ToUpper());
//        //            eventText = eventText.Replace("__apiLogicalName__", apiType);
//        //            string EventName = $"{apiType}{operation}{path.ToString().Replace(@"/", "").Replace(@"{", "").Replace(@"}", "")}";
//        //            events.Add(EventName, ParseYamlText(eventText));
//        //        }
//        //    }
//        //}

//        //private void
//        //    RemoveObsoleteResources(Dictionary<string, YamlMappingNode> apiTypes, YamlMappingNode tplResourcesNode)
//        //{

//        //    // Remove obsolete AWS::Serverless::Api resource values from template
//        //    if (tplResourcesNode.Children.ContainsKey("Api")) // if the Api resource exists
//        //    {
//        //        if (!apiTypes.ContainsKey("Api")) // if the Api is currently defined
//        //            tplResourcesNode.Children.Remove("Api"); // Api no longer in use
//        //        else
//        //        {   // Clear target Defintion body as we will be inserting new content from apiTypes specification
//        //            var children = tplResourcesNode.Children["Api"]["Properties"] as YamlMappingNode;
//        //            if (children.Children.ContainsKey("DefinitionBody"))
//        //                children.Children.Remove("DefinitionBody");
//        //        }
//        //    }

//        //    // Remove obsolete AWS::Serverless::Api resource values from template
//        //    if (tplResourcesNode.Children.ContainsKey("HttpApi"))
//        //    {
//        //        if (!apiTypes.ContainsKey("HttpApi"))
//        //            tplResourcesNode.Children.Remove("HttpApi"); // Api no longer in use
//        //        else
//        //        {   // Clear target Defintion body as we will  be inserting new content from apiTypes specification
//        //            var children = tplResourcesNode.Children["HttpApi"]["Properties"] as YamlMappingNode;
//        //            if (children.Children.ContainsKey("DefinitionBody"))
//        //                children.Children.Remove("DefinitionBody");
//        //        }
//        //    }

//        //}

//        //private void
//        //    CopyNonResourceItems(YamlMappingNode tplRootNode, YamlMappingNode newRootNode)
//        //{
//        //    // copy non Resource items from old to new template
//        //    foreach (KeyValuePair<YamlNode, YamlNode> tplRootNodeChild in tplRootNode)
//        //    {
//        //        var tplRootNodeChildKey = tplRootNodeChild.Key.ToString();
//        //        var tplRootNodeChildValue = tplRootNodeChild.Value as YamlNode;
//        //        if (!tplRootNodeChildKey.Equals("Resources"))
//        //            newRootNode.Add(tplRootNodeChildKey, tplRootNodeChildValue);
//        //    }

//        //}

//        ///// <summary>
//        /////  Copy nodes from source tree into target tree based on reference path
//        ///// </summary>
//        ///// <param name="reference">schema reference path ex: #/components/schemas/Order </param>
//        ///// <param name="apiRootNode">Source OpenApi document</param>
//        ///// <param name="targetRootNode">Target OpenApi document</param>
//        //private void
//        //    InsertNodes(string reference, YamlMappingNode targetRootNode)
//        //{
//        //    string[] referenceParts = reference.Split('/'); //ex: #/components/schemas/Order
//        //    if (!referenceParts[0].Equals("#"))
//        //        throw new Exception($"reference path must begin with #");

//        //    YamlMappingNode refNode = openApiSpecRootNode;
//        //    YamlMappingNode tarNode = targetRootNode;
//        //    for (int i = 1; i < referenceParts.Length; i++)
//        //    {
//        //        var partName = referenceParts[i];
//        //        refNode = refNode.Children[partName] as YamlMappingNode;
//        //        if (!tarNode.Children.ContainsKey(partName))
//        //        {
//        //            YamlMappingNode newNode =
//        //                (referenceParts.Length - 1 == i)
//        //                ? refNode
//        //                : new YamlMappingNode();
//        //            tarNode.Children.Add(partName, newNode);
//        //            tarNode = newNode;
//        //        }
//        //        else
//        //            tarNode = tarNode.Children[partName] as YamlMappingNode;
//        //    }
//        //}

//        ///// <summary>
//        ///// Discover references in node
//        ///// </summary>
//        ///// <param name="node"></param>
//        ///// <param name="apiRootNode"></param>
//        ///// <param name="references"></param>
//        ///// <returns></returns>
//        //public int
//        //    FindComponentReferences(YamlNode node, List<string> references)
//        //{
//        //    if (openApiSpecRootNode == null)
//        //        throw new ArgumentNullException();
//        //    if (node == null)
//        //        throw new ArgumentNullException();

//        //    int startingCount = references.Count;
//        //    FindComponentReferencesSub(node, references);
//        //    return references.Count - startingCount;

//        //}

//        //private void
//        //    FindComponentReferencesSub(YamlNode node, List<string> references)
//        //{
//        //    switch (node.NodeType)
//        //    {
//        //        case YamlNodeType.Alias:
//        //            throw new Exception("I don't know how to handle Alias nodes");
//        //        case YamlNodeType.Mapping:
//        //            foreach (KeyValuePair<YamlNode, YamlNode> childNode in ((YamlMappingNode)node).Children)
//        //                if (childNode.Key.ToString().Equals(@"$ref") && childNode.Value.NodeType == YamlNodeType.Scalar)
//        //                {
//        //                    var scalarNode = childNode.Value as YamlScalarNode;
//        //                    var reference = scalarNode.Value;
//        //                    if (!references.Contains(reference))
//        //                        references.Add(reference);

//        //                    // Look for subreferences in Components section
//        //                    string[] referenceParts = reference.Split('/'); //ex: #/components/schemas/Order
//        //                    if (!referenceParts[0].Equals("#"))
//        //                        throw new Exception($"reference path must begin with #");
//        //                    YamlNode refNode = openApiSpecRootNode;
//        //                    for (int i = 1; i < referenceParts.Length; i++)
//        //                        refNode = refNode[referenceParts[i]];

//        //                    FindComponentReferencesSub(refNode, references);
//        //                }
//        //                else
//        //                    FindComponentReferencesSub(childNode.Value, references);
//        //            break;
//        //        case YamlNodeType.Scalar:
//        //            break;
//        //        case YamlNodeType.Sequence:
//        //            foreach (YamlNode childNode in ((YamlSequenceNode)node).Children)
//        //                FindComponentReferencesSub(childNode, references);
//        //            break;
//        //    }
//        //}

//        #endregion
//    }
//}
