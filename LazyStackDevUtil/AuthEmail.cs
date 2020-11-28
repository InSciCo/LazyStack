using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Configuration;

using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

namespace LazyStackDevUtil
{
    public static class AuthEmail
    {
      
        public static string GetAuthCode(IConfiguration appConfig, DateTime verificationCodeSendTime, string emailTo)
        {
            // Start SignUp process - will send  a verification code to specified email account
            var email = appConfig["Gmail:Email"];
            var emailPassword = appConfig["Gmail:Password"];
            var verificationCode = string.Empty;

            bool foundCode = false;
            var tryCount = 0;
            do
            {
                Debug.WriteLine($"tryCount {tryCount}");
                Thread.Sleep(3000);
                try
                {
                    tryCount++;
                    var messages = new List<string>();
                    using (var mailClient = new ImapClient())
                    {
                        mailClient.Connect("imap.gmail.com", 993, true);
                        // Note: since we don't have an OAuth2 token, disable
                        // the XOAUTH2 authentication mechanism.
                        mailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                        mailClient.Authenticate(email, emailPassword);

                        var inbox = mailClient.Inbox;

                        var query =
                            SearchQuery.SubjectContains("Your verification code")
                            .And
                            (SearchQuery.DeliveredAfter(verificationCodeSendTime))
                            .And
                            (SearchQuery.ToContains(emailTo));

                        inbox.Open(FolderAccess.ReadOnly);

                        var results = inbox.Search(query);

                        foreach (var uid in results)
                        {
                            var message = inbox.GetMessage(uid);
                            Debug.WriteLine($"{message.Date} {verificationCodeSendTime} {message.HtmlBody}");
                            Debug.WriteLine($"{verificationCodeSendTime}");
                            Debug.WriteLine($"{message.Date.Subtract(verificationCodeSendTime)}");
                            Debug.WriteLine($"{message.Date} {verificationCodeSendTime} {message.HtmlBody}");
                            if (message.Date > verificationCodeSendTime)
                            {
                                var bodyparts = message.HtmlBody.Split(" ");
                                verificationCode = bodyparts[bodyparts.Length - 1];
                                foundCode = true;
                            }
                        }
                        mailClient.Disconnect(true);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Can't connect to email server {e.Message}");
                }
                if (tryCount > 5)
                    throw new Exception("Failed to find email with auth code");

            } while (!foundCode);

            return verificationCode;
        }
    }
}
