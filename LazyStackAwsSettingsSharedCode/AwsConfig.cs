using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Newtonsoft.Json.Linq;
using LazyStackBase;

namespace LazyStackAwsSettings
{

    //public class MethodMapItem
    //{
    //    public string Name { get; set; }
    //    public string ApiGateway { get; set; }
    //}

    /// <summary>
    /// Read, Write, Generate AwsSettings configuration file for 
    /// a solution.
    /// </summary>
    public class AwsConfig
    {

        public static async Task<string> GenerateSettingsJsonAsync(
            string profileName,
            string stackName)
        {
            //var awsSettings = await GetAwsSettings(profileName, stackName);
            var awsSettings = await GetAwsSettings(profileName, stackName);
            var awsSettingsText = Newtonsoft.Json.JsonConvert.SerializeObject(awsSettings, Newtonsoft.Json.Formatting.Indented);
            return awsSettingsText;
        }

        /// <summary>
        /// LoadCredentials works with IAM profiles and SSO profiles
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static async Task<(AWSCredentials creds, RegionEndpoint region)> LoadCredentials(string profileName)
        {
            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetProfile(profileName, out var profile))
                throw new Exception($"Error: Aws Profile \"{profileName}\" not found in shared credentials store.");

            var region = profile.Region;

            AWSCredentials credentials;
            if (!chain.TryGetAWSCredentials(profileName, out credentials))
                throw new Exception($"Error: Could not get AWS Credentials using specified profile \"{profileName}\".");

            // If using SSO profile, set up the SooVerificationCallback
            if (!string.IsNullOrEmpty(profile.Options.SsoSession))
            {
                var ssoCredentials = credentials as SSOAWSCredentials;
                ssoCredentials.Options.ClientName = "LazyStack CLI";
                ssoCredentials.Options.SsoVerificationCallback = args =>
                {
                    throw new Exception("Your SSO credentials have expried. Please login to your SSO portal. ex: aws sso login --profile my-sso-profile");
                };
            }
            return await Task.FromResult((credentials, region));
        }

        public static async Task<LzClientConfig> GetAwsSettings(string profileName, string stackName)
        {
            if (string.IsNullOrEmpty(profileName))
                throw new Exception($"Error: No ProfileName provided");

            if (string.IsNullOrEmpty(stackName))
                throw new Exception($"Error: No StackName provided");

            AWSCredentials creds;
            RegionEndpoint profileRegion;
            try { (creds, profileRegion) = await LoadCredentials(profileName); }
            catch (Exception ex) { throw new Exception(ex.Message); }

            AmazonCloudFormationClient cfClient = new AmazonCloudFormationClient(creds, profileRegion);

            // Read the stack to get information we won't get by reading the resources directly.
            // Note that this file can get pretty big, but since we rarely run this routine, performance
            // isn't a consideration.
            var getTemplateRequest = new GetTemplateRequest()
            {
                StackName = stackName,
                TemplateStage = Amazon.CloudFormation.TemplateStage.Processed
            };
            var templateResponse = await cfClient.GetTemplateAsync(getTemplateRequest);
            var templateBody = templateResponse.TemplateBody;
            var template = JObject.Parse(templateBody);

            string nextToken = null;
            int resourceCount = 0;

            JObject jsonResources = new JObject();
            do
            {
                var listStackResourcesRequest = new ListStackResourcesRequest() { StackName = stackName, NextToken = nextToken };
                var listStackResourcesResponse = await cfClient.ListStackResourcesAsync(listStackResourcesRequest);
                nextToken = listStackResourcesResponse.NextToken;
                foreach (var resource in listStackResourcesResponse.StackResourceSummaries)
                {
                    string httpId = string.Empty;
                    string httpStage = string.Empty;
                    resourceCount++;
                    switch (resource.ResourceType)
                    {
                        case "AWS::Cognito::UserPool":
                        case "AWS::Cognito::IdentityPool":
                        case "AWS::Cognito::UserPoolClient":
                        case "AWS::SQS::Queue":
                        case "AWS::Events::EventsBus":
                        case "AWS::StepFunctions::StateMachine":
                        case "AWS::CodeBuild::Project":
                        case "AWS::DynamoDB::Table":
                        case "AWS::ApiGatewayV2::Api":
                        case "AWS::ApiGateway::RestApi":
                        case "AWS::ApiGatewayV2::Stage":
                        case "AWS::ApiGateway::Stage":
                            var jobj = JObject.FromObject(resource);
                            jobj.Remove("DriftInformation");
                            jobj.Remove("ResourceStatus");
                            jobj.Remove("ResourceStatusReason");
                            jobj.Remove("ModuleInfo");
                            jobj.Remove("LastUpdatedTimestamp");
                            jobj.Remove("LogicalResourceId");
                            jsonResources[resource.LogicalResourceId] = jobj;
                            break;
                        default:
                            break;
                    }
                }
            } while (nextToken != null);


            var stsClient = new AmazonSecurityTokenServiceClient(profileRegion);
            var getCallerIdentityRequest = new GetCallerIdentityRequest();
            GetCallerIdentityResponse getCallerIdentityResponse = await stsClient.GetCallerIdentityAsync(getCallerIdentityRequest);
            string accountId = getCallerIdentityResponse.Account;
            var regionName = profileRegion.SystemName;
            var clientConfig = new LzClientConfig() { StackName = stackName, Account = accountId, Profile = profileName, Region = regionName };
            clientConfig.Resources = jsonResources; 
            clientConfig.DefaultService = "";
            var service = new LzService() { Auth = stackName };
            clientConfig.Services.Add("AWS", service);
            var defaultAuth = new JObject();
            defaultAuth["Type"] = "Cognito";
            defaultAuth["Region"] = regionName;
            clientConfig.Authenticators.Add(stackName, defaultAuth);

            // Generate ResourceReferences 
            // We make simplifying assumptions required to use LzHttpClient in the default fashion 
            // Only one Stage is used in the stack and the stage "name" is the same for all types of ApiGateways used
            // Only one Cognito Identity pool is used in the stack
            // Only one Cognito User pool is used in the stack
            // Only one Cognito Client Id is used in the stack
            // We always use https

            // Get Stage Info - last entry wins (but they should all be the same)
            string stageName = string.Empty;
            foreach (var resource in jsonResources.Properties())
            {
                var resourceName = resource.Name;
                var resourceType = (string)resource.Value["ResourceType"];
                switch(resourceType)
                {
                    case "AWS::ApiGatewayV2::Stage":
                        stageName = (string)resource.Value["PhysicalResourceId"];
                        break;
                    case "AWS::ApiGateway::Stage":
                        stageName = (string)resource.Value["PhysicalResourceId"];
                        break;
                    default:
                        break;
                }
            }

            // Get the resources we use for Service.Resources["AWS"]
            foreach(var resource in jsonResources.Properties())
            {
                var resourceName = resource.Name;
                var resourceType = (string)resource.Value["ResourceType"];
                var resourceId = (string)resource.Value["PhysicalResourceId"];
                switch (resourceType)
                {
                    case "AWS::Cognito::IdentityPool":
                        defaultAuth["IdentityPoolId"] = resourceId;
                        break;
                    case "AWS::Cognito::UserPoolClient":
                        defaultAuth["UserPoolClientId"] = resourceId;
                        break;
                    case "AWS::Cognito::UserPool":
                        defaultAuth["UserPoolId"] = resourceId;
                        break;
                    case "AWS::ApiGatewayV2::Api":
                        var httpApi = new JObject();
                        httpApi["ResourceType"] = resourceType;
                        httpApi["Id"] = resourceId;
                        try
                        {
                            var HttpApiSecureAuthType = (string)template["Resources"][resourceName]["Properties"]["Body"]["components"]["securitySchemes"]["OpenIdAuthorizer"]["type"];
                            if (HttpApiSecureAuthType.Equals("oauth2"))
                                httpApi["SecurityLevel"] = (int)AwsSettings.SecurityLevel.JWT;
                            else
                                httpApi["SecurityLevel"] = (int)AwsSettings.SecurityLevel.None;
                        }
                        catch
                        {
                            httpApi["SecurityLevel"] = (int)AwsSettings.SecurityLevel.None;
                        }
                        httpApi["Url"] = $"https://{httpApi["Id"]}.execute-api.{regionName}.amazonaws.com/{stageName}";
                        service.Resources.Add(resourceName, httpApi);
                        break;
                    case "AWS::ApiGateway::RestApi":
                        var restApi = new JObject();
                        restApi["ResourceType"] = resourceType;
                        restApi["Id"] = resourceId;
                        try
                        {
                            var apiAuthSecurityType = (string)template["Resources"][resourceName]["Properties"]["Body"]["securityDefinitions"]["AWS_IAM"]["x-amazon-apigateway-authtype"];
                            if (apiAuthSecurityType.Equals("awsSigv4"))
                                restApi["SecurityLevel"] = (int)AwsSettings.SecurityLevel.AwsSignatureVersion4;
                            else
                                restApi["SecurityLevel"] = (int)AwsSettings.SecurityLevel.None;
                        }
                        catch
                        {
                            restApi["SecurityLevel"] = (int)AwsSettings.SecurityLevel.None;
                        }

                        restApi["Url"] = $"https://{restApi["Id"]}.execute-api.{regionName}.amazonaws.com/{stageName}";
                        service.Resources.Add(resourceName, restApi);
                        break;
                }
            }

            // Todo: Handle CloudFront services
            // This may take some noodling so I'm waiting until I get the PetStore demo up on a cloud service to test 
            // some ideas. 


            return clientConfig;
        }

        public static async Task<string> GenerateMethodMapJsonAsync(
            string profileName,
            string stackName)
        {
            if (string.IsNullOrEmpty(profileName))
                throw new Exception($"Error: No ProfileName provided");

            if (string.IsNullOrEmpty(stackName))
                throw new Exception($"Error: No StackName provided");

            var sharedCredentialsFile = new SharedCredentialsFile(); // AWS finds the shared credentials store for us
            CredentialProfile profile = null;
            if (!sharedCredentialsFile.TryGetProfile(profileName, out profile))
                throw new Exception($"Error: Aws Profile \"{profileName}\" not found in shared credentials store.");

            AWSCredentials creds = null;
            if (!AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedCredentialsFile, out creds))
                throw new Exception($"Error: Could not get AWS Credentials using specified profile \"{profileName}\".");

            var awsSettings = new AwsSettings();
            if (!awsSettings.ContainsKey("StackName"))
                awsSettings.Add("StackName", stackName);
            else
                awsSettings["StackName"] = stackName;

            // Get Original Template
            var cfClient = new AmazonCloudFormationClient(creds);
            var getTemplateRequestOriginal = new GetTemplateRequest()
            {
                StackName = stackName,
                TemplateStage = Amazon.CloudFormation.TemplateStage.Original
            };

            var templateReponse = cfClient.GetTemplateAsync(getTemplateRequestOriginal).GetAwaiter().GetResult();
            //var templateBodyIndex = templateReponse.StagesAvailable.IndexOf("Original");
            var templateBody = templateReponse.TemplateBody; // Original is in yaml form
            //var tmplYaml = new StringReader(new YamlDotNet.Serialization.SerializerBuilder().Build().Serialize(templateBody));
            var tmplYaml = new StringReader(templateBody);
            var templYamlObj = new YamlDotNet.Serialization.DeserializerBuilder().Build().Deserialize(tmplYaml);
            templateBody = new YamlDotNet.Serialization.SerializerBuilder().JsonCompatible().Build().Serialize(templYamlObj);
            var jTemplateObjOriginal = JObject.Parse(templateBody);

            // Get all Stack Resources
            string nextToken = null;
            bool foundResources = false;
            var methodMap = new Dictionary<string, string>();
            do
            {
                var listStackResourcesRequest = new ListStackResourcesRequest() { StackName = stackName, NextToken = nextToken };
                var listStackResourcesResponse = await cfClient.ListStackResourcesAsync(listStackResourcesRequest);
                nextToken = listStackResourcesResponse.NextToken;

                foreach (var resource in listStackResourcesResponse.StackResourceSummaries)
                {
                    switch (resource.ResourceType)
                    {
                        case "AWS::Lambda::Function":
                            foundResources = true;
                            var funcName = resource.LogicalResourceId;
                            var lambdaEvents = jTemplateObjOriginal["Resources"][funcName]["Properties"]["Events"].Children();
                            foreach (JToken le in lambdaEvents)
                            {
                                var jObject = new JObject(le);
                                var name = jObject.First.First.Path;
                                var type = jObject[name]["Type"].ToString();
                                var apiId = string.Empty;

                                if (type.Equals("HttpApi"))
                                    apiId = jObject[name]["Properties"]["ApiId"]["Ref"].ToString();

                                else if (type.Equals("Api"))
                                    apiId = jObject[name]["Properties"]["RestApiId"]["Ref"].ToString();

                                if (!string.IsNullOrEmpty(apiId))
                                    methodMap.Add(name + "Async", apiId);
                            }
                            break;
                    }
                }
            } while (nextToken != null);

            if (!foundResources)
                throw new Exception($"Error: No Lambda resources found for specified stack.");

            var result = $"{{\"MethodMap\": {Newtonsoft.Json.JsonConvert.SerializeObject(methodMap, Newtonsoft.Json.Formatting.Indented)}}}";
            return result;

        }
    }
}
