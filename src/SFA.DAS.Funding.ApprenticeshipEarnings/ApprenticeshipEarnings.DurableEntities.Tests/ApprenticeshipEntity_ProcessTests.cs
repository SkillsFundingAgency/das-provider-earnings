using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Tests
{
    public class ApprenticeshipEntity_ProcessTests
    {
        private ApprenticeshipEntity _sut;
        private InternalApprenticeshipLearnerEvent _apprenticeshipLearnerEvent;
        private Mock<IAdjustedPriceProcessor> _mockAdjustedPriceProcessor;
        private decimal _expectedAdjustedPrice;

        [SetUp]
        public async Task WhenProcessingAnApprenticeshipLearnerEvent()
        {
            _apprenticeshipLearnerEvent = new InternalApprenticeshipLearnerEvent
            {
                EmployerType = EmployerType.NonLevy,
                ActualStartDate = new DateTime(2022, 8, 1),
                AgreedOn = new DateTime(2022, 06, 01),
                ApprenticeshipKey = "unit-test-apprenticeship",
                ApprovedOn = new DateTime(2022, 06, 01),
                CommitmentId = 112,
                EmployerId = 114,
                PlannedEndDate = new DateTime(2024, 7, 31),
                ProviderId = 116,
                TrainingCode = "able-seafarer",
                TransferSenderEmployerId = 118,
                Uln = 900000118,
                AgreedPrice = 15000
            };

            _expectedAdjustedPrice = 12000;

            _mockAdjustedPriceProcessor = new Mock<IAdjustedPriceProcessor>();
            _mockAdjustedPriceProcessor.Setup(x => x.CalculateAdjustedPrice(It.IsAny<decimal>()))
                .Returns(_expectedAdjustedPrice);

            _sut = new ApprenticeshipEntity(_mockAdjustedPriceProcessor.Object);
            await _sut.Process(_apprenticeshipLearnerEvent);
        }

        [Test]
        public void ShouldMapApprenticeshipKeyToEntity()
        {
            _sut.ApprenticeshipKey.Should().Be(_apprenticeshipLearnerEvent.ApprenticeshipKey);
        }

        [Test]
        public void ShouldMapCommitmentIdToEntity()
        {
            _sut.CommitmentId.Should().Be(_apprenticeshipLearnerEvent.CommitmentId);
        }

        [Test]
        public void ShouldMapApprovedOnToEntity()
        {
            _sut.ApprovedOn.Should().Be(_apprenticeshipLearnerEvent.ApprovedOn);
        }

        [Test]
        public void ShouldMapAgreedOnToEntity()
        {
            _sut.AgreedOn.Should().Be(_apprenticeshipLearnerEvent.AgreedOn);
        }

        [Test]
        public void ShouldMapUlnToEntity()
        {
            _sut.Uln.Should().Be(_apprenticeshipLearnerEvent.Uln);
        }

        [Test]
        public void ShouldMapProviderIdToEntity()
        {
            _sut.ProviderId.Should().Be(_apprenticeshipLearnerEvent.ProviderId);
        }

        [Test]
        public void ShouldMapEmployerIdToEntity()
        {
            _sut.EmployerId.Should().Be(_apprenticeshipLearnerEvent.EmployerId);
        }

        [Test]
        public void ShouldMapActualStartDateToEntity()
        {
            _sut.ActualStartDate.Should().Be(_apprenticeshipLearnerEvent.ActualStartDate);
        }

        [Test]
        public void ShouldMapPlannedEndDateToEntity()
        {
            _sut.PlannedEndDate.Should().Be(_apprenticeshipLearnerEvent.PlannedEndDate);
        }

        [Test]
        public void ShouldMapAgreedPriceToEntity()
        {
            _sut.AgreedPrice.Should().Be(_apprenticeshipLearnerEvent.AgreedPrice);
        }

        [Test]
        public void ShouldMapTrainingCodeToEntity()
        {
            _sut.TrainingCode.Should().Be(_apprenticeshipLearnerEvent.TrainingCode);
        }

        [Test]
        public void ShouldMapTransferSenderEmployerIdToEntity()
        {
            _sut.TransferSenderEmployerId.Should().Be(_apprenticeshipLearnerEvent.TransferSenderEmployerId);
        }

        [Test]
        public void ShouldMapEmployerTypeToEntity()
        {
            _sut.EmployerType.Should().Be(_apprenticeshipLearnerEvent.EmployerType);
        }

        [Test]
        public void ShouldSetTheAdjustedPriceToTheResultFromTheProcessor()
        {
            _sut.AdjustedPrice.Should().Be(_expectedAdjustedPrice);
        }
    }
}