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
using Amazon.Runtime;

namespace LazyStackAuth
{
    /// <summary>
    /// This client makes secure calls against AWS Gateways.
    /// It supports three security models:
    /// - none
    /// - JWT
    /// - AwsSignatureVersion4
    /// </summary>
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
            useLocalApi = false;
            this.awsSettings = appConfig.GetSection("Aws").Get<AwsSettings>();
            this.authProvider = authProvider;
            this.methodMap = appConfig.GetSection("MethodMap").GetChildren().ToDictionary(x => x.Key, x => x.Value);
        }

        readonly HttpClient httpClient;
        readonly AwsSettings awsSettings;
        protected LocalApi localApi;
        protected IConfiguration appConfig;
        protected IAuthProvider authProvider;
        Dictionary<string, string> methodMap;

        protected string localApiName = string.Empty;
        public string LocalApiName
        {
            get { return localApiName; }
            set
            {
                localApiName = value;
                if (!string.IsNullOrEmpty(localApiName))
                {
                    this.localApi = appConfig.GetSection("LocalApis").GetSection(localApiName).Get<LocalApi>();
                    // useLocalApi = true;
                }
            }
        }

        protected bool useLocalApi = false;
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
                //securityLevel = AwsSettings.SecurityLevel.None;
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
                        try
                        {
                            string token = "";
                            try {
                                token = await authProvider.GetJWTAsync();
                                requestMessage.Headers.Add("Authorization", token);
                            }
                            catch (Exception e)
                            {
                                // Ignore. We ignore this error and let the 
                                // api handle the missing token. This gives us a 
                                // way of testing an inproperly configured API.
                                Debug.WriteLine("authProvider.GetJWTAsync() failed");
                            }

                            response = await httpClient.SendAsync(
                                requestMessage,
                                httpCompletionOption,
                                cancellationToken);
                            }
                            catch (System.Exception e)
                            {
                                Debug.WriteLine($"Error: {e.Message}");
                            }
                        break;

                    case AwsSettings.SecurityLevel.AwsSignatureVersion4:
                        // Use full request signing process
                        try
                        {
                            var token = await authProvider.GetJWTAsync();
                            requestMessage.Headers.Add("LzIdentity", token);

                            var iCreds = await authProvider.GetCredsAsync();
                            var awsCreds = new ImmutableCredentials(iCreds.AccessKey, iCreds.SecretKey, iCreds.Token);

                            response = await httpClient.SendAsync(
                            requestMessage,
                            httpCompletionOption,
                            cancellationToken,
                            awsSettings.Region,
                            api.Service,
                            awsCreds);
                            return response;
                        }
                        catch (System.Exception e)
                        {
                            Debug.WriteLine($"Error: {e.Message}");
                        }
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
