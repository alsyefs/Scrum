using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace Scrum
{
    public class Email
    {
        public void sendEmail(string receiver, string body)
        {
            Configuration config = new Configuration();
            string emailAccount = config.getEmail();
            var fromAddress = new MailAddress(emailAccount);
            string fromPassword = config.getPassword();
            var toAddress = new MailAddress(receiver);
            string subject = "Scrum Mailing System";
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
                try
                {
                    smtp.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e);
                }
        }
    }
}