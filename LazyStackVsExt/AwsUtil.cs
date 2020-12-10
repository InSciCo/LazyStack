using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;

namespace LazyStackVsExt
{
    public class AwsUtil
    {
        public static async Task<AwsSettings> GetAsync(string stackName, string region)
        {
            var awsSettings = new AwsSettings();
            awsSettings.Region = region;
            var cfClient = new AmazonCloudFormationClient();

            var describeStackResourcesRequest = new DescribeStackResourcesRequest() { StackName = stackName };
            var describeStackResourcesResponse = await cfClient.DescribeStackResourcesAsync(describeStackResourcesRequest);

            foreach (var resource in describeStackResourcesResponse.StackResources)
            {
                switch (resource.ResourceType)
                {
                    case "AWS::Cognito::UserPool":
                        awsSettings.UserPoolId = resource.PhysicalResourceId;
                        break;
                    case "AWS::Cognito::UserPoolClient":
                        awsSettings.ClientId = resource.PhysicalResourceId;
                        break;
                    case "AWS::Cognito::IdentityPool":
                        awsSettings.IdentityPoolId = resource.PhysicalResourceId;
                        break;
                    case "AWS::ApiGatewayV2::Api":
                        var httpApi = new AwsSettings.Api()
                        {
                            Name = resource.LogicalResourceId,
                            Id = resource.PhysicalResourceId,
                            Type = "HttpApi",
                            Stage = "Dev"
                        };
                        awsSettings.ApiGateways.Add(httpApi);
                        break;
                    case "AWS::ApiGateway::RestApi":
                        var restApi = new AwsSettings.Api()
                        {
                            Name = resource.LogicalResourceId,
                            Id = resource.PhysicalResourceId,
                            Type = "Api",
                            Stage = "Dev"
                        };
                        awsSettings.ApiGateways.Add(restApi);
                        break;
                }
            }
            return awsSettings;
        }
    }
}
