using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LazyStackAuth
{
    // This version of LzHttpClient has AWS Cognito dependencies. You may 
    // create similar classes that satisfy the ILzHttpClient interface to 
    // use a different authentication service.
    public class LzHttpClient : ILzHttpClient
    {
        public LzHttpClient(
            IConfiguration appConfig,
            IAuthProvider authProvider,  // IAuthProviderCognito inherits IAuthProvider
            string localApiName = null) :
#if DEBUG
        this(appConfig, authProvider, new HttpClient(GetInsecureHandler()), localApiName)
        { }
#else
        this(appConfig, authProvider, new HttpClient(), localApiName)
        { }
#endif

        private LzHttpClient(
            IConfiguration appConfig,
            IAuthProvider authProvider,
            HttpClient httpClient,
            string localApiName = null)
        {
            this.appConfig = appConfig;
            this.httpClient = httpClient;
            LocalApiName = localApiName;
            this.awsSettings = appConfig.GetSection("Aws").Get<AwsSettings>();
            this.authProvider = authProvider;
            this.methodMap = appConfig.GetSection("MethodMap").GetChildren().ToDictionary(x => x.Key, x => x.Value);
        }

        readonly HttpClient httpClient;
        readonly AwsSettings awsSettings;
        private LocalApi localApi;
        private IConfiguration appConfig;
        IAuthProvider authProvider;
        Dictionary<string, string> methodMap;

        private string localApiName = string.Empty;
        public string LocalApiName
        {
            get {return localApiName; }
            set 
            { 
                localApiName = value;
                if (!string.IsNullOrEmpty(localApiName))
                {
                    this.localApi = appConfig.GetSection($"LocalApis:{localApiName}").Get<LocalApi>();
                    useLocalApi = true;
                }
            }
        }

        private bool useLocalApi = false;
        public bool UseLocalApi 
        { 
            get { return useLocalApi; } 
            set { useLocalApi = value; } 
        }

        // Note: CallerMember is inserted as a literal by the compiler in the IL so there is no 
        // performance penalty for using it.
        public async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage,
            HttpCompletionOption httpCompletionOption,
            CancellationToken cancellationToken,
            [CallerMemberName] string callerMemberName = null)
        {

            if (!methodMap.TryGetValue(callerMemberName, out string apiGatewayName))
                throw new Exception($"Error: {callerMemberName} not found in AwsSettings MethodMap");

            if (!awsSettings.ApiGateways.TryGetValue(apiGatewayName, out AwsSettings.Api api))
                throw new Exception($"Error: {apiGatewayName} not found in AwsSettings ApiGateways dictionary");

            var securityLevel = api.SecurityLevel;

            if (!string.IsNullOrEmpty(localApiName) && useLocalApi)
            {
                var uriBuilder = new UriBuilder(localApi.Scheme, localApi.Host, localApi.Port);

                // Issue: the AspNetCore server rejects a query with the ? encoded as %3F 
                // so the following doesn't work 
                // uriBuilder.Path = requestMessage.RequestUri.ToString(); 
                // the assignment encodes path query as %3F instead of ?
                // Here we encode the path separately and then build a
                // a new Uri from the uriBuilder and the path.
                var path = requestMessage.RequestUri.ToString(); // Unencoded
                path = Uri.EscapeUriString(path); // Encoded properly
                requestMessage.RequestUri = new Uri(uriBuilder.Uri, path);
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

                // Issue: the AspNetCore server rejects a query with the ? encoded as %3F 
                // so the following doesn't work 
                // uriBuilder.Path = requestMessage.RequestUri.ToString(); 
                // the assignment encodes path query as %3F instead of ?
                // Here we encode the path separately and then build a
                // a new Uri from the uriBuilder and the path.
                path = Uri.EscapeUriString(path); // Encoded properly
                requestMessage.RequestUri = new Uri(uriBuilder.Uri, path);
            }

            Debug.WriteLine($"requestMessage.Path {requestMessage.RequestUri.ToString()}");
            try
            {
                HttpResponseMessage response = null;
                // Note: If the call is being made against a local host then that host
                // will by default not pay any attention to the authorization header attached by 
                // the JWT or AwsSignatureVersion4 cases below. We assign the Headers
                // anyway in case you want to implement handling these headers 
                // in your local host for testing or any other purpose.

                switch (securityLevel)
                {
                    case AwsSettings.SecurityLevel.None:
                        response = await httpClient.SendAsync(
                            requestMessage,
                            httpCompletionOption,
                            cancellationToken);
                        break;

                    case AwsSettings.SecurityLevel.JWT:
                        // Use JWT Token signing process
                        requestMessage.Headers.Add("Authorization", ((AuthProviderCognito)authProvider).CognitoUser.SessionTokens.IdToken);
                        try
                        {
                            response = await httpClient.SendAsync(
                                requestMessage,
                                httpCompletionOption,
                                cancellationToken);

                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine($"Error: {e.Message}");
                        }
                        break;

                    case AwsSettings.SecurityLevel.AwsSignatureVersion4:
                        // Use full request signing process
                        // Get Temporary ImmutableCredentials :  AccessKey, SecretKey, Token
                        // This will refresh immutable credentials if necessary
                        // Calling AwsSignatureVersion4 extension method -- this signs the request message
                        var iCreds = await ((AuthProviderCognito)authProvider).Credentials.GetCredentialsAsync();

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
            catch (Exception e)
            {
                Debug.WriteLine($"Error: {e.Message}");
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);

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
