using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using LazyStackAuth;

namespace LazyStackAuthTest
{
    public class AuthRESTClient
    {
        public AuthRESTClient(AwsRestApiGateway awsApi, IAuthProvider authProvider)
        {
            this.awsApi = awsApi;
            this.authProvider = authProvider;
        }

        readonly AwsRestApiGateway awsApi;
        readonly IAuthProvider authProvider;

        // Very simple app client to test round trip call to lambda through rest apigateway
        public async Task<HttpStatusCode> ExecAsync(
            System.Net.Http.HttpMethod method,
            string path)
        {

            var uriBuilder = new UriBuilder(awsApi.HttpClient.BaseAddress);
            if (!string.IsNullOrEmpty(awsApi.Stage) && !awsApi.UseLocal)
                path = "/" + awsApi.Stage + "/" + path;
            uriBuilder.Path = path;

            var requestMessage = new HttpRequestMessage(method, uriBuilder.Uri);

            var completionOption = HttpCompletionOption.ResponseContentRead;
            var cancelationtoken = CancellationToken.None;

            HttpResponseMessage response = null;
            if (!awsApi.IsSecure)
            {
                response = await awsApi.HttpClient.SendAsync(
                    requestMessage,
                    completionOption,
                    cancelationtoken);
            }
            else
            if (awsApi.Type.Equals("Api", StringComparison.OrdinalIgnoreCase))
            {
                // Use full request signing process
                // Get Temporary ImmutableCredentials : AccessKey, SecretKey, Token
                var icreds = await authProvider.Credentials.GetCredentialsAsync(); // This will refresh creds if necessaary
                response = await awsApi.HttpClient.SendAsync(
                    requestMessage,
                    completionOption,
                    cancelationtoken,
                    awsApi.RegionEndpointStr,
                    "execute-api",
                    icreds);
            }
            else if (awsApi.Type.Equals("HttpApi", StringComparison.OrdinalIgnoreCase))
            {
                // Use JWT Token signing process
                requestMessage.Headers.Add("Authorization", authProvider.CognitoUser.SessionTokens.IdToken);
                response = await awsApi.HttpClient.SendAsync(
                    requestMessage,
                    completionOption,
                    cancelationtoken);
            }

            return response.StatusCode;

        }
    }
}
