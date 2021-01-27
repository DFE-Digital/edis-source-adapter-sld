using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Sld.Application.UnitTests.ChangeProcessorTests
{
    public class WhenCheckingForUpdatedProviders
    {
        private Mock<IStateStore> _stateStoreMock;
        private Mock<ISldClient> _sldClientMock;
        private Mock<ILogger<ChangeProcessor>> _loggerMock;
        private ChangeProcessor _processor;

        [SetUp]
        public void Arrange()
        {
            _stateStoreMock = new Mock<IStateStore>();

            _sldClientMock = new Mock<ISldClient>();
            _sldClientMock.Setup(sld => sld.ListAcademicYearsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] {"2021"});
            _sldClientMock.Setup(sld => sld.ListProvidersThatHaveSubmittedSince(
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<int>
                {
                    Items = new int[0],
                    TotalNumberOfItems = 0,
                    PageNumber = 0,
                    PageSize = 0,
                    TotalNumberOfPages = 0,
                });

            _loggerMock = new Mock<ILogger<ChangeProcessor>>();

            _processor = new ChangeProcessor(
                _stateStoreMock.Object,
                _sldClientMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task ThenItShouldGetListOfAcademicYearsFromSld()
        {
            var cancellationToken = new CancellationToken();

            await _processor.CheckForUpdatedProvidersAsync(cancellationToken);

            _sldClientMock.Verify(sld => sld.ListAcademicYearsAsync(cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldStopProcessingIfNoAcademicYearsReturnedFromSld()
        {
            _sldClientMock.Setup(sld => sld.ListAcademicYearsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new string[0]);

            await _processor.CheckForUpdatedProvidersAsync(CancellationToken.None);

            _stateStoreMock.Verify(store => store.GetLastPollTimeAsync(It.IsAny<CancellationToken>()),
                Times.Never);
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
        public async Task ThenItShouldGetAllPagesOfProviders()
        {
            var academicYear = "2021";
            DateTime? lastPoll = DateTime.Now;
            var cancellationToken = new CancellationToken();
            
            var rdm = new Random();
            _sldClientMock.Setup(sld => sld.ListProvidersThatHaveSubmittedSince(It.IsAny<string>(), It.IsAny<DateTime?>(), 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<int>
                {
                    Items = Enumerable.Range(1, 10).Select(x => rdm.Next(10000000, 99999999)).ToArray(),
                    PageNumber = 1,
                    PageSize = 10,
                    TotalNumberOfItems = 45,
                    TotalNumberOfPages = 3,
                });
            _sldClientMock.Setup(sld => sld.ListProvidersThatHaveSubmittedSince(It.IsAny<string>(), It.IsAny<DateTime?>(), 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<int>
                {
                    Items = Enumerable.Range(1, 10).Select(x => rdm.Next(10000000, 99999999)).ToArray(),
                    PageNumber = 2,
                    PageSize = 10,
                    TotalNumberOfItems = 25,
                    TotalNumberOfPages = 3,
                });
            _sldClientMock.Setup(sld => sld.ListProvidersThatHaveSubmittedSince(It.IsAny<string>(), It.IsAny<DateTime?>(), 3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SldPagedResult<int>
                {
                    Items = Enumerable.Range(1, 10).Select(x => rdm.Next(10000000, 99999999)).ToArray(),
                    PageNumber = 3,
                    PageSize = 10,
                    TotalNumberOfItems = 25,
                    TotalNumberOfPages = 3,
                });
            _sldClientMock.Setup(sld => sld.ListAcademicYearsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] {academicYear});
            _stateStoreMock.Setup(store => store.GetLastPollTimeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastPoll);

            await _processor.CheckForUpdatedProvidersAsync(cancellationToken);

            _sldClientMock.Verify(sld => sld.ListProvidersThatHaveSubmittedSince(
                    It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3));
            _sldClientMock.Verify(sld => sld.ListProvidersThatHaveSubmittedSince(academicYear, lastPoll, 1, cancellationToken),
                Times.Once);
            _sldClientMock.Verify(sld => sld.ListProvidersThatHaveSubmittedSince(academicYear, lastPoll, 2, cancellationToken),
                Times.Once);
            _sldClientMock.Verify(sld => sld.ListProvidersThatHaveSubmittedSince(academicYear, lastPoll, 3, cancellationToken),
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