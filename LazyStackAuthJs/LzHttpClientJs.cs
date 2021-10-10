using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using LazyStackAuth;

namespace LazyStackAuthJs
{
    /// <summary>
    /// Use LzHttpClientJs for Blazor support (at least until MS provides 
    /// the crypto lib functions missing in .NET core).
    /// </summary>
    public class LzHttpClientJs : LzHttpClientJWT
    {
        public LzHttpClientJs(
            IConfiguration appConfig,
            IAuthProvider authProvider,
            string localApiName = null) : base(appConfig, authProvider, localApiName)
        { }

        protected virtual async Task<HttpResponseMessage> SendSigned(
            HttpClient httpClient,
            HttpRequestMessage requestMessage,
            HttpCompletionOption httpCompletionOption,
            CancellationToken cancellationToken,
            string region,
            string service
            )
        {
            await Task.Delay(0);
            // Todo - implement using calls to javascript lib
            throw new NotImplementedException();
        }
    }
}
