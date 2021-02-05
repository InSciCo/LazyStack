using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Newtonsoft.Json;
using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime;
using Amazon;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;


namespace LazyStackAuthTests
{
    public class AwsAuthSettings
    {
        public string StackName { get; set; }
        public string ClientId { get; set; }
        public string UserPoolId { get; set; }
        public string IdentityPoolId { get; set; }
        public string Region { get; set; }

        public string BuildJsonWrapped(string name = "Aws")
        {
            var result = $"{{\"{name}\": {Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented)}}}";
            return result;
        }
    }

    public static class AwsConfigAuth
    {
        public static async Task<string> GenerateSettingsJson(string region, string stackName)
        {
            var cfClient = new AmazonCloudFormationClient(RegionEndpoint.GetBySystemName(region));
            var describeStackResourcesRequest = new DescribeStackResourcesRequest() { StackName = stackName };
            var describeStackResourcesResponse = await cfClient.DescribeStackResourcesAsync(describeStackResourcesRequest);
            var awsSettings = new AwsAuthSettings();

            foreach (var resource in describeStackResourcesResponse.StackResources)
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
                }
            awsSettings.Region = region;
            awsSettings.StackName = stackName;

            return awsSettings.BuildJsonWrapped();
        }

    }

}