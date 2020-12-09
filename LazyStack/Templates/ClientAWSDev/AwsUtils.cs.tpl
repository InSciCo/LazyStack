using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Newtonsoft.Json;

using __ProjAWS__;

namespace __ProjName__
{
    public class AwsUtils
    {
        public static AwsSettings GetAwsSettings(string stackName, string stage, string region, bool useLocal = false, string localHost = null, int localPort = 0)
        {
            var cfClient = new AmazonCloudFormationClient();
            var listExportsRequest = new ListExportsRequest();
            var exports = cfClient.ListExportsAsync(listExportsRequest).GetAwaiter().GetResult();

            var getTemplateRequest = new GetTemplateRequest() { StackName = stackName, TemplateStage = "Processed" };
            var template = cfClient.GetTemplateAsync(getTemplateRequest).GetAwaiter().GetResult();


            var describeStackResourcesRequest = new DescribeStackResourcesRequest() { StackName = stackName };
            var describeStackResourcesResponse = cfClient.DescribeStackResourcesAsync(describeStackResourcesRequest).GetAwaiter().GetResult();
            var awsSettings = new AwsSettings();
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
                        awsSettings.ApiGateways.Add( 
                            new AwsSettings.Api()
                            {
                                Name = resource.LogicalResourceId,
                                Id = resource.PhysicalResourceId,
                                Type = "HttpApi",
                                Stage = stage,
                            });
                        break;
                    case "AWS::ApiGateway::RestApi":
                        awsSettings.ApiGateways.Add( 
                            new  AwsSettings.Api()
                            {
                                Name = resource.LogicalResourceId,
                                Id = resource.PhysicalResourceId,
                                Type = "Api",
                                Stage = stage,
                            });
                        break;
                }
            }
            awsSettings.Region = region;
            awsSettings.UseLocal = useLocal;
            awsSettings.LocalHost = localHost;
            awsSettings.LocalPort = localPort;
            return awsSettings;
        }
        public static string GetAwsAppConfig(string stackName, string stage, string region, bool useLocal = false, string localHost = null, int localPort = 0)
        {
            var awsSettings = GetAwsSettings(stackName, stage, region, useLocal, localHost, localPort);
            return $"{{\"Aws\": {JsonConvert.SerializeObject(awsSettings)}}}";
        }
    }
}
