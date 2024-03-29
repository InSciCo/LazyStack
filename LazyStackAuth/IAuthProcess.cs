﻿using System.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LazyStackAuth
{
    /// <summary>
    /// This interface inherits from IAuthProvider and then extends it with 
    /// state and methods useful in most auth process flows. In addition, it 
    /// surfaces useful multi-language support for the UI. See properties:
    ///  LanguageCode 
    ///  AuthProcessMessage
    ///  AlertMessage
    /// 
    /// It also requires INotifyPropertyChanged to be implemented.
    /// Note: Implementations don't have to necessarily use INotifyProperyChanged 
    /// extensively. For example:
    /// 
    /// The AuthProcess class has a property bool IsChatty that when set to true 
    /// produces events for almost any change in a property. This "chatty" event 
    /// model is suitable for use in clients like Xamarin. 
    /// 
    /// Setting IsChatty to false reduces  the events produced by changes in 
    /// properties and is more suitable for clients like Blazor. This is because the 
    /// code-behind in the Blazor component is generally calling StateChanged() after 
    /// handling user input so events from the authProcessLite class would not be 
    /// useful. OTOH, there are some situations where an event could be useful or 
    /// even necessary so I'm leaving in the INotifyPropertyChanged.
    /// 
    /// </summary>
    public interface IAuthProcess: IAuthProvider, INotifyPropertyChanged
    {
        public bool IsChatty { get; set; }
        public bool ClearAllFields { get; set; } 
        public bool AssignFieldOnCheck { get; set; }

        public IAuthProvider AuthProvider { get; } // 

        public string Login { get; set; } //
        public string NewLogin { get; set; } //
        public string Password { get; set; } //
        public string NewPassword { get; set; } //
        public string Email { get; set; } //
        public string NewEmail { get; set; } //
        public string Phone { get; set; } //
        public string NewPhone { get; set; } //
        public string Code { get; set; }

        // UI Messages
        public string LoginLabel { get; }
        public string LoginFormatMessage { get; }

        public string NewLoginLabel { get; }
        public string NewLoginFormatMessage { get; }

        public string PasswordLabel { get; }
        public string PasswordFormatMessage { get; }

        public string NewPasswordLabel { get; }
        public string NewPasswordFormatMessage { get; }

        public string EmailLabel { get; }
        public string EmailFormatMessage { get; }

        public string NewEmailLabel { get; }
        public string NewEmailFormatMessage { get; }

        public string PhoneLabel { get; }
        public string PhoneFormatMessage { get; }

        public string NewPhoneLabel { get; }
        public string NewPhoneFormatMessage { get; }

        public string CodeLabel { get; }
        public string CodeFormatMessage { get; }


        // Property States
        public bool LoginNotVerified { get; } // 
        public bool NewLoginNotVerified { get; } //
        public bool EmailNotVerified { get; } //
        public bool NewEmailNotVerified { get; }
        public bool PasswordNotVerified { get; } //
        public bool NewPasswordNotVerified { get; } //
        public bool PhoneNotVerified { get; }
        public bool NewPhoneNotVerified { get; }
        public bool CodeNotVerified { get; } //

        // Auth state
        public bool IsNotSignedIn { get; } //

        // CurrentAuthProcess
        public string AuthProcessMessage { get; } //
        public bool HasActiveAuthProcess { get; } //
        public bool NoActiveAuthProcess { get; } //
        public bool IsSigningIn { get; } //
        public bool IsSigningUp { get; } //
        public bool IsResettingPassword { get; } //
        public bool IsUpdatingLogin { get; } //
        public bool IsUpdatingEmail { get; } //
        public bool IsUpdatingPhone { get; } //
        public bool IsUpdatingPassword { get; } //

        public string AuthChallengeMessage { get; } //
        public bool NoChallenge { get; } //

        public bool CurrentChallengeIsLogin { get; } //
        public bool CollectLogin { get; } //

        public bool CurrentChallengeIsNewLogin { get; } //
        public bool CollectNewLogin { get; }

        public bool CurrentChallengeIsPassword { get; } //
        public bool CollectPassword { get; }

        public bool CurrentChallengeIsNewPassword { get; } //
        public bool CollectNewPassword { get; }
        
        public bool CurrentChallengeIsEmail { get; } //
        public bool CollectEmail { get; } //

        public bool CurrentChallengeIsNewEmail { get; } //
        public bool CollectNewEmail { get; } //

        public bool CurrentChallengeIsPhone { get; } //
        public bool CollectPhone { get; } //

        public bool CurrentChallengeIsNewPhone { get; } //
        public bool CollectNewPhone { get; } //

        public bool CurrentChallengeIsCode { get; } //
        public bool CollectCode { get; } //

        // Alert States
        public string AlertMessage { get; } //
        public bool HasAlert { get; } //

        // Outher states
        public bool IsBusy { get; } //
        public bool IsNotBusy { get; } //
        public bool IsLongBusy { get; } //
        public bool IsNotLongBusy { get; } //


        // Methods
        public Task<AuthEventEnum> VerifyLoginAsync();
        public Task<AuthEventEnum> VerifyNewLoginAsync();
        public Task<AuthEventEnum> VerifyPasswordAsync(); // Happy Path Challenge: Code (optional)
        public Task<AuthEventEnum> VerifyNewPasswordAsync(); // Happy Path Challenge: Code (optional)
        public Task<AuthEventEnum> VerifyEmailAsync();
        public Task<AuthEventEnum> VerifyNewEmailAsync();
        public Task<AuthEventEnum> VerifyPhoneAsync();
        public Task<AuthEventEnum> VerifyNewPhoneAsync();
        public Task<AuthEventEnum> VerifyCodeAsync();

        public new bool CheckLoginFormat(string login = null);
        public bool CheckNewLoginFormat(string login = null);

        public new bool CheckEmailFormat(string email = null);
        public bool CheckNewEmailFormat(string email = null);

        public new bool CheckPasswordFormat(string password = null );
        public bool CheckNewPasswordFormat(string password = null);

        public new bool CheckPhoneFormat(string phone = null );
        public bool CheckNewPhoneFormat(string phone = null);

        public new bool CheckCodeFormat(string code = null );

    }
}