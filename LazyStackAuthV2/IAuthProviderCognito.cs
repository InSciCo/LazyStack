using System;
using System.Collections.Generic;
using System.Text;
using Amazon.CognitoIdentity;
using Amazon.Extensions.CognitoAuthentication;

namespace LazyStackAuthV2;

public interface IAuthProviderCognito : IAuthProvider
{
    public CognitoUser CognitoUser { get; }
    public CognitoAWSCredentials Credentials { get; }
}
