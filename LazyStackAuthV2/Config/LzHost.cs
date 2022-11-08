using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyStackAuthV2;

public interface ILzHost
{
    string Url { get; set; }
    bool IsMAUI { get; set; }
    bool IsWASM { get; }
    bool IsAndroid { get; set; }    
}

// This class contains information about the host that the 
// application was loaded from. For a WASM app this is the 
// website hosting the WASM. This class is not currently used
// for MAUI apps. 
public class LzHost : ILzHost
{
    public LzHost(string? url = null, bool isMAUI = true, bool isAndroid = false)
    {
        Url = url;
        IsMAUI = isMAUI;
        IsAndroid = isAndroid;
    }

    public string Url { get; set; } 
    public bool IsMAUI { get; set; }
    public bool IsWASM => !IsMAUI;
    public bool IsAndroid { get; set; }

}
