using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;

using LazyStackAuth;

namespace __AppName__ClientSDK.Client
{
    /// <summary>
    /// LazyStack ApiClient - replaces generated library's ApiClient
    /// This ApiClient removes the generated library's dependency on RestSharp and allows
    /// us to use Microsoft's Http libraries and the very well engineered AwsSignatureVersion4
    /// library by Mattias Kindborg. Signing API Gateway calls is non-trivial and his library is
    /// very robust. See https://github.com/FantasticFiasco/aws-signature-version-4. 
    /// Unforntunately, AWS doesn't provide their own library for API Gateway Request Signing - crazy but true.
    /// 
    /// We have no current requirements that cause us to replicate all the general request/respoinse 
    /// processing provided by RestSharp (and surfaced by the generated library's ApiClient) in this 
    /// client so read the following implementation details carefully. We are open to implementing
    /// additional features, similar to those in the original ApiClient, to satisfy evolving requirements.
    /// 
    /// We have broken out the various steps in preparing a request into separate virtual methods that
    /// you can easily override to implement/enhance request processing.
    /// 
    /// Implementation Details:
    /// - Using Microsoft HttpClient, HttpRequestMessage, HttpResponseMessage instead of RestSharp
    /// - Using AwsSignatureVersion4 to sign requests to AWS ApiGateway and AWS S3
    /// - Using Newtonsoft.Json
    /// - current supporting only application/json content - but you can override ProcessData() and 
    ///   ProcessResponse() to do anything you want.
    /// - ONLY supporting ASYNC calls 
    /// - Current limitations
    ///     - Cookies - override ProcessCookies() and ProcessResponse() to do cookie processing
    ///     - configuration -- on the roadmap
    ///     - FileParameters - override ProcessFileParamters() to implement this - we don't have default implementation at this time
    /// 
    /// openapi-generator OpenApi CSHARP Generation Note:
    /// We want to avoid modifying the library generation process so we do the following:
    /// 1. Generate the library using openapi-genrator
    /// 2. Create the LazyStack folder and place this ApiClient.cs file in it.
    /// 3. Modify the csproj file as follows:
    ///     - remove dependency on RestSharp
    ///     - add dependency on AwsSignatureVersion4
    ///     - add Compile Remove="Client\ApiClient.cs" directive
    ///     
    /// Implementation Notes:
    /// We are using WebUtility.UrlEncode to encode query values. This seems to be the "latest"
    /// .NET standard. There is controversy about the encoding this method performs. Specifically,
    /// the conversion of space characters to "+" charcters instead of "%20". I have no opinion on
    /// the matter but do note that there are complaints that OAuth and some other protocols don't
    /// like this. If this is a problem for your app, let us know. You can also subclass ApiClient
    /// and override the ProcessQuery method to implement your own query handling. 
    /// </summary>
    public class ApiClient : ISynchronousClient, IAsynchronousClient
    {
        /// <summary>
        /// Pleaase use ApiClient(baseUrl, strRegionendPoint, serviceName, icreds) constructor instead
        /// </summary>
        /// <param name="basePath"></param>
        public ApiClient(string basePath)
        {
            throw new NotImplementedException("Pleaase use ApiClient(baseUrl, strRegionendPoint, serviceName, creds) constructor");
        }

        /// <summary>
        /// Pleaase use ApiClient(baseUrl, strRegionendPoint, serviceName, icreds) constructor instead
        /// </summary>
        /// <param name="configuration"></param>
        public ApiClient(Client.Configuration configuration)
        {
            throw new NotImplementedException("Pleaase use ApiClient(AwsApi awsApi, User user) constructor instead");
        }

        /// <summary>
        /// Constructor assumes we are using library to talk with AWS so we pass in AWS related information.
        /// </summary>
        /// <param name="awsApi"></param>
        /// <param name="authProvider"></param>
        public ApiClient(AwsRestApiGateway awsApi, IAuthProvider authProvider)
        {
            this.awsApi = awsApi;
            this.authProvider = authProvider;
        }

        readonly AwsRestApiGateway awsApi;
        readonly IAuthProvider authProvider;

        public virtual void ProcessPath(
             System.Net.Http.HttpMethod method,
            string path,
            RequestOptions options,
            IReadableConfiguration configuration,
            UriBuilder uriBuilder)
        {
            // Prepend /<stage> to path. Note, stage is not used for local server
            if (!string.IsNullOrEmpty(awsApi.Stage) && !awsApi.UseLocal)
                path = "/" + awsApi.Stage + path;

            // Handle Path Parameters
            foreach (var pathParam in options.PathParameters)
                path = path.Replace("{" + pathParam.Key + "}", pathParam.Value);

            uriBuilder.Path = path;
        }

        public virtual void ProcessQuery(
             System.Net.Http.HttpMethod method,
            string path,
            RequestOptions options,
            IReadableConfiguration configuration,
            UriBuilder uriBuilder)
        {
            var queryString = string.Empty;
            bool firstKey = true;
            foreach (var key in options.QueryParameters.Keys)
            {
                foreach (var value in options.QueryParameters[key])
                {
                    queryString +=
                        (firstKey)
                        ? WebUtility.UrlEncode(key) + "=" + value
                        : "&" + WebUtility.UrlEncode(key) + "=" + value;
                    firstKey = false;
                }
            }
            uriBuilder.Query = queryString;
        }

        public virtual void ProcessHeaders(
            System.Net.Http.HttpMethod method,
            string path,
            RequestOptions options,
            IReadableConfiguration configuration,
            UriBuilder uriBuilder,
            HttpRequestMessage requestMessage)
        {
            //todo
        }

        public virtual void ProcessFormParameters(
             System.Net.Http.HttpMethod method,
            string path,
            RequestOptions options,
            IReadableConfiguration configuration,
            UriBuilder uriBuilder,
            HttpRequestMessage requestMessage)
        {
            //todo
        }

        public virtual void ProcessHeaderParameters(
             System.Net.Http.HttpMethod method,
            string path,
            RequestOptions options,
            IReadableConfiguration configuration,
            UriBuilder uriBuilder,
            HttpRequestMessage requestMessage)
        {
            //todo
        }

        public virtual void ProcessData(
            System.Net.Http.HttpMethod method,
            string path,
            RequestOptions options,
            IReadableConfiguration configuration,
            UriBuilder uriBuilder,
            HttpRequestMessage requestMessage)
        {
            // Handle Data -- only handling json
            if (options.Data != null)
            {
                var data = JsonConvert.SerializeObject(options.Data);
                requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
            }
        }

        public virtual void ProcessFileParameters(
            System.Net.Http.HttpMethod method,
            string path,
            RequestOptions options,
            IReadableConfiguration configuration,
            UriBuilder uriBuilder,
            HttpRequestMessage requestMessage)
        {
            // todo
        }

        public virtual void ProcessCookies(
            System.Net.Http.HttpMethod method,
            string path,
            RequestOptions options,
            IReadableConfiguration configuration,
            UriBuilder uriBuilder,
            HttpRequestMessage requestMessage)
        {
            // todo
        }

        public virtual void PreSendHandler(
            System.Net.Http.HttpMethod method,
            string path,
            RequestOptions options,
            IReadableConfiguration configuration,
            UriBuilder uriBuilder,
            HttpRequestMessage requestMessage)
        {
            // This is here for application level final processing prior to sending request
        }

        public virtual async Task<ApiResponse<T>> ProcessResponse<T>(
            HttpResponseMessage response
            )
        {
            // todo - Post-send raw HttpResponseMessage handling

            // process data
            string jsonBody = string.Empty;

            T jsonObject = default;
            if (response.Content != null)
            {
                jsonBody = await response.Content.ReadAsStringAsync();
                jsonObject = JsonConvert.DeserializeObject<T>(jsonBody);
            }

            var transformed = new ApiResponse<T>(response.StatusCode, new Multimap<string, string>(), jsonObject, jsonBody)
            {
                ErrorText = response.ReasonPhrase,
                Cookies = new List<Cookie>()
            };

            // Handle Response Headers 
            if (response.Headers != null)
                foreach (var header in response.Headers)
                    transformed.Headers.Add(header.Key, header.Value.ToString());

            // todo - Handle Cookies

            // todo - Post transformation handling

            return transformed;
        }

        /// <summary>
        /// Create HttpRequestMessage. Call SendAsync. Process HttpResponseMessage. return ApiResponse
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private async Task<ApiResponse<T>> ExecAsync<T>(
            System.Net.Http.HttpMethod method,
            string path,
            RequestOptions options,
            IReadableConfiguration configuration)
        {
            var uriBuilder = new UriBuilder(awsApi.HttpClient.BaseAddress);
            ProcessPath(method, path, options, configuration, uriBuilder);
            ProcessQuery(method, path, options, configuration, uriBuilder);

            var requestMessage = new HttpRequestMessage(method, uriBuilder.Uri);

            // todo - get options from configuration
            var completionOption = HttpCompletionOption.ResponseContentRead;
            var cancelationtoken = CancellationToken.None;

            ProcessHeaders(method, path, options, configuration, uriBuilder, requestMessage);
            ProcessFormParameters(method, path, options, configuration, uriBuilder, requestMessage);
            ProcessHeaderParameters(method, path, options, configuration, uriBuilder, requestMessage);
            ProcessData(method, path, options, configuration, uriBuilder, requestMessage);
            ProcessFileParameters(method, path, options, configuration, uriBuilder, requestMessage);
            ProcessCookies(method, path, options, configuration, uriBuilder, requestMessage);
            PreSendHandler(method, path, options, configuration, uriBuilder, requestMessage);

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
                // Get Temporary ImmutableCredentials :  AccessKey, SecretKey, Token
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

            return await ProcessResponse<T>(response);
        }

        /// <summary>
        /// Please use Async version of this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public ApiResponse<T> Delete<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            throw new NotImplementedException("Please use Async verion of call");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Task<ApiResponse<T>> DeleteAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            return ExecAsync<T>(System.Net.Http.HttpMethod.Delete, path, options, configuration);
        }

        /// <summary>
        /// Please use Async version of this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public ApiResponse<T> Get<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            throw new NotImplementedException("Please use Async verion of call");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Task<ApiResponse<T>> GetAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            var result = ExecAsync<T>(System.Net.Http.HttpMethod.Get, path, options, configuration);
            return result;
        }

        /// <summary>
        /// Please use Async version of this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public ApiResponse<T> Head<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            throw new NotImplementedException("Please use Async verion of call");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Task<ApiResponse<T>> HeadAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            return ExecAsync<T>(System.Net.Http.HttpMethod.Head, path, options, configuration);
        }

        /// <summary>
        /// Please use Async version of this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public ApiResponse<T> Options<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            throw new NotImplementedException("Please use Async verion of call");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Task<ApiResponse<T>> OptionsAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            return ExecAsync<T>(System.Net.Http.HttpMethod.Options, path, options, configuration);
        }

        /// <summary>
        /// Please use Async version of this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public ApiResponse<T> Patch<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            throw new NotImplementedException("Please use Async verion of call");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Task<ApiResponse<T>> PatchAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            return ExecAsync<T>(System.Net.Http.HttpMethod.Patch, path, options, configuration);
        }

        /// <summary>
        /// Please use Async version of this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public ApiResponse<T> Post<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            throw new NotImplementedException("Please use Async verion of call");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Task<ApiResponse<T>> PostAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            return ExecAsync<T>(System.Net.Http.HttpMethod.Post, path, options, configuration);
        }

        /// <summary>
        /// Please use Async version of this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public ApiResponse<T> Put<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            throw new NotImplementedException("Please use Async verion of call");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Task<ApiResponse<T>> PutAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null)
        {
            return ExecAsync<T>(System.Net.Http.HttpMethod.Put, path, options, configuration);
        }
    }
}
