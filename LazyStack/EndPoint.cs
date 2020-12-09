using System;
using System.Collections.Generic;
using System.Text;

namespace LazyStack
{
    public class EndPoint
    {
        public EndPoint(string name, AwsApi api)
        {
            Name = name;
            Api = api;
        }

        public string Name { get; }
        public AwsApi Api { get; }
    }
}
