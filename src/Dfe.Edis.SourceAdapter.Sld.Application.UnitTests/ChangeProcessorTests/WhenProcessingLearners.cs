using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Dfe.Edis.SourceAdapter.Sld.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Sld.Domain.Queuing;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Sld.Application.UnitTests.ChangeProcessorTests
{
    public class WhenProcessingLearners
    {
        private Mock<IStateStore> _stateStoreMock;
        private Mock<ISldClient> _sldClientMock;
        private Mock<IProviderQueue> _providerQueueMock;
        private Mock<ILearnerQueue> _learnerQueueMock;
        private Mock<ISldDataReceiver> _sldDataReceiverMock;
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
                    TotalNumberOfPages = 1,
                });

            _providerQueueMock = new Mock<IProviderQueue>();

            _learnerQueueMock = new Mock<ILearnerQueue>();

            _sldDataReceiverMock = new Mock<ISldDataReceiver>();

            _loggerMock = new Mock<ILogger<ChangeProcessor>>();

            _processor = new ChangeProcessor(
                _stateStoreMock.Object,
                _sldClientMock.Object,
                _providerQueueMock.Object,
                _learnerQueueMock.Object,
                _sldDataReceiverMock.Object,
                _loggerMock.Object);
        }

        [Test, AutoData]
        public async Task ThenItShouldGetSpecifiedPageOfLearnersForProvider(string academicYear, int ukprn, int pageNumber)
        {
            var cancellationToken = new CancellationToken();

            await _processor.ProcessLearnerAsync(academicYear, ukprn, pageNumber, cancellationToken);

            _sldClientMock.Verify(sld => sld.ListLearnersForProviderAsync(academicYear, ukprn, pageNumber, cancellationToken),
                Times.Once);
        }

        [Test, AutoData]
        public async Task ThenITShouldSendEachLearnerInPage(string academicYear, int ukprn, int pageNumber)
        {
            var cancellationToken = new CancellationToken();
            var learners = Enumerable.Range(0, 3).Select(index => new Learner
            {
                Ukprn = ukprn,
                Uln = 100000000 + index,
            }).ToArray();
            
            _sldClientMock.Setup(sld => sld.ListLearnersForProviderAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<Learner>
                {
                    Items = learners,
                    TotalNumberOfItems = 0,
                    PageNumber = 0,
                    PageSize = 0,
                    TotalNumberOfPages = 1,
                });

            await _processor.ProcessLearnerAsync(academicYear, ukprn, pageNumber, cancellationToken);

            _sldDataReceiverMock.Verify(receiver => receiver.SendLearnerAsync(It.IsAny<Learner>(), It.IsAny<CancellationToken>()),
                Times.Exactly(learners.Length));
            for (var i = 0; i < learners.Length; i++)
            {
                _sldDataReceiverMock.Verify(receiver => receiver.SendLearnerAsync(learners[i], cancellationToken),
                    Times.Once, $"Expected learner {i} to be sent");
            }
        }
    }
}