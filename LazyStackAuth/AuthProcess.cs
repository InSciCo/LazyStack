using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LazyStackAuth
{
    /// <summary>
    /// AuthProcess add events, INotifyPropertyChanged
    /// and workflow to classes implementing IAuthProvider
    /// </summary>
    public class AuthProcess : INotifyPropertyChanged
	{
		public AuthProcess(IAuthProvider authProvider)
		{
			_authProvider = authProvider;
		}

		#region Fields
		#endregion

		#region Properties
		readonly IAuthProvider _authProvider;
		public IAuthProvider AuthProvider { get { return _authProvider; } }

		private string _userLogin; 
		public string UserLogin
		{
			get { return _userLogin = _authProvider.UserLogin; }
			set
			{
				if (SetProperty(ref _userLogin, value))
					_authProvider.UserLogin = value;
			}
		}

		private string _userEmail;
		public string UserEmail
		{
			get { return _userEmail = _authProvider.UserEmail; }
			set
			{
				if (SetProperty(ref _userEmail, value))
					_authProvider.UserEmail = _userEmail;
			}
		}

		public bool IsAuthorized => _authProvider.IsAuthorized; 

		public bool HasChallenge => _authProvider.CurrentChallenge != AuthChallenges.None;

		public AuthChallenges CurrentChallenge => _authProvider.CurrentChallenge;

		public bool IsVerifyLoginChallenge => _authProvider.CurrentChallenge == AuthChallenges.Login;

		public bool IsVerifyPasswordChallenge => _authProvider.CurrentChallenge == AuthChallenges.Password;
		
		public bool IsVerifyPasswordUpdateChallenge => _authProvider.CurrentChallenge == AuthChallenges.PasswordUpdate;

		public bool IsVerifySignupChallenge => _authProvider.CurrentChallenge == AuthChallenges.SignUp;

		public bool IsVerifyPasswordResetChallenge=> _authProvider.CurrentChallenge == AuthChallenges.PasswordReset;

		public bool IsVerifyEmailChallenge => _authProvider.CurrentChallenge == AuthChallenges.Email;

		public bool CanCallStartAuth => !IsAuthorized; 

		public bool CanCallRefreshUserDetails => IsAuthorized;

		public bool CanCallSignOut => IsAuthorized; 

		public bool CanCallPasswordReset => !IsAuthorized;

		public bool CanCallEmailUpdate => IsAuthorized;

		#endregion

		#region Methods

		private void RaiseAllProperties()
        {
			// This looks a little event heavy but isn't really
			// an issue as most of the bindings are hidden
			// or unused in the UI for any given auth process step.
			RaisePropertyChanged(nameof(IsAuthorized));
			RaisePropertyChanged(nameof(UserLogin));
			RaisePropertyChanged(nameof(UserEmail));

			RaisePropertyChanged(nameof(HasChallenge));
			RaisePropertyChanged(nameof(CurrentChallenge));
			RaisePropertyChanged(nameof(IsVerifyLoginChallenge));
			RaisePropertyChanged(nameof(IsVerifyPasswordChallenge));
			RaisePropertyChanged(nameof(IsVerifyPasswordUpdateChallenge));
			RaisePropertyChanged(nameof(IsVerifySignupChallenge));
			RaisePropertyChanged(nameof(IsVerifyPasswordResetChallenge));
			RaisePropertyChanged(nameof(IsVerifyEmailChallenge));

			RaisePropertyChanged(nameof(CanCallStartAuth));
			RaisePropertyChanged(nameof(CanCallRefreshUserDetails));
			RaisePropertyChanged(nameof(CanCallSignOut));
			RaisePropertyChanged(nameof(CanCallPasswordReset));
			RaisePropertyChanged(nameof(CanCallEmailUpdate));
		}

		public async Task<AuthModuleEvent> StartAuthAsync()
        {
			var result = await _authProvider.StartAuthAsync();
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
        }
		
		public async Task<AuthModuleEvent> StartAuthAsync(string userLogin)
        {
			var result = await _authProvider.StartAuthAsync(userLogin);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> StartAuthAsync(string userLogin, string password)
        {
			var result = await _authProvider.StartAuthAsync(userLogin, password);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> VerifyLoginAsync(string userLogin)
		{
			var result = await _authProvider.VerifyLoginAsync(userLogin);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> VerifyPasswordAsync(string password)
		{
			var result = await _authProvider.VerifyPasswordAsync(password);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> VerifyMFACodeAsync(string mfaCode)
		{
			var result = await _authProvider.VerifyMFACodeAsync(mfaCode);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> VerifyPasswordUpdateAsync(string newPassword)
        {
			var result = await _authProvider.VerifyPasswordUpdateAsync(newPassword);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> RefreshUserDetailsAsync()
		{
			var result = await _authProvider.RefreshUserDetailsAsync();
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public AuthModuleEvent SignOut()
        {
			var result = _authProvider.SignOut();
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
        }

		public async Task<AuthModuleEvent> StartSignUpAsync(string userName, string password, string email)
        {
			var result = await _authProvider.StartSignUpAsync(userName, password, email);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> ResendVerifySignupCodeAsync(string userLogin)
        {
			var result = await _authProvider.ResendVerifySignupCodeAsync(userLogin);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> VerifySignUpAsync(string code)
        {
			var result = await _authProvider.VerifySignUpAsync(code);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> ResetPasswordAsync(string userLogin)
        {
			_authProvider.Clear(); // Clear any existing challenges
			var result = await _authProvider.ResetPasswordAsync(userLogin);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> VerifyPasswordResetAsync(string password, string code)
        {
			var result = await _authProvider.VerifyPasswordResetAsync(password, code);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> UpdateEmailAsync(string newUserEmail)
        {
			var result = await _authProvider.UpdateEmailAsync(newUserEmail);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> ResendVerifyEmailUpdateAsync(string newUserEmail)
		{
			var result = await _authProvider.ResendVerifyEmailUpdateAsync(newUserEmail);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public async Task<AuthModuleEvent> VerifyEmailUpdateAsync(string code)
        {
			var result = await _authProvider.VerifyEmailUpdateAsync(code);
			RaiseAuthModuleEvent(result);
			RaiseAllProperties();
			return result;
		}

		public void Clear()
        {
			RaiseAllProperties();
			_authProvider.Clear();
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

        void RaiseAuthModuleEvent(AuthModuleEvent r)
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
