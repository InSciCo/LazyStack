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

//Orginal Order:
///*1,*/AuthEventEnum.Alert_AuthProcessAlreadyStarted
///*2,*/AuthEventEnum.Alert_DifferentAuthProcessActive
///*3,*/AuthEventEnum.Alert_IncorrectAuthProcess
///*4,*/AuthEventEnum.Alert_NoActiveAuthProcess
///*5,*/AuthEventEnum.AuthEventEnum.Alert_AlreadySignedIn
///*6,*/AuthEventEnum.Alert_InternalSignInError
///*7,*/AuthEventEnum.Alert_InternalSignUpError
///*8,*/AuthEventEnum.Alert_InternalProcessError
///*9,*/AuthEventEnum.Alert_SignUpMissingLogin
///*10,*/AuthEventEnum.Alert_SignUpMissingPassword
///*11,*/AuthEventEnum.Alert_SignUpMissingEmail
///*12,*/AuthEventEnum.Alert_SignUpMissingCode
///*13,*/AuthEventEnum.Alert_AuthAlreadyStarted
///*14,*/AuthEventEnum.Alert_InvalidCallToResendAsyncCode
///*15,*/AuthEventEnum.Alert_AccountWithThatEmailAlreadyExists
///*16,*/AuthEventEnum.Alert_RefreshUserDetailsDone
///*17,*/AuthEventEnum.Alert_EmailAddressIsTheSame
///*18,*/AuthEventEnum.Alert_VerifyCalledButNoChallengeFound
///*19,*/AuthEventEnum.Alert_CantRetrieveUserDetails
///*20,*/AuthEventEnum.Alert_NeedToBeSignedIn
///*21,*/AuthEventEnum.Alert_InvalidOperationWhenSignedIn
///*22,*/AuthEventEnum.Alert_UserNotFound
///*23,*/AuthEventEnum.Alert_NotConfirmed
///*24,*/AuthEventEnum.Alert_NotAuthorized
///*25,*/AuthEventEnum.Alert_VerifyFailed
///*26,*/AuthEventEnum.Alert_LoginAlreadyUsed
///*27,*/AuthEventEnum.Alert_LoginMustBeSuppliedFirst
///*28,*/AuthEventEnum.Alert_LoginFormatRequirementsFailed
///*29,*/AuthEventEnum.Alert_PasswordFormatRequirementsFailed
///*30,*/AuthEventEnum.Alert_EmailFormatRequirementsFailed
///*31,*/AuthEventEnum.Alert_PhoneFormatRequirementsFailed
///*32,*/AuthEventEnum.Alert_TooManyAttempts
///*33,*/AuthEventEnum.Alert_NothingToDo
///*34,*/AuthEventEnum.Alert_OperationNotSupportedByAuthProvider
///*35,*/AuthEventEnum.Alert_Unknown


//TEST-COVERAGE==================================>
//at-least-1-test:--------------------------------
//4,5
//14,15
//17
//20
//26
//28,29
//on-hold:----------------------------------------
//6,7,8
//16
//18,19
//35
//to-dO:------------------------------------------
//18,19,
//21,22,23,24,25
//27
//30,31,32,33,34
//no-references:----------------------------------
//1,2,3
//9,10,11,12,13
//------------------------------------------------


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
            
            #region TEST 0: Alert_NoActiveAuthProcess (4)
            //Not signed in, run against starting state
            Assert.IsTrue(await authProcess.VerifyNewLoginAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            Assert.IsTrue(await authProcess.VerifyNewPasswordAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            Assert.IsTrue(await authProcess.VerifyEmailAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            Assert.IsTrue(await authProcess.VerifyNewEmailAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            Assert.IsTrue(await authProcess.VerifyCodeAsync() == AuthEventEnum.Alert_NoActiveAuthProcess);
            #endregion(4)

            #region TEST 1: Create First Account: SignUp, SignIn
            //StartSignUpAsync
            Assert.IsTrue(await authProcess.StartSignUpAsync() == AuthEventEnum.AuthChallenge);
            //VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            //VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.AuthChallenge);
            //VerifyEmailAsync
            authProcess.Email = email;
            verificationCodeSendTime = DateTime.UtcNow;
            Thread.Sleep(5000); // Account for a little drift among local and remote clock
            Assert.IsTrue(await authProcess.VerifyEmailAsync() == AuthEventEnum.AuthChallenge);
            //VerifyCodeAsync
            var verificationCode = AuthEmail.GetAuthCode(appConfig, verificationCodeSendTime, email);
            Assert.IsNotNull(verificationCode);
            authProcess.Code = verificationCode;
            Assert.IsTrue(await authProcess.VerifyCodeAsync() == AuthEventEnum.SignedUp);
            //StartSignInAsync
            Assert.IsTrue(await authProcess.StartSignInAsync() == AuthEventEnum.AuthChallenge);
            //VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            //VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.SignedIn);
            #endregion
            //-user with 'login' 'email' 'password' now registered with cognito
            //-user with 'login' 'email' 'password' now signed in

            //Prereq: user with 'login' 'email' 'password' signed in
            #region TEST 2: Alert_AlreadySignedIn (5)
            //try to sign in agian, and it fails
            Assert.IsTrue(await authProcess.StartSignInAsync() == AuthEventEnum.Alert_AlreadySignedIn);
            #endregion

            //Prereq: user with 'login' 'email' 'password' signed in
            //Prereq: no current authprocess
            #region TEST 3: Alert_InvalidCallToResendAsyncCode (14)
            //Attempt to resend code without authprocess
            Assert.IsTrue(await authProcess.ResendCodeAsync() == AuthEventEnum.Alert_InvalidCallToResendAsyncCode);
            #endregion

            //Prereq: user with 'login' 'email' 'password' signed in
            #region TEST 4: Alert_EmailAddressIsTheSame (17)
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
            #region TEST 6: Bad Signup: Alert_AccountWithThatEmailAlreadyExists (15)
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
            //When cognito is queried inside AuthProviderCognito.NextChallege() when providerClient.SignUpAsync(signUpRequest) method is called,
            //we recieve a 'Alert_InternalProcessError' instead of the expected 'AliasExistsException'. We may add a check to see if the email 
            //exists before attemping a signup request with cognito.
            #endregion

            //Prereq: user with 'login' 'email' 'password' registered with cognito
            //Prereq: no user currently signed in
            #region TEST 7: Alert_LoginAlreadyUsed (26)
            //Attempt signup with dulicate Alias, here login
            //StartSignUpAsync
            Assert.IsTrue(await authProcess.StartSignUpAsync() == AuthEventEnum.AuthChallenge);
            // VerifyLoginAsync
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            // VerifyPasswordAsync
            authProcess.Password = password;
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.AuthChallenge);
            // VerifyEmailAsync
            authProcess.Email = newEmail;
            verificationCodeSendTime = DateTime.UtcNow;
            Thread.Sleep(5000); // Account for a little drift among local and remote clock
            Assert.IsTrue(await authProcess.VerifyEmailAsync() == AuthEventEnum.Alert_LoginAlreadyUsed);
            Assert.IsTrue(await authProcess.ClearAsync() == AuthEventEnum.Cleared);

            #endregion

            //Prereq: user with 'login' 'email' 'password' registered with cognito
            //Prereq: no user currently signed in
            #region TEST 8: Alert_NeedToBeSignedIn (20)
            Assert.IsTrue(await authProcess.StartUpdateEmailAsync() == AuthEventEnum.Alert_NeedToBeSignedIn);
            #region clear
            Assert.IsTrue(await authProcess.ClearAsync() == AuthEventEnum.Cleared);
            #endregion
            Assert.IsTrue(await authProcess.StartUpdatePasswordAsync() == AuthEventEnum.Alert_NeedToBeSignedIn);
            #region clear
            Assert.IsTrue(await authProcess.ClearAsync() == AuthEventEnum.Cleared);
            #endregion
            Assert.IsTrue(await authProcess.StartUpdateEmailAsync() == AuthEventEnum.Alert_NeedToBeSignedIn);
            #region clear
            Assert.IsTrue(await authProcess.ClearAsync() == AuthEventEnum.Cleared);
            #endregion
            //Alerts which cannot be reached externally:
            //Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.Alert_NeedToBeSignedIn);
            //Assert.IsTrue(await authProcess.VerifyNewEmailAsync() == AuthEventEnum.Alert_NeedToBeSignedIn);
            //Assert.IsTrue(await authProcess.ResendCodeAsync() == AuthEventEnum.Alert_NeedToBeSignedIn);
            //Assert.IsTrue(await noaccess.RefreshUserDetailsAsync() == AuthEventEnum.Alert_NeedToBeSignedIn);
            #endregion
            
            //Prereq: user with 'login' 'email' 'password' registered with cognito
            //Prereq: no user currently signed in
            #region TEST 9: Alert_LoginFormatRequirementsFailed (28)
            //StartSignUpAsync
            Assert.IsTrue(await authProcess.StartSignUpAsync() == AuthEventEnum.AuthChallenge);
            //VerifyLoginAsync
            authProcess.Login = "?$@";
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.Alert_LoginFormatRequirementsFailed);
            #region clear
            Assert.IsTrue(await authProcess.ClearAsync() == AuthEventEnum.Cleared);
            #endregion
            #endregion

            //Prereq: user with 'login' 'email' 'password' registered with cognito
            //Prereq: no user currently signed in
            #region TEST 10: Alert_PasswordFormatRequirementsFailed (29)
            //StartSignUpAsync
            Assert.IsTrue(await authProcess.StartSignUpAsync() == AuthEventEnum.AuthChallenge);
            //VerifyLoginAsync
            now = DateTime.Now;
            login = $"TestUser{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-{now.Second}";
            authProcess.Login = login;
            Assert.IsTrue(await authProcess.VerifyLoginAsync() == AuthEventEnum.AuthChallenge);
            authProcess.Password = "?$@";
            //VerifyPasswordAsync
            Assert.IsTrue(await authProcess.VerifyPasswordAsync() == AuthEventEnum.Alert_PasswordFormatRequirementsFailed);
            #region clear
            Assert.IsTrue(await authProcess.ClearAsync() == AuthEventEnum.Cleared);
            #endregion
            #endregion
        }
    }
} 
