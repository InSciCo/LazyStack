using System.Collections.Generic;

namespace LazyStackAuth
{
    public interface IPhoneFormat
    {
        IEnumerable<string> CheckPhoneFormat(string phone, string languageCode);
    }
}