using System.Net;
using System.Net.Mail;

namespace ShopCoAPI.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // We pull these securely from appsettings.json
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var appPassword = _configuration["EmailSettings:AppPassword"];

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Allows us to send styled emails!
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}