using System;
using System.Collections.Generic;
using System.Text;
using Amazon.CognitoIdentity;
using Amazon.Extensions.CognitoAuthentication;

namespace LazyStackAuth
{
    public interface IAuthProviderCognito : IAuthProvider
    {
        public CognitoUser CognitoUser { get; }
        public CognitoAWSCredentials Credentials { get; }
    }
}
