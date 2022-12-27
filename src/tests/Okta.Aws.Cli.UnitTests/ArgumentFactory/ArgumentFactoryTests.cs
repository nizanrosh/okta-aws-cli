using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using Okta.Aws.Cli.Cli;
using Okta.Aws.Cli.Cli.Interfaces;
using Xunit;

namespace Okta.Aws.Cli.UnitTests.ArgumentFactory
{
    public class ArgumentFactoryTests
    {
        [Fact]
        public void CliArgumentFactory_Check_InvalidArgumentHandler()
        {
            var invalidArgumentHandlerMock = new Mock<InvalidArgumentHandler>(() =>
                new InvalidArgumentHandler(It.IsAny<IEnumerable<ICliArgumentHandler>>(), It.IsAny<IConfiguration>()));

            var argumentFactory = new CliArgumentFactory(new List<ICliArgumentHandler>(),
                invalidArgumentHandlerMock.Object);

            var handler = argumentFactory.GetHandler("someNonExistentArgument");

            Assert.Same(invalidArgumentHandlerMock.Object, handler);
        }
    }
}