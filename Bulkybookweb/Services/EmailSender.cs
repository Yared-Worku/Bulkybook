using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp; // Keep this
using MimeKit;
// Remove using System.Net.Mail; if it is there

namespace Bulkybookweb.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public EmailSender(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSettings = _config.GetSection("EmailSettings");
            var emailMessage = new MimeMessage();

            // Fix for the "Possible null reference" warning:
            string senderEmail = emailSettings["Sender"] ?? "noreply@bulkybook.com";

            emailMessage.From.Add(new MailboxAddress("Book Shelf Support", senderEmail));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            // EXPLICITLY use MailKit.Net.Smtp.SmtpClient here:
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(emailSettings["SmtpServer"], 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, emailSettings["Password"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}