using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LazyStackAuthV2;

namespace LazyStackAuthJs
{
    public interface IAuthProviderCognitoJs : IAuthProvider
    {
        //public void InitJsRuntime();
        public Task Init();
    }

}
