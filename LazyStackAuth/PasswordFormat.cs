﻿using System;
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

        public IEnumerable<string> CheckPasswordFormat(string password, string languageCode)
        {
            //Todo - use messages from appConfig

            if (!Regex.IsMatch(password, @"[A-Z]"))
                yield return appConfig[$"AuthFormatMessages:{languageCode}:Password01"];

            if (!Regex.IsMatch(password, @"[a-z]"))
                yield return appConfig[$"AuthFormatMessages:{languageCode}:Password02"];

            if (!Regex.IsMatch(password, @"[0-9]"))
                yield return appConfig[$"AuthFormatMessages:{languageCode}:Password03"];

            if (password.Length < 8)
                yield return appConfig[$"AuthFormatMessages:{languageCode}:Password04"];
        }
    }
}
