﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.CloudFormation.Internal;

using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using YamlDotNet;

namespace LazyStackAwsSettings
{

    public class MethodMapItem
    {
        public string Name { get; set; }
        public string ApiGateway { get; set; }
    }

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
            awsSettings.StackName = stackName;

            // Get Original Template
            AmazonCloudFormationClient cfClient = null;
            GetTemplateRequest getTemplateRequestOriginal = null;
            try
            {
                // Note the need to extract region from the profile! 
                cfClient = new AmazonCloudFormationClient(creds, profile.Region );
                getTemplateRequestOriginal = new GetTemplateRequest()
                {
                    StackName = stackName
                    , TemplateStage = Amazon.CloudFormation.TemplateStage.Original
                    
                };
            } catch (Exception ex)
            {

                throw new Exception($"Could not create AmazonCloudFormationClient");
            }
                
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
            string apiName = null;
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
                        apiName = resource.LogicalResourceId;
                        try
                        {
                            var HttpApiSecureAuthType = (string)jTemplateObjProcessed["Resources"][apiName]["Properties"]["Body"]["components"]["securitySchemes"]["OpenIdAuthorizer"]["type"];
                            if (HttpApiSecureAuthType.Equals("oauth2"))
                                httpApi.SecurityLevel = AwsSettings.SecurityLevel.JWT;
                            else
                                httpApi.SecurityLevel = AwsSettings.SecurityLevel.None;
                        }
                        catch
                        {
                            httpApi.SecurityLevel = AwsSettings.SecurityLevel.None;
                        }
                        httpApi.Stage = (string)jTemplateObjOriginal["Resources"][apiName]["Properties"]["StageName"];
                        break;
                    case "AWS::ApiGateway::RestApi":
                        var restApi = new AwsSettings.Api();
                        awsSettings.ApiGateways.Add(resource.LogicalResourceId, restApi);
                        restApi.Id = resource.PhysicalResourceId;
                        restApi.Type = "Api";
                        apiName = resource.LogicalResourceId;
                        try
                        {
                            var apiAuthSecurityType = (string)jTemplateObjProcessed["Resources"][apiName]["Properties"]["Body"]["securityDefinitions"]["AWS_IAM"]["x-amazon-apigateway-authtype"];
                            if (apiAuthSecurityType.Equals("awsSigv4"))
                                restApi.SecurityLevel = AwsSettings.SecurityLevel.AwsSignatureVersion4;
                            else
                                restApi.SecurityLevel = AwsSettings.SecurityLevel.None;
                        }
                        catch
                        {
                            restApi.SecurityLevel = AwsSettings.SecurityLevel.None;
                        }
                        restApi.Stage = (string)jTemplateObjOriginal["Resources"][apiName]["Properties"]["StageName"];
                        break;

                }

            return awsSettings.BuildJsonWrapped();
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
            awsSettings.StackName = stackName;

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

            // Get Stack Resources
            var describeStackResourcesRequest = new DescribeStackResourcesRequest() { StackName = stackName };
            var describeStackResourcesResponse = await cfClient.DescribeStackResourcesAsync(describeStackResourcesRequest);

            if (describeStackResourcesResponse.StackResources.Count == 0)
                throw new Exception($"Error: No resources found for specified stack.");

            var methodMap = new Dictionary<string, string>();
            foreach (var resource in describeStackResourcesResponse.StackResources)
                switch (resource.ResourceType)
                {
                    case "AWS::Lambda::Function":
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

            var result = $"{{\"MethodMap\": {Newtonsoft.Json.JsonConvert.SerializeObject(methodMap, Newtonsoft.Json.Formatting.Indented)}}}";
            return result;

        }
    }
}
