using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EMailFogBugz
{
    class CaseProcessor
    {
        private ILog _log { get; set; }
  
        public CaseProcessor(ILog log)
        {
            _log = log;
        }

        public async void ProcessCases(Task<IEnumerable<FogBugzCase>> theseCases, string sendToUsers = null)
        {
            foreach (var selectedCase in await theseCases)
            {
                MailMessage email = new MailMessage();
                if (sendToUsers != null)
                    email.To.Add(sendToUsers);
                else
                {
                    if (selectedCase.CustomerEmail != null && selectedCase.CustomerEmail != "")
                        email.To.Add(selectedCase.CustomerEmail);
                    if (selectedCase.Correspondent != null && selectedCase.Correspondent != "")
                        email.To.Add(selectedCase.Correspondent);
                }
                email.Subject = string.Format(@"Simplicity case {0} has been {1}.", selectedCase.ID, selectedCase.Status);
                email.IsBodyHtml = true;

                string templateName = "";
                if (selectedCase.Status.StartsWith("Closed"))
                    templateName = "ClosedCases";
                else
                    templateName = "ResolvedIssuesAwaitingClose";

                IEMailSender emailSender = new EMailSender();
                email.Body = emailSender.GetEmailBody(templateName, selectedCase);
                _log.Info("Sending e-mail to " + string.Join(",", email.To.Select(x => x.Address)));
                if (email.Body.Substring(0, 9) == "Exception")
                    _log.Error(email.Body);
                else
                    if (email.To.Count > 0)
                        emailSender.SendEMailAsync(email);
            }
        }
    }
}
