using System;
using System.Collections.Generic;
using System.Text;

namespace LazyStackAwsSettings
{
    /// <summary>
    /// This is a preliminary implementation to generate a aws-exports.js in the form 
    /// required by AWS Amplfiy clients. It will be extended with more functionality when 
    /// we add AWS Amplify JS client library generation. 
    /// </summary>
    public class AwsAmplifyConfig
    {
        public AwsAmplifyConfig(AwsSettings awsSettings)
        {
            Auth = new auth
            {
                identityPoolId = awsSettings.IdentityPoolId,
                region = awsSettings.Region,
                identityPoolRegion = awsSettings.Region,
                userPoolId = awsSettings.UserPoolId,
                userPoolWebClientId = awsSettings.ClientId,
                mandatorySignIn = false
            };

            API = new ApiSpec();
            API.endpoints = new endpointSpec[awsSettings.ApiGateways.Count];

            int i = 0;
            foreach (var kvp in awsSettings.ApiGateways)
            {
                var api = kvp.Value;
                var endpoint = new endpointSpec();
                endpoint.name = kvp.Key;
                var awshost = $"{api.Id}.{api.Service}.{awsSettings.Region}.{api.Host}";

                var uriBuilder = (api.Port == 443)
                    ? new UriBuilder(api.Scheme, awshost)
                    : new UriBuilder(api.Scheme, awshost, api.Port);

                var path = (!string.IsNullOrEmpty(api.Stage))
                    ? "/" + api.Stage 
                        : "";

                endpoint.endpoint = new Uri(uriBuilder.Uri, path).AbsoluteUri;
                endpoint.custom_header = $"#HeadersStatement-{api.SecurityLevel.ToString()}#";
                //switch(api.SecurityLevel)
                //{
                //    case AwsSettings.SecurityLevel.None:
                //        break;
                //    case AwsSettings.SecurityLevel.JWT:
                //        endpoint.custom_header = @"async () => { return { Authorization : `Bearer ${(await Auth.currentSession()).getIdToken.getJwtToken()}`}}";
                //        break;
                //    case AwsSettings.SecurityLevel.AwsSignatureVersion4:
                //        endpoint.custom_header = @"async () => { return { LzIdentity : `${(await Auth.currentSession()).getIdToken.getJwtToken()}`}}";
                //        break;
                //}

                API.endpoints[i++] = endpoint;
            }
        }

        public class endpointSpec
        {
            public string name { get; set; }
            public string endpoint { get; set; }
            public string custom_header { get; set; }
        }
        public class auth
        {
            public string identityPoolId { get; set; }
            public string region { get; set; }
            public string identityPoolRegion { get; set; }
            public string userPoolId { get; set; }
            public string userPoolWebClientId { get; set; }
            public bool mandatorySignIn { get; set; }
            public string authenticationFlowType { get; set; } = "USER_SRP_AUTH";
        }
        public class ApiSpec
        {
            public endpointSpec[] endpoints { get; set; }
        }

        // Properties
        public auth Auth { get; set; }
        public ApiSpec API { get; set; }

        // Methods
        public string BuildJson()
        {
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented); 
            // Update custom_header elements
            result = result.Replace("\"custom_header\": \"#HeadersStatement-JWT#\"",
                "custom_header: async () => { return { Authorization : (await Auth.currentSession()).getIdToken().getJwtToken()}; }");

            result = result.Replace("\"custom_header\": \"#HeadersStatement-AwsSignatureVersion4#\"",
                "custom_header: async () => { return { LzIdentity : (await Auth.currentSession()).getIdToken().getJwtToken()}; }");

            result = result.Replace("\"custom_header\": \"#HeadersStatement-None#\"", "");

            return result;
        }

        public string BuildAwsExportsJs(string configBody)
        {
            var result =
@"
import { Auth } from ""aws-amplify"";
const awsconfig = "
+ configBody 
+ @";
export default awsconfig;
";

            return result;
        }
        
    }

}
