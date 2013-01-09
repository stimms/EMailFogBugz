﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Xml.Linq;
using log4net;

namespace EMailFogBugz
{
    class Program
    {
        private static ILog _log;

        static void Main(string[] args)
        {
            CreateLogger();
            _log.Info("Logger Started");
            if(args.Length > 2)
                _log.Error("A user name and password is required");

            Program p = new Program();
            string fogBugzEmail = args[0] //set your FogBugz email here.
            string fogBugzPassword = args[0] //set your FogBugz email here.
            var token = p.GetToken(fogBugzEmail, fogBugzPassword);
            string sendToUsers = null; //Use this variable to send to just one user for testing instead of the users identified in the case. 
             

            //get resolved cases
            p.SetResolvedFilter(token);
            var resolvedIssues = p.GetCases(token);
            p.ProcessCases(resolvedIssues, sendToUsers);

            //get closed cases 
            p.SetClosedFilter(token);
            var closedCases = p.GetCases(token);
            p.ProcessCases(closedCases, sendToUsers);
            
            Console.WriteLine("done");
            Console.Read();
        }
        private static void FindCases(Task<HttpResponseMessage> response)
        {
            Console.Write("done.");
        }
        private async Task<TokenResponse> GetToken(string FogBugzEmail, string password)
        {
            HttpClient client = new HttpClient();
            //might benefit from parsing args to allow the user to set their username and password.
            var result = await client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?cmd=logon&email={1}&password={0}", password, FogBugzEmail ));
            return await ParseToken(await result.Content.ReadAsStringAsync());
        }
        private async Task<TokenResponse> ParseToken(string tokenXml)
        {
            XElement element = XElement.Parse(tokenXml);
            var result = new TokenResponse { Token = element.Elements().Where(x => x.Name == "token").First().Value };
            Console.WriteLine(result.Token);
            return result;
        }
        private void SetResolvedFilter(Task<TokenResponse> tokenResponse)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?token={0}&cmd=setCurrentFilter&sFilter=10", tokenResponse.Result.Token)).Wait();
        }
        private void SetClosedFilter(Task<TokenResponse> tokenResponse)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?token={0}&cmd=setCurrentFilter&sFilter=12", tokenResponse.Result.Token)).Wait();
        }
        private async Task<IEnumerable<FogBugzCase>> GetCases(Task<TokenResponse> token)
        {
            HttpClient client = new HttpClient();
            string url = String.Format("https://simplicity-wp.fogbugz.com/api.asp?cmd=search&token={0}&cols=ixBug,sTitle,sCorrespondent,sPersonAssignedTo,sLatestTextSummary,sProject,sStatus,sCustomerEmail,fReplied,dtOpened,dtResolved,dtClosed,sCategory,sPriority", (await token).Token);
            var result = await client.GetAsync(url);
            return ParseCases(await result.Content.ReadAsStringAsync());

        }
        private IEnumerable<FogBugzCase> ParseCases(string casesXML)
        {
            XElement root = XElement.Parse(casesXML);
            return from fbCase in root.Elements().Where(x => x.Name == "cases").Elements()
                   select new FogBugzCase
                   {
                       ID = fbCase.Elements().Where(x => x.Name == "ixBug").First().Value.ToString(),
                       Title = fbCase.Elements().Where(x => x.Name == "sTitle").First().Value,
                       LatestTextSummary = fbCase.Elements().Where(x => x.Name == "sLatestTextSummary").First().Value,
                       PersonAssignedTo = fbCase.Elements().Where(x => x.Name == "sPersonAssignedTo").First().Value,
                       Correspondent = fbCase.Elements().Where(x => x.Name == "sCorrespondent").FirstOrDefault() == null ? "" : fbCase.Elements().Where(x => x.Name == "sCorrespondent").FirstOrDefault().Value,
                       Project = fbCase.Elements().Where(x => x.Name == "sProject").First().Value,
                       Status = fbCase.Elements().Where(x => x.Name == "sStatus").First().Value,
                       CustomerEmail = fbCase.Elements().Where(x => x.Name == "sCustomerEmail").First().Value,
                       Opened = ParseStringToDate(fbCase.Elements().Where(x => x.Name == "dtOpened").First().Value),
                       Resolved = ParseStringToDate(fbCase.Elements().Where(x => x.Name == "dtResolved").FirstOrDefault().Value),
                       Closed = ParseStringToDate(fbCase.Elements().Where(x => x.Name == "dtClosed").First().Value),
                       Category = fbCase.Elements().Where(x => x.Name == "sCategory").First().Value,
                       Priority = fbCase.Elements().Where(x => x.Name == "sPriority").First().Value
                   };
        }
        public DateTime ParseStringToDate(string theDate)
        {
            //FogBugz String format: 2012-10-03T14:58:07Z
            DateTime variant;
            if (theDate != null && DateTime.TryParse(theDate, out variant) == true)
            {
                return DateTime.Parse(theDate, CultureInfo.InvariantCulture);
            }
            else
            {
                return DateTime.UtcNow;
            }
        }
        public async void ProcessCases(Task<IEnumerable<EMailFogBugz.FogBugzCase>> theseCases, string SendToUsers =  null)
        {
            foreach (var selectedCase in await theseCases)
            {
                MailMessage email = new MailMessage();
                if (SendToUsers != null)
                    email.To.Add(SendToUsers);
                else
                {
                    if (selectedCase.CustomerEmail != null && selectedCase.CustomerEmail != "")
                        email.To.Add(selectedCase.CustomerEmail);
                    if (selectedCase.Correspondent != null && selectedCase.Correspondent != "")
                        email.To.Add(selectedCase.Correspondent);
                }
                email.Subject = string.Format(@"FogBugz case {0} has been {1}.", selectedCase.ID, selectedCase.Status);
                email.IsBodyHtml = true;

                string templateName = "";
                if (selectedCase.Status.StartsWith("Closed"))
                    templateName = "ClosedCases";
                else 
                    templateName="ResolvedIssuesAwaitingClose";

                IEMailSender emailSender = new EMailSender();
                email.Body = emailSender.GetEmailBody(templateName, selectedCase);
                if (email.Body.Substring(0, 9) == "Exception")
                    _log.Error(email.Body);
                else
                    if (email.To.Count > 0)
                        emailSender.SendEMailAsync(email);
            }
        }
        private static void CreateLogger()
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = LogManager.GetLogger("EMailFogBugz");
        }
    }
}
