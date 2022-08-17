using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests;

public class EarningsProfileGenerator_GenerateEarningsTests
{
    private EarningsProfileGenerator _sut;
    private ApprenticeshipCreatedEvent _apprenticeshipLearnerEvent;
    private Mock<IOnProgramTotalPriceCalculator> _mockOnProgramTotalPriceCalculator;
    private Mock<IInstallmentsGenerator> _mockInstallmentsGenerator;
    private Mock<IMessageSession> _mockMessageSession;
    private Mock<IEarningsGeneratedEventBuilder> _mockEarningsGeneratedEventBuilder;
    private Mock<IAdjustedPriceCalculator> _mockAdjustedPriceCalculator;
    private decimal _expectedOnProgramTotalPrice;
    private decimal _expectedAdjustedPrice;
    private List<EarningsInstallment> _expectedEarningsInstallments;
    private EarningsGeneratedEvent _expectedEarningsGeneratedEvent;
    private EarningsProfile _result;
    private Fixture _fixture;

    [SetUp]
    public async Task SetUp()
    {
        _fixture = new Fixture();

        _apprenticeshipLearnerEvent = _fixture.Build<ApprenticeshipCreatedEvent>()
            .With(x => x.FundingType, FundingType.NonLevy)
            .With(x => x.Uln, _fixture.Create<long>().ToString)
            .Create();

        _expectedAdjustedPrice = 15000;
        _expectedOnProgramTotalPrice = 12000;

        _mockAdjustedPriceCalculator = new Mock<IAdjustedPriceCalculator>();
        _mockAdjustedPriceCalculator.Setup(x => x.CalculateAdjustedPrice(It.IsAny<ApprenticeshipCreatedEvent>()))
            .Returns(_expectedAdjustedPrice);

        _mockOnProgramTotalPriceCalculator = new Mock<IOnProgramTotalPriceCalculator>();
        _mockOnProgramTotalPriceCalculator.Setup(x => x.CalculateOnProgramTotalPrice(It.IsAny<decimal>()))
            .Returns(_expectedOnProgramTotalPrice);

        _expectedEarningsInstallments = new List<EarningsInstallment>
        {
            new EarningsInstallment
            {
                Amount = 1000,
                AcademicYear = 1920,
                DeliveryPeriod = 4
            },
            new EarningsInstallment
            {
                Amount = 1000,
                AcademicYear = 1920,
                DeliveryPeriod = 5
            }
        };

        _mockInstallmentsGenerator = new Mock<IInstallmentsGenerator>();
        _mockInstallmentsGenerator
            .Setup(x => x.Generate(It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(_expectedEarningsInstallments);

        _mockMessageSession = new Mock<IMessageSession>();

        _expectedEarningsGeneratedEvent = new EarningsGeneratedEvent { ApprenticeshipKey = Guid.NewGuid() };

        _mockEarningsGeneratedEventBuilder = new Mock<IEarningsGeneratedEventBuilder>();

        _mockEarningsGeneratedEventBuilder.Setup(x => x.Build(It.IsAny<ApprenticeshipCreatedEvent>(), It.IsAny<EarningsProfile>())).Returns(_expectedEarningsGeneratedEvent);

        _sut = new EarningsProfileGenerator(_mockOnProgramTotalPriceCalculator.Object, _mockInstallmentsGenerator.Object, _mockMessageSession.Object, _mockEarningsGeneratedEventBuilder.Object, _mockAdjustedPriceCalculator.Object);
        _result = await _sut.GenerateEarnings(_apprenticeshipLearnerEvent);
    }

    [Test]
    public void ShouldPassTheAdjustedPriceToTheOnProgramPriceCalculator()
    {
        _mockOnProgramTotalPriceCalculator.Verify(x => x.CalculateOnProgramTotalPrice(_expectedAdjustedPrice));
    }

    [Test]
    public void ShouldSetTheOnProgramTotalPriceToTheResultFromTheOnProgramPriceCalculator()
    {
        _result.OnProgramTotalPrice.Should().Be(_expectedOnProgramTotalPrice);
    }

    [Test]
    public void ShouldPassTheOnProgramTotalPriceAndCorrectDatesToTheInstallmentsGenerator()
    {
        _mockInstallmentsGenerator.Verify(x => x.Generate(_expectedOnProgramTotalPrice, _apprenticeshipLearnerEvent.ActualStartDate.GetValueOrDefault(), _apprenticeshipLearnerEvent.PlannedEndDate.GetValueOrDefault()));
    }

    [Test]
    public void ShouldSetTheInstallmentsToTheResultFromTheInstallmentsGenerator()
    {
        _result.Installments.Should().BeEquivalentTo(_expectedEarningsInstallments);
    }

    [Test]
    public void ShouldPublishEarningsGeneratedEventCorrectly()
    {
        _mockMessageSession.Verify(x => x.Publish(_expectedEarningsGeneratedEvent, It.IsAny<PublishOptions>()), Times.Once);
    }
}