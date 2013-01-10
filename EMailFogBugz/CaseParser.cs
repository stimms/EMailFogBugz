using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace EMailFogBugz
{
    class CaseParser
    {
        public IEnumerable<FogBugzCase> ParseCases(string casesXML)
        {
            XElement root = XElement.Parse(casesXML);
            return from fbCase in root.Elements().Where(x => x.Name == "cases").Elements()
                                        select new FogBugzCase {
                                            ID = fbCase.Elements().Where(x => x.Name == "ixBug").First().Value,
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

        private DateTime ParseStringToDate(string theDate)
        {
            //FogBugz String format: 2012-10-03T14:58:07Z
            DateTime variant;
            if (DateTime.TryParse(theDate, out variant) == true)
            {
                return variant;
            }
            else
            {
                return DateTime.UtcNow;
            }
        }
    }
}
