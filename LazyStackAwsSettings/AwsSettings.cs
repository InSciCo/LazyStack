using System.Collections.Generic;

namespace LazyStackAwsSettings
{
   public class AwsSettings : Dictionary<string, object>
   {
        public enum SecurityLevel
        {
            None,
            JWT,
            AwsSignatureVersion4
        }

        public class Api
        {
            public string Type { get; set; }
            public string Scheme { get; set; } = "https";
            public string Id { get; set; }
            public string Service { get; set; } = "execute-api";
            public string Host { get; set; } = "amazonaws.com";
            public int Port { get; set; } = 443;
            public string Stage { get; set; } = "";
            public SecurityLevel SecurityLevel { get; set; }
        }

        public AwsSettings()
        {
            this.Add("StackName", string.Empty);
            this.Add("Region", string.Empty);
            this.Add("ApiGateways", new Dictionary<string, Api>());
        }

        public string BuildJson()
        {
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            return result;
        }

        public string BuildJsonWrapped()
        {
            var result = $"{{\"Aws\": {Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented)}}}";
            return result;
        }
    }
}

