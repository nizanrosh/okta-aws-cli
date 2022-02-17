using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Moq;
using Okta.Auth.Sdk;
using Okta.Aws.Cli.Cli;
using Okta.Aws.Cli.Cli.Interfaces;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.MFA;
using Xunit;

namespace Okta.Aws.Cli.UnitTests.ArgumentFactory
{
    public class ArgumentFactoryTests
    {
        [Fact]
        public void CliArgumentFactory_Check_InvalidArgumentHandler()
        {
            var invalidArgumentHandlerMock = new Mock<InvalidArgumentHandler>(() => new InvalidArgumentHandler(It.IsAny<IEnumerable<ICliArgumentHandler>>(), It.IsAny<IHostApplicationLifetime>()));

            var argumentFactory = new CliArgumentFactory(new List<ICliArgumentHandler>(),
                invalidArgumentHandlerMock.Object);

            var handler = argumentFactory.GetHandler("someNonExistentArgument");

            Assert.Same(invalidArgumentHandlerMock.Object, handler);
        }
    }
}
