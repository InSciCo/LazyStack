using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Amazon.CognitoIdentity;
using Amazon.Extensions.CognitoAuthentication;


namespace __ProjName__
{
    public class LzHttpClient : IDisposable
    {

        public LzHttpClient(string regionEndPoint, string serviceName, CognitoUser cognitoUser, CognitoAWSCredentials cognitoAwsCredentials)
        {
            this.httpClient = new HttpClient();
            this.regionEndPoint = regionEndPoint;
            this.serviceName = serviceName;
            this.cognitoUser = cognitoUser;
            this.cognitoAwsCredentials = cognitoAwsCredentials;
        }

        public LzHttpClient(string regionEndPoint, string serviceName, CognitoUser cognitoUser, CognitoAWSCredentials cognitoAwsCredentials, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.regionEndPoint = regionEndPoint;
            this.serviceName = serviceName;
            this.cognitoUser = cognitoUser;
            this.cognitoAwsCredentials = cognitoAwsCredentials;
         }
        
        HttpClient httpClient;
        string regionEndPoint;
        string serviceName;
        CognitoUser cognitoUser;
        CognitoAWSCredentials cognitoAwsCredentials;

        public async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage, 
            HttpCompletionOption httpCompletionOption, 
            CancellationToken cancellationToken, 
            [CallerMemberName] string callerMemberName = null)
        {
            HttpResponseMessage response = null;
            switch(callerMemberName)
            {
__SecurityLevelNoneCases__

__SecurityLevelJwtCases__

__SecurityLevelSignedCases__

                default:
                    throw new Exception($"Error: Unknown OperationId {callerMemberName}");
            }
            return response;
        }
        
        public void Dispose()
        {
            httpClient.Dispose();
        }

    }
}
