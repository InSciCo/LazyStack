using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Auth.AccessControlPolicy;
using Amazon.Runtime;
using LazyStackAwsSettings;
using LazyStackBase;
using Newtonsoft.Json.Linq;

namespace LazyStackAuth;

/// <summary>
/// This ILzHttpClient supports calling Local, CloudFront or ApiGateway endpoints.
/// It is not an HttClient, instead it services SendAsync() calls made from the *SvcClientSDK and 
/// dispatches these calls to cached HttpClient(s) configured for each API. This allows each 
/// endpoint to be separately configured for security etc.
/// </summary>
public class LzHttpClient : NotifyBase, ILzHttpClient
{
    public LzHttpClient(
        ILzClientConfig clientConfig, // servcie connection info
        IMethodMapWrapper methodMap, // map of methods to api endpoints
        IAuthProvider authProvider, // Auth service. ex: AuthProviderCognito
        ILzHost lzHost // Runtime environment. IsMAUI, IsWASM, BaseURL etc.
        )
    {
        this.clientConfig = clientConfig; 
        this.methodMap = methodMap; // map of methods to api endpoints
        this.authProvider = authProvider;
        this.lzHost= lzHost;

    }
    private ILzClientConfig clientConfig;
    private LzRunConfig runConfig => clientConfig.RunConfig;
    private LzService service => clientConfig.Services[runConfig.Service];
    private JObject authenticator => clientConfig.Authenticators[service.Auth];

    private IMethodMapWrapper methodMap;
    private IAuthProvider authProvider;
    private ILzHost lzHost; 
    private Dictionary<string, HttpClient> httpClients = new();
    //private Dictionary<string, Api> Apis = new();
    private bool isServiceAvailable = false;
    public bool IsServiceAvailable
    {
        get { return isServiceAvailable; }  
        set
        {
            SetProperty(ref isServiceAvailable, value);
        }
    }
    private int[] serviceUnavailableCodes = new int[] { 400 };

    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage requestMessage,
        HttpCompletionOption httpCompletionOption,
        CancellationToken cancellationToken,
        [CallerMemberName] string? callerMemberName = null!)
    {
        // Lookup callerMemberName in methodMap to get api name, if not found then throw
        if (!methodMap.MethodMap.TryGetValue(callerMemberName!, out string? api))
            throw new Exception($"{nameof(LzHttpClient)}.{nameof(SendAsync)} failed. callerMemberName {callerMemberName} not found in MethodMap");

        // Find api endpoint data
        JObject resource = service.Resources[api];
        if (resource == null)
            throw new Exception($"{nameof(LzHttpClient)}.{nameof(SendAsync)} failed. Apis {api} not found in ClientConfig.");

        var securityLevel = (int)resource["SecurityLevel"]!;
        var resourceType = (string)resource["ResourceType"]!;
        var isLocal = resourceType == "Local" || resourceType == "LocalAndriod";
        var baseUrl = (string)resource["Url"]!;

        // Create new HttpClient for endpoint if one doesn't exist
        if (!httpClients.TryGetValue(baseUrl, out HttpClient? httpclient))
        {

            httpclient = isLocal && lzHost.IsMAUI
                ? new HttpClient(GetInsecureHandler())
                : new HttpClient();
            httpclient.BaseAddress = new Uri(baseUrl);
            httpClients.Add(baseUrl, httpclient);
        }

        try
        {
            HttpResponseMessage? response = null;
            switch (securityLevel)
            {
                case 0: // No security 
                    try
                    {
                        response = await httpclient.SendAsync(
                            requestMessage,
                            httpCompletionOption,
                            cancellationToken);
                        IsServiceAvailable = true;
                        return response;
                    }
                    catch (HttpRequestException e) 
                    {
                        // request failed due to an underlying issue such as network connectivity,
                        // DNS failure, server certificate validation or timeout
                        isServiceAvailable = false;
                        Console.WriteLine($"HttpRequestException {e.Message}");
                        return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                    } 
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error: {callerMemberName} {e.Message}");
                        return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                    }

                case 1: // Use JWT Token signing process
                    try
                    {
                        string? token = "";
                        try
                        {
                            token = await authProvider!.GetJWTAsync();
                            requestMessage.Headers.Add("Authorization", token);
                        }
                        catch
                        {
                            // Ignore. We ignore this error and let the 
                            // api handle the missing token. This gives us a 
                            // way of testing an inproperly configured API.
                            Debug.WriteLine("authProvider.GetJWTAsync() failed");
                            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                        }

                        response = await httpclient.SendAsync(
                            requestMessage,
                            httpCompletionOption,
                            cancellationToken);
                        //Console.WriteLine(callerMemberName);
                        IsServiceAvailable = true;  
                        return response;
                    }
                    catch (HttpRequestException e)
                    {
                        // request failed due to an underlying issue such as network connectivity,
                        // DNS failure, server certificate validation or timeout
                        Console.WriteLine($"HttpRequestException {e.Message}");
                        isServiceAvailable = false;
                        return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error: {callerMemberName} {e.Message}");
                        return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                    }
                case 2:
                    try
                    {
                        // Note: For this ApiGateway we need to add a copy of the JWT header 
                        // to make it available to the Lambda. This type of ApiGateway will not
                        // pass the authorization header through to the Lambda.
                        var token = await authProvider.GetJWTAsync();
                        requestMessage.Headers.Add("LzIdentity", token);

                        var iCreds = await authProvider.GetCredsAsync();
                        var awsCreds = new ImmutableCredentials(iCreds!.AccessKey, iCreds.SecretKey, iCreds.Token);

                        // Note. Using named parameters to satisfy version >= 3.x.x  signature of 
                        // AwsSignatureVersion4 SendAsync method.
                        response = await httpclient.SendAsync(
                        request: requestMessage,
                        completionOption: httpCompletionOption,
                        cancellationToken: cancellationToken,
                        regionName: clientConfig.Region,
                        serviceName: "execute-api",
                        credentials: awsCreds);
                        return response;
                    }
                    catch (System.Exception e)
                    {
                        Debug.WriteLine($"Error: {e.Message}");
                    }
                    break;
                    throw new Exception($"Security Level {securityLevel} not supported.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
        }
        return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
    }

    public void Dispose()
    {
        foreach (var httpclient in httpClients.Values)
            httpclient.Dispose();
    }

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
                if (cert!.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            }
        };
        return handler;
    }

}
