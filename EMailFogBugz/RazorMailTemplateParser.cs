﻿using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Dynamic;

namespace EMailFogBugz
{
    public class RazorMailTemplateParser : IMailTemplateParser
    {
        protected ITemplateLoader templateLoader = new FileSystemTemplateLoader();

        public string Parse(string templateName, CultureInfo culture, FogBugzCase parameters)
        {
            return RazorEngine.Razor.Parse(templateLoader.LoadTemplate(templateName, culture),  parameters);
            
        }
    }
}