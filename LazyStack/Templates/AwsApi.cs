using System;
using System.Collections.Generic;
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
            string localBaseUrl = null)
        {
            if (string.IsNullOrEmpty(apiId))
                return null;
            return new AwsApi(apiId, apiType, serviceName, regionEndpointStr, stage, secure, useLocal, localBaseUrl);
        }

        public AwsApi(
            string apiId,
            string apiType,
            string serviceName,
            string regionEndpointStr,
            string stage,
            bool secure = false,
            bool useLocal = false,
            string localBaseUrl = null)
        {

            ApiId = apiId;
            ApiType = apiType;
            ServiceName = serviceName;
            RegionEndpointStr = regionEndpointStr;
            Stage = stage;
            IsSecure = secure;
            UseLocal = useLocal;
            LocalBaseUrl = localBaseUrl;
        }

        public string ApiId { get; }
        public string ApiType { get; }
        public string ServiceName { get; }
        public string RegionEndpointStr { get; }
        public string Stage { get; }
        public bool IsSecure { get; }
        public bool UseLocal { get; }
        public string LocalBaseUrl { get; }

        public string BaseUrl
        {
            get
            {
                return
                  UseLocal
                  ? LocalBaseUrl
                  : $"https://{ApiId}.{ServiceName}.{RegionEndpointStr}.amazonaws.com/{Stage}";
            }
        }

    }
}
