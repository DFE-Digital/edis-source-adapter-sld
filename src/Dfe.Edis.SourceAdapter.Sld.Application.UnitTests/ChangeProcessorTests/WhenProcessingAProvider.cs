using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Dfe.Edis.SourceAdapter.Sld.Domain.Queuing;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Sld.Application.UnitTests.ChangeProcessorTests
{
    public class WhenProcessingAProvider
    {
        private Mock<IStateStore> _stateStoreMock;
        private Mock<ISldClient> _sldClientMock;
        private Mock<IProviderQueue> _providerQueueMock;
        private Mock<ILogger<ChangeProcessor>> _loggerMock;
        private ChangeProcessor _processor;

        [SetUp]
        public void Arrange()
        {
            _stateStoreMock = new Mock<IStateStore>();

            _sldClientMock = new Mock<ISldClient>();
            _sldClientMock.Setup(sld => sld.ListLearnersForProviderAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<Learner>
                {
                    Items = new Learner[0],
                    TotalNumberOfItems = 0,
                    PageNumber = 0,
                    PageSize = 0,
                    TotalNumberOfPages = 0,
                });

            _providerQueueMock = new Mock<IProviderQueue>();

            _loggerMock = new Mock<ILogger<ChangeProcessor>>();

            _processor = new ChangeProcessor(
                _stateStoreMock.Object,
                _sldClientMock.Object,
                _providerQueueMock.Object,
                _loggerMock.Object);
        }

        [Test, AutoData]
        public async Task ThenItShouldGetFirstPageOfLearnersForProvider(string academicYear, int ukprn)
        {
            var cancellationToken = new CancellationToken();

            await _processor.ProcessProviderAsync(academicYear, ukprn, cancellationToken);

            _sldClientMock.Verify(sld => sld.ListLearnersForProviderAsync(academicYear, ukprn, 1, cancellationToken),
                Times.Once);
        }
    }
}