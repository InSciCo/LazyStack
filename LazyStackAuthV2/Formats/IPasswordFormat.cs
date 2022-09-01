using System.Collections.Generic;

namespace LazyStackAuthV2;

public interface IPasswordFormat
{
    IEnumerable<string> CheckPasswordFormat(string password);
}