using HtmlAgilityPack;
using System.Net;
using System.Text;

namespace CQMParse;

public class MeasureParse
{
    private static string GetHtmlFromUrl(string url)
    {
        using WebClient web1 = new WebClient() ;
        string data = web1.DownloadString(url);
        return data;
    }
    
    public static void Parse(string measureUrl)
    {

        var filePath = @"C:\code\CQMParser\CQMParse\Antithrombotic Therapy By End of Hospital Day 2 10.6.000.html";
        var doc = new HtmlDocument();
        doc.Load(filePath);

        // var web = new HtmlWeb();
        // var doc = web.Load(measureUrl);

        //var tableOfContentsNode = doc.DocumentNode.SelectSingleNode("//a[@name='toc']");


        var codeNodes = CodeNode.GetCodeNodes(doc.DocumentNode);

        foreach(var cn in codeNodes)
        {
            Console.WriteLine(cn);
            Console.WriteLine("==============");
        }

        var terminals = Terminal.GetTerminals(doc.DocumentNode);
        foreach(var t in terminals)
        {
            Console.WriteLine(t);
            Console.WriteLine("--==--==--==--=");
        }

        CodeNode.PopulateSubSegments(codeNodes, terminals);

        var initalPopulation = codeNodes.First(x => x.Name.Equals("Initial Population"));
        var denominator = codeNodes.First(x => x.Name.Equals("Denominator"));
        var denominatorExclusions = codeNodes.First(x => x.Name.Equals("Denominator Exclusions"));
        var numerator = codeNodes.First(x => x.Name.Equals("Numerator"));
        var numeratorExclusions = codeNodes.First(x => x.Name.Equals("Numerator Exclusions"));
        var denominatorExceptions = codeNodes.First(x => x.Name.Equals("Denominator Exceptions"));
        var stratification = codeNodes.First(x => x.Name.Equals("Stratification"));

        var roots = new[] { initalPopulation, denominator, denominatorExclusions, numerator, numeratorExclusions, denominatorExceptions, stratification };
        
        var sb = new StringBuilder();
        foreach(var r in roots)
        {
            sb.AppendLine("============================================");
            sb.AppendLine($"{r.Name}:");
            sb.AppendLine(r.GetFormatted(0, "\t"));
        }

        var path = @"C:\temp\test.txt";
        if (File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, sb.ToString());
    }
}