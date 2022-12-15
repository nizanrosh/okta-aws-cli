using System.Diagnostics;
using System.Runtime.InteropServices;
using Installer;
using Kurukuru;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Installing okta-aws-cli...\n");

var configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();


var appPath = InstallerHelper.GetAppPath(configuration);
var output = InstallerHelper.GetOutputPath(configuration);

await Spinner.StartAsync("Installing...", async spinner =>
{
    spinner.SymbolSucceed = new SymbolDefinition("V", "V");

    var process = Process.Start(new ProcessStartInfo
    {
        FileName = "dotnet",
        WorkingDirectory = "../",
        Arguments =
            $"publish --output {output} --source https://api.nuget.org/v3/index.json --configuration Release --verbosity quiet"
    });

    await process!.WaitForExitAsync();

    spinner.Succeed("Installed successfully!");
});

Console.WriteLine("Adding app to machine path...\n");

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    var name = "PATH";
    var scope = EnvironmentVariableTarget.Machine;
    var oldValue = Environment.GetEnvironmentVariable(name, scope);

    if (InstallerHelper.ShouldUpdateWindowsPaths(oldValue!, appPath))
    {
        var newPaths = InstallerHelper.GetNewWindowsPaths(oldValue!, appPath);
        Environment.SetEnvironmentVariable(name, newPaths, scope);
    }
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    var linuxProfileFile = $"/home/{Environment.UserName}/.profile";
    var paths = await File.ReadAllLinesAsync(linuxProfileFile);

    if (InstallerHelper.ShouldUpdateLinuxPaths(paths, appPath))
    {
        var newPaths = InstallerHelper.GetNewLinuxPaths(paths, appPath);
        await File.WriteAllLinesAsync(linuxProfileFile, newPaths);
    }
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    var pathsFile = "/etc/paths.d/okta-aws-cli";

    if (!File.Exists(pathsFile))
    {
        await File.WriteAllTextAsync(pathsFile, appPath);
    }
}

Console.WriteLine("Done, press any key to exit...");
Console.ReadKey();