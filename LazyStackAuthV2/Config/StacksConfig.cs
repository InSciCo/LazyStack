using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LazyStackAuthV2;

public interface IStacksConfig
{
    public string CurrentStackName { get; set; }
    public Dictionary<string, StackConfig> Stacks { get; set; }
    [JsonIgnore]
    public StackConfig CurrentStack { get { return Stacks[CurrentStackName]; } }
}
public class StacksConfig : NotifyBase, IStacksConfig
{
    private string _currentStackName = "";
    public string CurrentStackName
    {
        get { return _currentStackName; }
        set { SetProperty(ref _currentStackName, value); }
    }
    public Dictionary<string, StackConfig> Stacks { get; set; } = new();
}

public interface IStackConfig
{
    public SaaSConfig SaaSConfig { get; set; }
    public ServiceConfig ServiceConfig { get; set; }
    public TenantConfig TenantConfig { get; set; }  
    public CognitoConfig CognitoConfig { get; set; }
    public RunConfig RunConfig { get; set; }
    public Dictionary<string, string> CurrentApis { get; }
}

public class StackConfig : IStackConfig
{
    public SaaSConfig SaaSConfig { get; set; }
    public ServiceConfig ServiceConfig { get; set; }
    public TenantConfig TenantConfig { get; set; }
    public CognitoConfig CognitoConfig { get; set; }
    public RunConfig RunConfig { get; set; }

    private Dictionary<string, string> _currentApis = new();
    [JsonIgnore]
    public Dictionary<string,string> CurrentApis
    {
        get
        {
            _currentApis.Clear();
            foreach (var apis in ServiceConfig.Apis)
                if (apis.Value.ApiUris.TryGetValue(RunConfig.Apis, out string uri))
                    _currentApis.Add(apis.Key, uri);
            return _currentApis;
        }
    }
    [JsonIgnore]
    public string CurrerntAssets
    {
        get { return ServiceConfig.AssetUris.FirstOrDefault(x => x.Key == RunConfig.Assets).Value; }
    }
}

public interface ITenantConfig
{
    public Dictionary<string,string> Tenants { get; set; }   
}

public class TenantConfig : ITenantConfig
{
    public Dictionary<string, string> Tenants { get; set; } = new();
}
public interface ISaaSConfig
{
    public string AccountId { get; set; }
    public string ConfigFilePath { get; set; }
}

public class SaaSConfig : ISaaSConfig
{
    public string AccountId { get; set; }
    public string ConfigFilePath { get; set; }
}
public class ServiceConfig
{
    public Dictionary<string, Api> Apis { get; set; }
    public Dictionary<string, string> AssetUris { get; set; }
}

public class Api
{
    public int SecurityLevel { get; set; }
    public Dictionary<string,string> ApiUris { get; set; }
}

public interface ICognitoConfig
{
    public string IdentityPoolId { get; set; }
    public string Region { get; set; }
    public string UserPoolId { get; set; }
    public string UserPoolClientId { get; set; }
}

public class CognitoConfig : ICognitoConfig
{
    public string IdentityPoolId { get; set; }
    public string Region { get; set; }
    public string UserPoolId { get; set; }
    public string UserPoolClientId { get; set; }
}

public interface IRunConfig
{
    public string Apis { get; set; }
    public string Assets { get; set; }
    public string BaseURL { get; set; }
    public string Tenant { get; set; }
}

public class RunConfig : NotifyBase, IRunConfig
{
    public string[] AllowedApis { get; set; } = { "ApiGateway", "CloudFront", "Local", "LocalAndroid" };
    private string _apis = "CloudFront";
    public string Apis 
    {
        get { return _apis; } 
        set { SetProperty(ref _apis, value); } 
    }
    public string[] AllowedAssets { get; set; } = { "CloudFront", "Local" };
    private string _assets = "CloudFront";
    public string Assets 
    { 
        get { return _assets; }
        set {  SetProperty(ref _assets, value); }   
    }
    private string _baseURL = string.Empty;
    public string BaseURL
    {
        get { return _baseURL; }
        set { SetProperty(ref _baseURL, value); }
    }

    private string _tenant = string.Empty; 
    public string Tenant
    {
        get { return _tenant; }
        set { SetProperty(ref _tenant, value); }
    }
}




