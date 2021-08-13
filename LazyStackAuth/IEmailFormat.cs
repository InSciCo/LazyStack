using System.Collections.Generic;

namespace LazyStackAuth
{
    public interface IEmailFormat
    {
        IEnumerable<string> CheckEmailFormat(string email);
    }
}