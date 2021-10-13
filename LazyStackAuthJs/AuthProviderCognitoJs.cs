using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LazyStackAuth; 

/// <summary>
/// AWS Authentication and Authorization Strategy
/// AWS Cognito User Pools are used for Authentication
/// AWS Cognito Identity Pools are used for Authorization
/// This implementation uses the AWS javascript cognito libs
/// This is designed  to be used with Blazor Interop
/// </summary>
namespace LazyStackAuthJs
{

    public class AwsCurrrentCredentials
    {
        [JsonPropertyName("accessKeyId")]
        public string AccessKeyId;
        [JsonPropertyName("authenticated")]
        public bool Authenticated;
        [JsonPropertyName("identityId")]
        public string IdentityId;
        [JsonPropertyName("identityId")]
        public string SecretAccessKey;
        [JsonPropertyName("secretAccessKey")]
        public string SessionToken;
    }

    /// <summary>
    /// Implements IAuthProvider using AWS Cognito as authentication provider
    /// 
    /// </summary>
    public class AuthProviderCognitoJs : IAuthProviderCognitoJs
    {
        public AuthProviderCognitoJs(
            IConfiguration appConfig, 
            IServiceProvider serviceProvider,
            ILoginFormat loginFormat,
            IPasswordFormat passwordFormat,
            IEmailFormat emailFormat,
            IPhoneFormat phoneFormat,
            ICodeFormat codeFormat,
            ILogger<AuthProviderCognitoJs> logger,
            string stackName = "Aws")
        {
            regionEndpoint = appConfig[$"{stackName}:Region"];
            clientId = appConfig[$"{stackName}:ClientId"];
            userPoolId = appConfig[$"{stackName}:UserPoolId"];
            identityPoolId = appConfig[$"{stackName}:IdentityPoolId"];
            this.serviceProvider = serviceProvider;
            this.loginFormat = loginFormat;
            this.passwordFormat = passwordFormat;
            this.emailFormat = emailFormat;
            this.phoneFormat = phoneFormat;
            this.codeFormat = codeFormat;
            this.logger = logger;
            logger.LogInformation("AuthProviderCognitoJs constructor");
        }

        #region AWS specific Fields
        private readonly string clientId;
        private readonly string userPoolId;
        private readonly string identityPoolId;
        private readonly string regionEndpoint;
        private IJSRuntime jsRuntime;
        private IJSObjectReference jsModule;

        private IServiceProvider serviceProvider;

        //private readonly AmazonCognitoIdentityProviderClient providerClient; 
        private readonly string userPool;
        private bool isConfigured = false; // set to true if Auth.configure() succeeds. See Init() method.

        private string authFlowResponse; // cleared after interim use

        // Format Support 
        public ILoginFormat loginFormat { get; private set; }
        public IPasswordFormat passwordFormat { get; private set; }
        public IEmailFormat emailFormat { get; private set; }
        public IPhoneFormat phoneFormat { get; private set; }
        public ICodeFormat codeFormat { get; private set; }

        #endregion Fields

        #region AWS specific Properties

        public string IpIdentity { get; set; } // Identity Pool Identity.
        public string UpIdentity { get; set; } // User Pool Identity. ie: JWT "sub" claim
        #endregion

        #region Fields
        private string login; // set by VerifyLogin
        private string newLogin; // set by VerifyNewLogin
        private string password; // set by VerifyPassword
        private string newPassword; // set by VerifyNewPassword
        private string email; // set by VerifyEmail
        private string newEmail; // set by VerifyNewEmail
        private string phone; // set by VerifyPhone
        private string newPhone; // set by VerifyNewPhone
        private string code; // set by VerifyCode 
        private ILogger<AuthProviderCognitoJs> logger;
        #endregion

        #region Properties
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


        public async Task<string> GetJWTAsync()
        {
            try
            {
                return await jsModule.InvokeAsync<string>("LzAuth.getIdToken");
            } catch (JSException e)
            {
                return string.Empty;
            }
        }

        public async Task<Creds> GetCredsAsync()
        {
            try
            {
                AwsCurrrentCredentials creds = await jsModule.InvokeAsync<AwsCurrrentCredentials>("LzAuth.currentCredentials");
                return new Creds()
                {
                    AccessKey = creds.AccessKeyId,
                    SecretKey = creds.SecretAccessKey,
                    Token = creds.SessionToken
                };

            } catch (JSException e)
            {
                return new Creds();
            }
        }

        public bool IsLoginFormatOk { get; private set; }
        public bool IsLoginVerified { get; private set; }

        public bool IsNewLoginFormatOk { get; private set; }
        public bool IsNewLoginVerified { get; private set; }

        public bool IsEmailFormatOk { get; private set; }
        public bool IsEmailVerified { get; private set; }

        public bool IsNewEmailFormatOk { get; private set; }
        public bool IsNewEmailVerified { get; private set; }

        public bool IsPasswordFormatOk { get; private set; }
        public bool IsPasswordVerified { get; private set; }

        public bool IsNewPasswordFormatOk { get; private set; }
        public bool IsNewPasswordVerified { get; private set; }

        public bool IsPhoneFormatOk { get; private set; }
        public bool IsPhoneVerified { get; private set; }

        public bool IsNewPhoneFormatOk { get; private set; }
        public bool IsNewPhoneVerified { get; private set; }

        public bool IsCodeFormatOk { get; private set; }
        public bool IsCodeVerified { get; private set; }

        public bool IsCleared { get { return !IsPasswordFormatOk && !IsNewPasswordFormatOk && !IsCodeFormatOk; } }

        public bool IsSignedIn { get; private set; }

        public bool HasChallenge { get { 
                return AuthChallengeList.Count > 0; 
            } }

        public bool CanSignOut => IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanSignIn => !IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanSignUp => !IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanResetPassword => !IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanUpdateLogin => false; // not supported in AWS Cognito
        public bool CanUpdateEmail => IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanUpdatePassword => IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanUpdatePhone => false; // not currently implemented
        public bool CanCancel => CurrentAuthProcess != AuthProcessEnum.None;
        public bool CanResendCode => CurrentChallenge == AuthChallengeEnum.Code;

        public bool IsChallengeLongWait
        {
            get
            {
                switch (CurrentChallenge)
                {
                    case AuthChallengeEnum.Code:
                        return true;

                    case AuthChallengeEnum.Password:
                        return CurrentAuthProcess == AuthProcessEnum.SigningIn;

                    case AuthChallengeEnum.Email:
                        return false;

                    case AuthChallengeEnum.Login:
                        return false;

                    case AuthChallengeEnum.NewEmail:
                        return true;

                    case AuthChallengeEnum.NewLogin:
                        return true;

                    case AuthChallengeEnum.NewPassword:
                        return true;

                    case AuthChallengeEnum.NewPhone:
                        return true;

                    case AuthChallengeEnum.None:
                        return false;
                }
                return true;
            }
        } // will the current challenge do a server roundtrip?

        private string[] _formatMessages = { };
        public string[] FormatMessages { get { return _formatMessages; } private set { _formatMessages = value; } }

        public string FormatMessage
        {
            get
            {
                return (FormatMessages?.Length > 0) ? FormatMessages[0] : "";
            }
        }

        public string LanguageCode { get; set; } = "en-US";

        #endregion Properties

        #region Init methods

        public void InitJsRuntime()
        {
            if (jsRuntime == null)
                jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
        }

        public async Task Init()
        {

            if (jsRuntime != null && !isConfigured)
            {
                try 
                {
                    if (jsModule == null)
                        jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/PiiSliceApp.js");

                    var config = new Dictionary<string, string>()
                    {
                            //{ "identityPoolId", identityPoolId },
                            { "region",regionEndpoint },
                            { "identityPoolRegion",regionEndpoint },
                            { "userPoolId", userPoolId },
                            { "userPoolWebClientId", clientId },
                            { "mandatorySignIn","true" },
                            { "authenticationFlowType","USER_SRP_AUTH" }
                    };

                    await jsModule.InvokeVoidAsync("LzAuth.configure", config);
                    isConfigured = true;

                }
                catch (JSException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                catch (Exception e)
                {
                    //todo - do we need a new AuthEvent?
                    //or do we want to just let the error perculate all the way up?
                    throw;
                }
            }
        }
        #endregion


        #region Challenge Flow Methods -- affect AuthChallengeList or IsAuthorized

        private void ClearSensitiveFields()
        {
            login = newLogin = string.Empty;
            password = newPassword = string.Empty;
            email = newEmail = string.Empty;
            phone = newPhone = string.Empty;
            code = string.Empty;
            IsLoginVerified = false;
            IsNewLoginVerified = false;
            IsEmailVerified = false;
            IsNewEmailVerified = false;
            IsPasswordVerified = false;
            IsNewPasswordVerified = false;
            IsPhoneVerified = false;
            IsNewPhoneVerified = false;
            IsCodeVerified = false;
            _formatMessages = new string[] { };
        }

        public virtual async void InternalClearAsync()
        {
            await Init();
            ClearSensitiveFields();
            AuthChallengeList.Clear();
            authFlowResponse = null;
            IsSignedIn = false;
            CurrentAuthProcess = AuthProcessEnum.None;
        }

        public virtual async Task<AuthEventEnum> ClearAsync()
        {
            await Init();
            InternalClearAsync();
            return AuthEventEnum.Cleared;
        }

        public virtual async Task<AuthEventEnum> SignOutAsync()
        {
            await Init();
            InternalClearAsync();
            return AuthEventEnum.SignedOut;
        }

        // Cancel the currently executing auth process
        public virtual async Task<AuthEventEnum> CancelAsync()
        {
            await Init();
            switch (CurrentAuthProcess)
            {
                case AuthProcessEnum.None:
                case AuthProcessEnum.SigningIn:
                case AuthProcessEnum.SigningUp:
                case AuthProcessEnum.ResettingPassword:
                    InternalClearAsync();
                    return AuthEventEnum.Canceled;

                default:
                    ClearSensitiveFields();
                    AuthChallengeList.Clear();
                    CurrentAuthProcess = AuthProcessEnum.None;
                    return AuthEventEnum.Canceled;
            }
        }

        public virtual async Task<AuthEventEnum> StartSignInAsync()
        {
            await Init();
            if (IsSignedIn)
                return AuthEventEnum.Alert_AlreadySignedIn;

            InternalClearAsync();
            CurrentAuthProcess = AuthProcessEnum.SigningIn;
            AuthChallengeList.Add(AuthChallengeEnum.Login);
            AuthChallengeList.Add(AuthChallengeEnum.Password);
            return AuthEventEnum.AuthChallenge;
        }

        public virtual async Task<AuthEventEnum> StartSignUpAsync()
        {
            if (IsSignedIn)
                return AuthEventEnum.Alert_AlreadySignedIn;

            await ClearAsync(); // calls Init() as well

            CurrentAuthProcess = AuthProcessEnum.SigningUp;

            AuthChallengeList.Add(AuthChallengeEnum.Login);
            AuthChallengeList.Add(AuthChallengeEnum.Password);
            AuthChallengeList.Add(AuthChallengeEnum.Email);
            return AuthEventEnum.AuthChallenge;
        }

        public virtual async Task<AuthEventEnum> StartResetPasswordAsync()
        {
            await Init();

            if (IsSignedIn)
                return AuthEventEnum.Alert_InvalidOperationWhenSignedIn;

            CurrentAuthProcess = AuthProcessEnum.ResettingPassword;

            AuthChallengeList.Add(AuthChallengeEnum.Login);
            AuthChallengeList.Add(AuthChallengeEnum.NewPassword);
            return AuthEventEnum.AuthChallenge;

        }

        public virtual async Task<AuthEventEnum> StartUpdateLoginAsync()
        {
            await Init();
            return AuthEventEnum.Alert_OperationNotSupportedByAuthProvider;
        }

        public virtual async Task<AuthEventEnum> StartUpdateEmailAsync()
        {
            await Init();
            if (!IsSignedIn)
                return AuthEventEnum.Alert_NeedToBeSignedIn;

            CurrentAuthProcess = AuthProcessEnum.UpdatingEmail;
            AuthChallengeList.Add(AuthChallengeEnum.NewEmail);
            return AuthEventEnum.AuthChallenge;
        }

        public virtual async Task<AuthEventEnum> StartUpdatePhoneAsync()
        {
            await Init();
            return AuthEventEnum.Alert_InternalProcessError;
        }

        public virtual async Task<AuthEventEnum> StartUpdatePasswordAsync()
        {
            await Init();
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

            await Init();

            try
            {
                switch (CurrentAuthProcess)
                {
                    case AuthProcessEnum.SigningIn:
                        if (IsSignedIn)
                            return AuthEventEnum.Alert_AlreadySignedIn;
                        this.login = login;
                        IsLoginVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Login);
                        return await NextChallenge();

                    case AuthProcessEnum.SigningUp:
                        if (IsSignedIn)
                            return AuthEventEnum.Alert_AlreadySignedIn;
                        this.login = login;
                        IsLoginVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Login);
                        return await NextChallenge();

                    case AuthProcessEnum.ResettingPassword:
                        if (IsSignedIn)
                            return AuthEventEnum.Alert_InvalidOperationWhenSignedIn;
                        this.login = login;
                        IsLoginVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Login);
                        return await NextChallenge();

                    default:
                        return AuthEventEnum.Alert_InternalProcessError;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyLogin() threw an exception {e}");
                return AuthEventEnum.Alert_Unknown;
            }
        }

        public virtual async Task<AuthEventEnum> VerifyNewLoginAsync(string newLogin)
        {
            await Init();
            return AuthEventEnum.Alert_OperationNotSupportedByAuthProvider;
        }

        public virtual async Task<AuthEventEnum> VerifyPasswordAsync(string password)
        {
            if (CurrentChallenge != AuthChallengeEnum.Password)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            if (!CheckPasswordFormat(password))
                return AuthEventEnum.Alert_PasswordFormatRequirementsFailed;

            await Init();

            try
            {
                switch (CurrentAuthProcess)
                {
                    case AuthProcessEnum.SigningIn:
                        if (IsSignedIn)
                            return AuthEventEnum.Alert_AlreadySignedIn;
                        if (!IsLoginVerified)
                            return AuthEventEnum.Alert_LoginMustBeSuppliedFirst;
                        this.password = password;
                        IsPasswordVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Password);
                        return await NextChallenge();

                    case AuthProcessEnum.SigningUp:
                        if (IsSignedIn)
                            return AuthEventEnum.Alert_AlreadySignedIn;
                        this.password = password;
                        IsPasswordVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Password);
                        return await NextChallenge();

                    case AuthProcessEnum.UpdatingPassword:
                        if (!IsSignedIn)
                            return AuthEventEnum.Alert_NeedToBeSignedIn;
                        this.password = password;
                        IsPasswordVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Password);
                        return await NextChallenge();

                    default:
                        return AuthEventEnum.Alert_InternalProcessError;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyPassword() threw an exception {e}");
                return AuthEventEnum.Alert_Unknown;
            }

        }

        public virtual async Task<AuthEventEnum> VerifyNewPasswordAsync(string newPassword)
        {
            if (CurrentChallenge != AuthChallengeEnum.NewPassword)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            if (!CheckPasswordFormat(newPassword))
                return AuthEventEnum.Alert_PasswordFormatRequirementsFailed;

            await Init();

            try
            {
                switch (CurrentAuthProcess)
                {
                    case AuthProcessEnum.SigningUp:
                        AuthChallengeList.Remove(AuthChallengeEnum.NewPassword);
                        return await NextChallenge();

                    case AuthProcessEnum.ResettingPassword:
                        this.newPassword = newPassword;
                        await jsModule.InvokeVoidAsync("LzAuth.forgotPassword", login);
                        AuthChallengeList.Remove(AuthChallengeEnum.NewPassword);
                        AuthChallengeList.Add(AuthChallengeEnum.Code);
                        return await NextChallenge();

                    case AuthProcessEnum.UpdatingPassword:
                        this.newPassword = newPassword;
                        await jsModule.InvokeVoidAsync("LzAuth.changePassword", password, newPassword);
                        AuthChallengeList.Remove(AuthChallengeEnum.NewPassword);
                        return await NextChallenge();

                    default:
                        return AuthEventEnum.Alert_InternalProcessError;
                }
            }
            catch (JSException e)
            {
                return GetAuthEventEnumForJsError(e);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyPassword() threw an exception {e}");
                return AuthEventEnum.Alert_Unknown;
            }
        }

        public virtual async Task<AuthEventEnum> VerifyEmailAsync(string email)
        {
            if (CurrentChallenge != AuthChallengeEnum.Email)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            if (!CheckEmailFormat(email))
                return AuthEventEnum.Alert_EmailFormatRequirementsFailed;

            await Init();

            try
            {
                switch (CurrentAuthProcess)
                {
                    case AuthProcessEnum.SigningUp:
                        IsEmailVerified = true;
                        this.email = email;
                        AuthChallengeList.Remove(AuthChallengeEnum.Email);
                        return await NextChallenge();

                    default:
                        return AuthEventEnum.Alert_InternalProcessError;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"UpdateEmail() threw an exception {e}");
                return AuthEventEnum.Alert_Unknown;
            }
        }

        public virtual async Task<AuthEventEnum> VerifyNewEmailAsync(string newEmail)
        {
            if (CurrentChallenge != AuthChallengeEnum.NewEmail)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            if (!CheckEmailFormat(newEmail))
                return AuthEventEnum.Alert_EmailFormatRequirementsFailed;

            await Init();

            try
            {
                switch (CurrentAuthProcess)
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
                        if (this.email.Equals(newEmail)) //todo - check
                        {
                            return AuthEventEnum.Alert_EmailAddressIsTheSame;
                        }

                        // Update the user email on the server
                        // Cognito sends a auth code when the Email attribute is changed
                        var attr = new Dictionary<string, string>() { { "email", newEmail } };
                        await jsModule.InvokeVoidAsync("LzAuth.updateUserAttributes", attr);
                        AuthChallengeList.Remove(AuthChallengeEnum.NewEmail);
                        IsEmailVerified = true;
                        return await NextChallenge();

                    default:
                        return AuthEventEnum.Alert_InternalProcessError;
                }
            }
            catch(JSException e)
            {
                return GetAuthEventEnumForJsError(e);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"UpdateEmail() threw an exception {e}");
                return AuthEventEnum.Alert_Unknown;
            }
        }

        public virtual async Task<AuthEventEnum> VerifyPhoneAsync(string phone)
        {
            if (CurrentChallenge != AuthChallengeEnum.Phone)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            if (!CheckPhoneFormat(phone))
                return AuthEventEnum.Alert_PhoneFormatRequirementsFailed;

            await Init();

            AuthChallengeList.Remove(AuthChallengeEnum.Phone);
            return await NextChallenge();
        }

        public virtual async Task<AuthEventEnum> VerifyNewPhoneAsync(string newPhone)
        {
            if (CurrentChallenge != AuthChallengeEnum.NewPhone)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            if (!CheckPhoneFormat(newPhone))
                return AuthEventEnum.Alert_PhoneFormatRequirementsFailed;

            await Init();

            AuthChallengeList.Remove(AuthChallengeEnum.NewPhone);
            return await NextChallenge();
        }

        public virtual async Task<AuthEventEnum> VerifyCodeAsync(string code)
        {
            if (CurrentAuthProcess == AuthProcessEnum.None)
                return AuthEventEnum.Alert_NoActiveAuthProcess;

            if (CurrentChallenge != AuthChallengeEnum.Code)
                return AuthEventEnum.Alert_VerifyCalledButNoChallengeFound;

            await Init();

            try
            {
                switch (CurrentAuthProcess)
                {
                    case AuthProcessEnum.None:
                        return AuthEventEnum.Alert_InternalProcessError;

                    case AuthProcessEnum.ResettingPassword:
                        // todo - call js
                        //await CognitoUser.ConfirmForgotPasswordAsync(code, newPassword).ConfigureAwait(false);
                        await jsModule.InvokeVoidAsync("LzAuth.forgotPasswordSubmit", login, code, newPassword);
                        AuthChallengeList.Remove(AuthChallengeEnum.Code);
                        return await NextChallenge();

                    case AuthProcessEnum.SigningUp:
                        await jsModule.InvokeVoidAsync("LzAuth.confirmSignUp", login, code);
                        IsCodeVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Code);
                        return await NextChallenge();

                    case AuthProcessEnum.SigningIn:
                        if (authFlowResponse == null) // authFlowResponse set during VerifyPassword
                            return AuthEventEnum.Alert_InternalSignInError;
                        AuthChallengeList.Remove(AuthChallengeEnum.Code);
                        return await NextChallenge();

                    case AuthProcessEnum.UpdatingEmail:
                        await jsModule.InvokeVoidAsync("LzAuth.verifyCurrentUserAttributeSubmit","email", code);
                        IsCodeVerified = true;
                        AuthChallengeList.Remove(AuthChallengeEnum.Code);
                        return await NextChallenge();

                    case AuthProcessEnum.UpdatingPhone:
                        return AuthEventEnum.Alert_InternalProcessError;

                    default:
                        return AuthEventEnum.Alert_InternalProcessError;

                }
            }
            catch(JSException e)
            {
                return GetAuthEventEnumForJsError(e);
            }
            catch (Exception e)
            {
                return AuthEventEnum.Alert_Unknown;
            }

        }

        public virtual async Task<AuthEventEnum> ResendCodeAsync()
        {
            if (CurrentChallenge != AuthChallengeEnum.Code)
                return AuthEventEnum.Alert_InvalidCallToResendAsyncCode;

            await Init();

            try
            {
                switch (CurrentAuthProcess)
                {
                    case AuthProcessEnum.UpdatingEmail:
                        // We need to re-submit the email change request for Amazon to resend the code
                        if (!IsSignedIn)
                            return AuthEventEnum.Alert_NeedToBeSignedIn;
                        var attributes = new Dictionary<string, string>() { { "email", email } };

                        await jsModule.InvokeVoidAsync("LzAuth.updateUserAttributes",attributes);

                        return AuthEventEnum.VerificationCodeSent;

                    case AuthProcessEnum.ResettingPassword:
                        await jsModule.InvokeVoidAsync("LzAuth.forgotPassword", login);
                        return AuthEventEnum.AuthChallenge;

                    case AuthProcessEnum.SigningUp:
                        await jsModule.InvokeVoidAsync("LzAuth.resendSignUp", login);
                        return AuthEventEnum.AuthChallenge;
                    default:
                        return AuthEventEnum.Alert_InvalidCallToResendAsyncCode;
                }

            }
            catch(JSException e)
            {
                return GetAuthEventEnumForJsError(e);
            }
            catch (Exception e)
            {
                return AuthEventEnum.Alert_Unknown;
            }
        }

        private async Task<AuthEventEnum> NextChallenge(AuthEventEnum lastAuthEventEnum = AuthEventEnum.AuthChallenge)
        {
            await Init();

            try
            {
                if (!HasChallenge)
                {
                    switch (CurrentAuthProcess)
                    {
                        case AuthProcessEnum.None:
                            return AuthEventEnum.Alert_NothingToDo;

                        case AuthProcessEnum.ResettingPassword:

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
                                // Request causes AWS to send Auth Code to user by email
                                var attr = new Dictionary<string, string>() { { "email", email } };
                                await jsModule.InvokeVoidAsync("LzAuth.signUp", login, password, attr);
                                if (!AuthChallengeList.Contains(AuthChallengeEnum.Code))
                                    AuthChallengeList.Add(AuthChallengeEnum.Code);

                                return AuthEventEnum.AuthChallenge;
                            }

                            CurrentAuthProcess = AuthProcessEnum.None;
                            ClearSensitiveFields();
                            return AuthEventEnum.SignedUp;

                        case AuthProcessEnum.SigningIn:
                            // await jsModule.InvokeVoidAsync("signIn", login, password);
                            await jsModule.InvokeVoidAsync("LzAuth.signIn", login, password);
                            IsSignedIn = true;
                            CurrentAuthProcess = AuthProcessEnum.None;
                            ClearSensitiveFields();
                            return AuthEventEnum.SignedIn;

                        case AuthProcessEnum.UpdatingEmail:
                            if (!IsCodeVerified)
                            {
                                AuthChallengeList.Add(AuthChallengeEnum.Code);
                                return AuthEventEnum.VerificationCodeSent;
                            }

                            CurrentAuthProcess = AuthProcessEnum.None;
                            ClearSensitiveFields();
                            return AuthEventEnum.EmailUpdateDone;

                        case AuthProcessEnum.UpdatingPassword:
                            await jsModule.InvokeVoidAsync("LzAuth.changePassword", password, newPassword);
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
            catch (JSException ex)
            {
                var msg = ex.Message;
                Debug.WriteLine(ex.Message);
                //return GetAuthEventEnumForJsError(e);
            }
            catch (Exception e)
            {
                string message = e.Message;

                return AuthEventEnum.Alert_Unknown;
            }

            return lastAuthEventEnum;
        }

        #endregion

        #region Non ChallengeFlow Methods - do not affect AuthChallengeList or IaAuthorized

        public bool CheckLoginFormat(string login)
        {
            //logger.LogDebug($"CheckLoginFormat called. login={login}");
            FormatMessages = loginFormat.CheckLoginFormat(login, LanguageCode).ToArray();
            //foreach (var msg in FormatMessages)
            //    logger.LogDebug($"FormatMessage[]={msg}");
            return IsLoginFormatOk = (FormatMessages.Length == 0);
        }

        public bool CheckEmailFormat(string email)
        {
            //Todo: Implement email content rules
            FormatMessages = emailFormat.CheckEmailFormat(email, LanguageCode).ToArray();
            return IsEmailFormatOk = FormatMessages.Length == 0;
        }

        public bool CheckPasswordFormat(string password)
        {
            FormatMessages = passwordFormat.CheckPasswordFormat(password, LanguageCode).ToArray();
            return IsPasswordFormatOk = (FormatMessages.Length == 0);
        }

        public bool CheckNewPasswordFormat(string password)
        {
            FormatMessages = passwordFormat.CheckPasswordFormat(password, LanguageCode).ToArray();
            return IsNewPasswordFormatOk = (FormatMessages.Length == 0);
        }

        public bool CheckCodeFormat(string code)
        {
            FormatMessages = codeFormat.CheckCodeFormat(code, LanguageCode).ToArray();
            return IsCodeFormatOk = FormatMessages.Length == 0;
        }

        public bool CheckPhoneFormat(string phone)
        {
            //todo - implement
            FormatMessages = phoneFormat.CheckPhoneFormat(phone, LanguageCode).ToArray();
            return IsPhoneFormatOk = FormatMessages.Length == 0;
        }

        private async Task NoOp()
        {
            await Task.Delay(0);
        }

        public virtual async Task<string> GetAccessToken()
        {
            await Init();

            try
            {
                return await jsRuntime.InvokeAsync<string>("LzAuth.getAccessToken");
            }
            catch 
            {
                return string.Empty;
            }

        }

        public virtual async Task<string> GetIdentityToken()
        {
            await Init();

            if (!string.IsNullOrEmpty(identityPoolId))
            { // Using Identity Pools

                //var credentials = new CognitoAWSCredentials(IdentityPoolId, RegionEndpoint);
                // todo - call js
                //CognitoAWSCredentials credentials = CognitoUser.GetCognitoAWSCredentials(identityPoolId, regionEndpoint);

                try
                {
                    // todo - call js
                    //var IpIdentity = await credentials.GetIdentityIdAsync();
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
                // todo - call js
                //if (CognitoUser.SessionTokens.IsValid())
                //    return CognitoUser.SessionTokens.IdToken;
                //if (await RefreshTokenAsync())
                //    return CognitoUser.SessionTokens.IdToken;
                //else
                    return null;
            }
        }

        private async Task<bool> RefreshTokenAsync()
        {
            await Init();
            
            try
            {

                // todo - call js
                //AuthFlowResponse context = await CognitoUser.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest
                //{
                //    AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
                //}).ConfigureAwait(false);

                return true;
            }
            catch 
            {
                return false;
            }
        }

        public virtual async Task<AuthEventEnum> RefreshUserDetailsAsync()
        {
            await Init();

            // todo - call js
            //if (CognitoUser == null)
            //    return AuthEventEnum.Alert_NeedToBeSignedIn;

            try
            {
                // Get the current user attributes from the server
                // and set UserEmail and IsUserEmailVerified
                // todo - call js
                //GetUserResponse getUserResponse = await CognitoUser.GetUserDetailsAsync().ConfigureAwait(false);
                //foreach (AttributeType item in getUserResponse.UserAttributes)
                //{
                //    if (item.Name.Equals("email"))
                //        email = item.Value;

                //    if (item.Name.Equals("email_verified"))
                //        IsEmailVerified = item.Value.Equals("true");
                //}
                return AuthEventEnum.Alert_RefreshUserDetailsDone;
            }
            catch(JSException e)
            {
                return GetAuthEventEnumForJsError(e);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"RefreshUserDetails threw an exception {e}");
                return AuthEventEnum.Alert_Unknown;
            }
        }

        #endregion

        private AuthEventEnum GetAuthEventEnumForJsError(JSException e)
        {
            string message = e.Message.Split((char)10)[0]; // we just want the first part of the message returned.
            Debug.WriteLine(message);
            switch (message)
            {
                case "UserNotFoundException": return AuthEventEnum.Alert_UserNotFound;
                case "UsernameExistsException": return AuthEventEnum.Alert_LoginAlreadyUsed;
                case "InvalidParameterException": return AuthEventEnum.Alert_InternalProcessError;
                case "InvalidPasswordException": return AuthEventEnum.Alert_PasswordFormatRequirementsFailed;
                case "TooManyRequestsException": return AuthEventEnum.Alert_TooManyAttempts;
                case "TooManyFailedAttemptsException": return AuthEventEnum.Alert_TooManyAttempts;
                case "NotAuthorizedException": return AuthEventEnum.Alert_NotAuthorized;
                case "UserNotConfirmedException": return AuthEventEnum.Alert_NotConfirmed;
                case "CodeMismatchException": return AuthEventEnum.Alert_VerifyFailed;
                case "AliasExistsException": return AuthEventEnum.Alert_AccountWithThatEmailAlreadyExists;
                case "LimitExceededException": return AuthEventEnum.Alert_LimitExceededException;
                default: return AuthEventEnum.Alert_Unknown;
            }
        }
    }
}

