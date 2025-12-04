using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaAplicacion.Interfaces
{

    public interface IExtendedEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendBulkEmailAsync(List<string> emails, string subject, string htmlMessage);
    }

}
