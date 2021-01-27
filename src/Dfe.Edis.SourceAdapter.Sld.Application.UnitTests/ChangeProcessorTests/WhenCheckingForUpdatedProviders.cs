using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Sld.Application.UnitTests.ChangeProcessorTests
{
    public class WhenCheckingForUpdatedProviders
    {
        private Mock<IStateStore> _stateStoreMock;
        private Mock<ILogger<ChangeProcessor>> _loggerMock;
        private ChangeProcessor _processor;

        [SetUp]
        public void Arrange()
        {
            _stateStoreMock = new Mock<IStateStore>();

            _loggerMock = new Mock<ILogger<ChangeProcessor>>();

            _processor = new ChangeProcessor(
                _stateStoreMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task ThenItShouldGetLastPollTimeFromStateStore()
        {
            var cancellationToken = new CancellationToken();

            await _processor.CheckForUpdatedProvidersAsync(cancellationToken);

            _stateStoreMock.Verify(store => store.GetLastPollTimeAsync(cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldSetLastPollTimeInStateStore()
        {
            var cancellationToken = new CancellationToken();

            await _processor.CheckForUpdatedProvidersAsync(cancellationToken);

            _stateStoreMock.Verify(store => store.SetLastPollTimeAsync(
                    It.Is<DateTime>(x => x >= DateTime.Now.AddSeconds(-2) && x <= DateTime.Now.AddSeconds(2)),
                    cancellationToken),
                Times.Once);
        }
    }
}