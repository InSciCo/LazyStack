using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyStackAuth;

public interface ICognitoConfig
{
    string IdentityPoolId { get; set; }
    string Region { get; set; }
    string UserPoolClientId { get; set; }
    string UserPoolId { get; set; }
}

public class CognitoConfig : ICognitoConfig
{
    public string IdentityPoolId { get; set; }
    public string Region { get; set; }
    public string UserPoolId { get; set; }
    public string UserPoolClientId { get; set; }
}

