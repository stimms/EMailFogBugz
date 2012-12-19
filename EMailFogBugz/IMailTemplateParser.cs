using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Dynamic;

namespace EMailFogBugz
{
    public interface IMailTemplateParser
    {
        string Parse(string templateName, CultureInfo culture, FogBugzCase parameters);
    }
}
