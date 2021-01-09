using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace LazyStackAuth
{
    
    public class LzHttpClient : ILzHttpClient
    {
        public LzHttpClient(IConfiguration appConfig, AuthProviderCognito authProvider, string localApiName = null) : 
#if DEBUG        
        this(appConfig, authProvider, new HttpClient(GetInsecureHandler()), localApiName) {}
#else
        this(appConfig, authprovider, new HttpClient(), localApiName) {}
#endif

        public LzHttpClient(IConfiguration appConfig, AuthProviderCognito authProvider, HttpClient httpClient, string localApiName = null)
        {
            this.httpClient = httpClient;
            this.localApiName = localApiName;
            this.awsSettings = appConfig.GetSection("Aws").Get<AwsSettings>();
            this.authProvider = authProvider;
        }
        
        readonly HttpClient httpClient;
        readonly AwsSettings awsSettings;
        readonly string localApiName = string.Empty;
        AuthProviderCognito authProvider;

        // Note: CallerMember is inserted as a literal by the compiler in the IL so there is no 
        // performance penalty for using it.
        public async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage, 
            HttpCompletionOption httpCompletionOption, 
            CancellationToken cancellationToken, 
            [CallerMemberName] string callerMemberName = null)
        {
            if(!awsSettings.MethodMap.TryGetValue(callerMemberName, out string apiGatewayName))
                throw new Exception($"Error: {callerMemberName} not found in AwsSettings MethodMap");

            if(!awsSettings.ApiGateways.TryGetValue(apiGatewayName, out AwsSettings.Api api))
                throw new Exception($"Error: {apiGatewayName} not found in AwsSettings ApiGateways dictionary");

            var securityLevel = api.SecurityLevel;

            if (!string.IsNullOrEmpty(localApiName))
            {
                if(!awsSettings.LocalApis.ContainsKey(localApiName))
                    throw new Exception($"Error: {localApiName} not found in AwsSettings");

                var localApi = awsSettings.LocalApis[localApiName];
               
                var uriBuilder = new UriBuilder(localApi.Scheme, localApi.Host, localApi.Port);
                uriBuilder.Path = requestMessage.RequestUri.ToString();
                requestMessage.RequestUri = uriBuilder.Uri;
            }
            else
            {
                var awshost = $"{api.Id}.{api.Service}.{awsSettings.Region}.{api.Host}";
               
                var uriBuilder = (api.Port == 443)
                    ? new UriBuilder(api.Scheme, awshost)
                    : new UriBuilder(api.Scheme, awshost, api.Port);

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
				case AwsSettings.SecurityLevel.None:
                    response = await httpClient.SendAsync(
                        requestMessage,
                        httpCompletionOption,
                        cancellationToken);
                    break;

				case AwsSettings.SecurityLevel.JWT:
                    // Use JWT Token signing process
                    requestMessage.Headers.Add("Authorization", authProvider.CognitoUser.SessionTokens.IdToken);
                    response = await httpClient.SendAsync(
                        requestMessage,
                        httpCompletionOption,
                        cancellationToken);
                    break; 

				case AwsSettings.SecurityLevel.AwsSignatureVersion4:
                    // Use full request signing process
                    // Get Temporary ImmutableCredentials :  AccessKey, SecretKey, Token
                    var iCreds = await authProvider.Credentials.GetCredentialsAsync(); // This will refresh immutable credentials if necessary
                    // Calling AwsSignatureVersion4 extension method -- this signs the request message
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
#if DEBUG        
        //https://docs.microsoft.com/en-us/xamarin/cross-platform/deploy-test/connect-to-local-web-services
        //Attempting to invoke a local secure web service from an application running in the iOS simulator 
        //or Android emulator will result in a HttpRequestException being thrown, even when using the managed 
        //network stack on each platform.This is because the local HTTPS development certificate is self-signed, 
        //and self-signed certificates aren't trusted by iOS or Android.
        public static HttpClientHandler GetInsecureHandler()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (cert.Issuer.Equals("CN=localhost"))
                        return true;
                    return errors == System.Net.Security.SslPolicyErrors.None;
                }
            };
            return handler;
        }
#endif
        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}