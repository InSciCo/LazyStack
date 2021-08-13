using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;


namespace LazyStackAuth
{
    public class CodeFormat : ICodeFormat
    {
        public CodeFormat(IConfiguration appConfig)
        {
            this.appConfig = appConfig;
        }

        protected IConfiguration appConfig;

        public IEnumerable<string> CheckCodeFormat(string code)
        {
            //todo - use appConfig messages

            if (code.Length < 6)
                yield return "Enter six digit code.";
        }
    }
}
