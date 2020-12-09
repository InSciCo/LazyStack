using System;
using System.Collections.Generic;
using System.Text;

using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;

namespace LazyStackVsExt
{
    public class AwsSettings
    {
        public class Api
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string Scheme { get; set; }
            public string Id { get; set; }
            public string Service { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public string Stage { get; set; }
        }

        public AwsSettings(
            string stackName,
            string region
            )
        {
            var cfClient = new AmazonCloudFormationClient();
            var describeStackResourcesRequest = new DescribeStackResourcesRequest() { StackName = stackName };
            var describeStackResourcesResponse = cfClient.DescribeStackResourcesAsync(describeStackResourcesRequest).GetAwaiter().GetResult();
            foreach (var resource in describeStackResourcesResponse.StackResources)
            {
                switch (resource.ResourceType)
                {
                    case "AWS::Cognito::UserPool":
                        UserPoolId = resource.PhysicalResourceId;
                        break;
                    case "AWS::Cognito::UserPoolClient":
                        ClientId = resource.PhysicalResourceId;
                        break;
                    case "AWS::Cognito::IdentityPool":
                        IdentityPoolId = resource.PhysicalResourceId;
                        break;
                    case "AWS::ApiGatewayV2::Api":
                        var httpApi = new Api()
                        {
                            Name = resource.LogicalResourceId,
                            Id = resource.PhysicalResourceId,
                            Type = "HttpApi",
                            Stage = "Dev"
                        };
                        ApiGateways.Add(httpApi);
                        break;
                    case "AWS::ApiGateway::RestApi":
                        var restApi = new Api()
                        {
                            Name = resource.LogicalResourceId,
                            Id = resource.PhysicalResourceId,
                            Type = "Api",
                            Stage = "Dev"
                        };
                        ApiGateways.Add(restApi);
                        break;
                }
            }

            Region = region;
        }

        public string ClientId { get; set; }
        public string UserPoolId { get; set; }
        public string IdentityPoolId { get; set; }
        public string Region { get; set; }
        public string DefaultScheme { get; set; }
        public string DefaultHost { get; set; }
        public int DefaultPort { get; set; }
        public string DefaultService { get; set; }
        public string LocalScheme { get; set; }
        public string LocalHost { get; set; }
        public int LocalPort { get; set; }
        public bool UseLocal { get; set; }
        public List<Api> ApiGateways { get; } = new List<Api>();

        public string BuildJson()
        {
            var result = $"{{\"Aws\": {Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented)}}}";
            return result;
        }

    }
}
