using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using AutoFixture;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests;

public class WhenGeneratingInstalments
{
    private readonly Fixture _fixture;

    public WhenGeneratingInstalments()
    {
        _fixture = new Fixture();
    }

    [TestCase(15000, 2010, 01, 05, 2012, 01, 04, 24, 500)]
    [TestCase(15000, 2010, 01, 05, 2011, 12, 31, 24, 500)]
    [TestCase(15000, 2010, 01, 05, 2011, 12, 30, 23, 521.73913)]
    public void ShouldGenerateCorrectInstallmentsForBasicEpisode(decimal total, int startYear, int startMonth, int startDay,
        int endYear, int endMonth, int endDay, int expectedNumberOfInstallments, decimal expectedInstallmentAmount)
    {
        var startDate = new DateTime(startYear, startMonth, startDay);
        var endDate = new DateTime(endYear, endMonth, endDay);
        var prices = new List<Price>
        {
            new(_fixture.Create<Guid>(), startDate, endDate, total, total + _fixture.Create<int>())
        };
        var actualInstallments = OnProgramPayments.GenerateEarningsForEpisodePrices(prices, out var onProgramTotal, out var completionPayment);

        actualInstallments.Should().HaveCount(expectedNumberOfInstallments);
        actualInstallments.Should().OnlyContain(x => x.Amount == expectedInstallmentAmount);
        onProgramTotal.Should().Be(12000);
        completionPayment.Should().Be(3000);
    }

    [Test]
    public void ShouldGenerateCorrectInstallmentsForComplexEpisode()
    {
        var startDate = new DateTime(2019, 11, 11);
        var endDate = new DateTime(2022, 11, 27);
        var firstPriceChangeEffectiveFromDate = new DateTime(2019, 12, 17);
        var secondPriceChangeEffectiveFromDate = new DateTime(2020, 05, 04);

        const int fundingBandMax = 18000;

        var prices = new List<Price>
        {
            new(_fixture.Create<Guid>(), startDate, firstPriceChangeEffectiveFromDate.AddDays(-1), 15000, fundingBandMax), //price below funding band max
            new(_fixture.Create<Guid>(), firstPriceChangeEffectiveFromDate, secondPriceChangeEffectiveFromDate.AddDays(-1), 20000, fundingBandMax), //price changed to above funding band max
            new(_fixture.Create<Guid>(), secondPriceChangeEffectiveFromDate, endDate, 17000, fundingBandMax) //price reduced back down below funding band max
        };
        var actualInstallments = OnProgramPayments.GenerateEarningsForEpisodePrices(prices, out var onProgramTotal, out var completionPayment);

        actualInstallments.Should().HaveCount(36);
        actualInstallments.Where(x => x.Amount == (decimal) 333.33333).Should().HaveCount(1);
        actualInstallments.Where(x => x.Amount == (decimal) 401.90476).Should().HaveCount(5);
        actualInstallments.Where(x => x.Amount == (decimal) 375.23810).Should().HaveCount(30);
        onProgramTotal.Should().Be(13600);
        completionPayment.Should().Be(3400);
    }

    [Test]
    public void ShouldGenerateCorrectAcademicYearsAndDeliveryPeriods()
    {
        var startDate = new DateTime(2018, 7, 10);
        var endDate = new DateTime(2019, 1, 5);
        var total = _fixture.Create<int>();
        var prices = new List<Price>
        {
            new(_fixture.Create<Guid>(), startDate, endDate, total, total + _fixture.Create<int>())
        };
        var actualInstallments = OnProgramPayments.GenerateEarningsForEpisodePrices(prices, out _, out _);

        var expectedDeliveryPeriods = new List<(short academicYear, int deliveryPeriod)>
        {
            (1718,12),
            (1819,1),
            (1819,2),
            (1819,3),
            (1819,4),
            (1819,5)
        };

        actualInstallments.Should().HaveCount(expectedDeliveryPeriods.Count);
        foreach (var expectedDeliveryPeriod in expectedDeliveryPeriods)
        {
            actualInstallments.Should().Contain(x => x.AcademicYear == expectedDeliveryPeriod.academicYear && x.DeliveryPeriod == expectedDeliveryPeriod.deliveryPeriod);
        }
    }
}