using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Newtonsoft.Json.Linq;
using YamlDotNet;
using System;
using System.Threading.Tasks;
using System.IO;

namespace LazyStack
{

    /// <summary>
    /// Read, Write, Generate AwsSettings configuration file for 
    /// a solution.
    /// </summary>
    public class AwsConfig
    {
        public static async Task<string> GenerateSettingsJsonAsync(
            string profileName, 
            string stackName, 
            bool includeLocalApis, 
            int localApiPort,
            ILogger logger)
        {
            if (logger == null)
                throw new Exception($"Error: Logger not configured.");

            if (string.IsNullOrEmpty(profileName))
                throw new Exception($"Error: No ProfileName provided");

            if (string.IsNullOrEmpty(stackName))
                throw new Exception($"Error: No StackName provided");

            if (includeLocalApis && localApiPort == 0)
                throw new Exception($"Error: IncludeLocalApis is true but LocalApiProt is 0");

            if (logger == null)
                throw new Exception($"Error: Logger is null");

            await logger.InfoAsync($"Generating Settings Json for AwsStack \"{stackName}\"");

            var sharedCredentialsFile = new SharedCredentialsFile(); // AWS finds the shared credentials store for us
            CredentialProfile profile = null;
            if (!sharedCredentialsFile.TryGetProfile(profileName, out profile))
                throw new Exception($"Error: Aws Profile \"{profileName}\" not found in shared credentials store.");

            AWSCredentials creds = null;
            if (!AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedCredentialsFile, out creds))
                throw new Exception($"Error: Could not get AWS Credentials using specified profile \"{profileName}\".");

            var awsSettings = new AwsSettings();
            awsSettings.StackName = stackName;

            try
            {
                // Get Original Template
                var cfClient = new AmazonCloudFormationClient(creds);
                var getTemplateRequestOriginal = new GetTemplateRequest()
                {
                    StackName = stackName
                    , TemplateStage = Amazon.CloudFormation.TemplateStage.Original
                    
                };

                var templateReponse = cfClient.GetTemplateAsync(getTemplateRequestOriginal).GetAwaiter().GetResult();
                //var templateBodyIndex = templateReponse.StagesAvailable.IndexOf("Original");
                var templateBody = templateReponse.TemplateBody; // Original is in yaml form
                //var tmplYaml = new StringReader(new YamlDotNet.Serialization.SerializerBuilder().Build().Serialize(templateBody));
                var tmplYaml = new StringReader(templateBody);
                var templYamlObj = new YamlDotNet.Serialization.DeserializerBuilder().Build().Deserialize(tmplYaml);
                templateBody = new YamlDotNet.Serialization.SerializerBuilder().JsonCompatible().Build().Serialize(templYamlObj);
                var jTemplateObjOriginal = JObject.Parse(templateBody);


                // Get Processed Template
                var getTemplateRequestProcessed = new GetTemplateRequest()
                {
                    StackName = stackName,
                    TemplateStage = Amazon.CloudFormation.TemplateStage.Processed
                };

                templateReponse = cfClient.GetTemplateAsync(getTemplateRequestProcessed).GetAwaiter().GetResult();
                //var templateBodyIndex = templateReponse.StagesAvailable.IndexOf("Original");
                templateBody = templateReponse.TemplateBody;
                var jTemplateObjProcessed = JObject.Parse(templateBody);

                // Get Stack Resources
                var describeStackResourcesRequest = new DescribeStackResourcesRequest() { StackName = stackName };
                var describeStackResourcesResponse = await cfClient.DescribeStackResourcesAsync(describeStackResourcesRequest);

                if (describeStackResourcesResponse.StackResources.Count == 0)
                    throw new Exception($"Error: No resources found for specified stack.");

                // Extract region from StackId ARN -- "arn:aws:cloudformation:us-east-1:..."
                var stackIdParts = describeStackResourcesResponse.StackResources[0].StackId.Split(':');
                awsSettings.Region = stackIdParts[3];

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
                        case "AWS::ApiGatewayV2::Api":
                            var httpApi = new AwsSettings.Api();
                            awsSettings.ApiGateways.Add(resource.LogicalResourceId, httpApi);
                            httpApi.Id = resource.PhysicalResourceId;
                            httpApi.Type = "HttpApi";
                            try
                            {
                                var apiName = resource.LogicalResourceId;
                                var HttpApiSecureAuthType = (string)jTemplateObjProcessed["Resources"][apiName]["Properties"]["Body"]["components"]["securitySchemes"]["OpenIdAuthorizer"]["type"];
                                if (HttpApiSecureAuthType.Equals("oauth2"))
                                    httpApi.SecurityLevel = AwsSettings.SecurityLevel.JWT;
                                else
                                    httpApi.SecurityLevel = AwsSettings.SecurityLevel.None;

                                // Note that  the processed template moves the stagename into the AWS::ApiGateway::Stage resource
                                httpApi.Stage = (string)jTemplateObjOriginal["Resources"][apiName]["Properties"]["StageName"];


                            }
                            catch
                            {
                                httpApi.SecurityLevel = AwsSettings.SecurityLevel.None;
                            }
                            break;
                        case "AWS::ApiGateway::RestApi":
                            var restApi = new AwsSettings.Api();
                            awsSettings.ApiGateways.Add(resource.LogicalResourceId, restApi);
                            restApi.Id = resource.PhysicalResourceId;
                            restApi.Type = "Api";
                            try
                            {
                                var apiName = resource.LogicalResourceId;
                                var apiAuthSecurityType = (string)jTemplateObjProcessed["Resources"][apiName]["Properties"]["Body"]["securityDefinitions"]["AWS_IAM"]["x-amazon-apigateway-authtype"];
                                if (apiAuthSecurityType.Equals("awsSignv4"))
                                    restApi.SecurityLevel = AwsSettings.SecurityLevel.AwsSignatureVersion4;
                                else
                                    restApi.SecurityLevel = AwsSettings.SecurityLevel.None;
                                restApi.Stage = (string)jTemplateObjOriginal["Resources"][apiName]["Properties"]["StageName"];
                            }
                            catch
                            {
                                restApi.SecurityLevel = AwsSettings.SecurityLevel.None;
                            }
                            break;
                    }

                if(includeLocalApis)
                {
                    awsSettings.LocalApis.Add("Local", new AwsSettings.LocalApi() { Host = "localhost", Scheme = "https", Port = localApiPort });
                    awsSettings.LocalApis.Add("LocalAndroid", new AwsSettings.LocalApi() { Host = "10.0.2.2", Scheme = "https", Port = localApiPort });
                }

                return awsSettings.BuildJsonWrapped();
            }
            catch (Exception e)
            {
                await logger.InfoAsync($"Error: {e.Message}");
                await logger.InfoAsync($"Warning: Stack \"{stackName}\" not found by CloudFormation. Has it been published?");
            }
            return null;
        }

    }
}
