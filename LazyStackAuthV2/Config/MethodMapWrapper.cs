using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyStackAuthV2;

public interface IMethodMapWrapper
{
    Dictionary<string, string> MethodMap { get; }
}

public class MethodMapWrapper : IMethodMapWrapper
{
    public Dictionary<string, string> MethodMap { get; init; }
}
