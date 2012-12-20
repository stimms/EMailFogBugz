using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using RazorEngine;

namespace EMailFogBugz
{
    public class FileSystemTemplateLoader : ITemplateLoader
    {
        public string LoadTemplate(string templateName, CultureInfo culture)
        {
            if (culture == null)
                culture = CultureInfo.InvariantCulture;
            return File.ReadAllText(Path.Combine("Views", culture.ToString(), templateName + ".cshtml"));
        }
        public string LoadTemplate(string templateName, CultureInfo culture, FogBugzCase cases)
        {
            if (culture == null)
                culture = CultureInfo.InvariantCulture;
            string text = File.ReadAllText(Path.Combine("Views", culture.ToString(), templateName + ".cshtml"));
            return Razor.Parse(text, cases);
        }
    }
}
