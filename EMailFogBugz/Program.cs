using System;
using log4net;
using System.Linq;
using System.Collections.Generic;

namespace EMailFogBugz
{
    class Program
    {
        private static ILog _log;

        static void Main(string[] args)
        {
            CreateLogger();
            _log.Info("Logger Started");
            if (args.Length < 2)
            {
                _log.Error("A user name and password is required");
                return;
            }

            string fogBugzEmail = args[0];
            string fogBugzPassword = args[1];
            string sendToUsers = "simon.timms@worleyparsons.com"; //Use this variable to send to just one user for testing instead of the users identified in the case. 

            var caseRepository = new CaseRepository(fogBugzEmail, fogBugzPassword, _log );

            new CaseProcessor(_log).ProcessCases(caseRepository.GetResolvedCases(), sendToUsers);
            new CaseProcessor(_log).ProcessCases(caseRepository.GetClosedCases(), sendToUsers);

            Console.WriteLine("done.");
            Console.ReadLine();
        }

        private static void CreateLogger()
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = LogManager.GetLogger("EMailFogBugz");
        }
    }
}
