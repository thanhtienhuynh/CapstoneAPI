using CapstoneAPI.DataSets.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Email
{
    public interface IEmailService
    {
        Task SendMail(EmailContent mailContent);
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
