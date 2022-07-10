using HtmlAgilityPack;
using System.Net;

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

        //if (tableOfContentsNode == null)
        //{
        //    Console.WriteLine("No Table Of Contents was found.");
        //    return;
        //}

        //var tocContents = tableOfContentsNode.NextSibling;
        //foreach (var tocEntry in tocContents.SelectNodes("/a"))
        //{
        //    Console.WriteLine(tocEntry.InnerHtml + " => " + tocEntry.Attributes["name"].Value);
        //}
        
        
        
        //Console.WriteLine(tableOfContentsNode.InnerHtml);
        
    }
}