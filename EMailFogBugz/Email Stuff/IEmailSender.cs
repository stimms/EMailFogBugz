using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace EMailFogBugz
{
    public interface IEMailSender
    {
        void SendEMailAsync(string subject, string body, params string[] destinations);
        void SendEMailAsync(MailMessage message);
        string GetEmailBody(string templateName, FogBugzCase selectedCase);
    }
}