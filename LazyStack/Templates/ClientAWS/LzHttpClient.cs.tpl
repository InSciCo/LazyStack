using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

using Amazon.CognitoIdentity;
using Amazon.Extensions.CognitoAuthentication;
using __SDKProj__;

namespace __ProjName__
{
    public class LzHttpClient : ILzHttpClient
    {
        public LzHttpClient(AwsSettings awsSettings) : this(awsSettings, new HttpClient()) {}

        public LzHttpClient(AwsSettings awsSettings, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.awsSettings = awsSettings;
        }
        
        readonly HttpClient httpClient;
        readonly AwsSettings awsSettings;

        // Note: CallerMember is inserted as a literal by the compiler in the IL so there is no 
        // performance penalty for using it.
        public async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage, 
            HttpCompletionOption httpCompletionOption, 
            CancellationToken cancellationToken, 
            [CallerMemberName] string callerMemberName = null)
        {
            // Note: AwsSettings contains generated code that resolves the correct
            // api and securityLevel to use for each endpoint method
            var (api, securityLevel) = awsSettings.ResolveForMethod(callerMemberName);

            if (awsSettings.UseLocal)
            {
                var localScheme = (!string.IsNullOrEmpty(awsSettings.LocalScheme))
                    ? awsSettings.LocalScheme
                    : "https";

                var localHost = (!string.IsNullOrEmpty(awsSettings.LocalHost))
                    ? awsSettings.LocalHost
                    : "localhost";

                var localPort = (awsSettings.LocalPort != 0)
                    ? awsSettings.LocalPort
                    : 5001;  // Kestrel     

                var uriBuilder = new UriBuilder(localScheme, localHost, localPort);
                uriBuilder.Path = requestMessage.RequestUri.ToString();
                requestMessage.RequestUri = uriBuilder.Uri;
            }
            else
            {
                var scheme = (!string.IsNullOrEmpty(api.Scheme))
                    ? api.Scheme
                    : (!string.IsNullOrEmpty(awsSettings.DefaultScheme))
                        ? awsSettings.DefaultScheme
                        : "https";

                var host = (!string.IsNullOrEmpty(api.Host))
                    ? api.Host
                    : (!string.IsNullOrEmpty(awsSettings.DefaultHost))
                        ? awsSettings.DefaultHost
                        : "amazonaws.com";

                var port = (api.Port != 0)
                    ? api.Port
                    : (awsSettings.DefaultPort != 0)
                        ? awsSettings.DefaultPort
                        : 443;

                var service = (!string.IsNullOrEmpty(api.Service))
                    ? api.Service
                    : (!string.IsNullOrEmpty(awsSettings.DefaultService))
                        ? awsSettings.DefaultService
                        : "execute-api";

                var awshost = $"{api.Id}.{service}.{awsSettings.Region}.{host}";
               
                var uriBuilder = (port == 443)
                    ? new UriBuilder(scheme, awshost)
                    : new UriBuilder(scheme, awshost, port);

                var path = (!string.IsNullOrEmpty(api.Stage)) 
                    ? "/" + api.Stage + "/" + requestMessage.RequestUri.ToString()
                    : requestMessage.RequestUri.ToString();
                uriBuilder.Path = path;
                requestMessage.RequestUri = uriBuilder.Uri;
            }

            HttpResponseMessage response = null;
            // Note: If the call is being made against a local host then that host
            // will by default not pay any attention to the authorization header attached by 
            // the JWT or AwsSignatureVersion4 cases below. We assign the Headers
            // anyway in case you want to implement handling these headers 
            // in your local host for testing or any other purpose.
            switch(securityLevel)
            {
				case SecurityLevel.None:
                    response = await httpClient.SendAsync(
                        requestMessage,
                        httpCompletionOption,
                        cancellationToken);
                    break;

				case SecurityLevel.JWT:
                    // Use JWT Token signing process
                    requestMessage.Headers.Add("Authorization", awsSettings.CognitoUser.SessionTokens.IdToken);
                    response = await httpClient.SendAsync(
                        requestMessage,
                        httpCompletionOption,
                        cancellationToken);
                    break; 

				case SecurityLevel.AwsSignatureVersion4:
                    // Use full request signing process
                    // Get Temporary ImmutableCredentials :  AccessKey, SecretKey, Token
                    var iCreds = await awsSettings.CognitoAwsCredentials.GetCredentialsAsync(); // This will refresh immutable credentials if necessaary
                    // Calling AwsSignatureVersion4 extension method -- this signes the request message
                    response = await httpClient.SendAsync(
                        requestMessage,
                        httpCompletionOption,
                        cancellationToken,
                        awsSettings.Region,
                        api.Service,
                        iCreds);
                    break;
            }
            return response;
        }
        
        public void Dispose()
        {
            httpClient.Dispose();
        }

    }
}
