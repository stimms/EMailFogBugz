using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EMailFogBugz
{
    class CaseRepository
    {
        private ILog _log { get; set; }
        private Task<TokenResponse> _token { get; set; }
        public CaseRepository(string fogBugzEmail, string fogBugzPassword, ILog log)
        {
            _log = log;
            _token = GetToken(fogBugzEmail, fogBugzPassword);
        }
        
        public async Task<TokenResponse> GetToken(string fogBugzEmail, string fogBugzPassword)
        {
            HttpClient client = new HttpClient();
            var result = await client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?cmd=logon&email={1}&password={0}", fogBugzPassword, fogBugzEmail));
            return await ParseToken(await result.Content.ReadAsStringAsync());
        }

        private async Task<TokenResponse> ParseToken(string tokenXml)
        {
            XElement element = XElement.Parse(tokenXml);
            var result = new TokenResponse { Token = element.Elements().Where(x => x.Name == "token").First().Value };
            _log.Info("API token is " + result.Token);
            return result;
        }

        private void SetResolvedFilter()
        {
            HttpClient client = new HttpClient();
            client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?token={0}&cmd=setCurrentFilter&sFilter=10", _token.Result.Token)).Wait();
        }

        private void SetClosedFilter()
        {
            HttpClient client = new HttpClient();
            client.GetAsync(String.Format("https://simplicity-wp.fogbugz.com/api.asp?token={0}&cmd=setCurrentFilter&sFilter=11", _token.Result.Token)).Wait();
        }

        public Task<IEnumerable<FogBugzCase>> GetClosedCases()
        {
            SetClosedFilter();
            return GetCases();
        }

        public Task<IEnumerable<FogBugzCase>> GetResolvedCases()
        {
            SetResolvedFilter();
            return GetCases();
        }

        private async Task<IEnumerable<FogBugzCase>> GetCases()
        {
            HttpClient client = new HttpClient();
            string url = String.Format("https://simplicity-wp.fogbugz.com/api.asp?cmd=search&token={0}&cols=ixBug,sTitle,sCorrespondent,sPersonAssignedTo,sLatestTextSummary,sProject,sStatus,sCustomerEmail,fReplied,dtOpened,dtResolved,dtClosed,sCategory,sPriority", (await _token).Token);
            var result = await client.GetAsync(url);
            return new CaseParser().ParseCases(await result.Content.ReadAsStringAsync());
        }
    }
}
