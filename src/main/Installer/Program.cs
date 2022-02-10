using System.Diagnostics;
using System.Runtime.InteropServices;

Console.WriteLine("Installing okta-aws-cli...");

var installerPath = Directory.GetCurrentDirectory();
var rootFolder = Directory.GetParent(installerPath)?.FullName;
ArgumentNullException.ThrowIfNull(rootFolder, nameof(rootFolder));

var appPath = Path.Combine(rootFolder, "app");

var process = Process.Start(new ProcessStartInfo
{
    FileName = "dotnet",
    WorkingDirectory = "../",
    Arguments = "publish --output app --source https://api.nuget.org/v3/index.json"
});

await process!.WaitForExitAsync();

Console.WriteLine("Adding app to machine path...");

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    var name = "PATH";
    var scope = EnvironmentVariableTarget.Machine;
    var oldValue = Environment.GetEnvironmentVariable(name, scope);

    if (!oldValue!.Contains(appPath, StringComparison.InvariantCultureIgnoreCase))
    {
        var newValue = oldValue + @$"{appPath};";
        Environment.SetEnvironmentVariable(name, newValue, scope);
    }
}
else
{
    var linuxProfileFile = $"/home/{Environment.UserName}/.profile";
    var paths = await File.ReadAllLinesAsync(linuxProfileFile);

    var linuxAppPath = $"export PATH=\"$PATH:{appPath}\"";

    if (!paths.Any(p => p.Equals(linuxAppPath)))
    {
        var newPaths = paths.Concat(new[] { linuxAppPath });
        await File.WriteAllLinesAsync(linuxProfileFile, newPaths);
    }
}



Console.WriteLine("Done.");