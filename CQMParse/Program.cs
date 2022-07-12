using CQMParse;

string GetAppDir()
{
    var codeDir = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
    var url = new Uri(codeDir);
    codeDir = url.AbsolutePath;
    codeDir = Path.GetDirectoryName(codeDir);
    return codeDir;
}

string GetCMSDir()
{
    var dir = GetAppDir();
    var cmsPath = Path.Combine(dir, "CMS");
    
    while (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(cmsPath))
    {
        dir = Path.GetDirectoryName(dir);
        if(!string.IsNullOrWhiteSpace(dir))
        cmsPath = Path.Combine(dir, "CMS");
    }
    if (Directory.Exists(cmsPath)) return cmsPath;
    throw new Exception("Could not find directory containing CQM Measure Files.  Expected it to be named CMS");
}


if (args.Any())
{
    var outputDir = GetAppDir();

    foreach(var arg in args)
    {
        var url = new Uri(arg);
        var m = new Measure(url);
        m.WriteSummary(outputDir);
    }
}
else
{
    var cmsDir = GetCMSDir();
    var files = Directory.GetFiles(cmsDir).Where(x=>Path.GetExtension(x).Equals(".html", StringComparison.InvariantCultureIgnoreCase));
    foreach(var f in files)
    {
        var m = new Measure(new Uri(f));
        m.WriteSummary(cmsDir);
    }
}