using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace DaLatTour.Models
{
    public class EmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = new MailAddress("discoverydalat9@gmail.com", "DaLatDisco");
            var toEmailAddress = new MailAddress(toEmail);
            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail.Address, "tcrc brwp pqjc ibsq")
            };

            using (var message = new MailMessage(fromEmail, toEmailAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                await smtpClient.SendMailAsync(message);
            }
        }
    }
}