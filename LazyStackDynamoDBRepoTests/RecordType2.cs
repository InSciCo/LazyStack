﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LazyStackDynamoDBRepoTests
{
    using System = global::System;

    // Note: Class definition should use same annoations as those generated by NSwag to ensure 
    // compatibility with the rest of the LazyStack toolchain
    class RecordType2
    {
        [Newtonsoft.Json.JsonProperty("id", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long Id { get; set; }

        [Newtonsoft.Json.JsonProperty("category", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Category { get; set; }

        [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Always)]
        public string Name { get; set; }
    }

}
