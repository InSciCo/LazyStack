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
/// .ConfigureAwait(false) on all async calls.
/// 
/// </summary>
namespace LazyStackAuth
{
    /// <summary>
    /// Implements ICognitoAuthProvider
    /// 
    /// </summary>
    public class AuthProviderCognito : IAuthProvider
    { 
        public AuthProviderCognito(IConfiguration appConfig, string stackName = "Aws")
        {
            regionEndpoint = RegionEndpoint.GetBySystemName(appConfig[$"{stackName}:Region"]);
            clientId = appConfig[$"{stackName}:ClientId"];
            userPoolId = appConfig[$"{stackName}:UserPoolId"];
            identityPoolId = appConfig[$"{stackName}:IdentityPoolId"];
            providerClient = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), regionEndpoint);
            userPool = new CognitoUserPool(userPoolId, clientId, providerClient);
        }

        #region AWS specific Fields
        private readonly string clientId;
        private readonly string userPoolId;
        private readonly string identityPoolId;
        private readonly RegionEndpoint regionEndpoint;
        private readonly AmazonCognitoIdentityProviderClient providerClient;
        private readonly CognitoUserPool userPool;
        private AuthFlowResponse authFlowResponse; // cleared after interim use
        #endregion Fields

        #region AWS specific Properties
        public CognitoAWSCredentials Credentials { get; private set; }
        public CognitoUser CognitoUser { get; private set; } // This object contains the AccessToken etc. we get from Amazon
        #endregion

        #region Fields
        private string login; // set by VerifyLogin
        private string password; // set by VerifyPassword
        private string newPassword; // set by VerifyNewPassword
        private string email; // set by VerifyEmail
        private string phone; // set by VerifyPhone
        private string code; // set by VerifyCode
        #endregion

        #region Properties
        public string IpIdentity { get; set; } // Identity Pool Identity.
        public string UpIdentity { get; set; } // User Pool Identity. ie: JWT "sub" claim
        public bool IsSignedIn { get; private set; }
        public AuthProcessEnum CurrentAuthProcess { get; private set; }

        public List<AuthChallengeEnum> AuthChallengeList { get; } = new List<AuthChallengeEnum>();
        public AuthChallengeEnum CurrentChallenge
        {
            get
            {
                return (AuthChallengeList.Count > 0)
                  ? AuthChallengeList[0]
                  : AuthChallengeEnum.None;
            }
        }
        public bool HasChallenge { get { return AuthChallengeList.Count > 0; } }

        public bool IsLoginFormatOk { get; private set; }
        public bool IsLoginVerified { get; private set; }

        public bool IsPasswordFormatOk { get; private set; }
        public bool IsPasswordVerified { get; private set; }

        public bool IsCodeFormatOk { get; private set; }
        public bool IsCodeVerified { get; private set; }

        public bool IsNewPasswordFormatOk { get; private set; }
        public bool IsNewPasswordVerified { get; private set; }

        public bool IsEmailFormatOk { get; private set; }
        public bool IsEmailVerified { get; private set; }

        public bool IsPhoneFormatOk { get; private set; }
        public bool IsPhoneVerified { get; private set; }
        
        public bool IsCleared { get { return !IsPasswordFormatOk && !IsNewPasswordFormatOk && !IsCodeFormatOk; } }

        public string Login { get { return login; } }
        public string Email { get { return email; } }
        public string Phone { get { return phone; } }
        #endregion Properties

        #region Challenge Flow Methods -- affect AuthChallengeList or IsAuthorized

        public virtual AuthEventEnum Clear()
        {
            ClearSensitiveFields();
            login = string.Empty;
            email = string.Empty;
            phone = string.Empty; 
            CognitoUser = null;
            AuthChallengeList.Clear();
            authFlowResponse = null;
            IsSignedIn = false;
            CurrentAuthProcess = AuthProcessEnum.None;
            return AuthEventEnum.Alert_NotAuthorized;
        }

        public virtual AuthEventEnum SignOut()
        {
            login = string.Empty;
            Clear();
            return AuthEventEnum.SignedOut;
        } 

        // Cancel the currently executing auth process
        public virtual AuthEventEnum Cancel()
        {
            switch(CurrentAuthProcess)
            {
                case AuthProcessEnum.None:
                    return AuthEventEnum.Canceled;
                case AuthProcessEnum.ResettingPassword:
                case AuthProcessEnum.SigningIn:
                case AuthProcessEnum.SigningUp:
                    return Clear();
                case AuthProcessEnum.UpdatingEmail:
                case AuthProcessEnum.UpdatingPassword:
                case AuthProcessEnum.UpdatingPhone:
                    CurrentAuthProcess = AuthProcessEnum.None;
                    return AuthEventEnum.Canceled;
                default:
                    return AuthEventEnum.Canceled;
            }
        }


        public virtual async Task<AuthEventEnum> StartSignInAsync()
        {
            await NoOp(); // used when the method doesn't call service

            if (IsSignedIn)
                return AuthEventEnum.Alert_AlreadySignedIn;

            Clear();

            CurrentAuthProcess = AuthProcessEnum.SigningIn;
            AuthChallengeList.Add(AuthChallengeEnum.Login);
            AuthChallengeList.Add(AuthChallengeEnum.Password);
            return AuthEventEnum.AuthChallenge;
        }

        public virtual async Task<AuthEventEnum> StartSignUpAsync()
        {
            await NoOp(); // used when the method doesn't call service

            if (IsSignedIn)
                return AuthEventEnum.Alert_AlreadySignedIn;

            Clear();

            CurrentAuthProcess = AuthProcessEnum.SigningUp;

            AuthChallengeList.Add(AuthChallengeEnum.Login);
            AuthChallengeList.Add(AuthChallengeEnum.Password);
            AuthChallengeList.Add(AuthChallengeEnum.Email);
            return AuthEventEnum.AuthChallenge;
        }

        public virtual async Task<AuthEventEnum> StartResetPasswordAsync()
        {
            await NoOp(); // In aws implementation we don't do a round trip to server for VerifyLogin

            if (IsSignedIn)
                return AuthEventEnum.Alert_InvalidOperationWhenSignedIn;

            Clear();

            CurrentAuthProcess = AuthProcessEnum.ResettingPassword;

            AuthChallengeList.Add(AuthChallengeEnum.Login);
            AuthChallengeList.Add(AuthChallengeEnum.NewPassword);
            return AuthEventEnum.AuthChallenge;

        }

        public virtual async Task<AuthEventEnum> StartUpdateEmailAsync()
        {
            await NoOp(); // used when the method doesn't call service

            if (!IsSignedIn)
                return AuthEventEnum.Alert_NeedToBeSignedIn;

            CurrentAuthProcess = AuthProcessEnum.UpdatingEmail;

            AuthChallengeList.Add(AuthChallengeEnum.Email);
            return AuthEventEnum.AuthChallenge;
        }

        public virtual async Task<AuthEventEnum> StartUpdatePhoneAsync()
        {
            await NoOp(); // used when the method doesn't call service
            return AuthEventEnum.Alert_InternalProcessError;
        }

        public virtual async Task<AuthEventEnum> StartUpdatePasswordAsync()
        {
            await NoOp(); // used when the method doesn't call service

            if (!IsSignedIn)
                return AuthEventEnum.Alert_NeedToBeSignedIn;

            CurrentAuthProcess = AuthProcessEnum.UpdatingPassword;

            AuthChallengeList.Add(AuthChallengeEnum.Password);
            AuthChallengeList.Add(AuthChallengeEnum.NewPassword);
            return AuthEventEnum.AuthChallenge;
        }

        public virtual async Task<AuthEventEnum> VerifyLoginAsync(string login)
        {
            if (CurrentChallenge != AuthChallengeEnum.Login)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            if (!CheckLoginFormat(login)) 
                return AuthEventEnum.Alert_LoginFormatRequirementsFailed;

            try
            {
                switch (CurrentAuthProcess)
                {
                    case AuthProcessEnum.SigningIn:
                        if (IsSignedIn)
                            return AuthEventEnum.Alert_AlreadySignedIn;
                        // We don't expect this to ever throw an exception as the AWS operation is local
                        CognitoUser = new CognitoUser(login, clientId, userPool, providerClient);
                        this.login = login;
                        IsLoginVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Login);
                        return await NextChallenge();
                
                    case AuthProcessEnum.SigningUp:
                        if (IsSignedIn)
                            return AuthEventEnum.Alert_AlreadySignedIn;
                        // We don't expect this to ever throw an exception
                        this.login = login;
                        IsLoginVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Login);
                        return await NextChallenge();

                    case AuthProcessEnum.ResettingPassword:
                        if (IsSignedIn)
                            return AuthEventEnum.Alert_AlreadySignedIn;
                        // This step may throw an exception if the call to ForgotPasswordAsync fails.
                        CognitoUser = new CognitoUser(login, clientId, userPool, providerClient);
                        await CognitoUser.ForgotPasswordAsync().ConfigureAwait(false);
                        this.login = login;
                        IsLoginVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Login);
                        return await NextChallenge();

                    default:
                        return AuthEventEnum.Alert_InternalProcessError;
                }
            }
            catch (TooManyRequestsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyLogin() threw an exception {e}");
                CognitoUser = null;
                return AuthEventEnum.Alert_Unknown;
            }
        }

        public virtual async Task<AuthEventEnum> VerifyPasswordAsync(string password)
        {
            if (CurrentChallenge != AuthChallengeEnum.Password)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            if (!CheckPasswordFormat(password))
                return AuthEventEnum.Alert_PasswordFormatRequirementsFailed;

            try
            {
                switch (CurrentAuthProcess)
                {
                    case AuthProcessEnum.SigningIn:
                        if (IsSignedIn)
                            return AuthEventEnum.Alert_AlreadySignedIn;
                        // When using Cognito, we must have already verified login to create the CognitoUser
                        // before we can collect password and attempt to sign in.
                        if (!IsLoginVerified)
                            return AuthEventEnum.Alert_LoginMustBeSuppliedFirst;
                        // This step may throw an exception in the AWS StartWithSrpAuthAsync call.
                        // AWS exceptions for sign in are a bit hard to figure out in some cases.
                        // Depending on the UserPool setup, AWS may request an auth code. This would be 
                        // detected and handled in the NextChallenge() call. 
                        authFlowResponse = await CognitoUser.StartWithSrpAuthAsync(
                            new InitiateSrpAuthRequest()
                            {
                                Password = password
                            }
                            ).ConfigureAwait(false);
                        this.password = password;
                        IsPasswordVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Password);
                        return await NextChallenge();

                    case AuthProcessEnum.SigningUp:
                        if (IsSignedIn)
                            return AuthEventEnum.Alert_AlreadySignedIn;
                        // We don't expect this step to throw an exception
                        this.password = password;
                        IsPasswordVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Password);
                        return await NextChallenge();

                    case AuthProcessEnum.UpdatingPassword:
                        if (!IsSignedIn)
                            return AuthEventEnum.Alert_NeedToBeSignedIn;
                        // We don't expect this step to throw an exception
                        this.password = password;
                        IsPasswordVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Password);
                        return await NextChallenge();

                    default:
                        return AuthEventEnum.Alert_InternalProcessError;
                }
            }
            catch (InvalidPasswordException) { return AuthEventEnum.Alert_PasswordFormatRequirementsFailed; }
            catch (TooManyRequestsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (NotAuthorizedException) { return AuthEventEnum.Alert_NotAuthorized; }
            catch (UserNotFoundException) { return AuthEventEnum.Alert_UserNotFound; }
            catch (UserNotConfirmedException) { return AuthEventEnum.Alert_NotConfirmed; }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyPassword() threw an exception {e}");
                CognitoUser = null;
                return AuthEventEnum.Alert_Unknown;
            }

        }

        public virtual async Task<AuthEventEnum> VerifyEmailAsync(string email)
        {
            if (CurrentChallenge != AuthChallengeEnum.Email)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            if (!CheckEmailFormat(email))
                return AuthEventEnum.Alert_EmailFormatRequirementsFailed;

            try
            {
                switch(CurrentAuthProcess)
                {
                    case AuthProcessEnum.UpdatingEmail:
                        if (!IsSignedIn)
                            return AuthEventEnum.Alert_NeedToBeSignedIn;
                        // Get the current values on the server. 
                        // This step may throw an exception in RefreshUserDetailsAsync. There seems to be
                        // no way to recover from this other than retry or abandon the process. Let the
                        // calling class figure out what is right for their usecase.
                        AuthEventEnum refreshUserDetailsResult = await RefreshUserDetailsAsync().ConfigureAwait(false);
                        if (refreshUserDetailsResult != AuthEventEnum.Alert_RefreshUserDetailsDone)
                            return AuthEventEnum.Alert_CantRetrieveUserDetails;

                        // make sure the values are different
                        if (this.email.Equals(email)) //todo - check
                        {
                            return AuthEventEnum.Alert_EmailAddressIsTheSame;
                        }

                        // Update the user email on the server
                        // This may throw an exception in the UpdateAttributesAsync call.
                        var attributes = new Dictionary<string, string>() { { "email", email } };
                        // Cognito sends a auth code when the Email attribute is changed
                        await CognitoUser.UpdateAttributesAsync(attributes).ConfigureAwait(false);

                        AuthChallengeList.Remove(AuthChallengeEnum.Email);
                        IsEmailVerified = true;
                        AuthChallengeList.Add(AuthChallengeEnum.Code);
                        return await NextChallenge();

                    case AuthProcessEnum.SigningUp:
                        IsEmailVerified = true;
                        this.email = email;
                        AuthChallengeList.Remove(AuthChallengeEnum.Email);
                        return await NextChallenge();

                    default:
                        return AuthEventEnum.Alert_InternalProcessError;
                }
            }
            catch (TooManyRequestsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (Exception e)
            {
                Debug.WriteLine($"UpdateEmail() threw an exception {e}");
                return AuthEventEnum.Alert_Unknown;
            }

        }

        public virtual async Task<AuthEventEnum> VerifyCodeAsync(string code)
        {
            if (CurrentAuthProcess == AuthProcessEnum.None)
                return AuthEventEnum.Alert_NoActiveAuthProcess;

            if (CurrentChallenge != AuthChallengeEnum.Code)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            try
            {
                switch (CurrentAuthProcess)
                {
                    case AuthProcessEnum.None:
                        return AuthEventEnum.Alert_InternalProcessError;

                    case AuthProcessEnum.ResettingPassword:
                        await CognitoUser.ConfirmForgotPasswordAsync(code, newPassword).ConfigureAwait(false);
                        AuthChallengeList.Remove(AuthChallengeEnum.Code);
                        return await NextChallenge();

                    case AuthProcessEnum.SigningUp:
                        var result = await providerClient.ConfirmSignUpAsync(
                            new ConfirmSignUpRequest
                            {
                                ClientId = clientId,
                                Username = login,
                                ConfirmationCode = code
                            }).ConfigureAwait(false);

                        IsCodeVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Code);
                        return await NextChallenge();

                    case AuthProcessEnum.SigningIn:
                        if (authFlowResponse == null) // authFlowResponse set during VerifyPassword
                            return AuthEventEnum.Alert_InternalSignInError;

                        authFlowResponse = await CognitoUser.RespondToSmsMfaAuthAsync(
                            new RespondToSmsMfaRequest()
                            {
                                SessionID = authFlowResponse.SessionID,
                                MfaCode = code
                            }
                            ).ConfigureAwait(false);

                        AuthChallengeList.Remove(AuthChallengeEnum.Code);
                        return await NextChallenge();

                    case AuthProcessEnum.UpdatingEmail:
                        await CognitoUser.VerifyAttributeAsync("email", code).ConfigureAwait(false);
                        IsCodeVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Code);
                        return await NextChallenge();

                    case AuthProcessEnum.UpdatingPhone:
                        return AuthEventEnum.Alert_InternalProcessError;

                    default:
                        return AuthEventEnum.Alert_InternalProcessError;

                }
            }
            catch (InvalidPasswordException) { return AuthEventEnum.Alert_PasswordFormatRequirementsFailed; }
            catch (TooManyRequestsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (NotAuthorizedException) { return AuthEventEnum.Alert_NotAuthorized; }
            catch (UserNotFoundException) { return AuthEventEnum.Alert_UserNotFound; }
            catch (UserNotConfirmedException) { return AuthEventEnum.Alert_NotConfirmed; }
            catch (CodeMismatchException) { return AuthEventEnum.Alert_VerifyFailed; }
            catch (AliasExistsException) { return AuthEventEnum.Alert_AccountWithThatEmailAlreadyExists; }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyCode() threw an exception {e}");
                CognitoUser = null;
                return AuthEventEnum.Alert_Unknown;
            }

        }

        public virtual async Task<AuthEventEnum> VerifyNewPasswordAsync(string newPassword)
        {
            if (CurrentChallenge != AuthChallengeEnum.NewPassword)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            if (!CheckPasswordFormat(newPassword))
                return AuthEventEnum.Alert_PasswordFormatRequirementsFailed;

            try
            {
                switch(CurrentAuthProcess)
                {
                    case AuthProcessEnum.SigningUp:
                        authFlowResponse = await CognitoUser.RespondToNewPasswordRequiredAsync(
                            new RespondToNewPasswordRequiredRequest()
                            {
                                SessionID = authFlowResponse.SessionID,
                                NewPassword = newPassword
                            }
                            ).ConfigureAwait(false);

                        this.newPassword = newPassword;
                        AuthChallengeList.Remove(AuthChallengeEnum.NewPassword);
                        return await NextChallenge();

                    case AuthProcessEnum.ResettingPassword:
                        this.newPassword = newPassword;
                        CognitoUser user = new CognitoUser(login, clientId, userPool, providerClient);
                        await user.ForgotPasswordAsync().ConfigureAwait(false); //todo - is this wrong?
                        AuthChallengeList.Remove(AuthChallengeEnum.NewPassword);
                        AuthChallengeList.Add(AuthChallengeEnum.Code);
                        return await NextChallenge();

                    case AuthProcessEnum.UpdatingPassword:
                        this.newPassword = newPassword;
                        AuthChallengeList.Remove(AuthChallengeEnum.NewPassword);
                        return await NextChallenge();

                    default:
                        return AuthEventEnum.Alert_InternalProcessError;
                }
            }
            catch (InvalidPasswordException) { return AuthEventEnum.Alert_PasswordFormatRequirementsFailed; }
            catch (TooManyRequestsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (NotAuthorizedException) { return AuthEventEnum.Alert_NotAuthorized; }
            catch (UserNotFoundException) { return AuthEventEnum.Alert_UserNotFound; }
            catch (UserNotConfirmedException) { return AuthEventEnum.Alert_NotConfirmed; }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyPassword() threw an exception {e}");
                CognitoUser = null;
                return AuthEventEnum.Alert_Unknown;
            }
        }

        public virtual async Task<AuthEventEnum> VerifyPhoneAsync(string phone)
        {
            if (CurrentChallenge != AuthChallengeEnum.Phone)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            if (!CheckPhoneFormat(phone))
                return AuthEventEnum.Alert_PhoneFormatRequirementsFailed;

            AuthChallengeList.Remove(AuthChallengeEnum.Phone);
            return await NextChallenge();
        }


        public virtual async Task<AuthEventEnum> ResendCodeAsync()
        {
            if (CognitoUser != null)
                return AuthEventEnum.Alert_InvalidOperationWhenSignedIn;

            //if (CurrentChallenge != AuthChallenges.SignUp)
            //    return AuthModuleEvent.SignUpNotStarted;

            try
            {
                switch(CurrentAuthProcess)
                {
                    case AuthProcessEnum.SigningUp:
                    case AuthProcessEnum.UpdatingEmail:
                    case AuthProcessEnum.ResettingPassword:
                        _ = await providerClient.ResendConfirmationCodeAsync(
                            new ResendConfirmationCodeRequest
                            {
                                ClientId = clientId,
                                Username = login
                            }).ConfigureAwait(false);

                        return AuthEventEnum.AuthChallenge;
                    default:
                        return AuthEventEnum.Alert_InvalidCallToResendAsyncCode;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine($"SignUp() threw an exception {e}");
                return AuthEventEnum.Alert_Unknown;
            }
        }

        //private async Task<bool> CheckForAWSChallenges()
        //{

        //    if (authFlowResponse.AuthenticationResult != null)
        //        return;

        //    if (authFlowResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED) // Update Passsword
        //    {
        //        if (!AuthChallengeList.Contains(AuthChallengeEnum.NewPassword))
        //            AuthChallengeList.Add(AuthChallengeEnum.NewPassword);
        //    }
        //    else
        //    if (authFlowResponse.ChallengeName == ChallengeNameType.SMS_MFA) // Multi-factor auth
        //    {
        //        if (!AuthChallengeList.Contains(AuthChallengeEnum.Code))
        //            AuthChallengeList.Add(AuthChallengeEnum.Code);
        //    }
        //}

        private async Task<AuthEventEnum> NextChallenge(AuthEventEnum lastAuthEventEnum = AuthEventEnum.AuthChallenge)
        {
            try
            {
                if (!HasChallenge)
                {
                    switch (CurrentAuthProcess)
                    {
                        case AuthProcessEnum.None:
                            return AuthEventEnum.Alert_NothingToDo;

                        case AuthProcessEnum.ResettingPassword:

                            if (authFlowResponse != null && authFlowResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED) // Update Passsword
                            {
                                if (!AuthChallengeList.Contains(AuthChallengeEnum.NewPassword))
                                    AuthChallengeList.Add(AuthChallengeEnum.NewPassword);
                                authFlowResponse = null;
                                return AuthEventEnum.AuthChallenge;
                            }

                            CurrentAuthProcess = AuthProcessEnum.None;
                            ClearSensitiveFields();
                            return AuthEventEnum.PasswordResetDone;

                        case AuthProcessEnum.SigningUp:

                            if (HasChallenge)
                                return AuthEventEnum.AuthChallenge;

                            if (!IsLoginFormatOk)
                                AuthChallengeList.Add(AuthChallengeEnum.Login);
                            else
                            if (!IsPasswordFormatOk) 
                                AuthChallengeList.Add(AuthChallengeEnum.Password);
                            else
                            if (!IsEmailFormatOk) 
                                AuthChallengeList.Add(AuthChallengeEnum.Email);

                            if (HasChallenge)
                                return AuthEventEnum.AuthChallenge;

                            if (!IsCodeVerified)
                            {
                                // Request Auth Code
                                var signUpRequest = new SignUpRequest()
                                {
                                    ClientId = clientId,
                                    Password = password,
                                    Username = login
                                };

                                signUpRequest.UserAttributes.Add(
                                    new AttributeType()
                                    {
                                        Name = "email",
                                        Value = email
                                    });

                                // This call may throw an exception
                                var result = await providerClient.SignUpAsync(signUpRequest).ConfigureAwait(false);

                                if (!AuthChallengeList.Contains(AuthChallengeEnum.Code))
                                    AuthChallengeList.Add(AuthChallengeEnum.Code);

                                return AuthEventEnum.AuthChallenge;
                            }

                            CurrentAuthProcess = AuthProcessEnum.None;
                            ClearSensitiveFields();
                            return AuthEventEnum.SignedUp;

                        case AuthProcessEnum.SigningIn:
                            if (authFlowResponse != null && authFlowResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED) // Update Passsword
                            {
                                if (!AuthChallengeList.Contains(AuthChallengeEnum.NewPassword))
                                    AuthChallengeList.Add(AuthChallengeEnum.NewPassword);
                                authFlowResponse = null;
                                return AuthEventEnum.AuthChallenge;
                            }

                            // Grab JWT from login to User Pools to extract User Pool Identity
                            //var token = new JwtSecurityToken(jwtEncodedString: CognitoUser.SessionTokens.IdToken);
                            //UpIdentity = token.Claims.First(c => c.Type == "sub").Value; // JWT sub cliam contains User Pool Identity

                            //// Note: creates Identity Pool identity if it doesn't exist
                            //Credentials = CognitoUser.GetCognitoAWSCredentials(identityPoolId, regionEndpoint);
                            //IpIdentity = await Credentials.GetIdentityIdAsync(); // Identity Pool Identity
                            IsSignedIn = true;
                            CurrentAuthProcess = AuthProcessEnum.None;
                            ClearSensitiveFields();
                            return AuthEventEnum.SignedIn;

                        case AuthProcessEnum.UpdatingEmail:
                            CurrentAuthProcess = AuthProcessEnum.None;
                            ClearSensitiveFields();
                            return AuthEventEnum.EmailUpdateDone;

                        case AuthProcessEnum.UpdatingPassword:
                            await CognitoUser.ChangePasswordAsync(password, newPassword).ConfigureAwait(false);
                            CurrentAuthProcess = AuthProcessEnum.None;
                            ClearSensitiveFields();
                            return AuthEventEnum.PasswordUpdateDone;

                        case AuthProcessEnum.UpdatingPhone:
                            CurrentAuthProcess = AuthProcessEnum.None;
                            ClearSensitiveFields();
                            return AuthEventEnum.PhoneUpdateDone;
                    }
                }
            }
            catch (UsernameExistsException) { return AuthEventEnum.Alert_LoginAlreadyUsed; }
            catch (InvalidParameterException) { return AuthEventEnum.Alert_InternalProcessError; }
            catch (InvalidPasswordException) { return AuthEventEnum.Alert_PasswordFormatRequirementsFailed; }
            catch (TooManyRequestsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthEventEnum.Alert_TooManyAttempts; }
            catch (Exception e)
            {
                Debug.WriteLine($"SignUp() threw an exception {e}");
                return AuthEventEnum.Alert_Unknown;
            }

            return lastAuthEventEnum;
        }

        #endregion

        #region Non ChallengeFlow Methods - do not affect AuthChallengeList or IaAuthorized

        public bool CheckLoginFormat(string login)
        {
            //Todo: Implement name content rules
            return IsLoginFormatOk = login.Length > 5;
        }

        public bool CheckEmailFormat(string email)
        {
            //Todo: Implement email content rules
            return IsEmailFormatOk = email.Length > 3 && email.Contains("@");
        }

        public bool CheckPasswordFormat(string password)
        {
            //Todo: Implement password content rules
            return IsPasswordFormatOk = password.Length >= 8;
        }

        public bool CheckNewPasswordFormat(string password)
        {
            return IsNewPasswordFormatOk = password.Length >= 8;
        }

        public bool CheckCodeFormat(string code)
        {
            // Todo: Implement Code content rules
            return IsCodeFormatOk = code.Length > 4;
        }

        public bool CheckPhoneFormat(string phone)
        {
            //todo - implement
            return IsPhoneFormatOk = false;
        }

        private void ClearSensitiveFields()
        {
            password = string.Empty;
            newPassword = string.Empty;
            code = string.Empty;
        }

        private async Task NoOp()
        {
            await Task.Yield(); // best implementation I know of Better than await Task.Delay(0);
        }

        public virtual async Task<string> GetAccessToken()
        {
            if (CognitoUser == null)
                return null;
            if (CognitoUser.SessionTokens.IsValid())
                return CognitoUser.SessionTokens.AccessToken;
            if(await RefreshTokenAsync())
                return CognitoUser.SessionTokens.AccessToken;
            else
                return null;
        }

        public virtual async Task<string> GetIdentityToken()
        {
            if (CognitoUser == null)
                return null; // Need to authenticate user first!

            if (!string.IsNullOrEmpty(identityPoolId))
            { // Using Identity Pools

                //var credentials = new CognitoAWSCredentials(IdentityPoolId, RegionEndpoint);
                CognitoAWSCredentials credentials = CognitoUser.GetCognitoAWSCredentials(identityPoolId, regionEndpoint);

                try
                {
                    var IpIdentity = await credentials.GetIdentityIdAsync();
                    Debug.WriteLine($" IpIdentity {IpIdentity}");

                    return IpIdentity;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{e.Message}");
                    return null;
                }
            }
            else // Using UserPools directly
            {
                if (CognitoUser.SessionTokens.IsValid())
                    return CognitoUser.SessionTokens.IdToken;
                if (await RefreshTokenAsync())
                    return CognitoUser.SessionTokens.IdToken;
                else
                    return null;
            }
        }

        private async Task<bool> RefreshTokenAsync()
        {
            if (CognitoUser == null)
                return false;

            try
            {
                AuthFlowResponse context = await CognitoUser.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest
                {
                    AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
                }).ConfigureAwait(false);

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"RefreshToken() threw an exception {e}");
                return false;
            }
        }

        public virtual async Task<AuthEventEnum> RefreshUserDetailsAsync()
        {
            if (CognitoUser == null)
                return AuthEventEnum.Alert_NeedToBeSignedIn;

            try
            {
                // Get the current user attributes from the server
                // and set UserEmail and IsUserEmailVerified
                GetUserResponse getUserResponse = await CognitoUser.GetUserDetailsAsync().ConfigureAwait(false);
                foreach (AttributeType item in getUserResponse.UserAttributes)
                {
                    if (item.Name.Equals("email"))
                        email = item.Value;

                    if (item.Name.Equals("email_verified"))
                        IsEmailVerified = item.Value.Equals("true");
                }
                return AuthEventEnum.Alert_RefreshUserDetailsDone;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"RefreshUserDetails threw an exception {e}");
                return AuthEventEnum.Alert_Unknown;
            }
        }

        #endregion
    }
}

