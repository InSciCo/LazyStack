using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;


namespace LazyStackAuth
{
    public class PasswordFormat : IPasswordFormat
    {
        public PasswordFormat (IConfiguration appConfig )
        {
            this.appConfig = appConfig;
        }

        protected IConfiguration appConfig;

        public IEnumerable<string> CheckPasswordFormat(string password)
        {
            //Todo - use messages from appConfig

            if (!Regex.IsMatch(password, @"[A-Z]"))
                yield return "Password must contain at least one capital letter";

            if (!Regex.IsMatch(password, @"[a-z]"))
                yield return "Password must contain at least one lowercase letter";

            if (!Regex.IsMatch(password, @"[0-9]"))
                yield return "Password must contain at least one digit";

            if (password.Length < 8)
                yield return "Password length must be at least 8 characters long.";
        }
    }
}
