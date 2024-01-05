// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


var sourceDir = @"C:\dev\azuredevops\volvogroup\Penta-ECOM2.0-ms\Penta.ECom.Backend";
var output = @"C:\dev\github\swizkon\es-labs\benchmarks\Fresh.Library.Model";

var folders = Directory.GetDirectories(sourceDir, "Generated", SearchOption.AllDirectories);

var files = folders
    .SelectMany(f => Directory.GetFiles(f, "*.cs", SearchOption.AllDirectories));

    //.OrderBy(x => Random.Shared.Next())
    //.Take(10);

var acts = files.Select(f =>
{
    var a = () =>
    {
        var parts = f.Split('\\');
        //foreach (var part in parts)
        //{
        //    Console.WriteLine(part);
        //}

        //Console.WriteLine(" ");

        // var s = File.ReadAllText(f);

        // var newPath = Path.Combine(output, "Data", parts[^2], parts[^4], Path.GetFileName(f).Replace(".g.cs", ".cs"));
        var newPath = Path.Combine(output, "Data", parts[^2], Path.GetFileName(f).Replace(".g.cs", ".cs"));
        Console.WriteLine(newPath.Replace(output, ""));
        var targetDirectory = Path.GetDirectoryName(newPath);
        // Console.WriteLine(targetD irectory);

        // Console.WriteLine("--------------------");

        Directory.CreateDirectory(targetDirectory);

        var proxyToExclude = f.Contains("ClientProxies") && !f.Contains("ClientProxy");
        if(!proxyToExclude)
        {
            File.Copy(f, newPath, true);
        }
        else
        {
            Console.WriteLine($"Excluding {newPath}");
        }
    };

    return a;
});

Parallel.Invoke(acts.ToArray());

return Environment.ExitCode;
    
//foreach (var file in files)
//{
//    var parts = file.Split('\\');
//    foreach (var part in parts)
//    {
//        Console.WriteLine(part);
//    }
//    Console.WriteLine(" ");

//    var s = await File.ReadAllTextAsync(file);

//    var newPath = Path.Combine(output, parts[^2], parts[^4], Path.GetFileName(file).Replace(".g.cs", ".cs"));
//    Console.WriteLine(newPath);
//    var fodler= Path.GetDirectoryName(newPath);
//    Console.WriteLine(fodler);

//    Console.WriteLine("--------------------");

//    Directory.CreateDirectory(fodler);

//    File.Copy(file, newPath, true);
//}