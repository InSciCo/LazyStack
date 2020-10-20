using System;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;

using LazyStackAuth;
using LazyStackDevUtil;

namespace LazyStackAuthTest
{
    [TestClass]
    public class SignUpTests
    {
        public SignUpTests()
        {
            var awsSettings = new AwsSettings("LazyStackAuthTest", "us-east-1");
            var json = awsSettings.BuildJson();
            appConfig = new ConfigurationBuilder()
                .AddUserSecrets<SignUpTests>() 
                .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
                .Build();
        }

        IConfiguration appConfig;

        [TestMethod]
        public void SignUp01()
        {
            // test happy path signup
            var now = DateTime.Now;
            var userLogin = $"TestUser{now.Year}-{now.Month}-{now.Day}-{now.Second}-{now.Millisecond}";
            var password = "TestUser1!";
            var email = appConfig["Gmail:Email"];

            // Use Google email alias trick to allow us to use a single gmail account for our testing
            // ex: me999@gmai.com becomes me999+2020-10-20-25-96@gmail.com
            // This avoids aws complaining that the email already exists
            // These are one-time use accounts
            var emailParts = email.Split("@");
            email = emailParts[0] + "+" + userLogin + "@" + emailParts[1];

            var authProvider = new AuthProviderCognito(appConfig);
            var authProcess = new AuthProcess(authProvider);

            var verificationCodeSendTime = DateTime.UtcNow.AddSeconds(-3); // verficationCode sent after this time
            var authResult = authProcess.StartSignUpAsync(userLogin, password, email).GetAwaiter().GetResult();
            Assert.IsTrue(authResult == AuthModuleEvent.AuthChallenge);
            Assert.IsTrue(authProcess.HasChallenge);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.SignUp);

            var verificationCode = AuthEmail.GetAuthCode(appConfig, verificationCodeSendTime, email);
            Assert.IsNotNull(verificationCode);

            // Verify SignUp
            authResult = authProcess.VerifySignUpAsync(verificationCode).GetAwaiter().GetResult();
            Assert.IsTrue(authResult == AuthModuleEvent.SignedUp);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.None);
            Assert.IsTrue(authProcess.HasChallenge == false);

            // SignIn to new account
            authResult = authProcess.StartAuthAsync(userLogin, password).GetAwaiter().GetResult();
            Assert.IsTrue(authProcess.IsAuthorized);

            // SignOut of new account
            authResult = authProcess.SignOut();
            Assert.IsTrue(authProcess.IsAuthorized == false);
            Assert.IsTrue(authProcess.CurrentChallenge == AuthChallenges.None);
            Assert.IsTrue(authProcess.HasChallenge == false);
        }
    }
}
