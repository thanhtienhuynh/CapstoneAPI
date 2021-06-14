using CapstoneAPI.DataSets.Email;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Serilog;
using System;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSetting mailSettings;

        private readonly ILogger _log = Log.ForContext<EmailService>();


        public EmailService(IOptions<EmailSetting> _emailSetting)
        {
            mailSettings = _emailSetting.Value;
        }

        // Gửi email, theo nội dung trong mailContent
        public async Task SendMail(EmailContent mailContent)
        {
            var email = new MimeMessage();
            email.Sender = new MailboxAddress(mailSettings.DisplayName, mailSettings.Mail);
            email.From.Add(new MailboxAddress(mailSettings.DisplayName, mailSettings.Mail));
            email.To.Add(MailboxAddress.Parse(mailContent.To));
            email.Subject = mailContent.Subject;


            var builder = new BodyBuilder();
            builder.HtmlBody = mailContent.Body;
            email.Body = builder.ToMessageBody();

            // dùng SmtpClient của MailKit
            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                smtp.Connect(mailSettings.Host, mailSettings.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(mailSettings.Mail, mailSettings.Password);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                // Gửi mail thất bại, nội dung email sẽ lưu vào thư mục mailssave
                System.IO.Directory.CreateDirectory("mailssave");
                var emailsavefile = string.Format(@"mailssave/{0}.eml", Guid.NewGuid());
                await email.WriteToAsync(emailsavefile);
                _log.Error(ex.ToString());
            }

            smtp.Disconnect(true);
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await SendMail(new EmailContent()
            {
                To = email,
                Subject = subject,
                Body = htmlMessage
            });
        }
    }
    
}
