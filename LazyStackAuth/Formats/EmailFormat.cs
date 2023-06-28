using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace LazyStackAuth;

public class EmailFormat : IEmailFormat
{
    public IEnumerable<string> CheckEmailFormat(string email)
    {
        string msg = null;

        try
        {
            var result = new MailAddress(email);
        }
        catch
        {
            msg = "AuthFormatMessages_Email01";
        }

        if (msg != null)
            yield return msg;
    }
}
