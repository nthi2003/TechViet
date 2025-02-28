using System.Net;
using System.Net.Mail;

namespace TechecomViet.Reponsitory
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("quocthi4321@gmail.com", "gnvqbpddtnaxbsoi")
            };
            return client.SendMailAsync(
                new MailMessage(from: "quocthi4321@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
