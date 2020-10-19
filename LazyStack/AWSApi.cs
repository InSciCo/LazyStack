using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace LazyStack
{
    public abstract class AwsApi
    {
        public AwsApi(AwsResource awsResource, SolutionModel solutionModel)
        {
            this.AwsResource = awsResource;
            this.solutionModel = solutionModel;
        }
        protected bool isConfigured;
        protected SolutionModel solutionModel;

        public AwsResource AwsResource { get; }
        public List<AWSLambda> Lambdas { get; } = new List<AWSLambda>();
        public abstract string ProxyFunctionName { get; }
        public abstract YamlMappingNode EventNode(string path, string httpOperation);

    }

    public class AwsApiRestApi : AwsApi 
    {
        public AwsApiRestApi(AwsResource awsResource, SolutionModel solutionModel) : base(awsResource, solutionModel)
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
    }

    public class AwsApiHttpApi : AwsApi
    {
        public AwsApiHttpApi(AwsResource awsResource, SolutionModel solutionModel) : base(awsResource, solutionModel)
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

    }


}
