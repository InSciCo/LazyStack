using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LazyStackAuthTests
{
    public class SampleData
    {
        [Newtonsoft.Json.JsonProperty("field1", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Field1 { get; set; }
    }
}
