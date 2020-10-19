using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.Extensions.Configuration;

using LazyStackAuth;

namespace LazyStackDevUtil
{
    /// <summary>
    /// This is NOT A TEST class. 
    /// SignUpTestUser1() is a utility class to make sure a required
    /// user is in the stack's User Pool. Do not confuse this simple happy
    /// path execution of signing up a new user with the SignUpTests.
    /// </summary>
    public static class Setup
    {
        public static void SignUpTestUser1<T>()
            where T : class
        {
            // Sign up TestUser1 if this user is not in the UserPool
            var awsSettings = new AwsSettings("LazyStackAuthTest", "us-east-1");
            var json = awsSettings.BuildJson();
            var appConfig = new ConfigurationBuilder()
                    .AddUserSecrets<T>()
                    .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
                    .Build();

            var userLogin = "TestUser1"; // Cognito Login
            var password = "TestUser1!"; // Cognito Password

            var authProvider = new AuthProviderCognito(appConfig);
            var authProcess = new AuthProcess(authProvider);
            // Try SignIn with TestUser1
            authProcess.StartAuthAsync(userLogin, password).GetAwaiter().GetResult();

            if (!authProcess.IsAuthorized)
            {   // SignUp

                var email = appConfig["Gmail:Email"];

                var verificationCodeSendTime = DateTime.UtcNow.AddSeconds(-3); // verficationCode sent after this time
                var authResult = authProcess.StartSignUpAsync(userLogin, password, email).GetAwaiter().GetResult();

                if (authResult == AuthModuleEvent.UserLoginAlreadyUsed)
                {
                    authResult = authProcess.ResendVerifySignupCodeAsync(userLogin).GetAwaiter().GetResult();
                }
                else if (authProcess.CurrentChallenge != AuthChallenges.SignUp)
                    throw new Exception($"Unexpected problem in user signup. Auth Error {authResult}");

                var verificationCode = AuthEmail.GetAuthCode(appConfig, verificationCodeSendTime);

                // Verify SignUp
                authResult = authProcess.VerifySignUpAsync(verificationCode).GetAwaiter().GetResult();
                if (authResult != AuthModuleEvent.SignedUp)
                    throw new Exception($"Auth Error {authResult}");

                // SignIn to new account
                authResult = authProcess.StartAuthAsync(userLogin, password).GetAwaiter().GetResult();
                if (!authProcess.IsAuthorized)
                    throw new Exception($"Auth Error {authResult}");
            }
            authProcess.SignOut();
        }
    }
}
