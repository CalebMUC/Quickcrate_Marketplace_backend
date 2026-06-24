using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Minimart_Api.Services
{
  
 

public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Implement your email sending logic here.
            // For now, just return a completed task.
            return Task.CompletedTask;
        }
    }

}

