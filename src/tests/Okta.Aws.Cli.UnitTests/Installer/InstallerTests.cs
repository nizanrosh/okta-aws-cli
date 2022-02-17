using System.Collections.Generic;
using System.Linq;
using Installer;
using Xunit;

namespace Okta.Aws.Cli.UnitTests.Installer;

public class InstallerTests
{
    [Fact]
    public void InstallerHelper_Check_GetNewWindowsPaths()
    {
        var oldPath = "\\some\\path";
        var appPath = "\\some\\new\\path";

        var expectedPath = $"{oldPath};{appPath};";
        var actualPath = InstallerHelper.GetNewWindowsPaths(oldPath, appPath);

        Assert.Equal(expectedPath, actualPath);

        oldPath = $"{oldPath};";
        expectedPath = $"{oldPath}{appPath};";
        actualPath = InstallerHelper.GetNewWindowsPaths(oldPath, appPath);

        Assert.Equal(expectedPath, actualPath);
    }

    [Fact]
    public void InstallerHelper_Check_ShouldUpdateWindowsPaths()
    {
        var oldPath = "\\some\\path;\\some\\new\\path";
        var appPath = "\\some\\new\\path";

        var result = InstallerHelper.ShouldUpdateWindowsPaths(oldPath, appPath);

        Assert.False(result);

        oldPath = "\\some\\path";

        result = InstallerHelper.ShouldUpdateWindowsPaths(oldPath, appPath);

        Assert.True(result);
    }

    [Fact]
    public void InstallerHelper_Check_GetNewLinuxPaths()
    {
        var oldPaths = new[]
        {
            "export PATH=\"$PATH:/home/user/path\"",
            "export PATH=\"$PATH:/home/user/path2\""
        };

        var appPath = "/home/user/path3";
        var linuxAppPath = $"export PATH=\"$PATH:{appPath}\"";

        var expectedResult = oldPaths.Concat(new[] { linuxAppPath });

        var actualResult = InstallerHelper.GetNewLinuxPaths(oldPaths, appPath);

        Assert.Equal(expectedResult, actualResult);
    }

    [Fact]
    public void InstallerHelper_Check_ShouldUpdateLinuxPaths()
    {
        var oldPaths = new List<string>
        {
            "export PATH=\"$PATH:/home/user/path\"",
            "export PATH=\"$PATH:/home/user/path2\""
        };

        var appPath = "/home/user/path3";

        var result = InstallerHelper.ShouldUpdateLinuxPaths(oldPaths.ToArray(), appPath);
        
        Assert.True(result);

        var linuxAppPath = $"export PATH=\"$PATH:{appPath}\"";
        oldPaths.Add(linuxAppPath);

        result = InstallerHelper.ShouldUpdateLinuxPaths(oldPaths.ToArray(), appPath);
        Assert.False(result);
    }
}