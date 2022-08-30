﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyStackAuth;

public interface IMethodMap
{
	Dictionary<string, string> Map { get; }
}

public class MethodMap : IMethodMap
{
	public Dictionary<string, string> Map { get; init; }
}
