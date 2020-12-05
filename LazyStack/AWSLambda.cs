using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
namespace LazyStack
{
    public class AWSLambda
    {
        public AWSLambda(AwsApi awsApi, AwsResource awsResource, SolutionModel solutionModel)
        {
            AwsResource = awsResource;
            AwsApi = awsApi;
            awsApi.Lambdas.Add(this);
            solutionModel.Lambdas[AwsResource.Name] = this;
        }
        public AwsResource AwsResource { get; }
        //public Dictionary<string, YamlMappingNode> Events { get; set; } = new Dictionary<string, YamlMappingNode>();
        public AwsApi AwsApi { get; set; }
        public YamlMappingNode OpenApiSpec { get; set; } // OpenApi specification specific to this lambda. Necessary for Nswag controller generation
    }
}
