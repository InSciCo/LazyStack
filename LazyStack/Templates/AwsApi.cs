using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace __AppName__ClientSDK.Client
{
    public class AwsApi
    {
        public static AwsApi GetAwsApi(
            string apiId,
            string apiType,
            string serviceName,
            string regionEndpointStr,
            string stage,
            bool secure = false,
            bool useLocal = false,
            Uri localUri = null)
        {
            if (string.IsNullOrEmpty(apiId))
                return null;
            return new AwsApi(apiId, apiType, serviceName, regionEndpointStr, stage, secure, useLocal, localUri);
        }

        public AwsApi(
            string apiId,
            string apiType,
            string serviceName,
            string regionEndpointStr,
            string stage,
            bool secure = false,
            bool useLocal = false,
            Uri localUri = null)
        {

            ApiId = apiId;
            ApiType = apiType;
            ServiceName = serviceName;
            RegionEndpointStr = regionEndpointStr;
            Stage = stage;
            IsSecure = secure;
            UseLocal = useLocal;
            LocalUri = localUri;
            HttpClient = new HttpClient(); // Each AwsApi has it's own HttpClient becuase it has a different base address
            // Note that /{stage} is NOT part of the base address. This is because HttpClient will discard everthing after
            // the host name. We prepend the stage to the path value in ApiClient.
            HttpClient.BaseAddress =
                (UseLocal)
                ? LocalUri
                : new Uri($"https://{ApiId}.{ServiceName}.{RegionEndpointStr}.amazonaws.com");
        }

        public readonly HttpClient HttpClient;

        public string ApiId { get; }
        public string ApiType { get; }
        public string ServiceName { get; }
        public string RegionEndpointStr { get; }
        public string Stage { get; } // Note that Stage is Prepended to path - not appended to BaseAddress. A subtle but important distiction!
        public bool IsSecure { get; }
        public bool UseLocal { get; }
        public Uri LocalUri { get; }
    }
}
