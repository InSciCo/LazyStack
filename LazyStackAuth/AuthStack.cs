using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace LazyStackAuth
{
    /// <summary>
    /// Read configuration to create AwsRestgApiGateway entries
    /// </summary>
    public class AuthStack
    {
        public AuthStack(IConfiguration appConfig)
        {
            regionEndpointStr = appConfig["Aws:Region"];
            clientId = appConfig["Aws:ClientId"];
            userPoolId = appConfig["Aws:UserPoolId"];
            identityPoolId = appConfig["Aws:IdentityPoolId"];
            var useLocal = false;
            Uri localUri = null;
            
            if(appConfig.GetSection("App").Exists())
            {
                var useLocalStr = appConfig["App:UseLocalUri"];
                useLocal = useLocalStr.Equals("true");
                localUri = new Uri(appConfig["App:LocalUri"]);
            }


            var apiGatewaysSection = appConfig.GetSection("Aws:ApiGateways");
            foreach (var item in apiGatewaysSection.GetChildren())
                if (!string.IsNullOrEmpty(item["Id"]))
                    switch (item["Type"])
                    {
                        case "Api":
                        case "HttpApi":
                            AwsRestApiGateways.Add(item["Name"],
                                new AwsRestApiGateway(
                                    name: item["Name"],
                                    id: item["Id"],
                                    type: item["Type"],
                                    regionEndpointStr: regionEndpointStr,
                                    stage: item["Stage"],
                                    isSecure: item["Secure"].Equals("true",StringComparison.OrdinalIgnoreCase),
                                    useLocal: useLocal,
                                    localUri: localUri
                                ));
                            break;
                        default:
                            throw new Exception($"Unknown Api Type {item["Type"]} in Api {item["Name"]}");
                    }
        }

        private readonly string userPoolId;
        private readonly string identityPoolId;
        private readonly string clientId;
        private readonly string regionEndpointStr;
        private readonly bool useLocal;
        private readonly Uri localUri;

        public Dictionary<string, AwsRestApiGateway> AwsRestApiGateways { get; } = new Dictionary<string, AwsRestApiGateway>();

        public override string ToString()
        {
            var result = $"UserPoolId: {userPoolId}\nIdentityPoolId: {identityPoolId}\nClientId: {clientId}\n";
            foreach (var api in AwsRestApiGateways)
                result += api.ToString();
            return result;
        }
    }
}
