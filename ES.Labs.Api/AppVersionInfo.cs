using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ES.Labs.Api;

public class AppVersionInfo
{
    public string Version { get; }

    public string Timestamp { get; }

    public AppVersionInfo()
    {
        Version = GetVersion();
        Timestamp = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
    }

    private static string GetVersion()
    {
        // git rev-parse HEAD
        var appAssembly = typeof(AppVersionInfo).Assembly;
        var infoVerAttr = appAssembly
            .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute))
            .FirstOrDefault();

        var informationalVersion = (infoVerAttr as AssemblyInformationalVersionAttribute)?.InformationalVersion ?? "1.0.0";

        return WithLocalRevision(informationalVersion);

        // "1.0.0+LOCALBUILD";
    }

    private static string WithLocalRevision(string informationalVersion)
    {
        if (informationalVersion.Contains("+"))
            return informationalVersion;
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo("git", "rev-parse HEAD")
            {
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = "git",
                Arguments = "rev-parse HEAD"
            }
        };
        
        var outputBuilder = new StringBuilder();
        var errorsBuilder = new StringBuilder();
        process.OutputDataReceived += (_, args) => outputBuilder.AppendLine(args.Data);
        process.ErrorDataReceived += (_, args) => errorsBuilder.AppendLine(args.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        
        // git.
        var result = outputBuilder.ToString().Trim();
        return informationalVersion + "+" + result;
    }
}