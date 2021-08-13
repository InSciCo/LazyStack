using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace LazyStackAuth
{
    public class EmailFormat : IEmailFormat
    {
        public EmailFormat(IConfiguration appConfig)
        {
            this.appConfig = appConfig;
        }

        protected IConfiguration appConfig;

        public IEnumerable<string> CheckEmailFormat(string email)
        {
            //todo - use appConfig messages

            //if (string.IsNullOrEmpty(email))
            //    yield return "";

            string msg = string.Empty;

            try
            {
                var result = new MailAddress(email);
            }
            catch
            {
                msg = "Enter a valid email address.";
            }

            yield return msg;
        }
    }
}
