using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.UserSecrets;
using System;
using System.IO;
using System.Text;
using LazyStackAuth;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;

using LazyStackDevUtil;

namespace LazyStackAuthTest
{
    [TestClass]
    public class SignInTests
    {
        public SignInTests()
        {
            var awsSettings = new AwsSettings("LazyStackAuthTest", "us-east-1");
            var json = awsSettings.BuildJson();
            appConfig = new ConfigurationBuilder()
                .AddUserSecrets<CallApiTests>() // used to get gmail account credentials for auth code
                .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
                .Build();

            userLogin = "TestUser1"; // Cognito Login
            password = "TestUser1!"; // Cognito Password

            // SignUpTestUser requires Gmail:Email and Gmail:Password in appConfig
            // The signup process sends your gmail account a code necessary for authentication
            Setup.SignUpTestUser<CallApiTests>(appConfig, userLogin, password); // Make sure TestUser1 exists in UserPool

        }

        IConfiguration appConfig;
        string userLogin;
        string password;

        [TestMethod]
        public void SignIn01()
        {
            // Cognito SignIn

            // Prerequsites: SignUpTest has been run successfully. SignUp creates
            // the user in Cognito. 
            var authProvider = new AuthProviderCognito(appConfig);
            var authProcess = new AuthProcess(authProvider);

            // Try SignIn starting with StartAuthAsync() -- no arguments
            var result = authProcess.StartAuthAsync().GetAwaiter().GetResult();
            Assert.IsTrue(result == AuthModuleEvent.AuthChallenge);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.Login);
            Assert.IsTrue(authProcess.HasChallenge == true);
            Assert.IsTrue(authProcess.IsAuthorized == false);

            result = authProcess.VerifyLoginAsync(userLogin).GetAwaiter().GetResult();
            Assert.IsTrue(result == AuthModuleEvent.AuthChallenge);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.Password);
            Assert.IsTrue(authProcess.HasChallenge == true);
            Assert.IsTrue(authProcess.IsAuthorized == false);

            result = authProcess.VerifyPasswordAsync(password).GetAwaiter().GetResult();
            Assert.IsTrue(result == AuthModuleEvent.Authorized);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.None);
            Assert.IsTrue(authProcess.HasChallenge == false);
            Assert.IsTrue(authProcess.IsAuthorized == true);

            result = authProcess.SignOut();
            Assert.IsTrue(result == AuthModuleEvent.SignedOut);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.None);
            Assert.IsTrue(authProcess.HasChallenge == false);
            Assert.IsTrue(authProcess.IsAuthorized == false);
        }

        [TestMethod]
        public void SignIn02()
        {
            var authProvider = new AuthProviderCognito(appConfig);
            var authProcess = new AuthProcess(authProvider);

            // Try SignIn starting StartAuthAsync(userLogin)
            var result = authProcess.StartAuthAsync(userLogin).GetAwaiter().GetResult();
            Assert.IsTrue(result == AuthModuleEvent.AuthChallenge);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.Password);
            Assert.IsTrue(authProcess.HasChallenge == true);
            Assert.IsTrue(authProcess.IsAuthorized == false);

            result = authProcess.VerifyPasswordAsync(password).GetAwaiter().GetResult();
            Assert.IsTrue(result == AuthModuleEvent.Authorized);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.None);
            Assert.IsTrue(authProcess.HasChallenge == false);
            Assert.IsTrue(authProcess.IsAuthorized == true);

            result = authProcess.SignOut();
            Assert.IsTrue(result == AuthModuleEvent.SignedOut);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.None);
            Assert.IsTrue(authProcess.HasChallenge == false);
            Assert.IsTrue(authProcess.IsAuthorized == false);

            // Try SignIn starting with StartAuthAysnc(userLogin, password)
            result = authProcess.StartAuthAsync(userLogin, password).GetAwaiter().GetResult();
            Assert.IsTrue(result == AuthModuleEvent.Authorized);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.None);
            Assert.IsTrue(authProcess.HasChallenge == false);
            Assert.IsTrue(authProcess.IsAuthorized == true);

            result = authProcess.SignOut();
            Assert.IsTrue(result == AuthModuleEvent.SignedOut);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.None);
            Assert.IsTrue(authProcess.HasChallenge == false);
            Assert.IsTrue(authProcess.IsAuthorized == false);

        }

        [TestMethod]
        public void SignIn03()
        {
            var authProvider = new AuthProviderCognito(appConfig);
            var authProcess = new AuthProcess(authProvider);

            // Test SignIn with illformed login
            var result = authProcess.StartAuthAsync("jj").GetAwaiter().GetResult();
            Assert.IsTrue(result == AuthModuleEvent.UserLoginRequirementsFailed);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.Login);
            Assert.IsTrue(authProcess.HasChallenge == true);
            Assert.IsTrue(authProcess.IsAuthorized == false);

            result = authProcess.StartAuthAsync("jj", password).GetAwaiter().GetResult();
            Assert.IsTrue(result == AuthModuleEvent.UserLoginRequirementsFailed);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.Login);
            Assert.IsTrue(authProcess.HasChallenge == true);
            Assert.IsTrue(authProcess.IsAuthorized == false);

            // Test SignIn with illformed password
            result = authProcess.StartAuthAsync(userLogin, "junk").GetAwaiter().GetResult();
            Assert.IsTrue(result == AuthModuleEvent.PasswordRequirementsFailed);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.Password);
            Assert.IsTrue(authProcess.HasChallenge == true);
            Assert.IsTrue(authProcess.IsAuthorized == false);
        }
    }
}
