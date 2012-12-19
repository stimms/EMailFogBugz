using System;
using System.Linq;
using System.Collections.Generic;

namespace EMailFogBugz
{
    public class IssueResolved 
    {
        public Guid UserID { get; set; }
        public string UserEMail { get; set; }
        public string Title { get; set; }
        public string Correspondent { get; set; }
        public string LatestTextSummary { get; set; }
        public string Project { get; set; }
        public string Status { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime Opened { get; set; }
        public DateTime Resolved { get; set; }
        public DateTime Closed { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
    }
}
