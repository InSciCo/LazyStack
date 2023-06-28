using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;


namespace LazyStackAuth;

public class CodeFormat : ICodeFormat
{

    public IEnumerable<string> CheckCodeFormat(string code)
    {
        if (code.Length != 6)
            yield return "AuthFormatMessages_Code01";
    }
}
