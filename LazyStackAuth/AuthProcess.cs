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
    /// AuthProcess add events, INotifyPropertyChanged
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
        #endregion 

        #region Properties
        public string LanguageCode { get; set; } = "en-US";

        readonly IAuthProvider _authProvider;
        public IAuthProvider AuthProvider { get { return _authProvider; } }
        public AuthChallengeEnum CurrentChallenge => _authProvider.CurrentChallenge;
        public AuthProcessEnum CurrentAuthProcess => _authProvider.CurrentAuthProcess;

        private string _login;
        public string Login
        {
            get { return _login; }
            set {
                SetProperty(ref _login, value);
                CheckLoginFormat();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set {
                SetProperty(ref _password, value);
                CheckPasswordFormat();
            }
        }

        private string _newPassword;
        public string NewPassword
        {
            get { return _newPassword; }
            set {
                SetProperty(ref _newPassword, value);
                CheckNewPasswordFormat();
            }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set {
                SetProperty(ref _email, value);
                CheckEmailFormat();
            }
        }

        private string _code;
        public string Code
        {
            get { return _code; }
            set {
                SetProperty(ref _code, value);
                CheckCodeFormat();
            }
        }

        private string _phone;
        public string Phone
        {
            get { return _phone; }
            set { SetProperty(ref _phone, value); }
        }

        // Entry Properties states
        public bool IsLoginFormatOk => _authProvider.IsLoginFormatOk;
        public bool IsLoginVerified => _authProvider.IsLoginVerified;
        public bool IsEmailFormatOk => _authProvider.IsEmailFormatOk;
        public bool IsEmailVerified => _authProvider.IsEmailVerified;
        public bool IsPasswordFormatOk => _authProvider.IsPasswordFormatOk;
        public bool IsPasswordVerified => _authProvider.IsPasswordVerified;
        public bool IsCodeFormatOk => _authProvider.IsCodeFormatOk;
        public bool IsCodeVerified => _authProvider.IsCodeVerified;
        public bool IsNewPasswordFormatOk => _authProvider.IsNewPasswordFormatOk;
        public bool IsNewPasswordVerified => _authProvider.IsNewPasswordVerified;
        public bool IsPhoneFormatOk => _authProvider.IsPhoneFormatOk;
        public bool IsPhoneVerified => _authProvider.IsPhoneVerified;

        // Auth state
        public bool IsSignedIn => _authProvider.IsSignedIn;
        public bool IsNotSignedIn => !_authProvider.IsSignedIn;

        // CurrentAuthProcess
        public bool HasActiveAuthProcess => CurrentAuthProcess != AuthProcessEnum.None;
        public bool NoActiveAuthProcess => CurrentAuthProcess == AuthProcessEnum.None;
        public bool IsSigningIn => CurrentAuthProcess == AuthProcessEnum.SigningIn;
        public bool IsSigningUp => CurrentAuthProcess == AuthProcessEnum.SigningUp;
        public bool IsResettingPassword => CurrentAuthProcess == AuthProcessEnum.ResettingPassword;
        public bool IsUpdatingEmail => CurrentAuthProcess == AuthProcessEnum.UpdatingEmail;
        public bool IsUpdatingPhone => CurrentAuthProcess == AuthProcessEnum.UpdatingPhone;
        public bool IsUpdatingPassword => CurrentAuthProcess == AuthProcessEnum.UpdatingPassword;


        // Challenge states
        public bool HasChallenge => _authProvider.CurrentChallenge != AuthChallengeEnum.None;
        public bool NoChallenge => _authProvider.CurrentChallenge == AuthChallengeEnum.None;

        public bool CurrentChallengeIsLogin => _authProvider.CurrentChallenge == AuthChallengeEnum.Login;
        public bool CollectLogin => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.Login);

        public bool CurrentChallengeIsPassword => _authProvider.CurrentChallenge == AuthChallengeEnum.Password;
        public bool CollectPassword => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.Password);

        public bool CurrentChallengeIsNewPassword => _authProvider.CurrentChallenge == AuthChallengeEnum.NewPassword;
        public bool CollectNewPassword => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.NewPassword);

        public bool CurrentChallengeIsEmail => _authProvider.CurrentChallenge == AuthChallengeEnum.Email;
        public bool CollectEmail => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.Email);

        public bool CurrentChallengeIsPhone => _authProvider.CurrentChallenge == AuthChallengeEnum.Phone;
        public bool CollectPhone => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.Phone);

        public bool CurrentChallengeIsCode => _authProvider.CurrentChallenge == AuthChallengeEnum.Code;
        public bool CollectCode => _authProvider.AuthChallengeList.Contains(AuthChallengeEnum.Code);

        // Alert states
        private string _alertMessage = string.Empty;
        public string AlertMessage { get { return _alertMessage; } }
        public bool HasAlert { get { return _alertMessage.Length > 0; } }

        // Currently Allowed AuthProcess
        public bool CanSignOut => IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanSignIn => !IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanSignUp => !IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanResetPassword => !IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanUpdateEmail => IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanUpdatePassword => IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;
        public bool CanUpdatePhone => IsSignedIn && CurrentAuthProcess == AuthProcessEnum.None;

        //public Dictionary<string, Dictionary<AuthEventEnum, string>> AlertMessages =
        //    new Dictionary<string, Dictionary<AuthEventEnum, string>> 
        //    {
        //        { "en-US", 
        //            new Dictionary<AuthEventEnum,string>
        //            {
        //                { AuthEventEnum.Alert_AuthAlreadyStarted, "Auth Aready Started" },
        //            }
        //        } 
        //    };
        #endregion

        #region Methods
        private void RaiseAllProperties()
        {
            // This looks a little event heavy but isn't really
            // an issue as most of the bindings are hidden
            // or unused in the UI for any given auth process step.
            RaisePropertyChanged(nameof(CurrentChallenge));
            RaisePropertyChanged(nameof(CurrentAuthProcess));

            RaisePropertyChanged(nameof(Login));
            RaisePropertyChanged(nameof(Password));
            RaisePropertyChanged(nameof(NewPassword));
            RaisePropertyChanged(nameof(Email));
            RaisePropertyChanged(nameof(Code));
            RaisePropertyChanged(nameof(Phone));

            RaisePropertyChanged(nameof(IsLoginFormatOk));
            RaisePropertyChanged(nameof(IsLoginVerified));
            RaisePropertyChanged(nameof(IsEmailFormatOk));
            RaisePropertyChanged(nameof(IsEmailVerified));
            RaisePropertyChanged(nameof(IsPasswordFormatOk));
            RaisePropertyChanged(nameof(IsPasswordVerified));
            RaisePropertyChanged(nameof(IsCodeFormatOk));
            RaisePropertyChanged(nameof(IsCodeVerified));
            RaisePropertyChanged(nameof(IsNewPasswordFormatOk));
            RaisePropertyChanged(nameof(IsNewPasswordVerified));
            RaisePropertyChanged(nameof(IsPhoneFormatOk));
            RaisePropertyChanged(nameof(IsPhoneVerified));

            RaisePropertyChanged(nameof(HasActiveAuthProcess));
            RaisePropertyChanged(nameof(NoActiveAuthProcess));

            RaisePropertyChanged(nameof(IsSignedIn));
            RaisePropertyChanged(nameof(IsNotSignedIn));

            RaisePropertyChanged(nameof(IsSigningIn));
            RaisePropertyChanged(nameof(IsSigningUp));
            RaisePropertyChanged(nameof(IsResettingPassword));
            RaisePropertyChanged(nameof(IsUpdatingEmail));
            RaisePropertyChanged(nameof(IsUpdatingPhone));
            RaisePropertyChanged(nameof(IsUpdatingPassword));

            RaisePropertyChanged(nameof(HasChallenge));
            RaisePropertyChanged(nameof(NoChallenge));

            RaisePropertyChanged(nameof(CurrentChallengeIsLogin));
            RaisePropertyChanged(nameof(CollectLogin));

            RaisePropertyChanged(nameof(CurrentChallengeIsPassword));
            RaisePropertyChanged(nameof(CollectPassword));

            RaisePropertyChanged(nameof(CurrentChallengeIsNewPassword));
            RaisePropertyChanged(nameof(CollectNewPassword));

            RaisePropertyChanged(nameof(CurrentChallengeIsEmail));
            RaisePropertyChanged(nameof(CollectEmail));

            RaisePropertyChanged(nameof(CurrentChallengeIsPhone));
            RaisePropertyChanged(nameof(CollectPhone));

            RaisePropertyChanged(nameof(CurrentChallengeIsCode));
            RaisePropertyChanged(nameof(CollectCode));

            RaisePropertyChanged(nameof(AlertMessage));
            RaisePropertyChanged(nameof(HasAlert));

            // Currently Allowed AuthProcess
            RaisePropertyChanged(nameof(CanSignOut));
            RaisePropertyChanged(nameof(CanSignIn));
            RaisePropertyChanged(nameof(CanSignUp));
            RaisePropertyChanged(nameof(CanResetPassword));
            RaisePropertyChanged(nameof(CanUpdateEmail));
            RaisePropertyChanged(nameof(CanUpdatePassword));
            RaisePropertyChanged(nameof(CanUpdatePhone));

        }

        private void ClearSensitiveFields()
        {
            Password = string.Empty;
            NewPassword = string.Empty;
            Code = string.Empty;
        }

        public AuthEventEnum Clear()
        {
            ClearSensitiveFields();
            return RaiseAuthModuleEventAndProperties(_authProvider.Clear());
        }

        public AuthEventEnum SignOut()
        {
            return RaiseAuthModuleEventAndProperties(_authProvider.SignOut());
        }

        public async Task<AuthEventEnum> StartSignInAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.StartSignInAsync());

        public async Task<AuthEventEnum> StartSignUpAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.StartSignUpAsync());

        public async Task<AuthEventEnum> StartResetPasswordAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.StartResetPasswordAsync());

        public async Task<AuthEventEnum> StartUpdateEmailAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.StartUpdateEmailAsync());

        public async Task<AuthEventEnum> StartUpdatePhoneAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.StartUpdatePhoneAsync());

        public async Task<AuthEventEnum> StartUpdatePasswordAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.StartUpdatePasswordAsync());

        public async Task<AuthEventEnum> VerifyLoginAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.VerifyLoginAsync(Login));

        public async Task<AuthEventEnum> VerifyPasswordAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.VerifyPasswordAsync(Password));

        public async Task<AuthEventEnum> VerifyEmailAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.VerifyEmailAsync(Email));

        public async Task<AuthEventEnum> VerifyCodeAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.VerifyCodeAsync(Code));

        public async Task<AuthEventEnum> VerifyNewPasswordAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.VerifyNewPasswordAsync(NewPassword));

        public async Task<AuthEventEnum> VerifyPhoneAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.VerifyPhoneAsync(Phone));

        public async Task<AuthEventEnum> ResendCodeAsync() => RaiseAuthModuleEventAndProperties(await _authProvider.ResendCodeAsync());

        public bool CheckLoginFormat() {
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

        protected virtual AuthEventEnum RaiseAuthModuleEventAndProperties(AuthEventEnum r)
        {
            OnAuthModuleEvent(new AuthModuleEventArgs(r));
            _alertMessage = string.Empty;
            if ((int)r >= (int)AuthEventEnum.Alert) // All enum items after the enum Alert are Alerts
            {
                string message = appConfig[$"AuthMessages:{LanguageCode}:{r.ToString()}"];
                _alertMessage =
                    string.IsNullOrEmpty(message)
                    ? r.ToString()
                    :message;
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
