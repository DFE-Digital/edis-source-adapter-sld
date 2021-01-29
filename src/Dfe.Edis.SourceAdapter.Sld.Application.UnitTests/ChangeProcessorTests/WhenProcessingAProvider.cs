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
        private Mock<ILearnerQueue> _learnerQueueMock;
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

            _loggerMock = new Mock<ILogger<ChangeProcessor>>();

            _processor = new ChangeProcessor(
                _stateStoreMock.Object,
                _sldClientMock.Object,
                _providerQueueMock.Object,
                _learnerQueueMock.Object,
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

        [Test, AutoData]
        public async Task ThenItShouldGetProviderState(string academicYear, int ukprn)
        {
            var cancellationToken = new CancellationToken();

            await _processor.ProcessProviderAsync(academicYear, ukprn, cancellationToken);

            _stateStoreMock.Verify(state => state.GetProviderStateAsync(academicYear, ukprn, cancellationToken),
                Times.Once);
        }

        [Test, AutoData]
        public async Task AndNoPriorProviderStateThenItShouldQueueLearners(string academicYear, int ukprn)
        {
            var cancellationToken = new CancellationToken();
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
                    TotalNumberOfPages = 3,
                });

            await _processor.ProcessProviderAsync(academicYear, ukprn, cancellationToken);

            _learnerQueueMock.Verify(q => q.EnqueueAsync(It.IsAny<LearnerQueueItem>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3));
            _learnerQueueMock.Verify(q => q.EnqueueAsync(
                    It.Is<LearnerQueueItem>(item => item.Ukprn == ukprn && item.AcademicYear == academicYear && item.PageNumber == 1),
                    cancellationToken),
                Times.Once);
            _learnerQueueMock.Verify(q => q.EnqueueAsync(
                    It.Is<LearnerQueueItem>(item => item.Ukprn == ukprn && item.AcademicYear == academicYear && item.PageNumber == 2),
                    cancellationToken),
                Times.Once);
            _learnerQueueMock.Verify(q => q.EnqueueAsync(
                    It.Is<LearnerQueueItem>(item => item.Ukprn == ukprn && item.AcademicYear == academicYear && item.PageNumber == 3),
                    cancellationToken),
                Times.Once);
        }

        [TestCase(100, 105)]
        [TestCase(100, 110)]
        [TestCase(100, 95)]
        [TestCase(100, 90)]
        [TestCase(100, 100)]
        public async Task AndNumberOfLearnersWithin10PercentOfStateThenItShouldQueueLearners(int previousNumberOfLearners, int currentNumberOfLearners)
        {
            var academicYear = "2021";
            var ukprn = 12345678;
            var cancellationToken = new CancellationToken();

            _sldClientMock.Setup(sld => sld.ListLearnersForProviderAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<Learner>
                {
                    Items = new Learner[0],
                    TotalNumberOfItems = currentNumberOfLearners,
                    PageNumber = 0,
                    PageSize = 0,
                    TotalNumberOfPages = 3,
                });
            _stateStoreMock.Setup(state => state.GetProviderStateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderState {NumberOfLearners = previousNumberOfLearners});

            await _processor.ProcessProviderAsync(academicYear, ukprn, cancellationToken);

            _learnerQueueMock.Verify(q => q.EnqueueAsync(It.IsAny<LearnerQueueItem>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3));
            _learnerQueueMock.Verify(q => q.EnqueueAsync(
                    It.Is<LearnerQueueItem>(item => item.Ukprn == ukprn && item.AcademicYear == academicYear && item.PageNumber == 1),
                    cancellationToken),
                Times.Once);
            _learnerQueueMock.Verify(q => q.EnqueueAsync(
                    It.Is<LearnerQueueItem>(item => item.Ukprn == ukprn && item.AcademicYear == academicYear && item.PageNumber == 2),
                    cancellationToken),
                Times.Once);
            _learnerQueueMock.Verify(q => q.EnqueueAsync(
                    It.Is<LearnerQueueItem>(item => item.Ukprn == ukprn && item.AcademicYear == academicYear && item.PageNumber == 3),
                    cancellationToken),
                Times.Once);
        }

        [TestCase(100, 111)]
        [TestCase(100, 89)]
        public async Task AndNumberOfLearnersNotWithin10PercentOfStateThenItShouldNotQueueLearners(int previousNumberOfLearners, int currentNumberOfLearners)
        {
            var academicYear = "2021";
            var ukprn = 12345678;
            var cancellationToken = new CancellationToken();

            _sldClientMock.Setup(sld => sld.ListLearnersForProviderAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<Learner>
                {
                    Items = new Learner[0],
                    TotalNumberOfItems = currentNumberOfLearners,
                    PageNumber = 0,
                    PageSize = 0,
                    TotalNumberOfPages = 3,
                });
            _stateStoreMock.Setup(state => state.GetProviderStateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderState {NumberOfLearners = previousNumberOfLearners});

            await _processor.ProcessProviderAsync(academicYear, ukprn, cancellationToken);

            _learnerQueueMock.Verify(q => q.EnqueueAsync(It.IsAny<LearnerQueueItem>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestCase(100, 105)]
        [TestCase(100, 110)]
        [TestCase(100, 95)]
        [TestCase(100, 90)]
        [TestCase(100, 100)]
        [TestCase(100, 111)]
        [TestCase(100, 89)]
        public async Task And4PreviousSubmissionsHaveBeenIgnoredThenItShouldQueueLearners(int previousNumberOfLearners, int currentNumberOfLearners)
        {
            var academicYear = "2021";
            var ukprn = 12345678;
            var cancellationToken = new CancellationToken();

            _sldClientMock.Setup(sld => sld.ListLearnersForProviderAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<Learner>
                {
                    Items = new Learner[0],
                    TotalNumberOfItems = currentNumberOfLearners,
                    PageNumber = 0,
                    PageSize = 0,
                    TotalNumberOfPages = 3,
                });
            _stateStoreMock.Setup(state => state.GetProviderStateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderState {NumberOfLearners = previousNumberOfLearners, IgnoredSubmissionCount = 4});

            await _processor.ProcessProviderAsync(academicYear, ukprn, cancellationToken);

            _learnerQueueMock.Verify(q => q.EnqueueAsync(It.IsAny<LearnerQueueItem>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3));
            _learnerQueueMock.Verify(q => q.EnqueueAsync(
                    It.Is<LearnerQueueItem>(item => item.Ukprn == ukprn && item.AcademicYear == academicYear && item.PageNumber == 1),
                    cancellationToken),
                Times.Once);
            _learnerQueueMock.Verify(q => q.EnqueueAsync(
                    It.Is<LearnerQueueItem>(item => item.Ukprn == ukprn && item.AcademicYear == academicYear && item.PageNumber == 2),
                    cancellationToken),
                Times.Once);
            _learnerQueueMock.Verify(q => q.EnqueueAsync(
                    It.Is<LearnerQueueItem>(item => item.Ukprn == ukprn && item.AcademicYear == academicYear && item.PageNumber == 3),
                    cancellationToken),
                Times.Once);
        }

        [Test, AutoData]
        public async Task AndNoPriorProviderStateThenItShouldAddProviderState(string academicYear, int ukprn)
        {
            var cancellationToken = new CancellationToken();
            _sldClientMock.Setup(sld => sld.ListLearnersForProviderAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<Learner>
                {
                    Items = new Learner[0],
                    TotalNumberOfItems = 25,
                    PageNumber = 0,
                    PageSize = 0,
                    TotalNumberOfPages = 1,
                });

            await _processor.ProcessProviderAsync(academicYear, ukprn, cancellationToken);

            _stateStoreMock.Verify(state => state.SetProviderStateAsync(
                    It.Is<ProviderState>(x => x.AcademicYear == academicYear && x.Ukprn == ukprn && x.NumberOfLearners == 25 && x.IgnoredSubmissionCount == 0),
                    cancellationToken),
                Times.Once);
        }

        [Test, AutoData]
        public async Task AndNumberOfLearnersWithin10PercentOfStateThenItShouldSetProviderState(string academicYear, int ukprn)
        {
            var cancellationToken = new CancellationToken();
            _sldClientMock.Setup(sld => sld.ListLearnersForProviderAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<Learner>
                {
                    Items = new Learner[0],
                    TotalNumberOfItems = 25,
                    PageNumber = 0,
                    PageSize = 0,
                    TotalNumberOfPages = 1,
                });

            await _processor.ProcessProviderAsync(academicYear, ukprn, cancellationToken);

            _stateStoreMock.Verify(state => state.SetProviderStateAsync(
                    It.Is<ProviderState>(x => x.AcademicYear == academicYear && x.Ukprn == ukprn && x.NumberOfLearners == 25 && x.IgnoredSubmissionCount == 0),
                    cancellationToken),
                Times.Once);
        }

        [Test, AutoData]
        public async Task AndNumberOfLearnersNotWithin10PercentOfStateThenItShouldIncrementIgnoredSubmissionsInState(string academicYear, int ukprn)
        {
            var cancellationToken = new CancellationToken();
            _sldClientMock.Setup(sld => sld.ListLearnersForProviderAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<Learner>
                {
                    Items = new Learner[0],
                    TotalNumberOfItems = 25,
                    PageNumber = 0,
                    PageSize = 0,
                    TotalNumberOfPages = 1,
                });
            _stateStoreMock.Setup(state => state.GetProviderStateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderState {NumberOfLearners = 100, IgnoredSubmissionCount = 2});

            await _processor.ProcessProviderAsync(academicYear, ukprn, cancellationToken);

            _stateStoreMock.Verify(state => state.SetProviderStateAsync(
                    It.Is<ProviderState>(x => x.NumberOfLearners == 100 && x.IgnoredSubmissionCount == 3),
                    cancellationToken),
                Times.Once);
        }
    }
}