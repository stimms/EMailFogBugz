using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autofac;
using Autofac.Configuration;
using log4net;
using RazorEngine;
using RazorEngine.Templating;

namespace EMailFogBugz
{
    class Program
    {
        protected IEMailSender mailSender = new EMailSender();
        protected IMailTemplateParser templateParser = new RazorMailTemplateParser();
        private static IContainer _container;
        private static ILog _log;

        public void Handle(FogBugzCase message)
        {
            dynamic model = new ExpandoObject();
            //should use new System.Globalization.CultureInfo(AppSettings.Culture)
            mailSender.SendEMailAsync(templateParser.Parse("ResolvedIssueResponse.Title", CultureInfo.InvariantCulture, model),
                                      templateParser.Parse("ResolvedIssueResponse", CultureInfo.InvariantCulture, model),
                                      message.UserEMail);
        }
                       
        static void Main(string[] args)
        {
            CreateLogger();
            _log.Info("Logger Started");
            
            Program p = new Program();
            var token = p.GetToken(args[0]);

            p.SetCurrentFilter(token);
            //var resolvedIssues = p.GetResolvedIssues(token);
            //p.ProcessResolvedIssues(resolvedIssues);
            var cases = p.GetCases(token);
            p.ProcessCases(cases);

           
            Console.WriteLine("done");
            Console.Read();
        }
        private static void FindCases(Task<HttpResponseMessage> response)
        {
            Console.Write("done.");
        }
        private async Task<TokenResponse> GetToken(string password)
        {
            HttpClient client = new HttpClient();
            var result = await client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?cmd=logon&email=katrina.senger@worleyparsons.com&password={0}", password));
            //var result = await client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?cmd=logon&email=simon.timms@worleyparsons.com&password={0}", password));
            return await ParseToken(await result.Content.ReadAsStringAsync());
        }

        private async Task<TokenResponse> ParseToken(string tokenXml)
        {
            XElement element = XElement.Parse(tokenXml);
            var result = new TokenResponse { Token = element.Elements().Where(x => x.Name == "token").First().Value };
            Console.WriteLine(result.Token);
            return result;
        }

        private void SetCurrentFilter(Task<TokenResponse> tokenResponse)
        {
            HttpClient client = new HttpClient();
            //client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?token={0}&cmd=setCurrentFilter&sFilter=inbox", tokenResponse.Result.Token)).Wait();
            client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?token={0}&pgx=LF&ixFilter=9", tokenResponse.Result.Token)).Wait();
        }

        private async Task<IEnumerable<FogBugzCase>> GetResolvedIssues(Task<TokenResponse> token)
        {
            HttpClient client = new HttpClient();
            string url = String.Format("https://simplicity-wp.fogbugz.com/api.asp?cmd=search&token={0}&cols=sTitle,sCorrespondent,sLatestTextSummary,sProject,sStatus,sCustomerEmail,fReplied,dtOpened,dtResolved,dtClosed,sCategory,sPriority", (await token).Token);
            var result = await client.GetAsync(url);
            return ParseResolvedIssues(await result.Content.ReadAsStringAsync());

        }
        private async Task<IEnumerable<FogBugzCase>> GetCases(Task<TokenResponse> token)
        {
            HttpClient client = new HttpClient();
            string url = String.Format("https://simplicity-wp.fogbugz.com/api.asp?cmd=search&token={0}&cols=sTitle,sCorrespondent,sLatestTextSummary,sProject,sStatus,sCustomerEmail,fReplied,dtOpened,dtResolved,dtClosed,sCategory,sPriority", (await token).Token);
            var result = await client.GetAsync(url);
            return ParseCases(await result.Content.ReadAsStringAsync());

        }
        private IEnumerable<FogBugzCase> ParseResolvedIssues(string resolvedIssuesXML)
        {
            XElement root = XElement.Parse(resolvedIssuesXML);
            return from fbCase in root.Elements().Where(x => x.Name == "cases").Elements()
                   select new FogBugzCase
                   {
                       Title = fbCase.Elements().Where(x => x.Name == "sTitle").First().Value,
                       LatestTextSummary = fbCase.Elements().Where(x => x.Name == "sLatestTextSummary").First().Value,
                       Correspondent = fbCase.Elements().Where(x => x.Name == "sCorrespondent").FirstOrDefault() == null ? "" : fbCase.Elements().Where(x => x.Name == "sCorrespondent").FirstOrDefault().Value
                   };
        }
        private IEnumerable<FogBugzCase> ParseCases(string issuesXML)
        {
            XElement root = XElement.Parse(issuesXML);
            //&cols=sTitle,sCorrespondent,sLatestTextSummary,sProject,sStatus,sCustomerEmail,fReplied,dtOpened,dtResolved,dtClosed,sCategory,sPriority
            return from fbCase in root.Elements().Where(x => x.Name == "cases").Elements()
                   select new FogBugzCase
                   {
                       Title = fbCase.Elements().Where(x => x.Name == "sTitle").First().Value,
                       LatestTextSummary = fbCase.Elements().Where(x => x.Name == "sLatestTextSummary").First().Value,
                       Correspondent = fbCase.Elements().Where(x => x.Name == "sCorrespondent").FirstOrDefault() == null ? "" : fbCase.Elements().Where(x => x.Name == "sCorrespondent").FirstOrDefault().Value,
                       Project = fbCase.Elements().Where(x => x.Name == "sProject").First().Value,
                       Status = fbCase.Elements().Where(x => x.Name == "sStatus").First().Value,
                       CustomerEmail = fbCase.Elements().Where(x => x.Name == "sCustomerEmail").First().Value,
                       Opened = ParseStringToDate (fbCase.Elements().Where(x => x.Name == "dtOpened").First().Value),
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
            if (theDate != null && DateTime.TryParse(theDate, out variant ) == true)
            {
                return DateTime.Parse(theDate,CultureInfo.InvariantCulture);
            }
            else
            {
                return DateTime.UtcNow;
            }
        }

        public async void ProcessCases(Task<IEnumerable<EMailFogBugz.FogBugzCase>> theseCases)
        {
            foreach (var selectedCase in await theseCases)
            {
                MailMessage email = new MailMessage();
                email.To.Add("katrina.senger@worleyparsons.com");
                if (selectedCase.CustomerEmail != null && selectedCase.CustomerEmail != "")
                    email.To.Add(selectedCase.CustomerEmail);
                if (selectedCase.Correspondent != null && selectedCase.Correspondent != "")
                    email.To.Add(selectedCase.Correspondent);
                email.Subject  = @"FogBugz Issues";
                email.IsBodyHtml = true;

                IEMailSender emailSender = new EMailSender();
                email.Body = emailSender.GetEmailBody("ResolvedIssuesAwaitingClose", selectedCase);
                if (email.Body.Substring(0, 9) == "Exception")
                    _log.Error(email.Body);
                else
                    emailSender.SendEMailAsync(email);

                
            }
        }
        private static void CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new ConfigurationSettingsReader("autofac"));
            containerBuilder.RegisterInstance(_log).AsImplementedInterfaces();
            _container = containerBuilder.Build();
        }
        private static void CreateLogger()
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = LogManager.GetLogger("EMailFogBugz");
        }

        public async void ProcessResolvedIssues(Task<IEnumerable<FogBugzCase>> resolvedIssues)
        {
            foreach (var resolvedIssue in await resolvedIssues)
            {
                string result = RazorEngine.Razor.Parse("@Model.Title", resolvedIssue);

                MailMessage email = new MailMessage();
                string[] destination = { "katrina.senger@worleyparsons.com" };
                string Subject =  "FogBugz Issues";
                string Body = result;
                
                IEMailSender emailSender = new EMailSender();
                emailSender.SendEMailAsync(Subject,Body,destination);
            }
        }

                
    }
}
