//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Threading.Tasks;

//using Microsoft.Extensions.Configuration;

//using Amazon;
//using Amazon.CognitoIdentity;
//using Amazon.CognitoIdentityProvider;
//using Amazon.CognitoIdentityProvider.Model;
//using Amazon.Extensions.CognitoAuthentication;
//using Amazon.Runtime;

///// <summary>
///// AWS Authentication and Authorization Strategy
///// AWS Cognito User Pools are used for Authentication
///// AWS Cognito Identity Pools are used for Authorization
///// 
///// API AuthModule - AWS Cognito Implementation
///// This code make use of the AWS SDK for .NET https://github.com/aws/aws-sdk-net/
///// specifically the AWSSDK.CognitoIdentity, AWSSDK.CognitoIdentityProvider 
///// and AWSSDK.Extensions.CognitoAuthentication packages.
///// 
///// References
///// https://www.nuget.org/packages/AWSSDK.Extensions.CognitoAuthentication.
///// https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/cognito-apis-intro.html
///// https://aws.amazon.com/blogs/developer/cognitoauthentication-extension-library-developer-preview
///// https://github.com/aws/aws-sdk-net-extensions-cognito/tree/master/src/Amazon.Extensions.CognitoAuthentication
/////
///// For more on CognitoIdentityProvider see: 
///// https://github.com/aws/aws-sdk-net/ 
///// https://docs.aws.amazon.com/cognito/latest/developerguide/cognito-reference.html
///// https://aws.amazon.com/blogs/mobile/
///// https://aws.amazon.com/blogs/mobile/sign-up-and-confirm-with-user-pools-using-csharp/
///// https://aws.amazon.com/blogs/mobile/tracking-and-remembering-devices-using-amazon-cognito-your-user-pools/
/////
///// Here are a few simple blogs that shows the bare basics
///// http://www.mcraesoft.com/authenticating-to-aws-cognito-from-windows/
///// 
///// Remember Device docs
///// https://aws.amazon.com/blogs/mobile/tracking-and-remembering-devices-using-amazon-cognito-your-user-pools/
///// 
///// A note about .ConfigureAwait()
///// None of the methods in this class use the UI context so we use
///// .ConfigureAwait(false) on all async calls into Cognito libs.
///// 
///// </summary>
//namespace LazyStackAuthV2;

///// <summary>
///// Implements IAuthProvider using AWS Cognito as authentication provider
///// 
///// </summary>
//public class AuthProviderCognito : IAuthProviderCognito
//{
//    public AuthProviderCognito(
//        ILoginFormat loginFormat,
//        IPasswordFormat passwordFormat,
//        IEmailFormat emailFormat,
//        ICodeFormat codeFormat,
//        IPhoneFormat phoneFormat,
//        IStacksConfig stacksConfig

//    )
//    {
//        this.loginFormat = loginFormat;
//        this.passwordFormat = passwordFormat;
//        this.emailFormat = emailFormat;
//        this.codeFormat = codeFormat;
//        this.phoneFormat = phoneFormat;
//        this.stacksConfig = stacksConfig;

//        foreach(var kvp in stacksConfig.Stacks)
//        {
//            authProviders.Add(kvp.Key, new AuthProviderCognitoBase(
//                loginFormat,
//                passwordFormat,
//                emailFormat,
//                codeFormat,
//                phoneFormat,
//                kvp.Value.CognitoConfig
//                ));
//        }

//    }

//    private ILoginFormat loginFormat;
//    private IPasswordFormat passwordFormat;
//    private IEmailFormat emailFormat;
//    private ICodeFormat codeFormat;
//    private IPhoneFormat phoneFormat;
//    private IStacksConfig stacksConfig;

//    private Dictionary<string, IAuthProviderCognito> authProviders = new();
//    private IAuthProviderCognito currentAuthProvider { get { return authProviders[stacksConfig.CurrentStackName];  } }

//    CognitoUser IAuthProviderCognito.CognitoUser => currentAuthProvider.CognitoUser;

//    CognitoAWSCredentials IAuthProviderCognito.Credentials => currentAuthProvider.Credentials;

//    List<AuthChallengeEnum> IAuthProvider.AuthChallengeList => currentAuthProvider.AuthChallengeList;

//    AuthChallengeEnum IAuthProvider.CurrentChallenge => currentAuthProvider.CurrentChallenge;

//    AuthProcessEnum IAuthProvider.CurrentAuthProcess => currentAuthProvider.CurrentAuthProcess;

//    bool IAuthProvider.IsLoginFormatOk => currentAuthProvider.IsLoginFormatOk;

//    bool IAuthProvider.IsLoginVerified => currentAuthProvider.IsLoginVerified;

//    bool IAuthProvider.IsNewLoginFormatOk => currentAuthProvider.IsNewEmailFormatOk;

//    bool IAuthProvider.IsNewLoginVerified => currentAuthProvider.IsNewEmailVerified;

//    bool IAuthProvider.IsEmailFormatOk => currentAuthProvider.IsEmailFormatOk;

//    bool IAuthProvider.IsEmailVerified => currentAuthProvider.IsEmailVerified;

//    bool IAuthProvider.IsNewEmailFormatOk => currentAuthProvider.IsNewEmailFormatOk;

//    bool IAuthProvider.IsNewEmailVerified => currentAuthProvider.IsNewEmailVerified;

//    bool IAuthProvider.IsPasswordFormatOk => currentAuthProvider.IsPasswordFormatOk;

//    bool IAuthProvider.IsPasswordVerified => currentAuthProvider.IsPasswordVerified;

//    bool IAuthProvider.IsNewPasswordFormatOk => currentAuthProvider.IsNewPasswordFormatOk;

//    bool IAuthProvider.IsNewPasswordVerified => currentAuthProvider.IsNewPasswordVerified;

//    bool IAuthProvider.IsPhoneFormatOk => currentAuthProvider.IsPhoneFormatOk;

//    bool IAuthProvider.IsPhoneVerified => currentAuthProvider.IsPhoneVerified;

//    bool IAuthProvider.IsNewPhoneFormatOk => currentAuthProvider.IsNewPhoneFormatOk;

//    bool IAuthProvider.IsNewPhoneVerified => currentAuthProvider.IsNewPhoneVerified;

//    bool IAuthProvider.IsCodeFormatOk => currentAuthProvider.IsCodeFormatOk;

//    bool IAuthProvider.IsCodeVerified => currentAuthProvider.IsCodeVerified;

//    bool IAuthProvider.IsCleared => currentAuthProvider.IsCleared;

//    bool IAuthProvider.IsSignedIn => currentAuthProvider.IsSignedIn;

//    bool IAuthProvider.HasChallenge => currentAuthProvider.HasChallenge;

//    string[] IAuthProvider.FormatMessages => currentAuthProvider.FormatMessages;

//    string IAuthProvider.FormatMessage => currentAuthProvider.FormatMessage;

//    bool IAuthProvider.CanSignOut => currentAuthProvider.CanSignOut;

//    bool IAuthProvider.CanSignUp => currentAuthProvider.CanSignUp;

//    bool IAuthProvider.CanSignIn => currentAuthProvider.CanSignIn;

//    bool IAuthProvider.CanResetPassword => currentAuthProvider.CanResetPassword;

//    bool IAuthProvider.CanUpdateLogin => currentAuthProvider.CanUpdateLogin;

//    bool IAuthProvider.CanUpdateEmail => currentAuthProvider.CanUpdateEmail;

//    bool IAuthProvider.CanUpdatePassword => currentAuthProvider.CanUpdatePassword;

//    bool IAuthProvider.CanUpdatePhone => currentAuthProvider.CanUpdatePhone;

//    bool IAuthProvider.CanCancel => currentAuthProvider.CanCancel;

//    bool IAuthProvider.CanResendCode => currentAuthProvider.CanResendCode;

//    bool IAuthProvider.IsChallengeLongWait => currentAuthProvider.IsChallengeLongWait;

//    void IAuthProvider.SetStack()
//    {
//        currentAuthProvider.SetStack();
//    }

//    Task<AuthEventEnum> IAuthProvider.ClearAsync()
//    {
//        return currentAuthProvider.ClearAsync();
//    }

//    Task<AuthEventEnum> IAuthProvider.CancelAsync()
//    {
//        return currentAuthProvider.CancelAsync();
//    }

//    Task<AuthEventEnum> IAuthProvider.SignOutAsync()
//    {
//        return currentAuthProvider.SignOutAsync();
//    }

//    Task<AuthEventEnum> IAuthProvider.StartSignInAsync()
//    {
//        return currentAuthProvider.StartSignInAsync();
//    }

//    Task<AuthEventEnum> IAuthProvider.StartSignUpAsync()
//    {
//        return currentAuthProvider.StartSignUpAsync();
//    }

//    Task<AuthEventEnum> IAuthProvider.StartResetPasswordAsync()
//    {
//        return currentAuthProvider.StartResetPasswordAsync();
//    }

//    Task<AuthEventEnum> IAuthProvider.StartUpdateLoginAsync()
//    {
//        return currentAuthProvider.StartUpdateLoginAsync();
//    }

//    Task<AuthEventEnum> IAuthProvider.StartUpdateEmailAsync()
//    {
//        return currentAuthProvider.StartUpdateEmailAsync();
//    }

//    Task<AuthEventEnum> IAuthProvider.StartUpdatePhoneAsync()
//    {
//        return currentAuthProvider.StartUpdatePhoneAsync();
//    }

//    Task<AuthEventEnum> IAuthProvider.StartUpdatePasswordAsync()
//    {
//        return currentAuthProvider.StartUpdatePasswordAsync();
//    }

//    Task<AuthEventEnum> IAuthProvider.VerifyLoginAsync(string login)
//    {
//        return currentAuthProvider.VerifyLoginAsync(login);
//    }

//    Task<AuthEventEnum> IAuthProvider.VerifyNewLoginAsync(string newLogin)
//    {
//        return currentAuthProvider.VerifyNewLoginAsync(newLogin);
//    }

//    Task<AuthEventEnum> IAuthProvider.VerifyPasswordAsync(string password)
//    {
//        return currentAuthProvider.VerifyPasswordAsync(password);
//    }

//    Task<AuthEventEnum> IAuthProvider.VerifyNewPasswordAsync(string newPassword)
//    {
//        return currentAuthProvider.VerifyNewPasswordAsync(newPassword);
//    }

//    Task<AuthEventEnum> IAuthProvider.VerifyEmailAsync(string email)
//    {
//        return currentAuthProvider.VerifyEmailAsync(email);
//    }

//    Task<AuthEventEnum> IAuthProvider.VerifyNewEmailAsync(string newEmail)
//    {
//        return currentAuthProvider.VerifyNewEmailAsync(newEmail);
//    }

//    Task<AuthEventEnum> IAuthProvider.VerifyPhoneAsync(string phone)
//    {
//        return currentAuthProvider.VerifyPhoneAsync(phone);
//    }

//    Task<AuthEventEnum> IAuthProvider.VerifyNewPhoneAsync(string newPhone)
//    {
//        return currentAuthProvider.VerifyNewPhoneAsync(newPhone);
//    }

//    Task<AuthEventEnum> IAuthProvider.VerifyCodeAsync(string code)
//    {
//        return currentAuthProvider.VerifyCodeAsync(code);
//    }

//    Task<AuthEventEnum> IAuthProvider.ResendCodeAsync()
//    {
//        return currentAuthProvider.ResendCodeAsync();
//    }

//    Task<AuthEventEnum> IAuthProvider.RefreshUserDetailsAsync()
//    {
//        return currentAuthProvider.RefreshUserDetailsAsync();
//    }

//    bool IAuthProvider.CheckLoginFormat(string login)
//    {
//        return currentAuthProvider.CheckLoginFormat(login);
//    }

//    bool IAuthProvider.CheckEmailFormat(string email)
//    {
//        return currentAuthProvider.CheckEmailFormat(email);
//    }

//    bool IAuthProvider.CheckPasswordFormat(string password)
//    {
//        return currentAuthProvider.CheckPasswordFormat(password);
//    }

//    bool IAuthProvider.CheckNewPasswordFormat(string password)
//    {
//        return currentAuthProvider.CheckNewPasswordFormat(password);
//    }

//    bool IAuthProvider.CheckPhoneFormat(string phone)
//    {
//        return currentAuthProvider.CheckPhoneFormat(phone);
//    }

//    bool IAuthProvider.CheckCodeFormat(string code)
//    {
//        return currentAuthProvider.CheckCodeFormat(code);
//    }

//    Task<Creds> IAuthProvider.GetCredsAsync()
//    {
//        return currentAuthProvider.GetCredsAsync();
//    }

//    Task<string> IAuthProvider.GetJWTAsync()
//    {
//        return currentAuthProvider.GetJWTAsync();
//    }
//}

