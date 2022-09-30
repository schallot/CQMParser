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

        public static string GetComparisonFileName(CmsVersion lhs, CmsVersion rhs)
        {
            return $"CMS{lhs.CmsNumber}Comparison_{lhs.Version}-{rhs.Version}.html";
        }

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
}
