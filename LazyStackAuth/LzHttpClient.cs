using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;

namespace LazyStackAuth
{
    /// <summary>
    /// LzHttpClient adds C# lib support for SecurityLevel.AwsSignatureVersion4
    /// This drags in the AWS runtime libs and doesn't work on Blazor.
    /// Use LzHttpClientJs for Blazor support (at least until MS provides 
    /// the crypto lib functions missing in .NET core).
    /// </summary>
    public class LzHttpClient : LzHttpClientJWT
    {
        public LzHttpClient(
            IConfiguration appConfig,
            IAuthProvider authProvider,
            string localApiName = null) : base(appConfig, authProvider, localApiName)
        { }

        protected override async Task<HttpResponseMessage> SendSignedAsync(
            HttpClient httpClient,
            HttpRequestMessage requestMessage,
            HttpCompletionOption httpCompletionOption,
            CancellationToken cancellationToken,
            string region,
            string service
            )
        {
            var token = await authProvider.GetJWTAsync();
            requestMessage.Headers.Add("LzIdentity", token);

            var iCreds = await authProvider.GetCredsAsync();
            var awsCreds = new ImmutableCredentials(iCreds.AccessKey, iCreds.SecretKey, iCreds.Token);

            //Debug.WriteLine("About to get credentials");
            //var awsCreds = await authProvider.Credentials.GetCredentialsAsync();

            var response = await httpClient.SendAsync(
            requestMessage,
            httpCompletionOption,
            cancellationToken,
            region,
            service,
            awsCreds);
            return response;
        }
    }
}
