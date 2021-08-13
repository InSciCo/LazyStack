using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using LazyStackAuth;


// Integration Tests for Auth library
// This integration test requires:
// An active PetStore stack
// AwsSettings file
// A gmail account
//  - gmail account security must have "Access to less secure apps" turned on
//  - We recommend creating a gmail account just for this purpose and that you don't use your primary gmail account
//  - We use gmail becuase it has an auto alias feature that makes it possible to create multiple Cognito user accounts
//      - gmail allows you to create an alias using the '+' char. ex: myemail@gmail.com, myemail+01@gmail.com
//      - each Cognito user account requires a unique email and it doesn't know myemail+01@gmail.com is an alias
//      - we create Cognito user accounts that all send email to the same email account through the individual aliases
// Visual Studio User Secrets containing the login and password for the gmail account:
//  {
//    "Gmail": {
//      "Email": "myemail@gmail.com",
//      "Password": "AbCDEFGHIJK!"
//    }
//  }
// 

namespace LazyStackAuthTests
{
    [TestClass]
    public class AuthTests
    {
        // Using Microsoft.Extensions.DependencyInjection  - not Xamarin's DI
        IServiceCollection services = new ServiceCollection();
        IServiceProvider serviceProvider;

        public AuthTests()
        {
            // Use AWS CloudFormation to get configuration of the LazyStackAuth stack.
            var awsSettings = AwsConfigAuth.GenerateSettingsJson("us-east-1", "LazyStackAuthTests").GetAwaiter().GetResult();
            using var authMessagesStream = Assembly.GetAssembly(typeof(IAuthProcess)).GetManifestResourceStream("LazyStackAuth.AuthMessages.json");
            IConfiguration appConfig = new ConfigurationBuilder()
                .AddUserSecrets<AuthTests>() //  used to get gmail account credentials for auth code
                .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(awsSettings)))
                .AddJsonStream(authMessagesStream)
                .Build();

            services.AddSingleton<IConfiguration>(appConfig);
            services.AddSingleton<IAuthProvider, AuthProviderCognito>(); // depends on IConfiguration
            services.AddSingleton<IAuthProcess, AuthProcess>(); // depends on IConfiguration, IAuthProvider

            serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Test SignUp, SignIn, ResetPassword, UpdateEmail
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestAuth()
        { 

            var authProcess = serviceProvider.GetService<IAuthProcess>();
            var appConfig = serviceProvider.GetService<IConfiguration>();

            // test happy path signup
            var now = DateTime.Now;
            var login = $"TestUser{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-{now.Second}";
            var password = "TestUser1!";
            var newPassword = "";

            // Use Google email alias trick to allow us to use a single gmail account for our testing
            // ex: me999@gmai.com becomes me999+2020-10-20-25-96@gmail.com
            // This avoids aws complaining that the email already exists
            // These are one-time use accounts
            var email = appConfig["Gmail:Email"];
            var emailParts = email.Split("@");
            email = emailParts[0] + "+" + login + "@" + emailParts[1];
            var newEmail = emailParts[0] + "+" + login + "new" + "@" + emailParts[1]; // used in update email test
            var verificationCodeSendTime = DateTime.UtcNow; // verificationCode sent after this time

            var stepResult = ObjectStateDump(10,  authProcess);
            #region Step: 10
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == false, "IsLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == false, "IsEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == true, "CanSignIn==true");
            Assert.IsTrue(authProcess.CanSignUp == true, "CanSignUp==true");
            Assert.IsTrue(authProcess.CanResetPassword == true, "CanResetPassword==true");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == false, "CanCancel==false");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 10

            // StartSignUpAsync
            Assert.IsTrue(await authProcess.StartSignUpAsync() == AuthEventEnum.AuthChallenge);
            stepResult = ObjectStateDump(11,  authProcess, stepResult);
            // State Changes Step: 11
            // CurrentChallenge==AuthChallengeEnum.Login
            // CurrentAuthProcess==AuthProcessEnum.SigningUp
            // HasActiveAuthProcess==true
            // NoActiveAuthProcess==false
            // IsSigningUp==true
            // HasChallenge==true
            // NoChallenge==false
            // CurrentChallengeIsLogin==true
            // CollectLogin==true
            // CollectPassword==true
            // CollectEmail==true
            // CanSignIn==false
            // CanSignUp==false
            // CanResetPassword==false
            // CanCancel==true
            #region Step: 11
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Login, "CurrentChallenge==AuthChallengeEnum.Login");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningUp, "CurrentAuthProcess==AuthProcessEnum.SigningUp");
            Assert.IsTrue(authProcess.IsLoginFormatOk == false, "IsLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == false, "IsEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == true, "IsSigningUp==true");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == true, "CurrentChallengeIsLogin==true");
            Assert.IsTrue(authProcess.CollectLogin == true, "CollectLogin==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == true, "CollectEmail==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 11

            // VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            stepResult = ObjectStateDump(12,  authProcess, stepResult);
            // State Changes Step: 12
            // CurrentChallenge==AuthChallengeEnum.Password
            // IsLoginFormatOk==true
            // IsLoginVerified==true
            // LoginNotVerified==false
            // CurrentChallengeIsLogin==false
            // CollectLogin==false
            // CurrentChallengeIsPassword==true
            // State Changes Step: 12
            // CurrentChallenge==AuthChallengeEnum.Password
            // IsLoginFormatOk==true
            // IsLoginVerified==true
            // LoginNotVerified==false
            // CurrentChallengeIsLogin==false
            // CollectLogin==false
            // CurrentChallengeIsPassword==true
            #region Step: 12
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Password, "CurrentChallenge==AuthChallengeEnum.Password");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningUp, "CurrentAuthProcess==AuthProcessEnum.SigningUp");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.LoginNotVerified == false, "LoginNotVerified==false");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == false, "IsEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == true, "IsSigningUp==true");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == true, "CurrentChallengeIsPassword==true");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == true, "CollectEmail==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 12


            // VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.AuthChallenge);
            stepResult = ObjectStateDump(13,  authProcess, stepResult);
            // State Changes Step: 13
            // CurrentChallenge==AuthChallengeEnum.Email
            // IsPasswordFormatOk==true
            // IsPasswordVerified==true
            // PasswordNotVerified==false
            // IsCleared==false
            // CurrentChallengeIsPassword==false
            // CollectPassword==false
            // CurrentChallengeIsEmail==true
            #region Step: 13
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Email, "CurrentChallenge==AuthChallengeEnum.Email");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningUp, "CurrentAuthProcess==AuthProcessEnum.SigningUp");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.LoginNotVerified == false, "LoginNotVerified==false");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == false, "IsEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.PasswordNotVerified == false, "PasswordNotVerified==false");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == false, "IsCleared==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == true, "IsSigningUp==true");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == true, "CurrentChallengeIsEmail==true");
            Assert.IsTrue(authProcess.CollectEmail == true, "CollectEmail==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 13


            // VerifyEmailAsync
            authProcess.Email = email;
            verificationCodeSendTime = DateTime.UtcNow;
            Thread.Sleep(5000); // Account for a little drift among local and remote clock
            Assert.IsTrue(await authProcess.VerifyEmailAsync() == AuthEventEnum.AuthChallenge);
            stepResult = ObjectStateDump(14,  authProcess, stepResult);
            // State Changes Step: 14
            // CurrentChallenge==AuthChallengeEnum.Code
            // IsEmailFormatOk==true
            // IsEmailVerified==true
            // EmailNotVerified==false
            // IsChallengeLongWait==true
            // CurrentChallengeIsEmail==false
            // CollectEmail==false
            // CurrentChallengeIsCode==true
            // CollectCode==true
            // CanResendCode==true
            #region Step: 14
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Code, "CurrentChallenge==AuthChallengeEnum.Code");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningUp, "CurrentAuthProcess==AuthProcessEnum.SigningUp");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.LoginNotVerified == false, "LoginNotVerified==false");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.EmailNotVerified == false, "EmailNotVerified==false");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.PasswordNotVerified == false, "PasswordNotVerified==false");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == false, "IsCleared==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == true, "IsSigningUp==true");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == true, "IsChallengeLongWait==true");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == true, "CurrentChallengeIsCode==true");
            Assert.IsTrue(authProcess.CollectCode == true, "CollectCode==true");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == true, "CanResendCode==true");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 14

            // VerifyCodeAsync
            var verificationCode = AuthEmail.GetAuthCode(appConfig, email);
            Assert.IsNotNull(verificationCode);
            authProcess.Code = verificationCode;
            Assert.IsTrue(await authProcess.VerifyCodeAsync() == AuthEventEnum.SignedUp);
            stepResult = ObjectStateDump(15,  authProcess, stepResult);
            // State Changes Step: 15
            // CurrentChallenge==AuthChallengeEnum.None
            // CurrentAuthProcess==AuthProcessEnum.None
            // IsLoginVerified==false
            // LoginNotVerified==true
            // IsEmailVerified==false
            // EmailNotVerified==true
            // IsPasswordFormatOk==false
            // IsPasswordVerified==false
            // PasswordNotVerified==true
            // IsCleared==true
            // HasActiveAuthProcess==false
            // NoActiveAuthProcess==true
            // IsSigningUp==false
            // IsChallengeLongWait==false
            // HasChallenge==false
            // NoChallenge==true
            // CurrentChallengeIsCode==false
            // CollectCode==false
            // CanSignIn==true
            // CanSignUp==true
            // CanResetPassword==true
            // CanCancel==false
            // CanResendCode==false
            #region Step: 15
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == true, "CanSignIn==true");
            Assert.IsTrue(authProcess.CanSignUp == true, "CanSignUp==true");
            Assert.IsTrue(authProcess.CanResetPassword == true, "CanResetPassword==true");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == false, "CanCancel==false");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 15


            // Test SignIn to new account
            // SignIn Happy Path
            Assert.IsTrue(await authProcess.StartSignInAsync() == AuthEventEnum.AuthChallenge);
            stepResult = ObjectStateDump(16,  authProcess, stepResult);
            // State Changes Step: 16
            // CurrentChallenge==AuthChallengeEnum.Login
            // CurrentAuthProcess==AuthProcessEnum.SigningIn
            // HasActiveAuthProcess==true
            // NoActiveAuthProcess==false
            // IsSigningIn==true
            // HasChallenge==true
            // NoChallenge==false
            // CurrentChallengeIsLogin==true
            // CollectLogin==true
            // CollectPassword==true
            // CanSignIn==false
            // CanSignUp==false
            // CanResetPassword==false
            // CanCancel==true
            #region Step: 16
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Login, "CurrentChallenge==AuthChallengeEnum.Login");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningIn, "CurrentAuthProcess==AuthProcessEnum.SigningIn");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == true, "IsSigningIn==true");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == true, "CurrentChallengeIsLogin==true");
            Assert.IsTrue(authProcess.CollectLogin == true, "CollectLogin==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 16

            // VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            stepResult = ObjectStateDump(20,  authProcess, stepResult);
            // State Changes Step: 20
            // CurrentChallenge==AuthChallengeEnum.Password
            // IsLoginVerified==true
            // LoginNotVerified==false
            // IsChallengeLongWait==true
            // CurrentChallengeIsLogin==false
            // CollectLogin==false
            // CurrentChallengeIsPassword==true
            #region Step: 20
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Password, "CurrentChallenge==AuthChallengeEnum.Password");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningIn, "CurrentAuthProcess==AuthProcessEnum.SigningIn");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.LoginNotVerified == false, "LoginNotVerified==false");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == true, "IsSigningIn==true");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == true, "IsChallengeLongWait==true");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == true, "CurrentChallengeIsPassword==true");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 20

            // VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.SignedIn);
            stepResult = ObjectStateDump(21,  authProcess, stepResult);
            // State Changes Step: 21
            // CurrentChallenge==AuthChallengeEnum.None
            // CurrentAuthProcess==AuthProcessEnum.None
            // IsLoginVerified==false
            // LoginNotVerified==true
            // IsSignedIn==true
            // IsNotSignedIn==false
            // HasActiveAuthProcess==false
            // NoActiveAuthProcess==true
            // IsSigningIn==false
            // IsChallengeLongWait==false
            // HasChallenge==false
            // NoChallenge==true
            // CurrentChallengeIsPassword==false
            // CollectPassword==false
            // CanSignOut==true
            // CanUpdateEmail==true
            // CanUpdatePassword==true
            // CanCancel==false
            #region Step: 21
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == true, "CanSignOut==true");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == true, "CanUpdateEmail==true");
            Assert.IsTrue(authProcess.CanUpdatePassword == true, "CanUpdatePassword==true");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == false, "CanCancel==false");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 21

            // SignOut
            Assert.IsTrue(await authProcess.SignOutAsync() == AuthEventEnum.SignedOut);
            stepResult = ObjectStateDump(22,  authProcess, stepResult);
            // State Changes Step: 22
            // IsSignedIn==false
            // IsNotSignedIn==true
            // CanSignOut==false
            // CanSignIn==true
            // CanSignUp==true
            // CanResetPassword==true
            // CanUpdateEmail==false
            // CanUpdatePassword==false
            #region Step: 22
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == true, "CanSignIn==true");
            Assert.IsTrue(authProcess.CanSignUp == true, "CanSignUp==true");
            Assert.IsTrue(authProcess.CanResetPassword == true, "CanResetPassword==true");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == false, "CanCancel==false");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 22

            // Test Reset Password
            Assert.IsTrue(await authProcess.StartResetPasswordAsync() == AuthEventEnum.AuthChallenge);
            stepResult = ObjectStateDump(30,  authProcess, stepResult);
            // State Changes Step: 30
            // CurrentChallenge==AuthChallengeEnum.Login
            // CurrentAuthProcess==AuthProcessEnum.ResettingPassword
            // HasActiveAuthProcess==true
            // NoActiveAuthProcess==false
            // IsResettingPassword==true
            // HasChallenge==true
            // NoChallenge==false
            // CurrentChallengeIsLogin==true
            // CollectLogin==true
            // CollectNewPassword==true
            // CanSignIn==false
            // CanSignUp==false
            // CanResetPassword==false
            // CanCancel==true
            #region Step: 30
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Login, "CurrentChallenge==AuthChallengeEnum.Login");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.ResettingPassword, "CurrentAuthProcess==AuthProcessEnum.ResettingPassword");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == true, "IsResettingPassword==true");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == true, "CurrentChallengeIsLogin==true");
            Assert.IsTrue(authProcess.CollectLogin == true, "CollectLogin==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == true, "CollectNewPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 30


            // VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(31,  authProcess, stepResult);
            // State Changes Step: 31
            // CurrentChallenge==AuthChallengeEnum.NewPassword
            // IsLoginVerified==true
            // LoginNotVerified==false
            // IsChallengeLongWait==true
            // CurrentChallengeIsLogin==false
            // CollectLogin==false
            // CurrentChallengeIsNewPassword==true
            #region Step: 31
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.NewPassword, "CurrentChallenge==AuthChallengeEnum.NewPassword");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.ResettingPassword, "CurrentAuthProcess==AuthProcessEnum.ResettingPassword");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.LoginNotVerified == false, "LoginNotVerified==false");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == true, "IsResettingPassword==true");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == true, "IsChallengeLongWait==true");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == true, "CurrentChallengeIsNewPassword==true");
            Assert.IsTrue(authProcess.CollectNewPassword == true, "CollectNewPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 31
            // VerifyNewPasswordAsync
            password = "TestUser1!reset";
            authProcess.NewPassword = password;
            verificationCodeSendTime = DateTime.UtcNow;
            Thread.Sleep(5000);
            Assert.IsTrue(await authProcess.VerifyNewPasswordAsync() == AuthEventEnum.AuthChallenge );
            stepResult = ObjectStateDump(32,  authProcess, stepResult);
            // State Changes Step: 32
            // CurrentChallenge==AuthChallengeEnum.Code
            // IsLoginVerified==true
            // LoginNotVerified==false
            // IsPasswordFormatOk==true
            // IsNewPasswordFormatOk==true
            // IsCleared==false
            // IsChallengeLongWait==true
            // CurrentChallengeIsLogin==false
            // CollectLogin==false
            // CollectNewPassword==false
            // CurrentChallengeIsCode==true
            // CollectCode==true
            // CanResendCode==true
            #region Step: 32
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Code, "CurrentChallenge==AuthChallengeEnum.Code");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.ResettingPassword, "CurrentAuthProcess==AuthProcessEnum.ResettingPassword");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.LoginNotVerified == false, "LoginNotVerified==false");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == true, "IsNewPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == false, "IsCleared==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == true, "IsResettingPassword==true");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == true, "IsChallengeLongWait==true");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == true, "CurrentChallengeIsCode==true");
            Assert.IsTrue(authProcess.CollectCode == true, "CollectCode==true");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == true, "CanResendCode==true");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 32
            // VerifyCodeAsync
            Thread.Sleep(5000);
            verificationCode = AuthEmail.GetAuthCode(appConfig, email);
            Assert.IsNotNull(verificationCode);
            authProcess.Code = verificationCode;

            Assert.IsTrue(await authProcess.VerifyCodeAsync() == AuthEventEnum.PasswordResetDone);
            ObjectStateDump(33,  authProcess, stepResult);
            // State Changes Step: 33
            // CurrentChallenge==AuthChallengeEnum.None
            // CurrentAuthProcess==AuthProcessEnum.None
            // IsLoginVerified==false
            // LoginNotVerified==true
            // IsPasswordFormatOk==false
            // IsNewPasswordFormatOk==false
            // IsCleared==true
            // HasActiveAuthProcess==false
            // NoActiveAuthProcess==true
            // IsResettingPassword==false
            // IsChallengeLongWait==false
            // HasChallenge==false
            // NoChallenge==true
            // CurrentChallengeIsCode==false
            // CollectCode==false
            // CanSignIn==true
            // CanSignUp==true
            // CanResetPassword==true
            // CanCancel==false
            // CanResendCode==false
            #region Step: 33
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == true, "CanSignIn==true");
            Assert.IsTrue(authProcess.CanSignUp == true, "CanSignUp==true");
            Assert.IsTrue(authProcess.CanResetPassword == true, "CanResetPassword==true");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == false, "CanCancel==false");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 33

            // Test Update Password
            Assert.IsTrue(await authProcess.StartSignInAsync() == AuthEventEnum.AuthChallenge);
            stepResult = ObjectStateDump(40,  authProcess, stepResult);
            // State Changes Step: 40
            // CurrentChallenge==AuthChallengeEnum.Login
            // CurrentAuthProcess==AuthProcessEnum.SigningIn
            // IsLoginVerified==false
            // LoginNotVerified==true
            // IsPasswordFormatOk==false
            // IsNewPasswordFormatOk==false
            // IsCleared==true
            // IsSigningIn==true
            // IsResettingPassword==false
            // IsChallengeLongWait==false
            // CurrentChallengeIsLogin==true
            // CollectLogin==true
            // CollectPassword==true
            // CurrentChallengeIsCode==false
            // CollectCode==false
            // CanResendCode==false
            #region Step: 40
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Login, "CurrentChallenge==AuthChallengeEnum.Login");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningIn, "CurrentAuthProcess==AuthProcessEnum.SigningIn");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == true, "IsSigningIn==true");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == true, "CurrentChallengeIsLogin==true");
            Assert.IsTrue(authProcess.CollectLogin == true, "CollectLogin==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 40
            // VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            authProcess.Password = password;
            stepResult = ObjectStateDump(41,  authProcess, stepResult);
            // State Changes Step: 41
            // CurrentChallenge==AuthChallengeEnum.Password
            // IsLoginVerified==true
            // LoginNotVerified==false
            // IsPasswordFormatOk==true
            // IsCleared==false
            // IsChallengeLongWait==true
            // CurrentChallengeIsLogin==false
            // CollectLogin==false
            // CurrentChallengeIsPassword==true
            #region Step: 41
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Password, "CurrentChallenge==AuthChallengeEnum.Password");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningIn, "CurrentAuthProcess==AuthProcessEnum.SigningIn");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.LoginNotVerified == false, "LoginNotVerified==false");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == false, "IsCleared==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == true, "IsSigningIn==true");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == true, "IsChallengeLongWait==true");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == true, "CurrentChallengeIsPassword==true");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 41

            // VerifyPasswordAsync
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.SignedIn);
            stepResult = ObjectStateDump(42,  authProcess, stepResult);
            // State Changes Step: 42
            // CurrentChallenge==AuthChallengeEnum.None
            // CurrentAuthProcess==AuthProcessEnum.None
            // IsLoginVerified==false
            // LoginNotVerified==true
            // IsPasswordFormatOk==false
            // IsCleared==true
            // IsSignedIn==true
            // IsNotSignedIn==false
            // HasActiveAuthProcess==false
            // NoActiveAuthProcess==true
            // IsSigningIn==false
            // IsChallengeLongWait==false
            // HasChallenge==false
            // NoChallenge==true
            // CurrentChallengeIsPassword==false
            // CollectPassword==false
            // CanSignOut==true
            // CanUpdateEmail==true
            // CanUpdatePassword==true
            // CanCancel==false
            #region Step: 42
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == true, "CanSignOut==true");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == true, "CanUpdateEmail==true");
            Assert.IsTrue(authProcess.CanUpdatePassword == true, "CanUpdatePassword==true");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == false, "CanCancel==false");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 42

            // Test Update Password -- using previous step's sign in
            Assert.IsTrue(await authProcess.StartUpdatePasswordAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(43,  authProcess, stepResult);
            // State Changes Step: 43
            // CurrentChallenge==AuthChallengeEnum.Password
            // CurrentAuthProcess==AuthProcessEnum.UpdatingPassword
            // HasActiveAuthProcess==true
            // NoActiveAuthProcess==false
            // IsUpdatingPassword==true
            // HasChallenge==true
            // NoChallenge==false
            // CurrentChallengeIsPassword==true
            // CollectPassword==true
            // CollectNewPassword==true
            // CanSignOut==false
            // CanUpdateEmail==false
            // CanUpdatePassword==false
            // CanCancel==true
            #region Step: 43
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Password, "CurrentChallenge==AuthChallengeEnum.Password");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.UpdatingPassword, "CurrentAuthProcess==AuthProcessEnum.UpdatingPassword");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == true, "IsUpdatingPassword==true");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == true, "CurrentChallengeIsPassword==true");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == true, "CollectNewPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 43

            // VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.AuthChallenge);
            stepResult = ObjectStateDump(44,  authProcess, stepResult);
            // State Changes Step: 44
            // CurrentChallenge==AuthChallengeEnum.NewPassword
            // CurrentAuthProcess==AuthProcessEnum.UpdatingPassword
            // IsPasswordFormatOk==true
            // IsPasswordVerified==true
            // PasswordNotVerified==false
            // IsCleared==false
            // HasActiveAuthProcess==true
            // NoActiveAuthProcess==false
            // IsUpdatingPassword==true
            // IsChallengeLongWait==true
            // HasChallenge==true
            // NoChallenge==false
            // CurrentChallengeIsNewPassword==true
            // CollectNewPassword==true
            // CanSignOut==false
            // CanUpdateEmail==false
            // CanUpdatePassword==false
            // CanCancel==true
            #region Step: 44
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.NewPassword, "CurrentChallenge==AuthChallengeEnum.NewPassword");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.UpdatingPassword, "CurrentAuthProcess==AuthProcessEnum.UpdatingPassword");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.PasswordNotVerified == false, "PasswordNotVerified==false");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == false, "IsCleared==false");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == true, "IsUpdatingPassword==true");
            Assert.IsTrue(authProcess.IsChallengeLongWait == true, "IsChallengeLongWait==true");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == true, "CurrentChallengeIsNewPassword==true");
            Assert.IsTrue(authProcess.CollectNewPassword == true, "CollectNewPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 44

            // VerifyNewPasswordAsync
            newPassword = "TestUser1!new";
            authProcess.NewPassword = newPassword;
            Assert.IsTrue(await authProcess.VerifyNewPasswordAsync() == AuthEventEnum.PasswordUpdateDone);
            password = newPassword;
            stepResult = ObjectStateDump(45,  authProcess, stepResult);
            // State Changes Step: 45
            // CurrentChallenge==AuthChallengeEnum.None
            // CurrentAuthProcess==AuthProcessEnum.None
            // IsPasswordVerified==false
            // PasswordNotVerified==true
            // IsNewPasswordFormatOk==true
            // HasActiveAuthProcess==false
            // NoActiveAuthProcess==true
            // IsUpdatingPassword==false
            // IsChallengeLongWait==false
            // HasChallenge==false
            // NoChallenge==true
            // CurrentChallengeIsNewPassword==false
            // CollectNewPassword==false
            // CanSignOut==true
            // CanUpdateEmail==true
            // CanUpdatePassword==true
            // CanCancel==false
            #region Step: 45
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == true, "IsNewPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == false, "IsCleared==false");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == true, "CanSignOut==true");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == true, "CanUpdateEmail==true");
            Assert.IsTrue(authProcess.CanUpdatePassword == true, "CanUpdatePassword==true");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == false, "CanCancel==false");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 45

            // Test Update email -- using previous test signin
            Assert.IsTrue(await authProcess.StartUpdateEmailAsync() == AuthEventEnum.AuthChallenge);
            stepResult = ObjectStateDump(50,  authProcess, stepResult);
            // State Changes Step: 50
            // CurrentChallenge==AuthChallengeEnum.NewEmail
            // CurrentAuthProcess==AuthProcessEnum.UpdatingEmail
            // IsPasswordFormatOk==false
            // IsNewPasswordFormatOk==false
            // IsCleared==true
            // HasActiveAuthProcess==true
            // NoActiveAuthProcess==false
            // IsUpdatingEmail==true
            // IsChallengeLongWait==true
            // HasChallenge==true
            // NoChallenge==false
            // CurrentChallengeIsNewEmail==true
            // CollectNewEmail==true
            // CanSignOut==false
            // CanUpdateEmail==false
            // CanUpdatePassword==false
            // CanCancel==true
            #region Step: 50
            Assert.IsTrue(authProcess.IsChatty == true, "IsChatty==true");
            Assert.IsTrue(authProcess.ClearAllFields == false, "ClearAllFields==false");
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.NewEmail, "CurrentChallenge==AuthChallengeEnum.NewEmail");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.UpdatingEmail, "CurrentAuthProcess==AuthProcessEnum.UpdatingEmail");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == true, "IsUpdatingEmail==true");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == true, "IsChallengeLongWait==true");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == true, "CurrentChallengeIsNewEmail==true");
            Assert.IsTrue(authProcess.CollectNewEmail == true, "CollectNewEmail==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 50

            // VerifyEmailAsync
            authProcess.NewEmail = newEmail;
            verificationCodeSendTime = DateTime.UtcNow;
            Thread.Sleep(5000); 
            Assert.IsTrue(await authProcess.VerifyNewEmailAsync() == AuthEventEnum.VerificationCodeSent);
            stepResult = ObjectStateDump(51,  authProcess, stepResult);
            // State Changes Step: 51
            // CurrentChallenge==AuthChallengeEnum.Code
            // IsEmailVerified==true
            // EmailNotVerified==false
            // CurrentChallengeIsNewEmail==false
            // CollectNewEmail==false
            // CurrentChallengeIsCode==true
            // CollectCode==true
            // CanResendCode==true
            #region Step: 51
            Assert.IsTrue(authProcess.IsChatty == true, "IsChatty==true");
            Assert.IsTrue(authProcess.ClearAllFields == false, "ClearAllFields==false");
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Code, "CurrentChallenge==AuthChallengeEnum.Code");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.UpdatingEmail, "CurrentAuthProcess==AuthProcessEnum.UpdatingEmail");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.EmailNotVerified == false, "EmailNotVerified==false");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == true, "IsUpdatingEmail==true");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == true, "IsChallengeLongWait==true");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == true, "CurrentChallengeIsCode==true");
            Assert.IsTrue(authProcess.CollectCode == true, "CollectCode==true");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == true, "CanCancel==true");
            Assert.IsTrue(authProcess.CanResendCode == true, "CanResendCode==true");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 51

            // VerifyCodeAsync
            verificationCode = AuthEmail.GetAuthCode(appConfig, newEmail);
            Assert.IsNotNull(verificationCode);
            authProcess.Code = verificationCode;
            Assert.IsTrue(await authProcess.VerifyCodeAsync() == AuthEventEnum.EmailUpdateDone);
            stepResult = ObjectStateDump(52,  authProcess, stepResult);
            // State Changes Step: 52
            // CurrentChallenge==AuthChallengeEnum.None
            // CurrentAuthProcess==AuthProcessEnum.None
            // IsEmailVerified==false
            // EmailNotVerified==true
            // IsCodeFormatOk==true
            // IsCleared==false
            // HasActiveAuthProcess==false
            // NoActiveAuthProcess==true
            // IsUpdatingEmail==false
            // IsChallengeLongWait==false
            // HasChallenge==false
            // NoChallenge==true
            // CurrentChallengeIsCode==false
            // CollectCode==false
            // CanSignOut==true
            // CanUpdateEmail==true
            // CanUpdatePassword==true
            // CanCancel==false
            // CanResendCode==false
            #region Step: 52
            Assert.IsTrue(authProcess.IsChatty == true, "IsChatty==true");
            Assert.IsTrue(authProcess.ClearAllFields == false, "ClearAllFields==false");
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == true, "IsCodeFormatOk==true");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == false, "IsCleared==false");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == true, "CanSignOut==true");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == true, "CanUpdateEmail==true");
            Assert.IsTrue(authProcess.CanUpdatePassword == true, "CanUpdatePassword==true");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == false, "CanCancel==false");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 52

            // Sign Out
            Assert.IsTrue(await authProcess.SignOutAsync() == AuthEventEnum.SignedOut);
            stepResult = ObjectStateDump(53,  authProcess, stepResult);
            // State Changes Step: 53
            // IsPasswordFormatOk==false
            // IsNewPasswordFormatOk==false
            // IsCodeFormatOk==false
            // IsCleared==true
            // IsSignedIn==false
            // IsNotSignedIn==true
            // CanSignOut==false
            // CanSignIn==true
            // CanSignUp==true
            // CanResetPassword==true
            // CanUpdateEmail==false
            // CanUpdatePassword==false
            #region Step: 53
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.LoginNotVerified == true, "LoginNotVerified==true");
            Assert.IsTrue(authProcess.IsNewLoginFormatOk == false, "IsNewLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsNewLoginVerified == false, "IsNewLoginVerified==false");
            Assert.IsTrue(authProcess.NewLoginNotVerified == true, "NewLoginNotVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.EmailNotVerified == true, "EmailNotVerified==true");
            Assert.IsTrue(authProcess.IsNewEmailFormatOk == false, "IsNewEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsNewEmailVerified == false, "IsNewEmailVerified==false");
            Assert.IsTrue(authProcess.NewEmailNotVerified == true, "NewEmailNotVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.PasswordNotVerified == true, "PasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.NewPasswordNotVerified == true, "NewPasswordNotVerified==true");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.PhoneNotVerified == true, "PhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsNewPhoneFormatOk == false, "IsNewPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPhoneVerified == false, "IsNewPhoneVerified==false");
            Assert.IsTrue(authProcess.NewPhoneNotVerified == true, "NewPhoneNotVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.CodeNotVerified == true, "CodeNotVerified==true");
            Assert.IsTrue(authProcess.IsCleared == true, "IsCleared==true");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingLogin == false, "IsUpdatingLogin==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.IsChallengeLongWait == false, "IsChallengeLongWait==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewLogin == false, "CurrentChallengeIsNewLogin==false");
            Assert.IsTrue(authProcess.CollectNewLogin == false, "CollectNewLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewEmail == false, "CurrentChallengeIsNewEmail==false");
            Assert.IsTrue(authProcess.CollectNewEmail == false, "CollectNewEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPhone == false, "CurrentChallengeIsNewPhone==false");
            Assert.IsTrue(authProcess.CollectNewPhone == false, "CollectNewPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == true, "CanSignIn==true");
            Assert.IsTrue(authProcess.CanSignUp == true, "CanSignUp==true");
            Assert.IsTrue(authProcess.CanResetPassword == true, "CanResetPassword==true");
            Assert.IsTrue(authProcess.CanUpdateLogin == false, "CanUpdateLogin==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            Assert.IsTrue(authProcess.CanCancel == false, "CanCancel==false");
            Assert.IsTrue(authProcess.CanResendCode == false, "CanResendCode==false");
            Assert.IsTrue(authProcess.IsBusy == false, "IsBusy==false");
            Assert.IsTrue(authProcess.IsNotBusy == true, "IsNotBusy==true");
            Assert.IsTrue(authProcess.IsLongBusy == false, "IsLongBusy==false");
            Assert.IsTrue(authProcess.IsNotLongBusy == true, "IsNotLongBusy==true");
            #endregion Step: 53

            // Invalid Login Format
            Assert.IsTrue(authProcess.CanSignIn);
            Assert.IsTrue(await authProcess.StartSignInAsync() == AuthEventEnum.AuthChallenge);
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin);
            authProcess.Login = "bad"; // login length is > 5
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.Alert_LoginFormatRequirementsFailed);
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin);
            authProcess.Login = login; // now provide good login
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);

            // Invalid Password Format
            authProcess.Password = "bad"; // password length >= 8
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.Alert_PasswordFormatRequirementsFailed);
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword);
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.SignedIn);
            Assert.IsTrue(authProcess.AuthProvider.IsCleared);

            Assert.IsTrue(await authProcess.SignOutAsync() == AuthEventEnum.SignedOut);
        }

        private List<string> ObjectStateDump(int step, IAuthProcess authProcess, List<string> prevStep = null)
        {
            var thisStep = new List<string>();
            
            string indent = "            ";
            // Generate Assert statements for curent AuthProcess State
            var strBld = new StringBuilder();

            strBld.AppendLine($"{indent}#region Step: {step}");
            foreach (var property in authProcess.GetType().GetProperties())
            {
                string rValue = string.Empty;
                object v = property.GetValue(authProcess);
                switch (v)
                {
                    case bool b:
                        rValue = b.ToString().ToLower();
                        break;

                    case AuthChallengeEnum c:
                        rValue = $"AuthChallengeEnum.{c.ToString()}";
                        break;

                    case AuthProcessEnum p:
                        rValue = $"AuthProcessEnum.{p.ToString()}";

                        break;

                    case AuthEventEnum e:
                        rValue = $"AuthEventEnum.{e.ToString()}";
                        break;
                }
                if (!string.IsNullOrEmpty(rValue))
                {
                    strBld.AppendLine($"{indent}Assert.IsTrue(authProcess.{property.Name}=={rValue}, \"{property.Name}=={rValue}\");");
                    thisStep.Add($"{property.Name}=={rValue}");
                }
            }
            strBld.AppendLine($"{indent}#endregion Step: {step}");

            int i = 0;
            var strBld2 = new StringBuilder();
            if (prevStep != null)
                foreach (var item in prevStep)
                {
                    if (!item.Equals(thisStep[i]))
                    {
                        strBld2.Append(indent);
                        strBld2.Append("// ");
                        strBld2.AppendLine(thisStep[i]);
                    }
                    i++;
                }
            var str2 = strBld2.ToString();
            if(!string.IsNullOrEmpty(str2))
            {
                Console.WriteLine($"{indent}// State Changes Step: {step}");
                Console.Write(strBld2);
            }
            Console.Write(strBld.ToString());

            return thisStep;
        }
    }
}
