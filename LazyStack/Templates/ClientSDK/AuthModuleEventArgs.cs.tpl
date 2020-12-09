using System;
namespace __ProjName__
{
    public enum AuthChallenges
    {
        None, // No challenge
        SignUp, // Verify Sign up
        Login, // Verify user login
        Password, // Verify user password
        MFACode, // Verify multi-factor authentication code
        PasswordUpdate, // Verify password update
        PasswordReset, // Verify password reset
        Email, // Verify email
        Phone // Verify phone number
    }

    public enum AuthModuleEvent
    {
        AuthChallenge, // One or more AuthChallenges are pending
        Authorized, // user is fully authenticated and authorized
        SignedUp, // User is signed up, user needs to sign in to continue
        SignedOut, 
        PasswordResetDone,
        PasswordUpdateDone,
        EmailUpdateDone,
        VerificationCodeSent,

        // Alert events 
        AlreadyAuthorized,
        AuthAlreadyStarted,
        AccountWithThatEmailAlreadyExists,
        RefreshUserDetailsDone,
        VerifyCalledButNoChallengeFound, // 
        CantRetrieveUserDetails, // Suggested Message = "Can not retrieve user details."
        NeedToBeSignedIn, // Suggested Message = "Need to be signed in"
        InvalidOperationWhenSignedIn, // Suggested Message "Invalid operation when signed in"
        UserNotFound,  // Suggested Message = "UserEmail not found"
        NotConfirmed,  // Suggested Mesage = "User Password Incorrect"
        NotAuthorized, // Suggested Message = "User Account not active"
        VerifyFailed, // Suggested Message = "Sorry, code didn't match, try again"
        UserLoginAlreadyUsed, // Suggested Message = "Sorry, that user name is already in use."
        UserLoginRequirementsFailed, 
        PasswordRequirementsFailed, // Suggested Message = "Sorry, that password doesn't meet minimum complexity requirements"
        EmailRequirementsFailed,
        TooManyAttempts, // Suggested Message = "Sorry, you have made too many attepts. Please try again later."
        NothingToDo,

        // Hail Marys
        Unknown // Suggested Message = "We encountered an unknown system error. Please try again."
    }

    public class AuthModuleEventArgs : EventArgs
    {
        public AuthModuleEvent Result { get; }

        public AuthModuleEventArgs(AuthModuleEvent r)
        {
            Result = r;
        }
    }
}
