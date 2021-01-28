using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Application;
using Dfe.Edis.SourceAdapter.Sld.Domain.Queuing;
using Dfe.Edis.SourceAdapter.Sld.WebJobs.Functions;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Sld.WebJobs.UnitTests.FunctionsTests.ProcessProviderItemTests
{
    public class WhenRunningOnQueueItemAvailable
    {
        private Mock<IChangeProcessor> _changeProcessorMock;
        private Mock<ILogger<ProcessProviderItem>> _loggerMock;
        private ProcessProviderItem _function;

        [SetUp]
        public void Arrange()
        {
            _changeProcessorMock = new Mock<IChangeProcessor>();

            _loggerMock = new Mock<ILogger<ProcessProviderItem>>();

            _function = new ProcessProviderItem(
                _changeProcessorMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task ThenItShouldProcessProviderQueueItem()
        {
            var cancellationToken = new CancellationToken();
            var queueItem = new ProviderQueueItem
            {
                AcademicYear = "2021",
                Ukprn = 12345678,
            };

            await _function.RunAsync(
                new CloudQueueMessage(JsonSerializer.Serialize(queueItem)),
                cancellationToken);

            _changeProcessorMock.Verify(processor => processor.ProcessProviderAsync(
                    queueItem.AcademicYear,
                    queueItem.Ukprn,
                    cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogAndRethrowExceptions()
        {
            var exception = new NullReferenceException("unit test error");
            _changeProcessorMock.Setup(processor => processor.ProcessProviderAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<NullReferenceException>(async () =>
                await _function.RunAsync(
                    new CloudQueueMessage(JsonSerializer.Serialize(new ProviderQueueItem())),
                    CancellationToken.None));
            Assert.AreSame(exception, actual);
        }
    }
}