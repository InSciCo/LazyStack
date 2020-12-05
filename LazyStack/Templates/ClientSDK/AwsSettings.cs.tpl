using System;
using System.Collections.Generic;

namespace __ProjName__
{
    class AwsSettings
    {
        public class Api
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public string Type { get; set; }
            public string Stage { get; set; } 
            public bool Secure { get; set; }
        }
        
        public string ClientId { get; }
        public string UserPoolId { get; }
        public string IdentityPoolId { get; }
        public string Region { get; }
        public List<Api> ApiGateways { get; } = new List<Api>();
    }
}
