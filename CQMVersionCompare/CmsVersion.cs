using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQMVersionCompare
{
    public class CmsVersion
    {
        public int CmsNumber { get; }
        public int Version { get; }
        public string FilePath { get; }

        

        public CmsVersion(string filePath)
        {
            FilePath = filePath;
            var name = Path.GetFileNameWithoutExtension(filePath);
            name = name.Replace("CMS", "");
            var split = name.Split("v");
            CmsNumber = int.Parse(split[0]);
            Version = int.Parse(split[1]);
        }
    }

    public class Diff
    {
        public CmsVersion LHS { get; set; }
        public CmsVersion RHS { get; set; }
        private static string GetComparisonFileName(CmsVersion lhs, CmsVersion rhs)
        {
            return $"CMS{lhs.CmsNumber}Comparison_{lhs.Version}-{rhs.Version}.html";
        }

        public string FileName => GetComparisonFileName(LHS, RHS);

        public string MarkdownLine =>
            $"* [CMS{LHS.CmsNumber} v{LHS.Version} vs v{RHS.Version}](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS{LHS.CmsNumber}Comparison_{LHS.Version}-{RHS.Version}.html)";
    }
}
