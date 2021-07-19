using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Runtime.CompilerServices; 
using System.Threading.Tasks;

// Code Naming Conventions
// Property backing fields start with _ ex: _authProvider
// Public members start with uppercase ex: Authprovider
// Private members start with lowercase ex: myYada
// Local varaibles start with lowercase ex: bool result

namespace LazyStackAuth
{
    /// <summary>
    /// AuthProcess add events, INotifyPropertyChangedk
    /// and workflow to classes implementing IAuthProvider
    /// </summary>
    public class AuthProcess : INotifyPropertyChanged, IAuthProcess
    {
        public AuthProcess(IConfiguration appConfig, IAuthProvider authProvider )
        {
            _authProvider = authProvider;
            this.appConfig = appConfig;
        }

        #region Fields
        readonly IConfiguration appConfig;
        AuthProcessEnum lastAuthProcessEnum = AuthProcessEnum.None;
        AuthChallengeEnum lastAuthChallengeEnum = AuthChallengeEnum.None;
        #endregion 

        #region Properties
        public string LanguageCode { get; set; } = "en-US";

        readonly IAuthProvider _authProvider;
        public IAuthProvider AuthProvider { get { return _authProvider; } }
        public AuthChallengeEnum CurrentChallenge => _authProvider.CurrentChallenge;
        public AuthProcessEnum CurrentAuthProcess => _authProvider.CurrentAuthProcess;

        private string _login = string.Empty;
        public string Login
        {
            get { return _login; }
            set {
                SetProperty(ref _login, value); 
                CheckLoginFormat();
            }
        }

        private string _newLogin = string.Empty;
        public string NewLogin
        {
            get { return _newLogin; }
            set
            {
                SetProperty(ref _newLogin, value);
                CheckLoginFormat();
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get { return _password; }
            set {
                SetProperty(ref _password, value);
                CheckPasswordFormat();
            }
        }

        private string _newPassword = string.Empty;
        public string NewPassword
        {
            get { return _newPassword; }
            set {
                SetProperty(ref _newPassword, value);
                CheckNewPasswordFormat();
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get { return _email; }
            set {
                SetProperty(ref _email, value);
                CheckEmailFormat();
            }
        }

        private string _newEmail = string.Empty;
        public string NewEmail
        {
            get { return _newEmail; }
            set
            {
                SetProperty(ref _newEmail, value);
                CheckEmailFormat();
            }
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get { return _phone; }
            set { SetProperty(ref _phone, value); }
        }

        private string _newPhone = string.Empty;
        public string NewPhone
        {
            get { return _newPhone; }
            set { SetProperty(ref _newPhone, value); }
        }

        private string _code = string.Empty;
        public string Code
        {
            get { return _code; }
            set {
                SetProperty(ref _code, value);
                CheckCodeFormat();
            }
        }

        // Entry Properties states
        public bool IsLoginFormatOk => _authProvider.IsLoginFormatOk;
        public bool IsLoginVerified => _authProvider.IsLoginVerified;
        public bool LoginNotVerified => !_authProvider.IsLoginVerified;

        public bool IsNewLoginFormatOk => _authProvider.IsNewLoginFormatOk;
        public bool IsNewLoginVerified => _authProvider.IsNewLoginVerified;
        public bool NewLoginNotVerified => !_authProvider.IsNewLoginVerified;

        public bool IsEmailFormatOk => _authProvider.IsEmailFormatOk;
        public bool IsEmailVerified => _authProvider.IsEmailVerified;
        public bool EmailNotVerified => !_authProvider.IsEmailVerified;

        public bool IsNewEmailFormatOk => _authProvider.IsNewEmailFormatOk;
        public bool IsNewEmailVerified => _authProvider.IsNewEmailVerified;
        public bool NewEmailNotVerified => !_authProvider.IsNewEmailVerified;

        public bool IsPasswordFormatOk => _authProvider.IsPasswordFormatOk;
        public bool IsPasswordVerified => _authProvider.IsPasswordVerified;
        public bool PasswordNotVerified => !_authProvider.IsPasswordVerified;

        public bool IsNewPasswordFormatOk => _authProvider.IsNewPasswordFormatOk;
        public bool IsNewPasswordVerified => _authProvider.IsNewPasswordVerified;
        public bool NewPasswordNotVerified => !_authProvider.IsNewPasswordVerified;

        public bool IsPhoneFormatOk => _authProvider.IsPhoneFormatOk;
        public bool IsPhoneVerified => _authProvider.IsPhoneVerified;
        public bool PhoneNotVerified => !_authProvider.IsPhoneVerified;

        public bool IsNewPhoneFormatOk => _authProvider.IsNewPhoneFormatOk;
        public bool IsNewPhoneVerified => _authProvider.IsNewPhoneVerified;
        public bool NewPhoneNotVerified => !_authProvider.IsNewPhoneVerified;

        public bool IsCodeFormatOk => _authProvider.IsCodeFormatOk;
        public bool IsCodeVerified => _authProvider.IsCodeVerified;
        public bool CodeNotVerified => !_authProvider.IsCodeVerified;

        // Auth state
        public bool IsSignedIn => _authProvider.IsSignedIn;
        public bool IsNotSignedIn => !_authProvider.IsSignedIn;

        // CurrentAuthProcess
        private string _authProcessMessage = string.Empty;
        public string AuthProcessMessage { get { return _authProcessMessage; } }
        public bool HasActiveAuthProcess => CurrentAuthProcess != AuthProcessEnum.None;
        public bool NoActiveAuthProcess => CurrentAuthProcess == AuthProcessEnum.None;
        public bool IsSigningIn => CurrentAuthProcess == AuthProcessEnum.SigningIn;
        public bool IsSigningUp => CurrentAuthProcess == AuthProcessEnum.SigningUp;
        public bool IsResettingPassword => CurrentAuthProcess == AuthProcessEnum.ResettingPassword;
        public bool IsUpdatingLogin => CurrentAuthProcess == AuthProcessEnum.UpdatingLogin;
        public bool IsUpdatingEmail => CurrentAuthProcess == AuthProcessEnum.UpdatingEmail;
        public bool IsUpdatingPhone => CurrentAuthProcess == AuthProcessEnum.UpdatingPhone;
        public bool IsUpdatingPassword => CurrentAuthProcess == AuthProcessEnum.UpdatingPassword;


        // Challenge states
        private string _authChallengeMessage = string.Empty;
        public string AuthChallengeMessage {  get { return _authChallengeMessage; } }

        public bool HasChallenge => _authProvider.CurrentChallenge != AuthChallengeEnum.None;
        public bool NoChallenge => _authProvider.CurrentChallenge == AuthChallengeEnum.None;

        public bool CurrentChallengeIsLogin => _authProvider.CurrentChallenge == AuthChallengeEnum.Login;
        public bool CollectLogin => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.Login);

        public bool CurrentChallengeIsNewLogin => _authProvider.CurrentChallenge == AuthChallengeEnum.NewLogin;
        public bool CollectNewLogin => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.NewLogin);

        public bool CurrentChallengeIsPassword => _authProvider.CurrentChallenge == AuthChallengeEnum.Password;
        public bool CollectPassword => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.Password);

        public bool CurrentChallengeIsNewPassword => _authProvider.CurrentChallenge == AuthChallengeEnum.NewPassword;
        public bool CollectNewPassword => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.NewPassword);

        public bool CurrentChallengeIsEmail => _authProvider.CurrentChallenge == AuthChallengeEnum.Email;
        public bool CollectEmail => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.Email);

        public bool CurrentChallengeIsNewEmail => _authProvider.CurrentChallenge == AuthChallengeEnum.NewEmail;
        public bool CollectNewEmail => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.NewEmail);

        public bool CurrentChallengeIsPhone => _authProvider.CurrentChallenge == AuthChallengeEnum.Phone;
        public bool CollectPhone => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.Phone);

        public bool CurrentChallengeIsNewPhone => _authProvider.CurrentChallenge == AuthChallengeEnum.NewPhone;
        public bool CollectNewPhone => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.NewPhone);

        public bool CurrentChallengeIsCode => _authProvider.CurrentChallenge == AuthChallengeEnum.Code;
        public bool CollectCode => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.Code);



        // Alert states
        private string _alertMessage = string.Empty;
        public string AlertMessage { get { return _alertMessage; } }
        public bool HasAlert { get { return _alertMessage.Length > 0; } }

        // Currently Allowed AuthProcess
        public bool CanSignOut => _authProvider.CanSignOut;
        public bool CanSignIn => _authProvider.CanSignIn;
        public bool CanSignUp => _authProvider.CanSignUp;
        public bool CanResetPassword => _authProvider.CanResetPassword;
        public bool CanUpdateLogin => _authProvider.CanUpdateLogin;
        public bool CanUpdateEmail => _authProvider.CanUpdateEmail;  
        public bool CanUpdatePassword => _authProvider.CanUpdatePassword;
        public bool CanUpdatePhone => _authProvider.CanUpdatePhone;
        public bool CanCancel => _authProvider.CanCancel;
        public bool CanResendCode => _authProvider.CanResendCode;

        public bool _IsBusy = false;
        public bool IsBusy 
        { 
            get { return _IsBusy; } 
            set { 
                SetProperty(ref _IsBusy, value);
                RaisePropertyChanged(nameof(IsNotBusy));
                RaisePropertyChanged(nameof(IsLongBusy));
            }
        }
        public bool IsNotBusy { get { return !_IsBusy; } }
        public bool IsLongBusy => _IsBusy && _authProvider.IsChallengeLongWait;
        public bool IsNotLongBusy => !IsLongBusy;

        #endregion

        #region Methods

        /// <summary>
        /// Raise All Properties except:
        /// AlertMessage, ProcessMessage, ChallengeMessage which are 
        /// handled directly in RaiseAuthModuleEventAndProperties
        /// </summary>
        private void RaiseAllProperties()
        {
            // This looks a little event heavy but isn't really
            // an issue as most of the bindings are hidden
            // or unused in the UI for any given auth process step.
            RaisePropertyChanged(nameof(CurrentChallenge));
            RaisePropertyChanged(nameof(CurrentAuthProcess));

            RaisePropertyChanged(nameof(Login));
            RaisePropertyChanged(nameof(NewLogin));
            RaisePropertyChanged(nameof(Password));
            RaisePropertyChanged(nameof(NewPassword));
            RaisePropertyChanged(nameof(Email));
            RaisePropertyChanged(nameof(NewEmail));
            RaisePropertyChanged(nameof(Phone));
            RaisePropertyChanged(nameof(NewPhone));
            RaisePropertyChanged(nameof(Code));

            RaisePropertyChanged(nameof(IsLoginFormatOk));
            RaisePropertyChanged(nameof(IsLoginVerified));
            RaisePropertyChanged(nameof(LoginNotVerified));

            RaisePropertyChanged(nameof(IsNewLoginFormatOk));
            RaisePropertyChanged(nameof(IsNewLoginVerified));
            RaisePropertyChanged(nameof(NewLoginNotVerified));

            RaisePropertyChanged(nameof(IsEmailFormatOk));
            RaisePropertyChanged(nameof(IsEmailVerified));
            RaisePropertyChanged(nameof(EmailNotVerified));

            RaisePropertyChanged(nameof(IsNewEmailFormatOk));
            RaisePropertyChanged(nameof(IsNewEmailVerified));
            RaisePropertyChanged(nameof(NewEmailNotVerified));

            RaisePropertyChanged(nameof(IsPasswordFormatOk));
            RaisePropertyChanged(nameof(IsPasswordVerified));
            RaisePropertyChanged(nameof(PasswordNotVerified));

            RaisePropertyChanged(nameof(IsNewPasswordFormatOk));
            RaisePropertyChanged(nameof(IsNewPasswordVerified));
            RaisePropertyChanged(nameof(NewPasswordNotVerified));

            RaisePropertyChanged(nameof(IsPhoneFormatOk));
            RaisePropertyChanged(nameof(IsPhoneVerified));
            RaisePropertyChanged(nameof(PhoneNotVerified));

            RaisePropertyChanged(nameof(IsNewPhoneFormatOk));
            RaisePropertyChanged(nameof(IsNewPhoneVerified));
            RaisePropertyChanged(nameof(NewPhoneNotVerified));

            RaisePropertyChanged(nameof(IsCodeFormatOk));
            RaisePropertyChanged(nameof(IsCodeVerified));
            RaisePropertyChanged(nameof(CodeNotVerified));

            RaisePropertyChanged(nameof(HasActiveAuthProcess));
            RaisePropertyChanged(nameof(NoActiveAuthProcess));

            RaisePropertyChanged(nameof(IsSignedIn));
            RaisePropertyChanged(nameof(IsNotSignedIn));

            RaisePropertyChanged(nameof(IsSigningIn));
            RaisePropertyChanged(nameof(IsSigningUp));
            RaisePropertyChanged(nameof(IsResettingPassword));
            RaisePropertyChanged(nameof(IsUpdatingLogin));
            RaisePropertyChanged(nameof(IsUpdatingEmail));
            RaisePropertyChanged(nameof(IsUpdatingPhone));
            RaisePropertyChanged(nameof(IsUpdatingPassword));

            RaisePropertyChanged(nameof(HasChallenge));
            RaisePropertyChanged(nameof(NoChallenge));

            RaisePropertyChanged(nameof(CurrentChallengeIsLogin));
            RaisePropertyChanged(nameof(CollectLogin));

            RaisePropertyChanged(nameof(CurrentChallengeIsNewLogin));
            RaisePropertyChanged(nameof(CollectNewLogin));

            RaisePropertyChanged(nameof(CurrentChallengeIsPassword));
            RaisePropertyChanged(nameof(CollectPassword));

            RaisePropertyChanged(nameof(CurrentChallengeIsNewPassword));
            RaisePropertyChanged(nameof(CollectNewPassword));

            RaisePropertyChanged(nameof(CurrentChallengeIsEmail));
            RaisePropertyChanged(nameof(CollectEmail));

            RaisePropertyChanged(nameof(CurrentChallengeIsNewEmail));
            RaisePropertyChanged(nameof(CollectNewEmail));

            RaisePropertyChanged(nameof(CurrentChallengeIsPhone));
            RaisePropertyChanged(nameof(CollectPhone));

            RaisePropertyChanged(nameof(CurrentChallengeIsNewPhone));
            RaisePropertyChanged(nameof(CollectNewPhone));

            RaisePropertyChanged(nameof(CurrentChallengeIsCode));
            RaisePropertyChanged(nameof(CollectCode));

            RaisePropertyChanged(nameof(HasAlert));

            // Currently Allowed AuthProcess
            RaisePropertyChanged(nameof(CanSignOut));
            RaisePropertyChanged(nameof(CanSignIn));
            RaisePropertyChanged(nameof(CanSignUp));
            RaisePropertyChanged(nameof(CanResetPassword));
            RaisePropertyChanged(nameof(CanUpdateLogin));
            RaisePropertyChanged(nameof(CanUpdateEmail));
            RaisePropertyChanged(nameof(CanUpdatePassword));
            RaisePropertyChanged(nameof(CanUpdatePhone));
            RaisePropertyChanged(nameof(CanCancel));
            RaisePropertyChanged(nameof(CanResendCode));

            RaisePropertyChanged(nameof(AlertMessage));

            RaisePropertyChanged(nameof(AuthChallengeMessage));

            RaisePropertyChanged(nameof(AuthProcessMessage));

        }

        private void ClearSensitiveFields()
        {
            Password = string.Empty;
            NewPassword = string.Empty;
            Code = string.Empty;
        }

        public async Task<AuthEventEnum> ClearAsync() => await Execute(_authProvider.ClearAsync());

        public async Task<AuthEventEnum> CancelAsync() 
        {
            ClearSensitiveFields();
            return await Execute(_authProvider.CancelAsync()); 
        }

        public async Task<AuthEventEnum> SignOutAsync() => await Execute(_authProvider.SignOutAsync());

        public async Task<AuthEventEnum> StartSignInAsync() => await Execute(_authProvider.StartSignInAsync());

        public async Task<AuthEventEnum> StartSignUpAsync() => await Execute(_authProvider.StartSignUpAsync());

        public async Task<AuthEventEnum> StartResetPasswordAsync() => await Execute(_authProvider.StartResetPasswordAsync());

        public async Task<AuthEventEnum> StartUpdateLoginAsync() => await Execute( _authProvider.StartUpdateLoginAsync());

        public async Task<AuthEventEnum> StartUpdateEmailAsync() => await Execute( _authProvider.StartUpdateEmailAsync());

        public async Task<AuthEventEnum> StartUpdatePhoneAsync() => await Execute( _authProvider.StartUpdatePhoneAsync());

        public async Task<AuthEventEnum> StartUpdatePasswordAsync() => await Execute( _authProvider.StartUpdatePasswordAsync());

        public async Task<AuthEventEnum> VerifyLoginAsync() => await Execute( _authProvider.VerifyLoginAsync(Login));

        public async Task<AuthEventEnum> VerifyNewLoginAsync() => await Execute( _authProvider.VerifyNewLoginAsync(NewLogin));

        public async Task<AuthEventEnum> VerifyPasswordAsync() => await Execute( _authProvider.VerifyPasswordAsync(Password));

        public async Task<AuthEventEnum> VerifyNewPasswordAsync() => await Execute( _authProvider.VerifyNewPasswordAsync(NewPassword));

        public async Task<AuthEventEnum> VerifyEmailAsync() => await Execute( _authProvider.VerifyEmailAsync(Email));

        public async Task<AuthEventEnum> VerifyNewEmailAsync() => await Execute( _authProvider.VerifyNewEmailAsync(NewEmail));

        public async Task<AuthEventEnum> VerifyPhoneAsync() => await Execute( _authProvider.VerifyPhoneAsync(Phone));

        public async Task<AuthEventEnum> VerifyNewPhoneAsync() => await Execute( _authProvider.VerifyNewPhoneAsync(NewPhone));

        public async Task<AuthEventEnum> VerifyCodeAsync() => await Execute( _authProvider.VerifyCodeAsync(Code));

        public async Task<AuthEventEnum> ResendCodeAsync() => await Execute( _authProvider.ResendCodeAsync());

        public bool CheckLoginFormat() 
        {
            var result = _authProvider.CheckLoginFormat(Login);
            RaisePropertyChanged(nameof(IsLoginFormatOk));
            return result;
        }

        public bool CheckEmailFormat()
        {

            var result = _authProvider.CheckEmailFormat(Email);
            RaisePropertyChanged(nameof(IsEmailFormatOk));
            return result;
        }

        public bool CheckPasswordFormat()
        {
            var result = _authProvider.CheckPasswordFormat(Password);
            RaisePropertyChanged(nameof(IsPasswordFormatOk));
            return result;
        }

        public bool CheckNewPasswordFormat()
        {
            var result = _authProvider.CheckNewPasswordFormat(NewPassword);
            RaisePropertyChanged(nameof(IsNewPasswordFormatOk));
            return result;
        }

        public bool CheckCodeFormat()
        {
            var result = _authProvider.CheckCodeFormat(Code);
            RaisePropertyChanged(nameof(IsCodeFormatOk));
            return result;
        }

        public bool CheckPhoneFormat()
        {
            var result = _authProvider.CheckPhoneFormat(Phone);
            RaisePropertyChanged(nameof(IsPhoneFormatOk));
            return result;
        }

        // Wrap an execution in a IsBusy and BusyDelay
        protected async virtual Task<AuthEventEnum> Execute(Task<AuthEventEnum> func)
        {
            IsBusy = true;
            var result = await RaiseAuthModuleEventAndProperties(await func);
            IsBusy = false;
            return result;
        }

        #endregion

        #region AuthProcessEvents
        public static event EventHandler<AuthModuleEventArgs> AuthModuleEventFired;

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnAuthModuleEvent(AuthModuleEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            AuthModuleEventFired?.Invoke(this, e);
        }

        protected async virtual Task<AuthEventEnum> RaiseAuthModuleEventAndProperties(AuthEventEnum r)
        {
            await Task.Delay(0);
            OnAuthModuleEvent(new AuthModuleEventArgs(r));
            // update alert message
            _alertMessage = string.Empty;
            if ((int)r >= (int)AuthEventEnum.Alert) // All enum items after the enum Alert are Alerts
            {
                string message = appConfig[$"AuthAlertMessages:{LanguageCode}:{r}"];
                _alertMessage =
                    message == null
                    ? r.ToString()
                    :message;
            }

            // update process message
            if (string.IsNullOrEmpty(AuthChallengeMessage) || lastAuthChallengeEnum != CurrentChallenge)
            {
                string challengeMessage = appConfig[$"AuthChallengeMessages:{LanguageCode}:{CurrentChallenge}"];
                _authChallengeMessage =
                    challengeMessage == null
                    ? CurrentChallenge.ToString()
                    : challengeMessage;
                lastAuthChallengeEnum = CurrentChallenge;
            }

            // update challenge message
            if(string.IsNullOrEmpty(AuthProcessMessage) || lastAuthProcessEnum != CurrentAuthProcess)
            {
                string processMessage = appConfig[$"AuthProcessMessages:{LanguageCode}:{CurrentAuthProcess}"];
                _authProcessMessage =
                    processMessage == null
                    ? CurrentAuthProcess.ToString()
                    : processMessage;
                lastAuthProcessEnum = CurrentAuthProcess;
            }

            if (r == AuthEventEnum.SignedIn 
                || r==AuthEventEnum.SignedUp 
                || r == AuthEventEnum.SignedOut 
                || r== AuthEventEnum.PasswordResetDone
                )
            {
                ClearSensitiveFields();
            }

            RaiseAllProperties();
            return r;
        }

        void RaiseAuthModuleEvent(AuthEventEnum r)
        {
            OnAuthModuleEvent(new AuthModuleEventArgs(r));
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <param name="onChanged">Action that is called after the property value has been changed.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="T:System.Runtime.CompilerServices.CallerMemberNameAttribute" />.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="T:System.Runtime.CompilerServices.CallerMemberNameAttribute" />.</param>
        [Obsolete("Please use the new RaisePropertyChanged method. This method will be removed to comply wth .NET coding standards. If you are overriding this method, you should overide the OnPropertyChanged(PropertyChangedEventArgs args) signature instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="args">The PropertyChangedEventArgs</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            this.PropertyChanged?.Invoke(this, args);
        }
        #endregion INotifyPropertyChanged Implementation
    }
}
