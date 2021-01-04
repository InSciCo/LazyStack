using System;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace LazyStack
{
    public class AwsResource
    {
        public AwsResource() { }

        public static AwsResource MakeGeneralResource(string resourceName, YamlMappingNode rootNode, SolutionModel solutionModel, bool isDefault)
        {
            var awsResource = new AwsResource()
            {
                RootNode = rootNode,
                Name = resourceName,
                IsDefault = isDefault,
            };

            if (rootNode.Children.TryGetValue("Type", out YamlNode node))
                awsResource.AwsType = node.ToString();
            else
                throw new Exception($"Error: Missing Type property for AwsResource {resourceName}");

            if (solutionModel.Resources.ContainsKey(resourceName))
            {
                if (!solutionModel.Resources[resourceName].IsDefault)
                    throw new Exception($"Error: Duplicate resource {resourceName} found");
                else
                if (!solutionModel.Resources[resourceName].AwsType.Equals(awsResource.AwsType))
                    throw new Exception($"Error: Mismatched AWS Type for resource {resourceName}");
                else
                    AwsResource.MergeLambdaResourceConfiguration(solutionModel.Resources[resourceName].RootNode, rootNode, solutionModel);
            }
            else
                solutionModel.Resources.Add(resourceName, awsResource);

            if (awsResource.IsRestApi && !solutionModel.Apis.ContainsKey(resourceName))
                solutionModel.Apis.Add(resourceName, new AwsApiRestApi(awsResource, solutionModel, resourceName));
            else 
            if (awsResource.IsHttpApi && !solutionModel.Apis.ContainsKey(resourceName))
                    solutionModel.Apis.Add(resourceName, new AwsApiHttpApi(awsResource, solutionModel, resourceName));

            return awsResource;
        }

        public static AwsResource MakeLambdaResource(SolutionModel solutionModel, string tag, AwsApi awsApi)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentException();

            var awsResource = new AwsResource()
            {
                AwsType = "AWS::Serverless::Function",
                Name = solutionModel.TagToFunctionName(tag)
            };

            YamlMappingNode resourceDefinitionNode = null;

            if (solutionModel.Resources.ContainsKey(awsResource.Name))
                throw new Exception($"Error: The {awsResource.Name} resource requested in tag {tag} already exists");

            // Create the resource document
            var lambdaResourceDefinition = new YamlMappingNode
            {
                { "Type", "AWS::Serverless::Function" }
            };

            lambdaResourceDefinition = MergeLambdaResourceConfiguration(lambdaResourceDefinition, resourceDefinitionNode, solutionModel);

            var codeUriPlatform = solutionModel.GetConfigProperty($"{awsResource.Name}/CodeUriPlatform", errorIfMissing: false);
            if (string.IsNullOrEmpty(codeUriPlatform))
                codeUriPlatform = solutionModel.GetConfigProperty("LambdaProjects/CodeUriPlatform", errorIfMissing: false);
            if (string.IsNullOrEmpty(codeUriPlatform))
                throw new Exception($"Error: Missing LambdaProjects/CodeUriPlatform directive.");


            var text = SolutionModel.YamlNodeToText(lambdaResourceDefinition); // Grab the lambda template
            text = SolutionModel.ReplaceTargets(
                text,
                new Dictionary<string, string> { { "__lambdaName__", awsResource.Name }, { "__codeUriPlatform__", codeUriPlatform } }
                );
            lambdaResourceDefinition = SolutionModel.ParseYamlText(text);

            awsResource.RootNode = lambdaResourceDefinition;

            solutionModel.Resources.Add(awsResource.Name, awsResource);

            // Note: 
            // AwsLambda adds itself to:
            // solutionModel.LambdasByTag
            // solutionModel.LambdasByName
            // awsApi.Lambdas
            new AWSLambda(awsApi, awsResource, solutionModel);

            return awsResource;
        }
        
        
        public string Name { get; set; }
        public string AwsType { get; private set; }
        public bool IsLambda { get { return AwsType.Equals("AWS::Serverless::Function"); } }
        public bool IsRestApi { get { return AwsType.Equals("AWS::Serverless::Api"); } }
        public bool IsHttpApi { get { return  AwsType.Equals("AWS::Serverless::HttpApi"); } }
        public bool IsDefault { get; set; }

        public YamlMappingNode RootNode { get; set; }

        /// <summary>
        /// Merge LazyStack default resource items and
        /// additional userResourceDefintion items
        /// </summary>
        /// <param name="resourceDefinition">resource definition document</param>
        /// <param name="userResourceConfiguration"></param>
        /// <param name="AwsType"></param>
        /// <param name="tag"></param>
        public static YamlMappingNode MergeLambdaResourceConfiguration(YamlMappingNode resourceDefinition, YamlMappingNode userResourceConfiguration, SolutionModel solutionModel)
        {
            string awsType;
            if (resourceDefinition.Children.TryGetValue("Type", out YamlNode node))
                awsType = node.ToString();
            else
                throw new Exception($"Error: No AWS Type specified");

            if (userResourceConfiguration != null 
                && userResourceConfiguration.NodeType == YamlNodeType.Mapping 
                && userResourceConfiguration.Children.TryGetValue("Properties",out node))
                switch(awsType)
                {
                    case "AWS::Serverless::Function":
                        if (node.NodeType != YamlNodeType.Mapping)
                            throw new Exception("User Resource Definition is not a Mapping node");
                        // Check for invalid property specifciations in user Resource Configuration
                        YamlMappingNode userProps = node as YamlMappingNode;
                        if (userProps.Children.ContainsKey("FunctionName"))
                            throw new Exception("User Resource Configuration may not contain Property FunctionName. LazyStack inserts this automatically.");
                        if (userProps.Children.ContainsKey("CodeUri"))
                            throw new Exception("User Resource Configuration may not contain Property CodeUri. LazyStack inserts this automatically.");
                        if (userProps.Children.ContainsKey("Handler"))
                            throw new Exception("User Resource Configuration may not contain Property Handler. LazyStack inserts this automatically.");
                        break;
                }

            // Merge in any LazyStack defaults
            if (solutionModel.DefaultResourceConfigurations != null
               && solutionModel.DefaultResourceConfigurations.Children.TryGetValue(awsType, out YamlNode defResourcesMappingNode))
                resourceDefinition = SolutionModel.MergeNode(resourceDefinition, defResourcesMappingNode) as YamlMappingNode;

            // Merge in userResource Definition
            if (userResourceConfiguration != null)
                resourceDefinition = SolutionModel.MergeNode(resourceDefinition, userResourceConfiguration) as YamlMappingNode;

            return resourceDefinition;
        }

        public string GetOutputItem(string prefix)
        {
            string OutputItem = string.Empty;
            switch (AwsType)
            {
                case "AWS::Serverless::Api":
                case "AWS::Serverless::HttpApi":
                  OutputItem =
                     $"{prefix}{Name}: \n"
                    + $"  Description: \"Gateway URL\"\n"
                    + $"  Value:\n"
                    + $"    Fn::Sub: https://${{{Name}}}.execute-api.${{AWS::Region}}.amazonaws.com/${{StageName}}\n"
                    + $"  Export:\n"
                    + $"    Name: \"{Name}\"\n";
                    break;
                case "AWS::Cognito::UserPool":
                    OutputItem =
                       $"{prefix}{Name}: \n"
                      + $"  Description: \"Cognito UserPoolId\"\n"
                      + $"  Value:\n"
                      + $"    Ref: {Name}\n"
                      + $"  Export:\n"
                      + $"    Name: \"{Name}\"\n";
                    break;
                case "AWS::Cognito::UserPoolClient":
                    OutputItem =
                       $"{prefix}{Name}: \n"
                      + $"  Description: \"Cognito UserPoolClientId\"\n"
                      + $"  Value:\n"
                      + $"    Ref: {Name}\n"
                      + $"  Export:\n"
                      + $"    Name: \"{Name}\"\n";
                    break;
                case "AWS::Cognito::IdentityPool":
                    OutputItem =
                       $"{prefix}{Name}: \n"
                      + $"  Description: \"Cognito IdentityPoolId\"\n"
                      + $"  Value:\n"
                      + $"    Ref: {Name}\n"
                      + $"  Export:\n"
                      + $"    Name: \"{Name}\"\n";
                    break;
            }

            return OutputItem;
        }

    }
}
