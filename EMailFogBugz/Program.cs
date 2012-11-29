using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EMailFogBugz
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            var token = p.GetToken(args[0]);
            p.SetCurrentFilter(token);
            var resolvedIssues = p.GetResolvedIssues(token);
            p.ProcessResolvedIssues(resolvedIssues);

           
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
            var result = await client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?cmd=logon&email=simon.timms@worleyparsons.com&password={0}", password));
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
            client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?token={0}&cmd=setCurrentFilter&sFilter=inbox", tokenResponse.Result.Token)).Wait();
        }

        private async Task<IEnumerable<ResolvedIssueResponse>> GetResolvedIssues(Task<TokenResponse> token)
        {
            HttpClient client = new HttpClient();
            string url = String.Format("https://simplicity-wp.fogbugz.com/api.asp?cmd=search&token={0}&cols=sTitle,sCorrespondent,sLatestTextSummary", (await token).Token);
            var result = await client.GetAsync(url);
            return ParseResolvedIssues(await result.Content.ReadAsStringAsync());

        }
        private IEnumerable<ResolvedIssueResponse> ParseResolvedIssues(string resolvedIssuesXML)
        {
            XElement root = XElement.Parse(resolvedIssuesXML);
            return from fbCase in root.Elements().Where(x => x.Name == "cases").Elements() select new ResolvedIssueResponse { Title = fbCase.Elements().Where(x => x.Name == "sTitle").First().Value,
                                                                                                                              LatestText = fbCase.Elements().Where(x => x.Name == "sLatestTextSummary").First().Value,
                                                                                                                              Correspondant = fbCase.Elements().Where(x => x.Name == "sCorrespondent").FirstOrDefault() == null ? "" : fbCase.Elements().Where(x => x.Name == "sCorrespondent").FirstOrDefault().Value
            };
        }
        public async void ProcessResolvedIssues(Task<IEnumerable<EMailFogBugz.ResolvedIssueResponse>> resolvedIssues)
        {
            foreach (var resolvedIssue in await resolvedIssues)
            {
                string result = RazorEngine.Razor.Parse("@Model.Title", resolvedIssue);


            }
        }
    }
}
