using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Application;
using Dfe.Edis.SourceAdapter.Sld.WebJobs.Functions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Sld.WebJobs.UnitTests.FunctionsTests.TimedCheckForChangeTests
{
    public class WhenRunningTimedCheckForChange
    {
        private Mock<IChangeProcessor> _changeProcessorMock;
        private Mock<ILogger<TimedCheckForChange>> _loggerMock;
        private TimedCheckForChange _function;

        [SetUp]
        public void Arrange()
        {
            _changeProcessorMock = new Mock<IChangeProcessor>();

            _loggerMock = new Mock<ILogger<TimedCheckForChange>>();

            _function = new TimedCheckForChange(
                _changeProcessorMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task ThenItShouldCheckForUpdatedProviders()
        {
            var cancellationToken = new CancellationToken();

            await _function.RunAsync(
                new TimerInfo(new ConstantSchedule(new TimeSpan(0, 1, 0)), new ScheduleStatus(), false),
                cancellationToken);

            _changeProcessorMock.Verify(processor => processor.CheckForUpdatedProvidersAsync(cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogAndRethrowExceptions()
        {
            var exception = new NullReferenceException("unit test error");
            _changeProcessorMock.Setup(processor => processor.CheckForUpdatedProvidersAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<NullReferenceException>(async () =>
                await _function.RunAsync(
                    new TimerInfo(new ConstantSchedule(new TimeSpan(0, 1, 0)), new ScheduleStatus(), false),
                    CancellationToken.None));
            Assert.AreSame(exception, actual);
        }
    }
}