using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;

namespace Bulkybookweb.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public EmailSender(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Fetch settings with fallback to prevent null crashes
            var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var senderEmail = _config["EmailSettings:Sender"] ?? "";
            var password = _config["EmailSettings:Password"] ?? "";
            // Use 587 as default for Render
            var port = int.Parse(_config["EmailSettings:Port"] ?? "587");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Book Shelf Support", senderEmail));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                // ADDED: Timeout to prevent the "moment of loading" from lasting forever
                client.Timeout = 10000; // 10 seconds

                // Explicitly use StartTls for port 587
                Console.WriteLine($"Attempting to connect to {smtpServer} on port {port}...");
                await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);

                // Use the App Password generated from Google
                Console.WriteLine($"Authenticating user: {senderEmail}...");
                await client.AuthenticateAsync(senderEmail, password);

                Console.WriteLine("Sending email...");
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);


            }
        }
    }
}