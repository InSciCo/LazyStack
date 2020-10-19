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
using System.Data;

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
        public AuthProviderCognito(IConfiguration appConfig)
        {
            regionEndpoint = RegionEndpoint.GetBySystemName(appConfig["Aws:Region"]);
            clientId = appConfig["Aws:ClientId"];
            userPoolId = appConfig["Aws:UserPoolId"];
            identityPoolId = appConfig["Aws:IdentityPoolId"];
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
        #endregion

        #region Properties
        // Bindable Properties
        public string UserLogin { get; set; }
        public string UserEmail { get; set; }
        public string IpIdentity { get; set; } // Identity Pool Identity.
        public string UpIdentity { get; set; } // User Pool Identity. ie: JWT "sub" claim
        public bool IsUserEmailVerified { get; private set; }
        public bool IsAuthorized { get; private set; }
        public List<AuthChallenges> AuthChallenges { get; } = new List<AuthChallenges>();
        public AuthChallenges CurrentChallenge 
        {
            get
            {
                return (AuthChallenges.Count > 0)
                  ? AuthChallenges[0]
                  : LazyStackAuth.AuthChallenges.None;
            }
        } 
        public bool HasChallenge { get { return AuthChallenges.Count > 0; } }
        #endregion Properties

        #region Methods
        public bool CheckUserLoginFormat(string userLogin)
        {
            //Todo: Implement name content rules
            return userLogin.Length > 5;
        }
        public bool CheckEmailFormat(string userEmail)
        {
            //Todo: Implement email content rules
            return userEmail.Length > 3 && userEmail.Contains("@");
        }
        public bool CheckPasswordFormat(string password)
        {
            //Todo: Implement password content rules
            return password.Length >= 8;
        }
        public bool CheckCodeFormat(string code)
        {
            // Todo: Implement Code content rules
            return code.Length > 4;
        }
        private async Task NoOp()
        {
            await Task.Yield(); // best implementation I know of Better than await Task.Delay(0);
        }

        public async Task<AuthModuleEvent> StartAuthAsync()
        {
            await NoOp(); // used when the method doesn't call service

            if (IsAuthorized)
                return AuthModuleEvent.AlreadyAuthorized;

            Clear();

            AuthChallenges.Add(LazyStackAuth.AuthChallenges.Login);
            AuthChallenges.Add(LazyStackAuth.AuthChallenges.Password);
            return AuthModuleEvent.AuthChallenge;
        }

        public async Task<AuthModuleEvent> StartAuthAsync(string userLogin)
        {
            if (IsAuthorized)
                return AuthModuleEvent.AlreadyAuthorized;

            Clear();

            AuthChallenges.Add(LazyStackAuth.AuthChallenges.Login);
            AuthChallenges.Add(LazyStackAuth.AuthChallenges.Password);
            return await VerifyLoginAsync(userLogin);
        }

        public async Task<AuthModuleEvent> StartAuthAsync(string userLogin, string password)
        {
            if (IsAuthorized)
                return AuthModuleEvent.AlreadyAuthorized;

            Clear();

            AuthChallenges.Add(LazyStackAuth.AuthChallenges.Login);
            AuthChallenges.Add(LazyStackAuth.AuthChallenges.Password);

            var result = await VerifyLoginAsync(userLogin);
            if (result != AuthModuleEvent.AuthChallenge)
                return result;

            return await VerifyPasswordAsync(password);
        }

        public async Task<AuthModuleEvent> VerifyLoginAsync(string userLogin) 
        {
            if (IsAuthorized)
                return AuthModuleEvent.AlreadyAuthorized;

            if(CurrentChallenge != LazyStackAuth.AuthChallenges.Login)
                return AuthModuleEvent.ChallengeVerifyWithoutRequest;

            if (!CheckUserLoginFormat(userLogin))
                return AuthModuleEvent.UserLoginRequirementsFailed;

            await NoOp(); // In aws implementation we don't do a round trip to server for VerifyLogin

            try
            {
                CognitoUser = new CognitoUser(userLogin, clientId, userPool, providerClient);
                AuthChallenges.Remove(LazyStackAuth.AuthChallenges.Login);
                return AuthModuleEvent.AuthChallenge;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyLogin() threw an exception {e}");
                CognitoUser = null;
                return AuthModuleEvent.Unknown;
            }
        }

        public async Task<AuthModuleEvent> VerifyPasswordAsync(string password)
        {
            if (IsAuthorized)
                return AuthModuleEvent.AlreadyAuthorized;

            if (CurrentChallenge != LazyStackAuth.AuthChallenges.Password)
                return AuthModuleEvent.ChallengeVerifyWithoutRequest;

            if (!CheckPasswordFormat(password))
                return AuthModuleEvent.PasswordRequirementsFailed;

            try
            {
                authFlowResponse = await CognitoUser.StartWithSrpAuthAsync(
                    new InitiateSrpAuthRequest()
                    {
                        Password = password
                    }
                    ).ConfigureAwait(false);

                AuthChallenges.Remove(LazyStackAuth.AuthChallenges.Password);

                CheckForChallenges(authFlowResponse); // Add any challenges presented by server

                if (CurrentChallenge == LazyStackAuth.AuthChallenges.None)
                    return await FinalizeAuthAsync();

                return AuthModuleEvent.AuthChallengeVerified;
            }
            catch (InvalidPasswordException) { return AuthModuleEvent.PasswordRequirementsFailed; }
            catch (TooManyRequestsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (NotAuthorizedException) { return AuthModuleEvent.NotAuthorized; }
            catch (UserNotFoundException) { return AuthModuleEvent.UserNotFound; }
            catch (UserNotConfirmedException) { return AuthModuleEvent.NotConfirmed; }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyPassword() threw an exception {e}");
                CognitoUser = null;
                return AuthModuleEvent.Unknown;
            }
        }

        public async Task<AuthModuleEvent> VerifyMFACodeAsync(string mfaCode)
        {
            if (IsAuthorized)
                return AuthModuleEvent.AlreadyAuthorized;

            if (CurrentChallenge != LazyStackAuth.AuthChallenges.MFACode)
                return AuthModuleEvent.ChallengeVerifyWithoutRequest;

            try
            {
                var mfaResponse =  await CognitoUser.RespondToSmsMfaAuthAsync(
                    new RespondToSmsMfaRequest()
                    {
                        SessionID = authFlowResponse.SessionID,
                        MfaCode  = mfaCode
                    }
                    ).ConfigureAwait(false);

                AuthChallenges.Remove(LazyStackAuth.AuthChallenges.MFACode);

                CheckForChallenges(authFlowResponse); // Add any challenges presented by server

                if (CurrentChallenge == LazyStackAuth.AuthChallenges.None)
                    return await FinalizeAuthAsync();

                return AuthModuleEvent.AuthChallengeVerified;
            }
            catch (InvalidPasswordException) { return AuthModuleEvent.PasswordRequirementsFailed; }
            catch (TooManyRequestsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (NotAuthorizedException) { return AuthModuleEvent.NotAuthorized; }
            catch (UserNotFoundException) { return AuthModuleEvent.UserNotFound; }
            catch (UserNotConfirmedException) { return AuthModuleEvent.NotConfirmed; }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyPassword() threw an exception {e}");
                CognitoUser = null;
                return AuthModuleEvent.Unknown;
            }
        }

        public async Task<AuthModuleEvent> VerifyPasswordUpdateAsync(string newPassword)
        {
            if (IsAuthorized)
                return AuthModuleEvent.AlreadyAuthorized;

            if (CurrentChallenge != LazyStackAuth.AuthChallenges.PasswordUpdate)
                return AuthModuleEvent.ChallengeVerifyWithoutRequest;

            try
            {
                var newPasswordResponse = await CognitoUser.RespondToNewPasswordRequiredAsync(
                    new RespondToNewPasswordRequiredRequest()
                    {
                        SessionID = authFlowResponse.SessionID,
                        NewPassword = newPassword
                    }
                    ).ConfigureAwait(false);

                AuthChallenges.Remove(LazyStackAuth.AuthChallenges.PasswordUpdate);

                CheckForChallenges(authFlowResponse); // Add any challenges presented by server

                if (CurrentChallenge == LazyStackAuth.AuthChallenges.None)
                    return await FinalizeAuthAsync();

                return AuthModuleEvent.AuthChallengeVerified;
            }
            catch (InvalidPasswordException) { return AuthModuleEvent.PasswordRequirementsFailed; }
            catch (TooManyRequestsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (NotAuthorizedException) { return AuthModuleEvent.NotAuthorized; }
            catch (UserNotFoundException) { return AuthModuleEvent.UserNotFound; }
            catch (UserNotConfirmedException) { return AuthModuleEvent.NotConfirmed; }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyPassword() threw an exception {e}");
                CognitoUser = null;
                return AuthModuleEvent.Unknown;
            }
        }

        private void CheckForChallenges(AuthFlowResponse authFlowResponse)
        {
            if (authFlowResponse.AuthenticationResult != null)
                return;

            if (authFlowResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED) // Update Passsword
            {
                if(!AuthChallenges.Contains(LazyStackAuth.AuthChallenges.PasswordUpdate))
                    AuthChallenges.Add(LazyStackAuth.AuthChallenges.PasswordUpdate);
            }
            else if (authFlowResponse.ChallengeName == ChallengeNameType.SMS_MFA) // Multi-factor auth
            {
                if(!AuthChallenges.Contains(LazyStackAuth.AuthChallenges.MFACode))
                    AuthChallenges.Add(LazyStackAuth.AuthChallenges.MFACode);
            }
        }

        // Called from SignInAsync(), UpdatePasswordAsync()
        private async Task<AuthModuleEvent> FinalizeAuthAsync()
        {
            authFlowResponse = null;

            // todo: This will probably need to change to Sync
            AuthModuleEvent refreshUserDetailsResult = await RefreshUserDetailsAsync().ConfigureAwait(false); 
            if (refreshUserDetailsResult != AuthModuleEvent.RefreshUserDetailsDone)
            {
                SignOut();
                return AuthModuleEvent.CantRetrieveUserDetails;
            }

            // Add additional application level challenges here


            // Grab JWT from login to User Pools to extract User Pool Identity
            var token = new JwtSecurityToken(jwtEncodedString: CognitoUser.SessionTokens.IdToken);
            UpIdentity = token.Claims.First(c => c.Type == "sub").Value; // JWT sub cliam contains User Pool Identity

            // Note: creates Identity Pool identity if it doesn't exist
            Credentials = CognitoUser.GetCognitoAWSCredentials(identityPoolId, regionEndpoint);
            IpIdentity = await Credentials.GetIdentityIdAsync(); // Identity Pool Identity
            IsAuthorized = true;
            return AuthModuleEvent.Authorized;
        }

        public async Task<AuthModuleEvent> RefreshUserDetailsAsync()
        {
            if (CognitoUser == null)
                return AuthModuleEvent.NeedToBeSignedIn;

            try
            {
                // Get the current user attributes from the server
                // and set UserEmail and IsUserEmailVerified
                GetUserResponse getUserResponse = await CognitoUser.GetUserDetailsAsync().ConfigureAwait(false);
                foreach (AttributeType item in getUserResponse.UserAttributes)
                {
                    if (item.Name.Equals("email"))
                        UserEmail = item.Value;

                    if (item.Name.Equals("email_verified"))
                        IsUserEmailVerified = item.Value.Equals("true");
                }
                return AuthModuleEvent.RefreshUserDetailsDone;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"RefreshUserDetails threw an exception {e}");
                return AuthModuleEvent.Unknown;
            }
        }

        public void Clear()
        {
            CognitoUser = null;
            AuthChallenges.Clear();
            authFlowResponse = null;
            IsAuthorized = false;
        }


        public AuthModuleEvent SignOut()
        {
            UserLogin = string.Empty;
            Clear();
            return AuthModuleEvent.SignedOut;
        }

        public async Task<AuthModuleEvent> StartSignUpAsync(string userLogin, string password, string email)
        {
            if(IsAuthorized)
                return AuthModuleEvent.AlreadyAuthorized;

            Clear();

            if (!CheckUserLoginFormat(userLogin))
                return AuthModuleEvent.UserLoginRequirementsFailed;

            if (!CheckPasswordFormat(password))
                return AuthModuleEvent.PasswordRequirementsFailed;

            if (!CheckEmailFormat(email))
                return AuthModuleEvent.EmailRequirementsFailed;

            try
            {
                var signUpRequest = new SignUpRequest()
                {
                    ClientId = clientId,
                    Password = password,
                    Username = userLogin
                };

                signUpRequest.UserAttributes.Add(
                    new AttributeType()
                    {
                        Name = "email",
                        Value = email
                    });

                var result = await providerClient.SignUpAsync(signUpRequest).ConfigureAwait(false);

                UserLogin = userLogin;
                UserEmail = email;

                AuthChallenges.Add(LazyStackAuth.AuthChallenges.SignUp);
                return AuthModuleEvent.AuthChallenge;
            }
            catch (UsernameExistsException) { return AuthModuleEvent.UserLoginAlreadyUsed; }
            catch (InvalidParameterException) { return AuthModuleEvent.PasswordRequirementsFailed; }
            catch (Exception e)
            {
                Debug.WriteLine($"SignUp() threw an exception {e}");
                return AuthModuleEvent.Unknown;
            }
        }

        public async Task<AuthModuleEvent> ResendVerifySignupCodeAsync(string userLogin)
        {
            if (CognitoUser != null)
                return AuthModuleEvent.InvalidOperationWhenSignedIn;

            //if (CurrentChallenge != LazyStackAuth.AuthChallenges.SignUp)
            //    return AuthModuleEvent.SignUpNotStarted;

            try
            {
                var result = await providerClient.ResendConfirmationCodeAsync(
                    new ResendConfirmationCodeRequest
                    {
                        ClientId = clientId,
                        Username = userLogin
                    }).ConfigureAwait(false);

                UserLogin = userLogin;
                Debug.WriteLine("Requested resend of signup confirmation code.");
                return AuthModuleEvent.AuthChallenge;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"SignUp() threw an exception {e}");
                return AuthModuleEvent.Unknown;
            }
        }

        public async Task<AuthModuleEvent> VerifySignUpAsync(string code)
        {
            if (IsAuthorized)
                return AuthModuleEvent.AlreadyAuthorized;

            if (CurrentChallenge != LazyStackAuth.AuthChallenges.SignUp)
                return AuthModuleEvent.ChallengeVerifyWithoutRequest;

            try
            {
                var result = await providerClient.ConfirmSignUpAsync(
                    new ConfirmSignUpRequest
                    {
                        ClientId = clientId,
                        Username = UserLogin,
                        ConfirmationCode = code
                    }).ConfigureAwait(false);

                AuthChallenges.Remove(LazyStackAuth.AuthChallenges.SignUp);

                return AuthModuleEvent.SignedUp;
            }
            catch (CodeMismatchException) { return AuthModuleEvent.VerifyFailed; }
            catch (TooManyRequestsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (Exception e)
            {
                Debug.WriteLine($"VerifyWithCode() threw an exception {e}");
                return AuthModuleEvent.Unknown;
            }
        }

        public async Task<AuthModuleEvent> ResetPasswordAsync(string userLogin)
        {
            if(IsAuthorized)
                return AuthModuleEvent.InvalidOperationWhenSignedIn;

            if (HasChallenge)
                return AuthModuleEvent.AuthAlreadyStarted;

            try
            {
                CognitoUser user = new CognitoUser(userLogin, clientId, userPool, providerClient);
                await user.ForgotPasswordAsync().ConfigureAwait(false);
                AuthChallenges.Add(LazyStackAuth.AuthChallenges.PasswordReset);
                return AuthModuleEvent.AuthChallenge;
            }
            catch (TooManyRequestsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (Exception e)
            {
                Debug.WriteLine($"ForgotPassword() threw an exception {e}");
                return AuthModuleEvent.Unknown;
            }
        }

        public async Task<AuthModuleEvent> VerifyPasswordResetAsync(string password, string code)
        {
            if (IsAuthorized)
                return AuthModuleEvent.InvalidOperationWhenSignedIn;

            if (CurrentChallenge != LazyStackAuth.AuthChallenges.PasswordReset)
                return AuthModuleEvent.ChallengeVerifyWithoutRequest;

            try
            {
                CognitoUser user = new CognitoUser(UserLogin, clientId, userPool, providerClient);
                await user.ConfirmForgotPasswordAsync(code, password).ConfigureAwait(false);
                AuthChallenges.Remove(LazyStackAuth.AuthChallenges.PasswordReset);
                return AuthModuleEvent.PasswordResetDone;
            }
            catch (InvalidPasswordException) { return AuthModuleEvent.PasswordRequirementsFailed; }
            catch (CodeMismatchException) { return AuthModuleEvent.VerifyFailed; }
            catch (TooManyRequestsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (Exception e)
            {
                Debug.WriteLine($"ForgotPassword() threw an exception {e}");
                return AuthModuleEvent.Unknown;
            }
        }

        public async Task<AuthModuleEvent> UpdateEmailAsync(string newUserEmail)
        {
            if (!IsAuthorized)
                return AuthModuleEvent.NeedToBeSignedIn;

            try
            {
                // Get the current values on the server
                AuthModuleEvent refreshUserDetailsResult = await RefreshUserDetailsAsync().ConfigureAwait(false);
                if (refreshUserDetailsResult != AuthModuleEvent.RefreshUserDetailsDone)
                    return AuthModuleEvent.CantRetrieveUserDetails;

                // make sure the values are different
                if (UserEmail.Equals(newUserEmail) && IsUserEmailVerified)
                    return AuthModuleEvent.NothingToDo; // Nothing to do

                // Update the user email on the server
                // This will place the user email in an unverified state
                var attributes = new Dictionary<string, string>()
                {
                    { "email", newUserEmail }
                };
                IsUserEmailVerified = false;

                await CognitoUser.UpdateAttributesAsync(attributes).ConfigureAwait(false);
                if(!AuthChallenges.Contains(LazyStackAuth.AuthChallenges.Email))
                    AuthChallenges.Add(LazyStackAuth.AuthChallenges.Email);
                return AuthModuleEvent.AuthChallenge;
            }
            catch (TooManyRequestsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (Exception e)
            {
                Debug.WriteLine($"UpdateEmail() threw an exception {e}");
                return AuthModuleEvent.Unknown;
            }
        }

        public async Task<AuthModuleEvent> ResendVerifyEmailUpdateAsync(string newUserEmail)
        {
            if (!IsAuthorized)
                return AuthModuleEvent.NeedToBeSignedIn;

            return await UpdateEmailAsync(newUserEmail);
        }

        public async Task<AuthModuleEvent> VerifyEmailUpdateAsync(string code)
        {
            if (IsAuthorized)
                return AuthModuleEvent.NeedToBeSignedIn;

            if (!AuthChallenges.Contains(LazyStackAuth.AuthChallenges.Email))
                return AuthModuleEvent.ChallengeVerifyWithoutRequest;

            try
            {
                await CognitoUser.VerifyAttributeAsync("email", code).ConfigureAwait(false);
                IsUserEmailVerified = true;
                AuthChallenges.Remove(LazyStackAuth.AuthChallenges.Email);
                return AuthModuleEvent.AuthChallengeVerified;
            }
            catch (TooManyRequestsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (TooManyFailedAttemptsException) { return AuthModuleEvent.TooManyAttempts; }
            catch (Exception e)
            {
                Debug.WriteLine($"UpdatePassword() threw an exception {e}");
                return AuthModuleEvent.Unknown;
            }
        }

        public async Task<string> GetAccessToken()
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

        public async Task<string> GetIdentityToken()
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
        #endregion
    }
}

