using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;


namespace LazyStackAuthV2;

public class PasswordFormat : IPasswordFormat
{
    public IEnumerable<string> CheckPasswordFormat(string password)
    {
        //Todo - use messages from appConfig

        if (!Regex.IsMatch(password, @"[A-Z]"))
            yield return "AuthFormatMessages_Password01";

        if (!Regex.IsMatch(password, @"[a-z]"))
            yield return "AuthFormatMessages_Password02";

        if (!Regex.IsMatch(password, @"[0-9]"))
            yield return "AuthFormatMessages_Password03";

        if (password.Length < 8)
            yield return "AuthFormatMessages_Password04";
    }
}
