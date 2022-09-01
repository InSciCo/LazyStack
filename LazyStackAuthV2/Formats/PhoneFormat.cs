using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace LazyStackAuthV2;

public class PhoneFormat : IPhoneFormat
{
    public PhoneFormat(IConfiguration appConfig)
    {
        this.appConfig = appConfig;
    }

    protected IConfiguration appConfig;

    public IEnumerable<string> CheckPhoneFormat(string phone)
    {
        if (false) // todo - create a ruleset for phone numbers
            yield return "AuthFormatMessages_Phone01";
    }
}
