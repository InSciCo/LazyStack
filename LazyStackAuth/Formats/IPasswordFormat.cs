using System.Collections.Generic;

namespace LazyStackAuth;

public interface IPasswordFormat
{
    IEnumerable<string> CheckPasswordFormat(string password);
}