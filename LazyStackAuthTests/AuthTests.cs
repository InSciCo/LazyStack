using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using LazyStackAuth;
using LazyStack;

// Integration Tests for Auth library
// This integration test requires:
// An active PetStore stack
// AwsSettings file
// A gmail account
//  - gmail account security must have "Access to less secure apps" turned on
//  - We recommend creating a gmail account just for this purpose and that you don't use your real gmail account
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

namespace PetStoreClientTests
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
            var startStep = 0;
            var endStep = 9999;

            var authProcess = serviceProvider.GetService<IAuthProcess>();
            var appConfig = serviceProvider.GetService<IConfiguration>();

            // test happy path signup
            var now = DateTime.Now;
            var login = $"TestUser{now.Year}-{now.Month}-{now.Day}-{now.Second}-{now.Millisecond}";
            var password = "TestUser1!";

            // Use Google email alias trick to allow us to use a single gmail account for our testing
            // ex: me999@gmai.com becomes me999+2020-10-20-25-96@gmail.com
            // This avoids aws complaining that the email already exists
            // These are one-time use accounts
            var email = appConfig["Gmail:Email"];
            var emailParts = email.Split("@");
            email = emailParts[0] + "+" + login + "@" + emailParts[1];
            var email2 = emailParts[0] + "+" + login + "new" + "@" + emailParts[1]; // used in update email test
            var verificationCodeSendTime = DateTime.UtcNow; // verificationCode sent after this time


            ObjectStateDump(10, startStep, endStep, authProcess);
            #region Step: 10
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == false, "IsLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.IsEmailFormatOk == false, "IsEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == true, "CanSignIn==true");
            Assert.IsTrue(authProcess.CanSignUp == true, "CanSignUp==true");
            Assert.IsTrue(authProcess.CanResetPassword == true, "CanResetPassword==true");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 10

            // StartSignUpAsync
            Assert.IsTrue(await authProcess.StartSignUpAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(11, startStep, endStep, authProcess);
            #region Step: 11
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Login, "CurrentChallenge==AuthChallengeEnum.Login");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningUp, "CurrentAuthProcess==AuthProcessEnum.SigningUp");
            Assert.IsTrue(authProcess.IsLoginFormatOk == false, "IsLoginFormatOk==false");
            Assert.IsTrue(authProcess.IsLoginVerified == false, "IsLoginVerified==false");
            Assert.IsTrue(authProcess.IsEmailFormatOk == false, "IsEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == true, "IsSigningUp==true");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == true, "CurrentChallengeIsLogin==true");
            Assert.IsTrue(authProcess.CollectLogin == true, "CollectLogin==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == true, "CollectEmail==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 11

            // VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(12, startStep, endStep, authProcess);
            #region Step: 12
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Password, "CurrentChallenge==AuthChallengeEnum.Password");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningUp, "CurrentAuthProcess==AuthProcessEnum.SigningUp");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == false, "IsEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == false, "IsPasswordVerified==false");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == true, "IsSigningUp==true");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == true, "CurrentChallengeIsPassword==true");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == true, "CollectEmail==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 12

            // VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(13, startStep, endStep, authProcess);
            #region Step: 13
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Email, "CurrentChallenge==AuthChallengeEnum.Email");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningUp, "CurrentAuthProcess==AuthProcessEnum.SigningUp");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == false, "IsEmailFormatOk==false");
            Assert.IsTrue(authProcess.IsEmailVerified == false, "IsEmailVerified==false");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == true, "IsSigningUp==true");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == true, "CurrentChallengeIsEmail==true");
            Assert.IsTrue(authProcess.CollectEmail == true, "CollectEmail==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 13

            // VerifyEmailAsync
            authProcess.Email = email;
            Assert.IsTrue(await authProcess.VerifyEmailAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(14, startStep, endStep, authProcess);
            #region Step: 14
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Code, "CurrentChallenge==AuthChallengeEnum.Code");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningUp, "CurrentAuthProcess==AuthProcessEnum.SigningUp");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == false, "IsCodeVerified==false");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == true, "IsSigningUp==true");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == true, "CurrentChallengeIsCode==true");
            Assert.IsTrue(authProcess.CollectCode == true, "CollectCode==true");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 14

            // VerifyCodeAsync
            var verificationCode = AuthEmail.GetAuthCode(appConfig, verificationCodeSendTime, email);
            Assert.IsNotNull(verificationCode);
            authProcess.Code = verificationCode;
            Assert.IsTrue(await authProcess.VerifyCodeAsync() == AuthEventEnum.SignedIn);
            ObjectStateDump(15, startStep, endStep, authProcess);
            #region Step: 15
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == true, "CanSignIn==true");
            Assert.IsTrue(authProcess.CanSignUp == true, "CanSignUp==true");
            Assert.IsTrue(authProcess.CanResetPassword == true, "CanResetPassword==true");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 15

            // Test SignIn to new account
            // SignIn Happy Path
            Assert.IsTrue(await authProcess.StartSignInAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(16, startStep, endStep, authProcess);
            #region Step: 16
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Login, "CurrentChallenge==AuthChallengeEnum.Login");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningIn, "CurrentAuthProcess==AuthProcessEnum.SigningIn");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == true, "IsSigningIn==true");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == true, "CurrentChallengeIsLogin==true");
            Assert.IsTrue(authProcess.CollectLogin == true, "CollectLogin==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 16

            // VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(20, startStep, endStep, authProcess);
            #region Step: 20
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Password, "CurrentChallenge==AuthChallengeEnum.Password");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningIn, "CurrentAuthProcess==AuthProcessEnum.SigningIn");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == true, "IsSigningIn==true");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == true, "CurrentChallengeIsPassword==true");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 20

            // VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.SignedIn);
            ObjectStateDump(21, startStep, endStep, authProcess);
            #region Step: 21
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == true, "CanSignOut==true");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == true, "CanUpdateEmail==true");
            Assert.IsTrue(authProcess.CanUpdatePassword == true, "CanUpdatePassword==true");
            Assert.IsTrue(authProcess.CanUpdatePhone == true, "CanUpdatePhone==true");
            #endregion Step: 21

            // SignOut
            Assert.IsTrue(authProcess.SignOut() == AuthEventEnum.SignedOut);
            ObjectStateDump(22, startStep, endStep, authProcess);
            #region Step: 22
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == true, "CanSignIn==true");
            Assert.IsTrue(authProcess.CanSignUp == true, "CanSignUp==true");
            Assert.IsTrue(authProcess.CanResetPassword == true, "CanResetPassword==true");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 22

            // Test Reset Password
            Assert.IsTrue(await authProcess.StartResetPasswordAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(30, startStep, endStep, authProcess);
            #region Step: 30
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Login, "CurrentChallenge==AuthChallengeEnum.Login");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.ResettingPassword, "CurrentAuthProcess==AuthProcessEnum.ResettingPassword");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == true, "IsResettingPassword==true");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == true, "CurrentChallengeIsLogin==true");
            Assert.IsTrue(authProcess.CollectLogin == true, "CollectLogin==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == true, "CollectNewPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 30

            // VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(31, startStep, endStep, authProcess);
            #region Step: 31
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.NewPassword, "CurrentChallenge==AuthChallengeEnum.NewPassword");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.ResettingPassword, "CurrentAuthProcess==AuthProcessEnum.ResettingPassword");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == true, "IsResettingPassword==true");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == true, "CurrentChallengeIsNewPassword==true");
            Assert.IsTrue(authProcess.CollectNewPassword == true, "CollectNewPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 31

            // VerifyNewPasswordAsync
            password = "TestUser1!reset";
            authProcess.NewPassword = password;
            Assert.IsTrue(await authProcess.VerifyNewPasswordAsync() == AuthEventEnum.AuthChallenge );
            ObjectStateDump(32, startStep, endStep, authProcess);
            #region Step: 32
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Code, "CurrentChallenge==AuthChallengeEnum.Code");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.ResettingPassword, "CurrentAuthProcess==AuthProcessEnum.ResettingPassword");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == true, "IsNewPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == true, "IsResettingPassword==true");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == true, "CurrentChallengeIsCode==true");
            Assert.IsTrue(authProcess.CollectCode == true, "CollectCode==true");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 32

            // VerifyCodeAsync
            verificationCode = AuthEmail.GetAuthCode(appConfig, verificationCodeSendTime, email);
            Assert.IsNotNull(verificationCode);
            authProcess.Code = verificationCode;
            Assert.IsTrue(await authProcess.VerifyCodeAsync() == AuthEventEnum.PasswordResetDone);
            ObjectStateDump(33, startStep, endStep, authProcess);
            #region Step: 33
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == true, "CanSignIn==true");
            Assert.IsTrue(authProcess.CanSignUp == true, "CanSignUp==true");
            Assert.IsTrue(authProcess.CanResetPassword == true, "CanResetPassword==true");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 33

            // Test Update Password
            Assert.IsTrue(await authProcess.StartSignInAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(40, startStep, endStep, authProcess);
            #region Step: 40
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Login, "CurrentChallenge==AuthChallengeEnum.Login");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningIn, "CurrentAuthProcess==AuthProcessEnum.SigningIn");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == true, "IsSigningIn==true");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == true, "CurrentChallengeIsLogin==true");
            Assert.IsTrue(authProcess.CollectLogin == true, "CollectLogin==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 40

            // VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            authProcess.Password = password;
            ObjectStateDump(41, startStep, endStep, authProcess);
            #region Step: 41
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Password, "CurrentChallenge==AuthChallengeEnum.Password");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.SigningIn, "CurrentAuthProcess==AuthProcessEnum.SigningIn");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == true, "IsSigningIn==true");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == true, "CurrentChallengeIsPassword==true");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 41


            // VerifyPasswordAsync
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.SignedIn);
            ObjectStateDump(42, startStep, endStep, authProcess);
            #region Step: 42
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == true, "CanSignOut==true");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == true, "CanUpdateEmail==true");
            Assert.IsTrue(authProcess.CanUpdatePassword == true, "CanUpdatePassword==true");
            Assert.IsTrue(authProcess.CanUpdatePhone == true, "CanUpdatePhone==true");
            #endregion Step: 42

            // Test Update Password -- using previous step's sign in
            Assert.IsTrue(await authProcess.StartUpdatePasswordAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(43, startStep, endStep, authProcess);
            #region Step: 43
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Password, "CurrentChallenge==AuthChallengeEnum.Password");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.UpdatingPassword, "CurrentAuthProcess==AuthProcessEnum.UpdatingPassword");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == true, "IsUpdatingPassword==true");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == true, "CurrentChallengeIsPassword==true");
            Assert.IsTrue(authProcess.CollectPassword == true, "CollectPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == true, "CollectNewPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 43

            // VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(44, startStep, endStep, authProcess);
            #region Step: 44
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.NewPassword, "CurrentChallenge==AuthChallengeEnum.NewPassword");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.UpdatingPassword, "CurrentAuthProcess==AuthProcessEnum.UpdatingPassword");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == true, "IsUpdatingPassword==true");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == true, "CurrentChallengeIsNewPassword==true");
            Assert.IsTrue(authProcess.CollectNewPassword == true, "CollectNewPassword==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 44

            // VerifyNewPasswordAsync
            password = "TestUser1!new";
            authProcess.NewPassword = password;
            Assert.IsTrue(await authProcess.VerifyNewPasswordAsync() == AuthEventEnum.PasswordUpdateDone);
            ObjectStateDump(45, startStep, endStep, authProcess);
            #region Step: 45
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == true, "IsNewPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == true, "CanSignOut==true");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == true, "CanUpdateEmail==true");
            Assert.IsTrue(authProcess.CanUpdatePassword == true, "CanUpdatePassword==true");
            Assert.IsTrue(authProcess.CanUpdatePhone == true, "CanUpdatePhone==true");
            #endregion Step: 45


            // Test Update email -- using previous test signin
            Assert.IsTrue(await authProcess.StartUpdateEmailAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(50, startStep, endStep, authProcess);
            #region Step: 50
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Email, "CurrentChallenge==AuthChallengeEnum.Email");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.UpdatingEmail, "CurrentAuthProcess==AuthProcessEnum.UpdatingEmail");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == true, "IsNewPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == true, "IsUpdatingEmail==true");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == true, "CurrentChallengeIsEmail==true");
            Assert.IsTrue(authProcess.CollectEmail == true, "CollectEmail==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 50

            // VerifyEmailAsync
            authProcess.Email = email2;
            Assert.IsTrue(await authProcess.VerifyEmailAsync() == AuthEventEnum.AuthChallenge);
            ObjectStateDump(51, startStep, endStep, authProcess);
            #region Step: 51
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.Code, "CurrentChallenge==AuthChallengeEnum.Code");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.UpdatingEmail, "CurrentAuthProcess==AuthProcessEnum.UpdatingEmail");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == true, "IsNewPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == true, "HasActiveAuthProcess==true");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == false, "NoActiveAuthProcess==false");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == true, "IsUpdatingEmail==true");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == true, "HasChallenge==true");
            Assert.IsTrue(authProcess.NoChallenge == false, "NoChallenge==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == true, "CurrentChallengeIsCode==true");
            Assert.IsTrue(authProcess.CollectCode == true, "CollectCode==true");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
            #endregion Step: 51

            // VerifyCodeAsync
            verificationCode = AuthEmail.GetAuthCode(appConfig, verificationCodeSendTime, email2);
            Assert.IsNotNull(verificationCode);
            authProcess.Code = verificationCode;
            Assert.IsTrue(await authProcess.VerifyCodeAsync() == AuthEventEnum.EmailUpdateDone);
            ObjectStateDump(52, startStep, endStep, authProcess);
            #region Step: 52
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == true, "IsPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == true, "IsCodeFormatOk==true");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == true, "IsNewPasswordFormatOk==true");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == true, "IsSignedIn==true");
            Assert.IsTrue(authProcess.IsNotSignedIn == false, "IsNotSignedIn==false");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == true, "CanSignOut==true");
            Assert.IsTrue(authProcess.CanSignIn == false, "CanSignIn==false");
            Assert.IsTrue(authProcess.CanSignUp == false, "CanSignUp==false");
            Assert.IsTrue(authProcess.CanResetPassword == false, "CanResetPassword==false");
            Assert.IsTrue(authProcess.CanUpdateEmail == true, "CanUpdateEmail==true");
            Assert.IsTrue(authProcess.CanUpdatePassword == true, "CanUpdatePassword==true");
            Assert.IsTrue(authProcess.CanUpdatePhone == true, "CanUpdatePhone==true");
            #endregion Step: 52

            // Sign Out
            authProcess.SignOut();
            ObjectStateDump(53, startStep, endStep, authProcess);
            #region Step: 53
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallengeEnum.None, "CurrentChallenge==AuthChallengeEnum.None");
            Assert.IsTrue(authProcess.CurrentAuthProcess == AuthProcessEnum.None, "CurrentAuthProcess==AuthProcessEnum.None");
            Assert.IsTrue(authProcess.IsLoginFormatOk == true, "IsLoginFormatOk==true");
            Assert.IsTrue(authProcess.IsLoginVerified == true, "IsLoginVerified==true");
            Assert.IsTrue(authProcess.IsEmailFormatOk == true, "IsEmailFormatOk==true");
            Assert.IsTrue(authProcess.IsEmailVerified == true, "IsEmailVerified==true");
            Assert.IsTrue(authProcess.IsPasswordFormatOk == false, "IsPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsPasswordVerified == true, "IsPasswordVerified==true");
            Assert.IsTrue(authProcess.IsCodeFormatOk == false, "IsCodeFormatOk==false");
            Assert.IsTrue(authProcess.IsCodeVerified == true, "IsCodeVerified==true");
            Assert.IsTrue(authProcess.IsNewPasswordFormatOk == false, "IsNewPasswordFormatOk==false");
            Assert.IsTrue(authProcess.IsNewPasswordVerified == false, "IsNewPasswordVerified==false");
            Assert.IsTrue(authProcess.IsPhoneFormatOk == false, "IsPhoneFormatOk==false");
            Assert.IsTrue(authProcess.IsPhoneVerified == false, "IsPhoneVerified==false");
            Assert.IsTrue(authProcess.IsSignedIn == false, "IsSignedIn==false");
            Assert.IsTrue(authProcess.IsNotSignedIn == true, "IsNotSignedIn==true");
            Assert.IsTrue(authProcess.HasActiveAuthProcess == false, "HasActiveAuthProcess==false");
            Assert.IsTrue(authProcess.NoActiveAuthProcess == true, "NoActiveAuthProcess==true");
            Assert.IsTrue(authProcess.IsSigningIn == false, "IsSigningIn==false");
            Assert.IsTrue(authProcess.IsSigningUp == false, "IsSigningUp==false");
            Assert.IsTrue(authProcess.IsResettingPassword == false, "IsResettingPassword==false");
            Assert.IsTrue(authProcess.IsUpdatingEmail == false, "IsUpdatingEmail==false");
            Assert.IsTrue(authProcess.IsUpdatingPhone == false, "IsUpdatingPhone==false");
            Assert.IsTrue(authProcess.IsUpdatingPassword == false, "IsUpdatingPassword==false");
            Assert.IsTrue(authProcess.HasChallenge == false, "HasChallenge==false");
            Assert.IsTrue(authProcess.NoChallenge == true, "NoChallenge==true");
            Assert.IsTrue(authProcess.CurrentChallengeIsLogin == false, "CurrentChallengeIsLogin==false");
            Assert.IsTrue(authProcess.CollectLogin == false, "CollectLogin==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPassword == false, "CurrentChallengeIsPassword==false");
            Assert.IsTrue(authProcess.CollectPassword == false, "CollectPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsNewPassword == false, "CurrentChallengeIsNewPassword==false");
            Assert.IsTrue(authProcess.CollectNewPassword == false, "CollectNewPassword==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsEmail == false, "CurrentChallengeIsEmail==false");
            Assert.IsTrue(authProcess.CollectEmail == false, "CollectEmail==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsPhone == false, "CurrentChallengeIsPhone==false");
            Assert.IsTrue(authProcess.CollectPhone == false, "CollectPhone==false");
            Assert.IsTrue(authProcess.CurrentChallengeIsCode == false, "CurrentChallengeIsCode==false");
            Assert.IsTrue(authProcess.CollectCode == false, "CollectCode==false");
            Assert.IsTrue(authProcess.HasAlert == false, "HasAlert==false");
            Assert.IsTrue(authProcess.CanSignOut == false, "CanSignOut==false");
            Assert.IsTrue(authProcess.CanSignIn == true, "CanSignIn==true");
            Assert.IsTrue(authProcess.CanSignUp == true, "CanSignUp==true");
            Assert.IsTrue(authProcess.CanResetPassword == true, "CanResetPassword==true");
            Assert.IsTrue(authProcess.CanUpdateEmail == false, "CanUpdateEmail==false");
            Assert.IsTrue(authProcess.CanUpdatePassword == false, "CanUpdatePassword==false");
            Assert.IsTrue(authProcess.CanUpdatePhone == false, "CanUpdatePhone==false");
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

            Assert.IsTrue(authProcess.SignOut() == AuthEventEnum.SignedOut);
        }

        private void ObjectStateDump(int step, int startStep, int endStep, IAuthProcess authProcess)
        {
            if (step < startStep || step > endStep)
                return;
           
            string indent = "            ";
            // Generate Assert statements for curent AuthProcess State
            Console.WriteLine($"{indent}#region Step: {step}");
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
                if(!string.IsNullOrEmpty(rValue))
                    Console.WriteLine($"{indent}Assert.IsTrue(authProcess.{property.Name}=={rValue}, \"{property.Name}=={rValue}\");");

            }
            Console.WriteLine($"{indent}#endregion Step: {step}");
        }
    }
}
