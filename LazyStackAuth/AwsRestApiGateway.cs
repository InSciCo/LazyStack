using System;
using System.Net.Http;

namespace LazyStackAuth
{
    public class AwsRestApiGateway
    {

        public AwsRestApiGateway(
            string name,
            string id,
            string type,
            string regionEndpointStr,
            string stage,
            bool isSecure,
            bool useLocal,
            Uri localUri
            )
        {
            Name = name;
            Id = id;
            Type = type;
            RegionEndpointStr = regionEndpointStr;
            Stage = stage;
            IsSecure = isSecure;
            UseLocal = useLocal;
            LocalUri = localUri;
            HttpClient = new HttpClient
            {
                BaseAddress =
                        (UseLocal)
                        ? LocalUri
                        : new Uri($"https://{Id}.execute-api.{RegionEndpointStr}.amazonaws.com")
            };
        }

        public string Name { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public string Stage { get; set; }
        public bool IsSecure { get; set; }
        public bool UseLocal { get; set; }
        public Uri LocalUri { get; set; }
        public string RegionEndpointStr { get; set; }
        public HttpClient HttpClient { get; }

        public override string ToString()
        {
            return $"Api Name:{Name} Id:{Id} Type:{Type} Stage:{Stage} Secure:{IsSecure}\n";
        }
    }
}
