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


namespace LazyStack
{
    public class LzEnvironment
    {
        // Add environments in LazyStack.yaml
        public string ProfileName { get; set; } // required
        public string RegionName { get; set; } // required
        public string StackName { get; set; } // Defaults to SolutionName<envName>
        public string Stage { get; set; } // Defaults to envName
        public string Domain { get; set; } = "amazonaws.com";
        public string UriCodeTarget { get; set; } = "Debug";
        public string UriCodePlatform { get; set; } = "netcoreapp3.1";
        public bool IncludeLocalApis { get; set; } = false;
    }
    
    /// <summary>
    /// Read, Write, Generate AwsSettings configuration file for 
    /// a solution.
    /// </summary>
    public class AwsConfig
    {

        /// <summary>
        /// Generate a AWS settings file for specified environment.
        /// </summary>
        /// <param name="environmentName"></param>
        /// <returns></returns>
        public static async Task GenerateSettingsFileAsync(string solutionRootFolderPath, string environmentName, ILogger logger)
        {
            if(logger == null)
                throw new Exception($"Error: Logger not configured.");

            if(string.IsNullOrEmpty(solutionRootFolderPath))
                throw new Exception($"Error: No solutionRootFolderPath provided");

            if(string.IsNullOrEmpty(environmentName))
                throw new Exception($"Error: No environmentName provided");
           
            await logger.InfoAsync($"Generating Settings for environment \"{environmentName}\"");

            // Read template LazyStack Environments
            var executingAssemblyFilePath = Assembly.GetExecutingAssembly().Location;
            var executingAssemblyFolderPath = Path.GetDirectoryName(executingAssemblyFilePath);
            var lazyStackTemplateFolderPath = Path.Combine(executingAssemblyFolderPath, "Templates");
            if (!Directory.Exists(lazyStackTemplateFolderPath))
                throw new System.Exception("LazyStack Templates folder Missing. Check installation.");
            var templateLazyStackFilePath = Path.Combine(lazyStackTemplateFolderPath, "LazyStack.yaml");
            if (!File.Exists(templateLazyStackFilePath))
                throw new Exception($"Error: No LazyStack.yaml found in LazyStack installtion template folder");

            var templateLazyStackFileText = File.ReadAllText(templateLazyStackFilePath);
            var templateLazyStackFileRoot = SolutionModel.ParseYamlText(templateLazyStackFileText);

            // Read LazyStack Environments
            var lazyStackFilePath = Path.Combine(solutionRootFolderPath, "LazyStack.yaml");
            if (!File.Exists(lazyStackFilePath))
                    throw new Exception($"Error: No LazyStack.yaml found in solution folder");

            var lazyStackFileText = File.ReadAllText(lazyStackFilePath);
            var lazyStackFileRoot = SolutionModel.ParseYamlText(lazyStackFileText);

            // Merge right into left
            var mergedLazyStackFileRoot = SolutionModel.MergeNode(templateLazyStackFileRoot, lazyStackFileRoot) as YamlMappingNode;
            var inspectMerge = SolutionModel.YamlNodeToText(mergedLazyStackFileRoot);

            YamlNode outNode;
            var deserializer = new DeserializerBuilder().Build();

            // Get Environments
            YamlMappingNode environmentsNode = null;
            if (SolutionModel.GetNamedProperty(mergedLazyStackFileRoot, "Environments", out outNode))
                environmentsNode = outNode as YamlMappingNode;

            if (environmentsNode == null)
                throw new Exception($"Error: No Environments defined in LazyStack.yaml");

            var environments = deserializer.Deserialize<Dictionary<string, LzEnvironment>>(SolutionModel.YamlNodeToText(environmentsNode));

            if(environments.Count == 0)
                throw new Exception($"Error: No Environments defined in LazyStack.yaml");

            if (!environments.TryGetValue(environmentName, out LzEnvironment environment))
                throw new Exception($"Error: \"{environmentName}\" not found in LazyStack Environments");

            // Get optional LocalApis
            YamlMappingNode localApisNode = null;
            if (SolutionModel.GetNamedProperty(mergedLazyStackFileRoot, "LocalApis", out outNode))
                localApisNode = outNode as YamlMappingNode;

            var localApis = new Dictionary<string, AwsSettings.LocalApi>();
            if(localApisNode != null)
                localApis = deserializer.Deserialize<Dictionary<string, AwsSettings.LocalApi>>(SolutionModel.YamlNodeToText(localApisNode));

            // Load SolutionModelAwsSettings.json file
            var solutionModelAwsSettingsFilePath = Path.Combine(solutionRootFolderPath, "SolutionModelAwsSettings.json");
            if (!File.Exists(solutionModelAwsSettingsFilePath))
                throw new Exception($"Error: \"SolutionModelAwsSettings.json\" not found in solution folder");
            var awsSettingsText = File.ReadAllText(solutionModelAwsSettingsFilePath);
            var awsSettings = JsonConvert.DeserializeObject<AwsSettings>(awsSettingsText);

            // Assign some AwsSettings values from selected environment
            foreach(var apiGateway in awsSettings.ApiGateways.Values)
            {
                apiGateway.Host = environment.Domain;
                apiGateway.Stage = environment.Stage;
            }
            var regionName = environment.RegionName;
            awsSettings.Region = regionName;

            // Use Cloud Formation to get the rest of the configuration values
            var regions = new Dictionary<string, RegionEndpoint>();
            foreach (var regionItem in RegionEndpoint.EnumerableAllRegions)
                regions.Add(regionItem.SystemName, regionItem);

            // Validate Aws Region
            if (!regions.ContainsKey(regionName))
                throw new Exception($"Error: AWS Region \"{regionName}\" specified in environment \"{environmentName}\" not found.");
            var region = regions[environment.RegionName];

            var profileName = environment.ProfileName;
            var sharedCredentialsFile = new SharedCredentialsFile(); // AWS finds the shared credentials store for us
            CredentialProfile profile = null;
            if (!sharedCredentialsFile.TryGetProfile(profileName, out profile))
                throw new Exception($"Error: Aws Profile \"{profileName}\" not found in shared credentials store.");

            AWSCredentials creds = null;
            if (!AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedCredentialsFile, out creds))
                throw new Exception($"Error: Could not get AWS Credentials using specified profile.");

            var stackName = environment.StackName;
            awsSettings.StackName = stackName;

            try
            {
                var cfClient = new AmazonCloudFormationClient(creds);
                var describeStackResourcesRequest = new DescribeStackResourcesRequest() { StackName = stackName };
                var describeStackResourcesResponse = await cfClient.DescribeStackResourcesAsync(describeStackResourcesRequest);

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
                            if(awsSettings.ApiGateways.TryGetValue(resource.LogicalResourceId, out AwsSettings.Api httpApi))
                            {
                                httpApi.Id = resource.PhysicalResourceId;
                                httpApi.Type = "HttpApi";
                            }

                            break;
                        case "AWS::ApiGateway::RestApi":
                            if (awsSettings.ApiGateways.TryGetValue(resource.LogicalResourceId, out AwsSettings.Api restApi))
                            {
                                restApi.Id = resource.PhysicalResourceId;
                                restApi.Type = "Api";
                            }
                            break; 
                    }

                if (environment.IncludeLocalApis && localApis.Count > 0)
                    awsSettings.LocalApis = localApis;

                var settingsFileText = awsSettings.BuildJsonWrapped();
                File.WriteAllText(Path.Combine(solutionRootFolderPath,$"{environmentName}.AwsSettings.json"), settingsFileText);
            }
            catch
            {
                await logger.InfoAsync($"Warning: Stack \"{stackName}\" in environment \"{environmentName}\" not found by CloudFormation. Has it been published?");
            }
        }
    }
}
