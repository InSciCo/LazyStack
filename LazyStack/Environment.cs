using System;
using System.Collections.Generic;
using System.Text;

namespace LazyStack
{
    public class Environment
    {
        // Add environments in LazyStack.yaml
        public string ProfileName { get; set; } // required
        public string RegionName { get; set; } // required
        public string StackName { get; set; } // Defaults to SolutionName<envName>
        public string Stage { get; set; } // Defaults to envName
        public string Domain { get; set; } = "amazonaws.com";
        public string UriCodeTarget { get; set; } = "Debug";
        public string UriCodePlatform { get; set; } = "netcoreapp3.1";
        public bool IncludeLocalApis { get; set; } = false;
        public int LocalApiPort { get; set; } = 5001;
    }
}
