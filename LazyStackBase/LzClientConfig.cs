using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LazyStackBase
{

    public interface ILzClientConfig
    {
        public string Profile { get; set; }
        public string Account { get; set; }
        public string Region { get; set; }
        public string StackName { get; set; }
        public Dictionary<string, JObject> Authenticators { get; set; }
        public string DefaultService { get; set; }
        public Dictionary<string, LzService> Services { get; set; }
        public LzRunConfig RunConfig { get; set; }
        public Dictionary<string, string> RelatedResources { get; set; }
        public JObject Resources { get; set; }
    }
    public class LzClientConfig  : ILzClientConfig
    {
        public string Profile { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string StackName { get; set; } = string.Empty;   
        public string Region { get; set; } = string.Empty;  
        public Dictionary<string, JObject> Authenticators { get; set; } = new();
        public string DefaultService { get; set; } = string.Empty;
        public Dictionary<string, LzService> Services { get; set; } = new();
        public LzRunConfig RunConfig { get; set; } = new();
        public Dictionary<string,string> RelatedResources { get; set; } = new();
        public JObject Resources { get; set; } = new();
    }
    public class LzService
    {
        public string Auth { get; set; } = string.Empty;
        public Dictionary<string, JObject> Resources { get; set; } = new();
    }
    public class LzRunConfig
    {
        public string Service { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
    }
}

