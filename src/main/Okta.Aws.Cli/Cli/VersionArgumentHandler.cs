﻿using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Okta.Aws.Cli.Cli;

public class VersionArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "--version";

    public VersionArgumentHandler(IConfiguration configuration) : base(configuration)
    {
    }

    protected override Task HandleInternal(string[] args, CancellationToken cancellationToken)
    {
        var version = GetAppVersion();

        Console.WriteLine(version);

        return Task.CompletedTask;
    }
}