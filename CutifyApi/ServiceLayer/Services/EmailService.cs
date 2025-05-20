using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using ServiceLayer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Send(string to, string subject, string html, string from = null)
        {
            var section = _configuration.GetSection("SmtpSettings");
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(from ?? section["From"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = html };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect(section["Host"], int.Parse(section["Port"]), SecureSocketOptions.StartTls);
            smtp.Authenticate(section["UserName"], section["Password"]);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
