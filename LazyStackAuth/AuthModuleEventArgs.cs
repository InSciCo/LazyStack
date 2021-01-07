using System;
namespace LazyStackAuth
{
    public enum AuthProcessEnum
    {
        None,
        SigningIn,
        SigningUp,
        ResettingPassword,
        UpdatingEmail,
        UpdatingPhone,
        UpdatingPassword
    }

    // Get the specified challenge value(s) and then call a verify method based on CurrentAuthProcess
    public enum AuthChallengeEnum
    {
        None, // No challenge
        Login, 
        Password, 
        NewPassword,
        Email, 
        Phone, 
        Code
    }

    public enum AuthEventEnum
    {
        AuthChallenge, // One or more AuthChallenges are pending
        SignedIn, // User is fully authenticated and authorized
        SignedUp, // User is signed up, user needs to sign in to continue
        SignedOut, 
        PasswordResetDone,
        PasswordUpdateDone,
        PhoneUpdateDone,
        EmailUpdateDone,
        VerificationCodeSent,
        Canceled,

        // Alert events 
        Alert, // Any enum value >= than this enum item is an Alert - Alert not used itself.
        Alert_AuthProcessAlreadyStarted,
        Alert_DifferentAuthProcessActive,
        Alert_IncorrectAuthProcess,
        Alert_NoActiveAuthProcess,
        Alert_AlreadySignedIn,
        Alert_InternalSignInError,
        Alert_InternalSignUpError,
        Alert_InternalProcessError,
        Alert_SignUpMissingLogin,
        Alert_SignUpMissingPassword,
        Alert_SignUpMissingEmail,
        Alert_SignUpMissingCode,
        Alert_AuthAlreadyStarted,
        Alert_InvalidCallToResendAsyncCode,
        Alert_AccountWithThatEmailAlreadyExists,
        Alert_RefreshUserDetailsDone,
        Alert_EmailAddressIsTheSame,
        Alert_VerifyCalledButNoChallengeFound, // 
        Alert_CantRetrieveUserDetails, // Suggested Message = "Can not retrieve user details."
        Alert_NeedToBeSignedIn, // Suggested Message = "Need to be signed in"
        Alert_InvalidOperationWhenSignedIn, // Suggested Message "Invalid operation when signed in"
        Alert_UserNotFound,  // Suggested Message = "UserEmail not found"
        Alert_NotConfirmed,  // Suggested Mesage = "User Password Incorrect"
        Alert_NotAuthorized, // Suggested Message = "User Account not active"
        Alert_VerifyFailed, // Suggested Message = "Sorry, code didn't match, try again"
        Alert_LoginAlreadyUsed, // Suggested Message = "Sorry, that user name is already in use."
        Alert_LoginMustBeSuppliedFirst,
        Alert_LoginFormatRequirementsFailed, 
        Alert_PasswordFormatRequirementsFailed, // Suggested Message = "Sorry, that password doesn't meet minimum complexity requirements"
        Alert_EmailFormatRequirementsFailed,
        Alert_PhoneFormatRequirementsFailed,
        Alert_TooManyAttempts, // Suggested Message = "Sorry, you have made too many attepts. Please try again later."
        Alert_NothingToDo,

        // Hail Marys
        Alert_Unknown // Suggested Message = "We encountered an unknown system error. Please try again."
    }

    public class AuthModuleEventArgs : EventArgs
    {
        public AuthEventEnum Result { get; }

        public AuthModuleEventArgs(AuthEventEnum r)
        {
            Result = r;
        }
    }
}
