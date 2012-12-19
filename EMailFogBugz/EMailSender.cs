using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace EMailFogBugz
{
    public class EMailSender : IEMailSender
    {
        public ISmtpClient SmtpClient { get; set; }
        public ITemplateLoader templateLoader { get; set; }
        public IMailTemplateParser templateParser { get; set; }
        public FileSystemTemplateLoader fileParser { get; set; }
        

        public void SendEMailAsync(string subject, string body, params string[] destinations)
        {
            if (destinations.Length == 0)
                return;

            SmtpClient.SendAsync(CreateMessage(subject, body, destinations)); 
        }
        public void SendEMailAsync(MailMessage message)
        {
            var smtpClient = new SmtpClient();
            smtpClient.SendAsync(message,Guid.NewGuid()); 
        }

        protected MailMessage CreateMessage(string subject, string body, IEnumerable<string> destinations)
        {
            var message = new MailMessage();
            destinations.ToList().ForEach(x => message.To.Add(x));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            return message;
        }
        public string GetEmailBody(string templateName, FogBugzCase selectedCase)
        {
            string emailBody = null;
            if (templateName == null)
            {
                throw new ArgumentNullException("templateName");
            }
            var parser = new FileSystemTemplateLoader();
            emailBody = parser.LoadTemplate(templateName, null, selectedCase);
            return emailBody;
        }
    }
}