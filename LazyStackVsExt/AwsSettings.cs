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
            public string Name { get; set; }
            public string Id { get; set; }
            public string Type { get; set; }
            public string Stage { get; set; }
            public bool Secure { get; set; }
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
                        var secure = !resource.LogicalResourceId.Contains("Unsecure");
                        var httpApi = new Api()
                        {
                            Name = resource.LogicalResourceId,
                            Id = resource.PhysicalResourceId,
                            Type = "HttpApi",
                            Stage = "Dev",
                            Secure = !resource.LogicalResourceId.Contains("Unsecure")
                        };
                        ApiGateways.Add(httpApi);
                        break;
                    case "AWS::ApiGateway::RestApi":
                        var secure2 = !resource.LogicalResourceId.Contains("Unsecure");
                        var restApi = new Api()
                        {
                            Name = resource.LogicalResourceId,
                            Id = resource.PhysicalResourceId,
                            Type = "Api",
                            Stage = "Dev",
                            Secure = !resource.LogicalResourceId.Contains("Unsecure")
                        };
                        ApiGateways.Add(restApi);
                        break;
                }
            }

            Region = region;

        }
        private AmazonCloudFormationClient cfClient;

        public string ClientId { get; }
        public string UserPoolId { get; }
        public string IdentityPoolId { get; }
        public string Region { get; }
        public List<Api> ApiGateways { get; } = new List<Api>();

        public string BuildJson()
        {
            var result = $"{{\"Aws\": {Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented)}}}";
            return result;
        }

    }
}
