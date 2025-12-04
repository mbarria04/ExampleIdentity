using CapaAplicacion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CapaAplicacion.Servicios
{
    public class EmailSender : IExtendedEmailSender
    {
        // Our private configuration variables
        private string host;
        private int port;
        private bool enableSSL;
        private bool useDefaultCredentials;
        private string userName;
        private string password;
        public EmailSender(string host, int port, bool enableSSL, bool useDefaultCredentials, string userName, string password)
        {
            this.host = host;
            this.port = port;
            this.enableSSL = enableSSL;
            this.useDefaultCredentials = useDefaultCredentials;
            this.userName = userName;
            this.password = password;
        }

        // Use our configuration to send the email by using SmtpClient
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(host, port)
            {
                UseDefaultCredentials = useDefaultCredentials,
                Credentials = new NetworkCredential(userName, password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = enableSSL
            };
            return client.SendMailAsync(
                new MailMessage(userName, email, subject, htmlMessage) { IsBodyHtml = true }
            );
        }

        public Task SendBulkEmailAsync(List<string> emails, string subject, string htmlMessage)
        {
            var client = new SmtpClient(host, port)
            {
                UseDefaultCredentials = useDefaultCredentials,
                Credentials = new NetworkCredential(userName, password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = enableSSL
            };

            var message = new MailMessage();
            message.From = new MailAddress(userName);
            foreach (string to in emails)
            {
                message.To.Add(new MailAddress(to));
            }
            message.Subject = subject;
            message.Body = htmlMessage;
            message.IsBodyHtml = true;

            return client.SendMailAsync(message);
        }
    }
}