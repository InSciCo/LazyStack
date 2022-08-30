using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyStackAuth;

public class SvcConfig
{
    public Dictionary<string, Api> Apis { get; set; }
    public Dictionary<string, AssetUri> AssetUris { get; set; }
}

public class Api
{
    public string Key { get; set; }
    public int SecurityLevel { get; set; }
    public string ApiGateway { get; set; }
    public string CloudFrontApi { get; set; }
    public string LocalApi { get; set; }
    public string LocalAndroidApi { get; set; }
}

public class AssetUri
{
    public string Key { get; set; }
    public string Uri { get; set; }
}

public class RunConfig
{
    public string[] AllowedApis { get; set; } = { "ApiGateway", "CloudFront", "Local" };
    private string apis = "CloudFront";
    public string Apis { get; set; }
    
    public string[] AllowedAssets { get; set; } = { "CloudFront", "Local" };
    private string assets = "CloudFront";
    public string Assets { get; set; }
}


public class StackConfig
{
    public string Key { get; set; }
    public SvcConfig SvcConfig { get; set; } 
    public CognitoConfig CognitoConfig { get; set; }
    public RunConfig RunConfig { get; set; }
}

public interface IStacksConfig
{
    public string CurrentStack { get; set; }
    public Dictionary<string, StackConfig> Stacks { get; set; }
}

public class StacksConfig : IStacksConfig
{
    public string CurrentStack { get; set; } = "";
    public Dictionary<string, StackConfig> Stacks { get; set; }
   
}

