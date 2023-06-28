using System.Collections.Generic;

namespace LazyStackAuth;

public interface ICodeFormat
{
    IEnumerable<string> CheckCodeFormat(string code);
}
