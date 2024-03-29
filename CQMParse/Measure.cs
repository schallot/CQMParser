using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;

namespace CQMParse
{
    public class Measure
    {
        public Uri SourceLocation { get; }
        public string SourceLocationAbsolute => SourceLocation?.AbsolutePath;
        public CodeNode InitalPopulation { get; }
        public CodeNode Denominator { get; }
        public CodeNode DenominatorExclusions { get; }
        public CodeNode Numerator { get; }
        public CodeNode NumeratorExclusions { get; }
        public CodeNode DenominatorExceptions { get; }
        public CodeNode[] OtherPopulationCriteria { get; }

        (int measureNumber, string versionString, int versionNumber) MeasureId { get; }
        

        private (int measureNumber, string versionString, int versionNumber) GetMeasureId(HtmlDocument doc)
        {
            var allNodes = doc.DocumentNode.Descendants();

            var ecqmIdSearchStrings = new[] { "eMeasure Identifier", "eCQM Identifier" };
            var idNode = allNodes.Where(x => (x.Name == "td" || x.Name == "th") && ecqmIdSearchStrings.Any(y => x.InnerText.Contains(y))).First();
            var id = idNode.GetNextElementWithName("td").InnerText;

            var versionSearchStrings = new[] { "eMeasure Version number", "eCQM Version" };
            var version = allNodes.Where(x => (x.Name == "td" || x.Name == "th") && versionSearchStrings.Any(y => x.InnerText.Contains(y))).First().GetNextElementWithName("td").InnerText;


            var measureNumber = int.Parse(id);
            var versionNumber = int.Parse(version.Trim().Split('.').First());
            return new(measureNumber, version, versionNumber);
        }


        private static string GetHtmlFromUrl(string url)
        {
            using WebClient web1 = new WebClient();
            string data = web1.DownloadString(url);
            return FindAndReplaceInMeasureHtml(data);
        }

        private static HtmlDocument GetHtmlFromUri(Uri uri)
        {
            string measureHtml;
            if (uri.IsFile)
            {
                measureHtml = File.ReadAllText(uri.LocalPath);
                measureHtml = FindAndReplaceInMeasureHtml(measureHtml);
            }
            else
            {
                measureHtml = GetHtmlFromUrl(uri.OriginalString);
            }
            var doc = new HtmlDocument();
            doc.LoadHtml(measureHtml);
            return doc;
        }

        /// <summary>
        /// For weird cases, we might have to do a little find/replace to get things to work,
        /// because every dang measure seems to follow its own invented conventions.
        /// </summary>
        /// <returns></returns>
        private static string FindAndReplaceInMeasureHtml(string html)
        {
            html = html.Replace(" (Excluding Blood Transfusion)", " [Excluding Blood Transfusions]"); // usually we can ignore things in brackets.  Not so for this one case in 1028.  Plus there's plurality mismatch to deal with...
            html = html.Replace(" (Excluding Blood Transfusions)", " [Excluding Blood Transfusions]"); // usually we can ignore things in brackets.  Not so for this one case in 1028.  Plus there's plurality mismatch to deal with...
            return html;
        }

        public Measure(Uri uri) : this(GetHtmlFromUri(uri), uri)
        {           
        }

        public Measure(HtmlDocument doc, Uri uri)
        {
            SourceLocation = uri;
            MeasureId = GetMeasureId(doc);            
            var codeNodes = CodeNode.GetCodeNodes(doc.DocumentNode);

            //foreach (var cn in codeNodes)
            //{
            //    Console.WriteLine(cn);
            //    Console.WriteLine("==============");
            //}

            var terminals = Terminal.GetTerminals(doc.DocumentNode);
            //foreach (var t in terminals)
            //{
            //    Console.WriteLine(t);
            //    Console.WriteLine("--==--==--==--=");
            //}

            CodeNode.PopulateSubSegments(codeNodes, terminals);

            InitalPopulation = codeNodes.First(x => x.Name.Equals("Initial Population"));
            Denominator = codeNodes.First(x => x.Name.Equals("Denominator"));
            DenominatorExclusions = codeNodes.First(x => x.Name.Equals("Denominator Exclusions"));
            Numerator = codeNodes.First(x => x.Name.Equals("Numerator"));
            NumeratorExclusions = codeNodes.First(x => x.Name.Equals("Numerator Exclusions"));
            DenominatorExceptions = codeNodes.FirstOrDefault(x => x.Name.Equals("Denominator Exceptions"));

            var allNodes = doc.DocumentNode.Descendants();
            var populationCriteriaNodes = InitalPopulation.CodeHtmlNode.GetMostRecentCommonAncestor(Denominator.CodeHtmlNode).Descendants().ToArray();
            OtherPopulationCriteria = codeNodes.Where(x => populationCriteriaNodes.Contains(x.CodeHtmlNode) && !MainPopulationCriteria.Contains(x)).ToArray();            
        }

        CodeNode[] MainPopulationCriteria => new[] { InitalPopulation, Denominator, DenominatorExclusions, Numerator, NumeratorExclusions, DenominatorExceptions }.Where(x => x != null).ToArray();

        public string GetSummary()
        {
            var sb = new StringBuilder();
            foreach (var r in MainPopulationCriteria.Union(OtherPopulationCriteria))
            {
                sb.AppendLine("=============================================================================================================================================================================");
                sb.AppendLine($"{r.Name}:");
                sb.AppendLine(r.GetFormatted(0, "\t"));
            }

            return sb.ToString();
        }
        
        public void WriteSummary(string outputDirectory)
        {
            Console.WriteLine($"Processing measure {SourceLocation.LocalPath}");
            var s = GetSummary();
            var path = Path.Combine(outputDirectory, $"CMS{MeasureId.measureNumber}v{MeasureId.versionNumber}.txt");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            Console.WriteLine($"Wrote measure summary to {path}");
            File.WriteAllText(path, s);
        }
    }
}
