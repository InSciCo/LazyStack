using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;

namespace LazyStackAuth
{
    /// <summary>
    /// General authentication flow Interface - not specific to any auth provider
    /// </summary>
    public interface IAuthProvider
    {
        /// <summary>
        /// User Name
        /// </summary>
        public string UserLogin { get; set; }

        /// <summary>
        /// User email address
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// Is User Email Verified
        /// </summary>
        public bool IsUserEmailVerified { get; }

        /// <summary>
        ///  Is User Signed On
        /// </summary>
        public bool IsAuthorized { get; }

        /// <summary>
        /// List of current AuthChallenges
        /// </summary>
        public List<AuthChallenges> AuthChallenges { get; }

        public CognitoAWSCredentials Credentials { get; }

        public CognitoUser CognitoUser { get; }

        public bool HasChallenge { get; }

        /// <summary>
        /// CurrentChallenge if any,  otherwise null
        /// </summary>
        public AuthChallenges CurrentChallenge { get; }

        /// <summary>
        /// Check if user supplied name satisfies auth requirements
        /// </summary>
        /// <returns>bool</returns>
        public bool CheckUserLoginFormat(string userLogin);

        /// <summary>
        /// Check if user supplied email has valid format
        /// </summary>
        /// <returns>bool</returns>
        public bool CheckEmailFormat(string userEmail);

        /// <summary>
        /// Check if user supplied email has valid format
        /// </summary>
        /// <param name="password"></param>
        /// <returns>bool</returns>
        public bool CheckPasswordFormat(string password);

        /// <summary>
        /// Check if code entered by user has valid format
        /// </summary>
        /// <returns>bool</returns>
        public bool CheckCodeFormat(string code);

        /// <summary>
        /// Call to authorize user.
        /// Prep:
        /// !IsAuthorized
        /// 
        /// AuthModuleEvents:
        /// - AuthChallenges -- Happy Path
        ///     - Login
        ///     - Password
        /// 
        /// - AuthModelEvent errors
        /// 
        /// </summary>
        /// <returns>AuthModuleEvent.AuthChallenge</returns>
        public Task<AuthModuleEvent> StartAuthAsync();


        /// <summary>
        /// Call to authorize user.
        /// Prep:
        /// !IsAuthorized
        /// 
        /// AuthModuleEvents:
        /// - AuthChallenges -- Happy Path
        ///     - Login 
        ///     - Password -- Happy Path
        ///
        /// - AuthModelEvent errors
        /// 
        /// </summary>
        /// <returns>AuthModuleEvent.AuthChallenge</returns>
        public Task<AuthModuleEvent> StartAuthAsync(string userLogin);

        /// <summary>
        /// Call to authorize user.
        /// Prep:
        /// !IsAuthorized
        /// 
        /// AuthModuleEvents:
        /// - SignInDone -- Happy Path
        /// - AuthChallenges
        ///     - Login 
        ///     - Password
        ///     
        /// - AuthModelEvent errors
        /// 
        /// </summary>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> StartAuthAsync(string userLogin, string password);

        /// <summary>
        /// Verify Login
        /// Prep:
        /// !IsAuthorized
        /// CurrentChallenge == Login
        /// 
        /// AuthModuleEvents
        /// - AuthChallengeVerified -- Happy path when additional challenges exist
        /// 
        /// - AuthModelEvent errors
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> VerifyLoginAsync(string userLogin);

        /// <summary>
        /// Verify Password
        /// Prep:
        /// !IsAuthorized
        /// CurrentChallenge == Password
        ///     
        /// AuthModuleEvents
        /// - Authroized -- Happy Path when there are no more challenges
        /// - AuthChallengeVerified -- Happy Path when additional challenges exist
        /// 
        /// - AuthModelEvent errors
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> VerifyPasswordAsync(string password);

        /// <summary>
        /// Verify MFA Code
        /// Prep:
        /// !IsAuthorized
        /// CurrentChallenge == MfaCode
        /// 
        /// AuthModuleEvents
        /// - Authorized -- Happy Path when there are no more challenges
        /// - AuthChallengeVerified -- Happy Path when additional challenges exist
        /// 
        /// </summary>
        /// <param name="mfaCode"></param>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> VerifyMFACodeAsync(string mfaCode);

        /// <summary>
        /// Verify PasswordUpdate
        /// Prep:
        /// !IsAuthorized
        /// CurrentChallenge == PasswordUpdate
        /// 
        /// AuthModuleEvents 
        /// - Authorized -- Happy path when there are no more challenges
        /// - AuthChallengeVerified -- Happy Path when additional challenges exist
        /// 
        /// </summary>
        /// <param name="newPassword"></param>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> VerifyPasswordUpdateAsync(string newPassword);

        /// <summary>
        /// Refresh User details
        /// Prep:
        /// IsAuthorized
        /// 
        /// Possible AuthModuleEvents:
        /// - RefreshUserDetailsDone -- Happy Path
        /// 
        /// - AuthModelEvent errors
        /// </summary>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> RefreshUserDetailsAsync();

        /// <summary>
        /// Sign user out
        /// Prep:
        /// IsAuthorized
        /// 
        /// Possible AuthModuleEvents:
        /// - SignOutDone -- Only Path
        /// 
        /// </summary>
        /// <returns>AuthModuleEvent</returns>
        public AuthModuleEvent SignOut();

        /// <summary>
        /// Sign user up
        /// Prep:
        /// !IsAuthorized
        /// CurrentChallenge == SignUp
        /// 
        /// AuthModuleEvents:
        /// - VerificationCodeSent -- Happy path
        /// - AuthChallenges
        ///     - SignUp -- Happy path
        ///
        /// - AuthModelEvent errors
        /// 
        /// </summary>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> StartSignUpAsync(string userName, string password, string email);

        /// <summary>
        /// User requests resend of verification code
        /// Prep:
        /// - !IsAuthorized
        /// - CurrentChallenge == SignUp
        /// 
        /// Possible AuthModuleEvents:
        /// - AuthChallenge -- Happy Path
        ///     
        /// - AuthModuleEvent errors
        /// </summary>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> ResendVerifySignupCodeAsync(string userLogin);

        /// <summary>
        /// Verify signup using code sent to user
        /// Prep:
        /// !IsAuthorized
        /// CurrentChallenge == SignUp
        /// 
        /// Possible AuthModuleEvents:
        /// - Authorized -- Happy Path
        ///    
        /// - AuthModuleEvent errors
        /// </summary>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> VerifySignUpAsync(string code);

        /// <summary>
        /// User requests password reset
        /// Prep:
        /// !IsAuthorized
        /// !HasChallenge
        /// 
        /// Possible AuthModuleEvents:
        /// - AuthChallenge -- Happy Path
        ///
        /// - AuthModuleEvent errors
        /// </summary>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> ResetPasswordAsync(string userLogin);

        /// <summary>
        /// User entered code to verify password reset
        /// Prep:
        /// !IsAuthorized
        /// CurrentChallenge == PasswordReset
        ///
        /// AuthModuleEvents:
        /// - PasswordResetDone -- Happy Path
        /// 
        /// - AuthModuleEvent errors
        /// 
        /// </summary>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> VerifyPasswordResetAsync(string password, string code);

        /// <summary>
        /// User requests email address update
        /// Prep:
        /// IsAuthorized
        /// 
        /// AuthModuleEvents:
        /// - AuthChallenge -- Happy Path
        ///     - VerifyEmail
        /// - NothingToDo -- current email matches newUserEmail
        /// 
        /// - AuthModuleEvent errors
        /// 
        /// </summary>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> UpdateEmailAsync(string newUserEmail);

        /// <summary>
        /// User requests email address update resend of confirmation
        /// Prep:
        /// IsAuthorized
        /// CurrentChallenge == Email
        /// 
        /// AuthModuleEvents:
        /// - AuthChallenge -- Happy Path
        ///     - VerifyEmail
        ///     
        /// - NothingToDo -- current email matches newUserEmail
        /// 
        /// - AuthModuleEvent errors
        /// 
        /// </summary>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> ResendVerifyEmailUpdateAsync(string newUserEmail);

        /// <summary>
        /// User entered email verification code
        /// Prep:
        /// - IsAuthorized
        /// 
        /// Possible AuthModuleEvents:
        /// - UpdateEmailDone -- Happy Path
        /// 
        /// - NeedToBeSignedIn
        /// - TooManyAttempts
        /// - Unknown
        /// </summary>
        /// <returns>AuthModuleEvent</returns>
        public Task<AuthModuleEvent> VerifyEmailUpdateAsync(string code);

        public void Clear();
    }
}
