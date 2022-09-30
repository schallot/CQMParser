using System.Diagnostics;
using CQMVersionCompare;

var inputDir = @"C:\code\CQMParser\CompiledMeasures";
var outputDir = Path.Combine(inputDir, "ComparisonReports");
if (!Directory.Exists(outputDir))
{
    Directory.CreateDirectory(outputDir);
}
var groups = Directory.GetFiles(inputDir, "*.txt")
    .Select(x=>new CmsVersion(x))
    .GroupBy(x=>x.CmsNumber);

var winMerge = @"C:\Program Files\WinMerge\WinMergeU.exe";

foreach (var g in groups)
{
    var items = g.OrderByDescending(x => x.Version).ToArray();
    if (items.Length > 1)
    {
        var rhs = items[0];
        var lhs = items[1];
        var reportName = CmsVersion.GetComparisonFileName(lhs, rhs);
        var reportPath = Path.Combine(outputDir, reportName);
        var si = new ProcessStartInfo(winMerge, $"\"{lhs.FilePath}\" \"{rhs.FilePath}\" -minimize -noninteractive -ignorews -ignorecodepage -ignoreeol -ignoreblanklines -u -or \"{reportPath}\"");
        if(File.Exists(reportPath)) File.Delete(reportPath);
        var p = Process.Start(si);
        p.WaitForExit();
    }
}

