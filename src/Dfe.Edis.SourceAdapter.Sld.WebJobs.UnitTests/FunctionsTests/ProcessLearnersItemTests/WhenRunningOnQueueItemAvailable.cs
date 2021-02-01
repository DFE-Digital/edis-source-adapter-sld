using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Application;
using Dfe.Edis.SourceAdapter.Sld.Domain.Queuing;
using Dfe.Edis.SourceAdapter.Sld.WebJobs.Functions;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Sld.WebJobs.UnitTests.FunctionsTests.ProcessLearnersItemTests
{
    public class WhenRunningOnQueueItemAvailable
    {
        private Mock<IChangeProcessor> _changeProcessorMock;
        private Mock<ILogger<ProcessLearnersItem>> _loggerMock;
        private ProcessLearnersItem _function;

        [SetUp]
        public void Arrange()
        {
            _changeProcessorMock = new Mock<IChangeProcessor>();

            _loggerMock = new Mock<ILogger<ProcessLearnersItem>>();

            _function = new ProcessLearnersItem(
                _changeProcessorMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task ThenItShouldProcessLearnersQueueItem()
        {
            var cancellationToken = new CancellationToken();
            var queueItem = new LearnerQueueItem()
            {
                AcademicYear = "2021",
                Ukprn = 12345678,
                PageNumber = 23,
            };

            await _function.RunAsync(
                new CloudQueueMessage(JsonSerializer.Serialize(queueItem)),
                cancellationToken);

            _changeProcessorMock.Verify(processor => processor.ProcessLearnerAsync(
                    queueItem.AcademicYear,
                    queueItem.Ukprn,
                    queueItem.PageNumber,
                    cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogAndRethrowExceptions()
        {
            var exception = new NullReferenceException("unit test error");
            _changeProcessorMock.Setup(processor => processor.ProcessLearnerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<NullReferenceException>(async () =>
                await _function.RunAsync(
                    new CloudQueueMessage(JsonSerializer.Serialize(new LearnerQueueItem())),
                    CancellationToken.None));
            Assert.AreSame(exception, actual);
        }
    }
}