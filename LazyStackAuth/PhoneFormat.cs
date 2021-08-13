using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace LazyStackAuth
{
    public class PhoneFormat : IPhoneFormat
    {
        public PhoneFormat(IConfiguration appConfig)
        {
            this.appConfig = appConfig;
        }

        protected IConfiguration appConfig;

        public IEnumerable<string> CheckPhoneFormat(string phone)
        {
            //todo - use appConfig messages
            yield return "";
        }
    }
}
