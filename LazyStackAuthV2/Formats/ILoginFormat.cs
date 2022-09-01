using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace LazyStackAuthV2;

public interface ILoginFormat
{
    public IEnumerable<string> CheckLoginFormat(string password);
}
