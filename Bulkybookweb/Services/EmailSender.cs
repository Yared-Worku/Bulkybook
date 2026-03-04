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
                // Increase timeout slightly for the cloud handshake
                client.Timeout = 20000;

                // Bypass SSL certificate issues that sometimes happen on shared cloud IPs
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                Console.WriteLine($"Attempting to connect to {smtpServer} on port {port}...");

                // Connect using StartTls (Port 2525 supports this too)
                await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);

                Console.WriteLine($"Authenticating with SendGrid...");
                // Remember: Username is literally "apikey", password is the SG.xxx key
                await client.AuthenticateAsync("apikey", password);

                Console.WriteLine("Sending email...");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
                Console.WriteLine("Email sent successfully!");
            }
        }
    }
}