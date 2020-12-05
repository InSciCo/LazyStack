using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace LazyStack
{

    public enum SecurityLevel
    {
        none,
        jwt,
        signed
    }

    public abstract class AwsApi
    {
        public AwsApi(AwsResource awsResource, SolutionModel solutionModel, string name)
        {
            this.AwsResource = awsResource;
            this.solutionModel = solutionModel;
            this.Name = name;
        }
        protected bool isConfigured;
        protected SolutionModel solutionModel;

        public string Name { get;  }
        public AwsResource AwsResource { get; }
        public List<AWSLambda> Lambdas { get; } = new List<AWSLambda>();
        public abstract string ProxyFunctionName { get; }
        public SecurityLevel SecurityLevel { get; set; }

        public abstract YamlMappingNode EventNode(string path, string httpOperation);
        public abstract SecurityLevel DiscoverSecurityLevel();
        
    }

    public class AwsApiRestApi : AwsApi 
    {
        public AwsApiRestApi(AwsResource awsResource, SolutionModel solutionModel, string name) : base(awsResource, solutionModel, name)
        {
        }

        public override string ProxyFunctionName { get; } = "APIGatewayProxyFunction";

        public override YamlMappingNode EventNode(string path, string httpOperation)
        {
            var newNode = new YamlMappingNode
            {
                { "Type", "Api" },
                { "Properties", new YamlMappingNode()
                    {
                        { "RestApiId", new YamlMappingNode()
                            {
                                {"Ref", AwsResource.Name }
                            }
                        },
                        { "Path", path },
                        { "Method", httpOperation.ToUpper() },
                        { "Auth", new YamlMappingNode()
                            {
                            { "InvokeRole", "NONE" }  
                            }   
                        }
                    }
                }
            };

            return newNode;
        }

        public override SecurityLevel DiscoverSecurityLevel()
        {
            if (SolutionModel.NamedPropertyExists(AwsResource.RootNode, $"Properties/Auth"))
            {
                SecurityLevel = SecurityLevel.signed; // Just assume signed for now. ToDo - allow for JWT and other security options
            }
            else SecurityLevel = SecurityLevel.none;

            return SecurityLevel;
        }
    }

    public class AwsApiHttpApi : AwsApi
    {
        public AwsApiHttpApi(AwsResource awsResource, SolutionModel solutionModel, string name) : base(awsResource, solutionModel, name)
        { 
        }

        public override string ProxyFunctionName { get; } = "APIGatewayHttpApiV2ProxyFunction";

        public override YamlMappingNode EventNode(string path, string httpOperation)
        {
            var newNode = new YamlMappingNode
            {
                { "Type", "HttpApi" },
                { "Properties", new YamlMappingNode()
                    {
                        { "ApiId", new YamlMappingNode()
                            {
                                {"Ref", AwsResource.Name }
                            }
                        },
                        { "Path", path },
                        { "Method", httpOperation.ToUpper() }
                    }
                }
            };

            return newNode;
        }

        public override SecurityLevel DiscoverSecurityLevel()
        {
            if (SolutionModel.NamedPropertyExists(AwsResource.RootNode, "Properties/Auth"))
            {
                SecurityLevel = SecurityLevel.jwt;
            }
            else SecurityLevel = SecurityLevel.none;

            return SecurityLevel;
        }

    }


}
