﻿ using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LazyStackAuthV2;

/// <summary>
/// This ILzHttpClient supports calling Local, CloudFront or ApiGateway endpoints.
/// It is not an HttClient, instead it services SendAsync() calls made from the *SvcClientSDK and 
/// dispatches these calls to cached HttpClient(s) configured for each API. This allows each 
/// endpoint to be separately configured for security etc.
/// Note that we do not support AwsSignatureVersion4 in this class. .NET doesn't have the required
/// crypto libs necessary to implement this in WASM. We will add it back in when they fix this.
/// If we need it sooner, we can use preprocessor directives to make it available in MAUI targets.
/// </summary>
public class LzHttpClient : ILzHttpClient
{
    public LzHttpClient(
        IStackConfig stackConfig,
        IMethodMapWrapper methodMap, // map of methods to api endpoints
        IAuthProvider authProvider)
    {
        this.stackConfig = stackConfig;
        this.methodMap = methodMap; // map of methods to api endpoints
        this.authProvider = authProvider;
    }
    private IStackConfig stackConfig;
    private RunConfig runConfig { get { return stackConfig.RunConfig; } }
    private ServiceConfig svcConfig { get { return stackConfig.ServiceConfig; } }
    private IMethodMapWrapper methodMap;
    private IAuthProvider authProvider;
    private Dictionary<string, HttpClient> httpClients = new();
    private Dictionary<string, Api> Apis = new();

    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage requestMessage,
        HttpCompletionOption httpCompletionOption,
        CancellationToken cancellationToken,
        [CallerMemberName] string callerMemberName = null!)
    {
        // Lookup callerMemberName in methodMap to get api name, if not found then throw
        if (!methodMap.MethodMap.TryGetValue(callerMemberName, out string api))
            throw new Exception($"{nameof(LzHttpClient)}.{nameof(SendAsync)} failed. callerMemberName {callerMemberName} not found in methodMap");

        // Find api endpoint data
        if (!svcConfig.Apis.TryGetValue(api, out Api apiEndpoint))
            throw new Exception($"{nameof(LzHttpClient)}.{nameof(SendAsync)} failed. Apis {api} not found in runConfig.");

        var securityLevel = apiEndpoint.SecurityLevel;

        var apiskey = runConfig.Apis;
        var isLocal = apiskey == "Local";
#if ANDROID
       if(apiskey == "Local") 
            apiskey = "LocalAndroid";
#endif

        string baseUrl = "";
        try
        {
            baseUrl = apiEndpoint.ApiUris[apiskey];
        } catch { 
            throw new Exception($"{nameof(LzHttpClient)}.{nameof(SendAsync)} failed. Apis {runConfig.Apis} value not supported.");
        }

        if (string.IsNullOrEmpty(baseUrl))
            throw new Exception($"{nameof(LzHttpClient)}.{nameof(SendAsync)} failed. Apis {runConfig.Apis} uri value is null or empty.");

        // Create new HttpClient for endpoint if one doesn't exist
        if (!httpClients.TryGetValue(baseUrl, out HttpClient httpclient))
        {

            var isMAUI = false;
#if ANDROID || WINDOWS || IOS || TZEN || MACOS
            isMAUI = true;  
#endif
            httpclient = isLocal && isMAUI
                ? new HttpClient(GetInsecureHandler())
                : new HttpClient();
            httpclient.BaseAddress = new Uri(baseUrl);
            httpClients.Add(baseUrl, httpclient);
        }

        try
        {
            HttpResponseMessage response = null;
            switch (securityLevel)
            {
                case 0: // No security 
                    response = await httpclient.SendAsync(
                        requestMessage,
                        httpCompletionOption,
                        cancellationToken);

                    return response;

                case 1: // Use JWT Token signing process
                    try
                    {
                        string token = "";
                        try
                        {
                            // TODO - we sometimes get an error perculating up to the Blazor component level 
                            // when we make this call. Need to figure out what is going on.
                            token = await authProvider.GetJWTAsync();
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
                        return response;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error: {e.Message}");
                        return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                    }
                case 2:
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