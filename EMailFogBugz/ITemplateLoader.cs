using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace EMailFogBugz
{
    public interface ITemplateLoader
    {
        string LoadTemplate(string templateName, CultureInfo culture);
    }
}