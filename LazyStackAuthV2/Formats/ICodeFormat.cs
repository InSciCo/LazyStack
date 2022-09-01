using System.Collections.Generic;

namespace LazyStackAuthV2;

public interface ICodeFormat
{
    IEnumerable<string> CheckCodeFormat(string code);
}
