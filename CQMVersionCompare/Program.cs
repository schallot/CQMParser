using System.Diagnostics;
using System.Security.Cryptography;
using CQMVersionCompare;

var inputDir = @"C:\code\CQMParser\CompiledMeasures";
var outputDir = Path.Combine(inputDir, "ComparisonReports");
if (!Directory.Exists(outputDir))
{
    Directory.CreateDirectory(outputDir);
}
var groups = Directory.GetFiles(inputDir, "*.txt")
    .Select(x=>new CmsVersion(x))
    .GroupBy(x=>x.CmsNumber).OrderBy(x=>x.Key);

var winMerge = @"C:\Program Files\WinMerge\WinMergeU.exe";

var diffs = new List<Diff>();

foreach (var g in groups)
{
    var items = g.OrderByDescending(x => x.Version).ToArray();
    if (items.Length > 1)
    {
        var rhs = items[0];
        var lhs = items[1];
        var diff = new Diff() {LHS = lhs, RHS = rhs};
        var reportName = diff.FileName;
        var reportPath = Path.Combine(outputDir, reportName);
        var si = new ProcessStartInfo(winMerge, $"\"{lhs.FilePath}\" \"{rhs.FilePath}\" -minimize -noninteractive -ignorews -ignorecodepage -ignoreeol -ignoreblanklines -u -or \"{reportPath}\"");
        if(File.Exists(reportPath)) File.Delete(reportPath);
        var p = Process.Start(si);
        p.WaitForExit();
        diffs.Add(diff);
    }
}

var readmePath = @"C:\code\CQMParser\README.md";
var readmeTxt = File.ReadAllText(readmePath);
var compSectionHeader = "## Version Comparisons";
if (readmeTxt.Contains(compSectionHeader))
{
    readmeTxt = readmeTxt.Substring(0, readmeTxt.LastIndexOf(compSectionHeader));
}
readmeTxt = $"{readmeTxt}{compSectionHeader}\r\nIt can sometimes be useful to compare versions of the compiled measures.  To aid with this, I've included WinMerge diffs of the two most recent versions of each measure:\r\n";
foreach (var diff in diffs)
{
    readmeTxt = $"{readmeTxt}\r\n{diff.MarkdownLine}";
}
File.WriteAllText(readmePath, readmeTxt);