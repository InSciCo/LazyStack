using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyStackAuthV2;

public interface ILzHost
{
    string Url { get; set; }
}

// This class contains information about the host that the 
// application was loaded from. For a WASM app this is the 
// website hosting the WASM. This class is not currently used
// for MAUI apps. 
public class LzHost : ILzHost
{
    public LzHost(string? url = null)
    {
        Url = url;  
    }

    public string Url { get; set; } 

}
