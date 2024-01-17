using System.Diagnostics;
using System.Runtime.InteropServices;
using Installer;
using Installer.Installers;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Installing okta-aws-cli...\n");

var configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();


var appPath = InstallerHelper.GetAppPath(configuration);
var output = InstallerHelper.GetOutputPath(configuration);

Console.WriteLine("Installing...");

var factory = new InstallersFactory();
var installer = factory.GetInstaller();
await installer.Install();

Console.WriteLine("Done, press any key to exit...");
Console.ReadKey();