using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LazyStackAuth;

namespace LazyStackAuthJs
{
    public interface IAuthProviderCognitoJs : IAuthProvider
    {
        public void InitJsRuntime();
        public Task Init();
    }

}
