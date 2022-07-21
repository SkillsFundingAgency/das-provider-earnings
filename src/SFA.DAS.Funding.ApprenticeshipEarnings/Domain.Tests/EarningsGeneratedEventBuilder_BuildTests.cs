using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Tests;

public class EarningsGeneratedEventBuilder_BuildTests
{
    private EarningsGeneratedEventBuilder _sut;
    private ApprenticeshipCreatedEvent _apprenticeshipLearnerEvent;
    private EarningsProfile _earningsProfile;
    private EarningsGeneratedEvent _result;

    [SetUp]
    public void SetUp()
    {
        _sut = new EarningsGeneratedEventBuilder();

        _apprenticeshipLearnerEvent = new ApprenticeshipCreatedEvent
        {
            FundingType = FundingType.NonLevy,
            ActualStartDate = new DateTime(2022, 8, 1),
            ApprenticeshipKey = Guid.NewGuid(),
            EmployerAccountId = 114,
            PlannedEndDate = new DateTime(2024, 7, 31),
            UKPRN = 116,
            TrainingCode = "able-seafarer",
            FundingEmployerAccountId = 118,
            Uln = 900000118,
            AgreedPrice = 20000
        };

        _earningsProfile = new EarningsProfile
        {
            AdjustedPrice = 15000,
            Installments = new List<EarningsInstallment>
            {
                new EarningsInstallment
                {
                    Amount = 1000,
                    AcademicYear = 1920,
                    DeliveryPeriod = 5
                },
                new EarningsInstallment
                {
                    Amount = 2000,
                    AcademicYear = 1920,
                    DeliveryPeriod = 6
                }
            }
        };

        _result = _sut.Build(_apprenticeshipLearnerEvent, _earningsProfile);
    }

    [Test]
    public void ShouldPopulateTheApprenticeshipKeyCorrectly()
    {
        _result.ApprenticeshipKey.Should().Be(_apprenticeshipLearnerEvent.ApprenticeshipKey);
    }

    [Test]
    public void ShouldPopulateTheUlnCorrectly()
    {
        _result.FundingPeriods.First().Uln.Should().Be(_apprenticeshipLearnerEvent.Uln);
    }

    [Test]
    public void ShouldPopulateTheEmployerIdCorrectly()
    {
        _result.FundingPeriods.First().EmployerId.Should().Be(_apprenticeshipLearnerEvent.EmployerAccountId);
    }

    [Test]
    public void ShouldPopulateTheProviderIdCorrectly()
    {
        _result.FundingPeriods.First().ProviderId.Should().Be(_apprenticeshipLearnerEvent.UKPRN);
    }

    [Test]
    public void ShouldPopulateTheTransferSenderEmployerIdCorrectly()
    {
        _result.FundingPeriods.First().TransferSenderEmployerId.Should().Be(_apprenticeshipLearnerEvent.FundingEmployerAccountId);
    }

    [Test]
    public void ShouldPopulateTheAgreedPriceCorrectly()
    {
        _result.FundingPeriods.First().AgreedPrice.Should().Be(_apprenticeshipLearnerEvent.AgreedPrice);
    }

    [Test]
    public void ShouldPopulateTheStartDateCorrectly()
    {
        _result.FundingPeriods.First().StartDate.Should().Be(_apprenticeshipLearnerEvent.ActualStartDate);
    }

    [Test]
    public void ShouldPopulateTheTrainingCodeCorrectly()
    {
        _result.FundingPeriods.First().TrainingCode.Should().Be(_apprenticeshipLearnerEvent.TrainingCode);
    }

    [Test]
    public void ShouldPopulateTheEmployerTypeCorrectly()
    {
        _result.FundingPeriods.First().EmployerType.Should().Be(EmployerType.NonLevy);
    }

    [Test]
    public void ShouldBuildTheRightNumberOfDeliveryPeriods()
    {
        _result.FundingPeriods.First().DeliveryPeriods.Count.Should().Be(2);
    }

    [Test]
    public void ShouldPopulateTheFirstDeliveryPeriodCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.FirstOrDefault(x => x.Period == 5).Should().NotBeNull();
    }

    [Test]
    public void ShouldPopulateTheSecondDeliveryPeriodCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.FirstOrDefault(x => x.Period == 6).Should().NotBeNull();
    }

    [Test]
    public void ShouldPopulateTheFirstDeliveryPeriodCalendarMonthCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 5).CalendarMonth.Should().Be(12);
    }

    [Test]
    public void ShouldPopulateTheSecondDeliveryPeriodCalendarMonthCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 6).CalendarMonth.Should().Be(1);
    }

    [Test]
    public void ShouldPopulateTheFirstDeliveryPeriodCalendarYearCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 5).CalenderYear.Should().Be(2019);
    }

    [Test]
    public void ShouldPopulateTheSecondDeliveryPeriodCalendarYearCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 6).CalenderYear.Should().Be(2020);
    }

    [Test]
    public void ShouldPopulateTheFirstDeliveryPeriodAcademicYearCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 5).AcademicYear.Should().Be(1920);
    }

    [Test]
    public void ShouldPopulateTheSecondDeliveryPeriodAcademicYearCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 6).AcademicYear.Should().Be(1920);
    }

    [Test]
    public void ShouldPopulateTheFirstDeliveryPeriodLearningAmountCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 5).LearningAmount.Should().Be(1000);
    }

    [Test]
    public void ShouldPopulateTheSecondDeliveryPeriodLearningAmountCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 6).LearningAmount.Should().Be(2000);
    }
}