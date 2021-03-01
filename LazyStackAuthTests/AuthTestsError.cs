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

//Errors to create tests for
//
//Alert_AuthProcessAlreadyStarted,
//Alert_DifferentAuthProcessActive,
//Alert_IncorrectAuthProcess,
//Alert_NoActiveAuthProcess,
//Alert_AlreadySignedIn,
//Alert_InternalSignInError,
//Alert_InternalSignUpError,
//Alert_InternalProcessError,
//Alert_SignUpMissingLogin,
//Alert_SignUpMissingPassword,
//Alert_SignUpMissingEmail,
//Alert_SignUpMissingCode,
//Alert_AuthAlreadyStarted,
//Alert_InvalidCallToResendAsyncCode,
//Alert_AccountWithThatEmailAlreadyExists,
//Alert_RefreshUserDetailsDone,
//Alert_EmailAddressIsTheSame,
//Alert_VerifyCalledButNoChallengeFound, 
//Alert_CantRetrieveUserDetails, 
//Alert_NeedToBeSignedIn, 
//Alert_InvalidOperationWhenSignedIn, 
//Alert_UserNotFound,  
//Alert_NotConfirmed,  
//Alert_NotAuthorized, 
//Alert_VerifyFailed, 
//Alert_LoginAlreadyUsed, 
//Alert_LoginMustBeSuppliedFirst,
//Alert_LoginFormatRequirementsFailed, 
//Alert_PasswordFormatRequirementsFailed, 
//Alert_EmailFormatRequirementsFailed,
//Alert_PhoneFormatRequirementsFailed,
//Alert_TooManyAttempts, 
//Alert_NothingToDo,
//Alert_OperationNotSupportedByAuthProvider,

//// Hail Marys
//Alert_Unknown

namespace PetStoreClientTests
{
    [TestClass]
    public class AuthTestsError
    {
        // Using Microsoft.Extensions.DependencyInjection  - not Xamarin's DI
        IServiceCollection services = new ServiceCollection();
        IServiceProvider serviceProvider;

        public AuthTestsError()
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
        public async Task TestAuthError()
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

            #region TEST 0: Alert_NoActiveAuthProcess
            //Not signed in, run against starting state
            Assert.IsTrue(await authProcess.VerifyNewLoginAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            Assert.IsTrue(await authProcess.VerifyNewPasswordAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            Assert.IsTrue(await authProcess.VerifyEmailAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            Assert.IsTrue(await authProcess.VerifyNewEmailAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            Assert.IsTrue(await authProcess.VerifyCodeAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            #endregion

            #region TEST 1: Create First Account: SignUp, SignIn
            //StartSignUpAsync
            Assert.IsTrue(await authProcess.StartSignUpAsync() == AuthEventEnum.AuthChallenge);
            // VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            // VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.AuthChallenge);
            // VerifyEmailAsync
            authProcess.Email = email;
            verificationCodeSendTime = DateTime.UtcNow;
            Thread.Sleep(5000); // Account for a little drift among local and remote clock
            Assert.IsTrue(await authProcess.VerifyEmailAsync() == AuthEventEnum.AuthChallenge);
            // VerifyCodeAsync
            var verificationCode = AuthEmail.GetAuthCode(appConfig, verificationCodeSendTime, email);
            Assert.IsNotNull(verificationCode);
            authProcess.Code = verificationCode;
            Assert.IsTrue(await authProcess.VerifyCodeAsync() == AuthEventEnum.SignedUp);
            // StartSignInAsync
            Assert.IsTrue(await authProcess.StartSignInAsync() == AuthEventEnum.AuthChallenge);
            // VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            //VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.SignedIn);
            #endregion
            //-user with 'login' 'email' 'password' now registered with cognito
            //-user with 'login' 'email' 'password' now signed in

            //Prereq: user with 'login' 'email' 'password' signed in
            #region TEST 2: Alert_AlreadySignedIn
            //try to sign in agian, and it fails
            Assert.IsTrue(await authProcess.StartSignInAsync() == AuthEventEnum.Alert_AlreadySignedIn);
            #endregion

            //Prereq: user with 'login' 'email' 'password' signed in
            //Prereq: no current authprocess
            #region TEST 3: Alert_InvalidCallToResendAsyncCode
            //Attempt to resend code without authprocess
            Assert.IsTrue(await authProcess.ResendCodeAsync() == AuthEventEnum.Alert_InvalidCallToResendAsyncCode);
            #endregion

            //Prereq: user with 'login' 'email' 'password' signed in
            #region TEST 4: Alert_EmailAddressIsTheSame
            //Attempt to update email to the email used to create the account
            Assert.IsTrue(await authProcess.StartUpdateEmailAsync() == AuthEventEnum.AuthChallenge);
            authProcess.NewEmail = email;
            Assert.IsTrue(await authProcess.VerifyNewEmailAsync() == AuthEventEnum.Alert_EmailAddressIsTheSame);
            #endregion

            //Prereq: user with 'login' 'email' 'password' signed in
            #region TEST 6: SignOut
            Assert.IsTrue(await authProcess.SignOutAsync() == AuthEventEnum.SignedOut);
            #endregion

            //Prereq: user with 'login' 'email' 'password' registered with cognito
            //Prereq: no user currently signed in
            #region TEST 6: Bad Signup: Alert_AccountWithThatEmailAlreadyExists
            now = DateTime.Now;
            var login2 = $"TestUser{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-{now.Second} + _2";
            var password2 = "TestUser1!";

            //StartSignUpAsync
            Assert.IsTrue(await authProcess.StartSignUpAsync() == AuthEventEnum.AuthChallenge);
            // VerifyLoginAsync
            authProcess.Login = login2;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            // VerifyPasswordAsync
            authProcess.Password = password2;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.AuthChallenge);
            // VerifyEmailAsync()
            authProcess.Email = email; //this causes the alert
            verificationCodeSendTime = DateTime.UtcNow;
            Thread.Sleep(5000);
            Assert.IsTrue(await authProcess.VerifyEmailAsync() == AuthEventEnum.Alert_InternalProcessError);
            Assert.IsTrue(await authProcess.ClearAsync() == AuthEventEnum.Cleared);
            //Issue 2021-0001
            /*When cognito is queried inside AuthProviderCognito.NextChallege() when providerClient.SignUpAsync(signUpRequest) method is called,
            we recieve a 'Alert_InternalProcessError' instead of the expected 'AliasExistsException'. We may add a check to see if the email 
            exists before attemping a signup request with cognito. */
            #endregion

            //Prereq: user with 'login' 'email' 'password' registered with cognito
            //Prereq: no user currently signed in
            #region TEST 7: Alert_LoginAlreadyUsed
            //Attempt signup with dulicate Alias, here login
            /* await authProcess.SignOutAsync();
            //create new email
            var _email = email;
            email = appConfig["Gmail:Email"];
            emailParts = email.Split("@");
            email = emailParts[0] + "+" + login1 + "cow" + "@" + emailParts[1];
            //reset login to what it was
            //authProcess.Login = _login;
            //attmept signup 
            // StartSignUpAsync
            Assert.IsTrue(await authProcess.StartSignUpAsync() == AuthEventEnum.AuthChallenge);
            // VerifyLoginAsync
            authProcess.Login = login1;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            // VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.AuthChallenge);
            // VerifyEmailAsync()
            authProcess.Email = email;
            verificationCodeSendTime = DateTime.UtcNow;
            Thread.Sleep(5000); // Account for a little drift among local and remote clock
            Assert.IsTrue(await authProcess.VerifyEmailAsync() == AuthEventEnum.AuthChallenge);
            // VerifyCodeAsync
            verificationCode = AuthEmail.GetAuthCode(appConfig, verificationCodeSendTime, email);
            Assert.IsNotNull(verificationCode);
            authProcess.Code = verificationCode;
            //Assert.IsTrue(await authProcess.?? == AuthEventEnum.Alert_LoginAlreadyUsed);
            #endregion
            //*/

        }
    }
} 


//ToDo
//Alert_NoActiveAuthProcess
    //Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);//cognito implmentation?
    //Assert.IsTrue(await authProcess.VerifyPhoneAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);//cognito implmentation?
    //Assert.IsTrue(await authProcess.VerifyNewPhoneAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);//cognito implmentation?

//Alert_InternalSignInError
//Alert_InternalProcessError */