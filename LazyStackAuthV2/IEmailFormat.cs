using System.Collections.Generic;

namespace LazyStackAuthV2;

public interface IEmailFormat
{
    IEnumerable<string> CheckEmailFormat(string email, string languageCode);
}