using System.Collections.Generic;


namespace LazyStackVsExt
{
    public class AwsSettings
    {
        public class Api
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string Scheme { get; set; }
            public string Id { get; set; }
            public string Service { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public string Stage { get; set; }
        }

        public string ClientId { get; set; }
        public string UserPoolId { get; set; }
        public string IdentityPoolId { get; set; }
        public string Region { get; set; }
        public string DefaultScheme { get; set; }
        public string DefaultHost { get; set; }
        public int DefaultPort { get; set; }
        public string DefaultService { get; set; }
        public string LocalScheme { get; set; }
        public string LocalHost { get; set; }
        public int LocalPort { get; set; }
        public bool UseLocal { get; set; }
        public List<Api> ApiGateways { get; } = new List<Api>();

        public string BuildJson()
        {
            var result = $"{{\"Aws\": {Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented)}}}";
            return result;
        }
    }
}
