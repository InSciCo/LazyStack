using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;

using Amazon.CognitoIdentityProvider.Model;

using System.Diagnostics;

namespace __AppName__ClientSDK.Client
{
    /// <summary>
    /// LazyStack ApiClient - replaces generated library's ApiClient
    /// This ApiClient removes the generated library's dependency on RestSharp and allows
    /// us to use Microsoft's Http libraries and the very well engineered AwsSignatureVersion4
    /// library by Mattias Kindborg. Signing API Gateway calls is non-trivial and his library is
    /// very robust. See https://github.com/FantasticFiasco/aws-signature-version-4. Unforntunately,
    /// AWS doesn't provide their own library for API Gateway Request Signing - crazy but true. 
    /// We have no current requirements that cause us to replicate all the general request/respoinse 
    /// processing provided by RestSharp (and surfaced by the generated library's ApiClient) in this 
    /// client so read the following implementation details carefully. We are open to implementing
    /// additional features, similar to those in the original ApiClient, to satisfy evolving requirements.
    /// 
    /// Implementation Details:
    /// - Using Microsoft HttpClient, HttpRequestMessage, HttpResponseMessage instead of RestSharp
    /// - Using AwsSignatureVersion4 to sign requests to AWS ApiGateway and AWS S3
    /// - Using Newtonsoft.Json
    /// - ONLY supporting application/json content
    /// - ONLY supporting ASYNC calls as our client apps are coded for Async
    /// - Currently not supporting:
    ///     - cookies
    ///     - configuration -- on the roadmap
    ///     - pre and post sendAsync processing -- on the roadmap
    /// - todo -- flesh out list of unsupported items that were supported in original ApiClient
    /// 
    /// openapi-generator OpenApi CSHARP Generation Note:
    /// We want to avoid modifying the library generation process so we do the following:
    /// 1. Generate the library using openapi-genrator
    /// 2. Create the LazyStack folder and place this ApiClient.cs file in it.
    /// 3. Modify the csproj file as follows:
    ///     - remove dependency on RestSharp
    ///     - add dependency on AwsSignatureVersion4
    ///     - add Compile Remove="Client\ApiClient.cs" directive
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
            throw new NotImplementedException("Pleaase use ApiClient(baseUrl, strRegionendPoint, serviceName, creds) constructor");
        }

        /// <summary>
        /// Constructor assumes we are using library to talk with AWS so we pass in AWS related information.
        /// AWS Notes:
        /// todo - support requesting new creds when existing creds expire
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param user="user"></param>
        /// <param "apiType="apiType"></param>
        public ApiClient(AwsApi awsApi, User user)
        {
            this.awsApi = awsApi;
            this.user = user;
        }

        readonly AwsApi awsApi;
        readonly User user;

        /// <summary>
        /// Create HttpClient and HttpRequestMessage. Call SendAsync. Process HttpResponseMessage. return ApiResponse
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

            var client = new HttpClient(); //todo - Should we use long lived HttpClient?

            // Handle Path Parameters
            foreach (var pathParam in options.PathParameters)
                path = path.Replace("{" + pathParam.Key + "}", pathParam.Value);

            var requestUri = new Uri(awsApi.BaseUrl + path);

            // todo - get completion option from configuration (review docs on this)
            var completionOption = HttpCompletionOption.ResponseContentRead;

            // todo - get cancelationtoken from configuration
            var cancelationtoken = CancellationToken.None;

            var requestMessage = new HttpRequestMessage(method, requestUri);

            // Handle Query Parameters

            // Handle Default Headers

            // Handle Form Parameters

            // Handle Header Parameters

            // Handle Data -- only handling json
            if (options.Data != null)
            {
                var data = JsonConvert.SerializeObject(options.Data);
                requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
            }

            // Handle File Parameters

            // Handle Cookies

            // put in pre-send handling
            HttpResponseMessage response = null;
            if (!awsApi.IsSecure)
            {
                response = await client.SendAsync(
                    requestMessage,
                    completionOption,
                    cancelationtoken);
            }
            else
            if (awsApi.ApiType.Equals("Api", StringComparison.OrdinalIgnoreCase))
            {
                // Use full request signing process
                // Get Temporary ImmutableCredentials :  AccessKey, SecretKey, Token
                var icreds = await user.AWSCredentials.GetCredentialsAsync(); // This will refresh creds if necessaary
                response = await client.SendAsync(
                    requestMessage,
                    completionOption,
                    cancelationtoken,
                    user.RegionEndpointStr,
                    awsApi.ServiceName,
                    icreds);
            }
            else if (awsApi.ApiType.Equals("HttpApi", StringComparison.OrdinalIgnoreCase))
            {
                // Use JWT Token signing process
                requestMessage.Headers.Add("Authorization", user.IdToken);
                response = await client.SendAsync(
                    requestMessage,
                    completionOption,
                    cancelationtoken);
            }

            // todo - Post-send raw HttpResponseMessage handling

            // process data
            string jsonBody = string.Empty;
            T jsonObject = default; // todo - be sure to test behavior of generic default
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

            // Handle Cookies -- todo, how to get cookies out of HttpResponseMessage?


            // todo - Post transformation handling

            return transformed;
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
            return ExecAsync<T>(System.Net.Http.HttpMethod.Get, path, options, configuration);
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
