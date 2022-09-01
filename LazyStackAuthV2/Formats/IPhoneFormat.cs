using System.Collections.Generic;

namespace LazyStackAuthV2;

public interface IPhoneFormat
{
    IEnumerable<string> CheckPhoneFormat(string phone);
}