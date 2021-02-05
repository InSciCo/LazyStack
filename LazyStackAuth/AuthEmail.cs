using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Configuration;

using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

namespace LazyStackAuthTests
{
    /// <summary>
    /// Opens your gmail account and retrieves authorization codes sent to that 
    /// email account by the AuthProvider.
    /// </summary>
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
                try
                {
                    tryCount++;
                    var messages = new List<string>();
                    using var mailClient = new ImapClient();
                    mailClient.Connect("imap.gmail.com", 993, true);
                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    mailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                    mailClient.Authenticate(email, emailPassword);

                    var inbox = mailClient.Inbox;

                    //var query =
                    //    SearchQuery.SubjectContains("Your verification code")
                    //    .And
                    //    (SearchQuery.DeliveredAfter(verificationCodeSendTime))
                    //    .And
                    //    (SearchQuery.ToContains(emailTo));

                    var query = SearchQuery.ToContains(emailTo);

                    inbox.Open(FolderAccess.ReadOnly);

                    var results = inbox.Search(query);

                    foreach (var uid in results)
                    {
                        var message = inbox.GetMessage(uid);
                        // message.Date is a DataTimeOffset of message received
                        var datetime = message.Date.UtcDateTime; // convert to UTC with no offset
                        var rcvdTicks = datetime.Ticks;
                        Debug.WriteLine($"{datetime} {verificationCodeSendTime} {message.HtmlBody}");

                        var sentTicks = verificationCodeSendTime.Ticks;
                        Debug.WriteLine($" sentTicks {sentTicks}");
                        Debug.WriteLine($" rcvdTicks {rcvdTicks}");
                        Debug.WriteLine($" diffTicks {rcvdTicks - sentTicks}");
                        if (rcvdTicks > sentTicks)
                        {
                            var bodyparts = message.HtmlBody.Split(" ");
                            verificationCode = bodyparts[^1];
                            foundCode = true;
                        }
                    }
                    mailClient.Disconnect(true);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Can't connect to email server {e.Message}");
                }
                if (tryCount > 5)
                    throw new Exception("Failed to find email with auth code");

                if(!foundCode)
                    Thread.Sleep(1000);

            } while (!foundCode);

            return verificationCode;
        }
    }
}
