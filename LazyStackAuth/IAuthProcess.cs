using System.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LazyStackAuth
{
    public interface IAuthProcess: INotifyPropertyChanged
    {
        public IAuthProvider AuthProvider { get; }
        public AuthChallengeEnum CurrentChallenge { get; }
        public AuthProcessEnum CurrentAuthProcess { get; }

        public string LanguageCode { get; set; }

        public string Login { get; set; }
        public string NewLogin { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string Email { get; set; }
        public string NewEmail { get; set; }
        public string Phone { get; set; }
        public string NewPhone { get; set; }
        public string Code { get; set; }

        // Property States
        public bool IsLoginFormatOk { get; }
        public bool IsLoginVerified { get; }
        public bool LoginNotVerified { get; }

        public bool IsNewLoginFormatOk { get; }
        public bool IsNewLoginVerified { get; }
        public bool NewLoginNotVerified { get; }

        public bool IsEmailFormatOk { get; }
        public bool IsEmailVerified { get; }
        public bool EmailNotVerified { get; }

        public bool IsNewEmailFormatOk { get; }
        public bool IsNewEmailVerified { get; }
        public bool NewEmailNotVerified { get; }

        public bool IsPasswordFormatOk { get; }
        public bool IsPasswordVerified { get; }
        public bool PasswordNotVerified { get; }

        public bool IsNewPasswordFormatOk { get; }
        public bool IsNewPasswordVerified { get; }
        public bool NewPasswordNotVerified { get; }

        public bool IsPhoneFormatOk { get; }
        public bool IsPhoneVerified { get; }
        public bool PhoneNotVerified { get; }

        public bool IsNewPhoneFormatOk { get; }
        public bool IsNewPhoneVerified { get; }
        public bool NewPhoneNotVerified { get; }

        public bool IsCodeFormatOk { get; }
        public bool IsCodeVerified { get; }
        public bool CodeNotVerified { get; }

        // Auth state
        public bool IsSignedIn { get; }
        public bool IsNotSignedIn { get; }

        // CurrentAuthProcess
        public string AuthProcessMessage { get; }
        public bool HasActiveAuthProcess { get; }
        public bool NoActiveAuthProcess { get; }
        public bool IsSigningIn { get; }
        public bool IsSigningUp { get; }
        public bool IsResettingPassword { get; }
        public bool IsUpdatingLogin { get; }
        public bool IsUpdatingEmail { get; }
        public bool IsUpdatingPhone { get; }
        public bool IsUpdatingPassword { get; }


        // Challenge states
        public string AuthChallengeMessage { get; }
        public bool HasChallenge { get; }
        public bool NoChallenge { get; }

        public bool CurrentChallengeIsLogin { get; }
        public bool CollectLogin { get; }

        public bool CurrentChallengeIsNewLogin { get; }
        public bool CollectNewLogin { get; }

        public bool CurrentChallengeIsPassword { get; }
        public bool CollectPassword { get; }

        public bool CurrentChallengeIsNewPassword { get; }
        public bool CollectNewPassword { get; }

        public bool CurrentChallengeIsEmail { get; }
        public bool CollectEmail { get; }

        public bool CurrentChallengeIsNewEmail { get; }
        public bool CollectNewEmail { get; }

        public bool CurrentChallengeIsPhone { get; }
        public bool CollectPhone { get; }

        public bool CurrentChallengeIsNewPhone { get; }
        public bool CollectNewPhone { get; }

        public bool CurrentChallengeIsCode { get; }
        public bool CollectCode { get; }

        // Alert States
        public string AlertMessage { get; }
        public bool HasAlert { get; }


        // Currently Allowed AuthProcess
        public bool CanSignOut { get; }
        public bool CanSignIn { get; }
        public bool CanSignUp { get; }
        public bool CanResetPassword { get; }
        public bool CanUpdateLogin { get; }
        public bool CanUpdateEmail { get; }
        public bool CanUpdatePassword { get; }
        public bool CanUpdatePhone { get; }
        public bool CanCancel { get; }
        public bool CanResendCode { get; }

        // Methods
        public AuthEventEnum Clear();
        public AuthEventEnum Cancel();
        public AuthEventEnum SignOut();

        public Task<AuthEventEnum> StartSignInAsync(); // Happy Path Challenges: Login, Password
        public Task<AuthEventEnum> StartSignUpAsync(); // Happy Path Challenges: Login, Password, Email, SignUp
        public Task<AuthEventEnum> StartResetPasswordAsync(); // Happy Path Challenges: Login, Password
        public Task<AuthEventEnum> StartUpdateLoginAsync(); // Happy Path Challenges: Email
        public Task<AuthEventEnum> StartUpdateEmailAsync(); // Happy Path Challenges: Email
        public Task<AuthEventEnum> StartUpdatePhoneAsync(); // Happy Path Challenges: Email
        public Task<AuthEventEnum> StartUpdatePasswordAsync(); 

        public Task<AuthEventEnum> VerifyLoginAsync();
        public Task<AuthEventEnum> VerifyNewLoginAsync();
        public Task<AuthEventEnum> VerifyPasswordAsync(); // Happy Path Challenge: Code (optional)
        public Task<AuthEventEnum> VerifyNewPasswordAsync(); // Happy Path Challenge: Code (optional)
        public Task<AuthEventEnum> VerifyEmailAsync();
        public Task<AuthEventEnum> VerifyNewEmailAsync();
        public Task<AuthEventEnum> VerifyPhoneAsync();
        public Task<AuthEventEnum> VerifyNewPhoneAsync();
        public Task<AuthEventEnum> VerifyCodeAsync();

        public Task<AuthEventEnum> ResendCodeAsync(); // Happy Path Challenge: Code
        //public Task<AuthEventEnum> ChangePasswordAsync(string oldPassword, string newPassword);
        //public Task<AuthEventEnum> RefreshUserDetailsAsync();

        public bool CheckLoginFormat();
        public bool CheckEmailFormat();
        public bool CheckPasswordFormat();
        public bool CheckNewPasswordFormat();
        public bool CheckPhoneFormat();
        public bool CheckCodeFormat();

        //public event PropertyChangedEventHandler PropertyChanged;

    }
}