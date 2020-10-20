using System;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace LazyStack
{
    public class AwsResource
    {
        /// <summary>
        /// Call when creating resource from aws_default.yaml or sam template
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="rootNode"></param>
        /// <param name="solutionModel"></param> 
        public AwsResource(string resourceName, YamlMappingNode rootNode, SolutionModel solutionModel, bool isDefault) 
        {
            RootNode = rootNode;
            Name = resourceName;
            IsDefault = isDefault;

            if (rootNode.Children.TryGetValue("Type", out YamlNode node))
                AwsType = node.ToString();
            else
                throw new Exception($"Error: Missing Type property for x-lz-AwsResource {resourceName}");

            if (solutionModel.Resources.ContainsKey(resourceName))
            {
                if (!solutionModel.Resources[resourceName].IsDefault)
                    throw new Exception($"Error: Duplicate resource {resourceName} found");
                else
                if (!solutionModel.Resources[resourceName].AwsType.Equals(AwsType))
                    throw new Exception($"Error: Mismatched AWS Type for resource {resourceName}");
                else
                    MergeResourceConfiguration(solutionModel.Resources[resourceName].RootNode, rootNode, solutionModel);
            }
            else
                solutionModel.Resources.Add(Name, this);

            if (IsRestApi && !solutionModel.Apis.ContainsKey(Name))
                solutionModel.Apis.Add(Name, new AwsApiRestApi(this, solutionModel));
            else 
            if (IsHttpApi && !solutionModel.Apis.ContainsKey(Name))
                solutionModel.Apis.Add(Name, new AwsApiHttpApi(this, solutionModel));
        }


        /// <summary>
        /// Call when parsing Tags Object - x-lz-AwsResource directive to create AWS::Serverless::Function objects
        /// </summary>
        /// <param name="resourceNode"></param>
        /// <param name="solutionModel"></param>
        /// <param name="tag"></param>
        /// <param name="awsApi"></param>
        public AwsResource(SolutionModel solutionModel, string tag, AwsApi awsApi)
        {
            AwsType = "AWS::Serverless::Function";
            YamlMappingNode resourceDefinitionNode = null;

            if (string.IsNullOrEmpty(tag))
                throw new ArgumentException();

            Name = solutionModel.TagToFunctionName(solutionModel.AppName,tag);

            if (solutionModel.Resources.ContainsKey(Name))
                throw new Exception($"Error: The {Name} resource requested in tag {tag} already exists");

            // Create the resource document
            var lambdaResourceDefinition = new YamlMappingNode
            {
                { "Type", "AWS::Serverless::Function" }
            };

            var errorMsg = MergeResourceConfiguration(lambdaResourceDefinition, resourceDefinitionNode, solutionModel);
            if (!string.IsNullOrEmpty(errorMsg))
                throw new Exception($"Error: Can't process properties for {tag} {errorMsg}");

            var text = SolutionModel.YamlNodeToText(lambdaResourceDefinition);
            text = SolutionModel.ReplaceTargets(
                text,
                new Dictionary<string, string> { { "__lambdaName__", Name } }
                );
            lambdaResourceDefinition = SolutionModel.ParseYamlText(text);

            RootNode = lambdaResourceDefinition;

            solutionModel.Resources.Add(Name, this);

            // Note: 
            // AwsLambda adds itself to:
            // solutionModel.LambdasByTag
            // solutionModel.LambdasByName
            // awsApi.Lambdas
            new AWSLambda(awsApi, this, solutionModel);
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
        private string MergeResourceConfiguration(YamlMappingNode resourceDefinition, YamlMappingNode userResourceConfiguration, SolutionModel solutionModel)
        {
            var msg = string.Empty;
            string awsType;
            if (resourceDefinition.Children.TryGetValue("Type", out YamlNode node))
                awsType = node.ToString();
            else
                return "No AWS Type specified";
            if (userResourceConfiguration != null 
                && userResourceConfiguration.NodeType == YamlNodeType.Mapping 
                && userResourceConfiguration.Children.TryGetValue("Properties",out node))
                switch(awsType)
                {
                    case "AWS::Serverless::Function":
                        if (node.NodeType != YamlNodeType.Mapping)
                            return $"User Resource Definition is not a Mapping node";
                        // Check for invalid property specifciations in user Resource Configuration
                        YamlMappingNode userProps = node as YamlMappingNode;
                        if (userProps.Children.ContainsKey("FunctionName"))
                            return $"User Resource Configuration may not contain Property FunctionName. LazyStack inserts this automatically.";
                        if (userProps.Children.ContainsKey("CodeUri"))
                            return $"User Resource Configuration may not contain Property CodeUri. LazyStack inserts this automatically.";
                        if (userProps.Children.ContainsKey("Handler"))
                            return $"User Resource Configuration may not contain Property Handler. LazyStack inserts this automatically.";
                        break;
                }

            // Merge in any LazyStack defaults
            if (solutionModel.DefaultResourceConfigurations != null
               && solutionModel.DefaultResourceConfigurations.Children.TryGetValue(awsType, out YamlNode defResourcesMappingNode))
                msg = SolutionModel.MergeNode(resourceDefinition, defResourcesMappingNode);
            if (!string.IsNullOrEmpty(msg))
                return msg;

            if (userResourceConfiguration == null)
                return string.Empty;

            // Merge in userResource Definition
            if (userResourceConfiguration != null)
               msg = SolutionModel.MergeNode(resourceDefinition, userResourceConfiguration);
            if (!string.IsNullOrEmpty(msg))
                return msg;

            return string.Empty;
        }

    }
}
