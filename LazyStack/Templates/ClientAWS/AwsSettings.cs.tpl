using System;
using System.Collections.Generic;
using Amazon.CognitoIdentity;
using Amazon.Extensions.CognitoAuthentication;

namespace __ProjName__
{
    public enum SecurityLevel
    {
        None,
        JWT,
        AwsSignatureVersion4
    }

    public class AwsSettings 
    {
        public class Api
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string Scheme {get; set; }
            public string Id { get; set; }
            public string Service {get; set;}
            public string Host {get; set;}
            public int Port {get; set;}
            public string Stage { get; set; } 
        }
        
        public string ClientId { get; set;}
        public string UserPoolId { get; set;}
        public string IdentityPoolId { get; set; }
        public string Region { get; set; }
        public CognitoUser CognitoUser { get; set; }
        public CognitoAWSCredentials CognitoAwsCredentials { get; set;}
        public string DefaultScheme {get; set;}
        public string DefaultHost {get; set;}
        public int DefaultPort {get; set;}
        public string DefaultService {get; set;}
        public string LocalScheme {get; set;}
        public string LocalHost {get; set;}
        public int LocalPort {get; set;}
        public bool UseLocal {get; set;}
        public List<Api> ApiGateways { get; } = new List<Api>();

        private Dictionary<string,AwsSettings.Api> apiMap = new Dictionary<string, AwsSettings.Api>();

        public (AwsSettings.Api api, SecurityLevel securityLevel) ResolveForMethod(string methodName)
        {
            if(apiMap.Count == 0)
                foreach(var apiGateway in ApiGateways)
                    apiMap.Add(apiGateway.Name, apiGateway);

            AwsSettings.Api api;

            // Assign apiGatewayName based on endpoint
            // Note: The compiler will convert a large number of switch statements to a Dictionary
            switch(methodName)
            {
__MemberCaseStatements__
                default:
                    throw new Exception($"Error: Unknown OperationId {methodName}");
            }

            // Assign secuityLevel based on name of gateway
            var securityLevel = SecurityLevel.None;
            switch(api.Name)
            {
__GatewayCaseStatements__
                default:
                    throw new Exception($"Error: Unknown ApiGateway {api.Name}");
            }
            return (api, securityLevel);
        }
    }
}
