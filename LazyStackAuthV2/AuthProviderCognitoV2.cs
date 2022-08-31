using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;

/// <summary>
/// AWS Authentication and Authorization Strategy
/// AWS Cognito User Pools are used for Authentication
/// AWS Cognito Identity Pools are used for Authorization
/// 
/// API AuthModule - AWS Cognito Implementation
/// This code make use of the AWS SDK for .NET https://github.com/aws/aws-sdk-net/
/// specifically the AWSSDK.CognitoIdentity, AWSSDK.CognitoIdentityProvider 
/// and AWSSDK.Extensions.CognitoAuthentication packages.
/// 
/// References
/// https://www.nuget.org/packages/AWSSDK.Extensions.CognitoAuthentication.
/// https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/cognito-apis-intro.html
/// https://aws.amazon.com/blogs/developer/cognitoauthentication-extension-library-developer-preview
/// https://github.com/aws/aws-sdk-net-extensions-cognito/tree/master/src/Amazon.Extensions.CognitoAuthentication
///
/// For more on CognitoIdentityProvider see: 
/// https://github.com/aws/aws-sdk-net/ 
/// https://docs.aws.amazon.com/cognito/latest/developerguide/cognito-reference.html
/// https://aws.amazon.com/blogs/mobile/
/// https://aws.amazon.com/blogs/mobile/sign-up-and-confirm-with-user-pools-using-csharp/
/// https://aws.amazon.com/blogs/mobile/tracking-and-remembering-devices-using-amazon-cognito-your-user-pools/
///
/// Here are a few simple blogs that shows the bare basics
/// http://www.mcraesoft.com/authenticating-to-aws-cognito-from-windows/
/// 
/// Remember Device docs
/// https://aws.amazon.com/blogs/mobile/tracking-and-remembering-devices-using-amazon-cognito-your-user-pools/
/// 
/// A note about .ConfigureAwait()
/// None of the methods in this class use the UI context so we use
/// .ConfigureAwait(false) on all async calls into Cognito libs.
/// 
/// </summary>
namespace LazyStackAuthV2;

/// <summary>
/// Implements IAuthProvider using AWS Cognito as authentication provider
/// 
/// </summary>
public class AuthProviderCognito : AuthProviderCognitoBase
{
    public AuthProviderCognito(
        ILoginFormat loginFormat,
        IPasswordFormat passwordFormat,
        IEmailFormat emailFormat,
        ICodeFormat codeFormat,
        IPhoneFormat phoneFormat,
        IStacksConfig stacksConfig

    )
    {
        this.loginFormat = loginFormat;
        this.passwordFormat = passwordFormat;
        this.emailFormat = emailFormat;
        this.codeFormat = codeFormat;
        this.phoneFormat = phoneFormat;
        this.stacksConfig = stacksConfig;
        SetStack();
    }


}

