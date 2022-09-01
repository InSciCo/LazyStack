using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace LazyStackAuthV2;

public class LoginFormat : ILoginFormat
{
    public LoginFormat(IConfiguration appConfig)
    {
        this.appConfig = appConfig;
    }

    protected IConfiguration appConfig;

    /// <summary>
    /// Creates an enumeration with input requirements.
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    public IEnumerable<string> CheckLoginFormat(string login)
    {
        if (login.Length < 8)
            yield return "AuthFormatMessages_Login01";
    }
}
