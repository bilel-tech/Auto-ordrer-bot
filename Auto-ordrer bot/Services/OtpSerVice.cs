using AE.Net.Mail;
using Auto_ordrer_bot.Models;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Auto_ordrer_bot.Services
{
    public static class OtpSerVice
    {
        async public static Task<string> GetOtp(Config conf, string otpRef)
        {
            var otp = "N/A";
            try
            {
                //var uri=new Uri("imap.gmail.com");
                var client = new ImapClient("imap.gmail.com", conf.EmailUserName, conf.EmailPassword, AuthMethods.Login, 993, true);
                //client.IdleTimeout = 30;
                var xx = client.SelectMailbox("INBOX");
                var messages = new List<AE.Net.Mail.MailMessage>();
                try
                {
                    messages = client.GetMessages(client.GetMessageCount() - 30, client.GetMessageCount(), false).OrderByDescending(m => m.Date).ToList();
                }
                catch (Exception)
                {
                    var xxx = client.GetMessageCount();
                    messages = client.GetMessages(0, 20, false).OrderByDescending(m => m.Date).ToList();
                }

                foreach (var message in messages)
                {
                    var body = message.Body;
                    var subject = message.Subject;
                    if (subject.Contains("IndusInd Bank"))
                    {

                        var refOtp = body.Substring(body.IndexOf("*ECOM"), 19).Replace("*", "");
                        if (refOtp == otpRef)
                        {
                            otp = body.Substring(body.IndexOf("Customer,") + 10, 8).Replace("*", "").Replace("\n", "");
                            break;
                        }
                    }
                }
                return otp;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while extracting OTP", ex);
            }
            return otp;
        }
    }
}
