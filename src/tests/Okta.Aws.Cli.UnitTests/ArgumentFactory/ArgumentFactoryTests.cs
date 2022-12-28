using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Okta.Aws.Cli.Cli;
using Okta.Aws.Cli.Cli.Interfaces;
using Xunit;

namespace Okta.Aws.Cli.UnitTests.ArgumentFactory
{
    [TestClass]
    public class ArgumentFactoryTests
    {
        [TestMethod]
        public void CliArgumentFactory_Check_InvalidArgumentHandler()
        {
            var invalidArgumentHandlerMock = new Mock<InvalidArgumentHandler>(() =>
                new InvalidArgumentHandler(It.IsAny<IEnumerable<ICliArgumentHandler>>(), It.IsAny<IConfiguration>()));

            var argumentFactory = new CliArgumentFactory(new List<ICliArgumentHandler>(),
                invalidArgumentHandlerMock.Object);

            var handler = argumentFactory.GetHandler("someNonExistentArgument");

            invalidArgumentHandlerMock.Object.Should().BeSameAs(handler);
        }
    }
}