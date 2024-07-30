﻿using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenCalculateEarnings
{
    private readonly Fixture _fixture = new();
    private Apprenticeship.Apprenticeship _sut;
    private Mock<ISystemClockService> _mockSystemClock = null!;

    [SetUp]
    public void SetUp()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2021, 8, 30));

        var agreedPrice = _fixture.Create<decimal>();
        var actualStartDate = new DateTime(2021, 1, 15);
        var plannedEndDate = new DateTime(2021, 12, 31);
        _sut = _fixture.CreateApprenticeship(actualStartDate, plannedEndDate, agreedPrice);
    }

    [Test]
    public void ThenTheOnProgramTotalIsCalculated()
    {
        _sut.CalculateEarnings(_mockSystemClock.Object);
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile!.OnProgramTotal.Should().Be(_sut.ApprenticeshipEpisodes.Single().Prices.Single().AgreedPrice * .8m);
    }

    [Test]
    public void ThenTheCompletionAmountIsCalculated()
    {
        _sut.CalculateEarnings(_mockSystemClock.Object);
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile!.CompletionPayment.Should().Be(_sut.ApprenticeshipEpisodes.Single().Prices.Single().AgreedPrice * .2m);
    }

    [Test]
    public void ThenTheInstalmentsAreGeneratedWithAmountsTo5dp()
    {
        _sut.CalculateEarnings(_mockSystemClock.Object);

        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile!.Instalments.Count.Should().Be(12);
        currentEpisode.EarningsProfile!.Instalments.Should().AllSatisfy(x => x.Amount.Should().Be(decimal.Round(currentEpisode.EarningsProfile!.OnProgramTotal / 12m, 5)));
    }

    [Test]
    public void ThenEarningsCalculatedEventIsCreated()
    {
        _sut.CalculateEarnings(_mockSystemClock.Object);

        var events = _sut.FlushEvents();
        events.Should().ContainSingle(x => x.GetType() == typeof(EarningsCalculatedEvent));
    }
    
    [Test]
    public void ThenTheEarningsProfileIdIsGenerated()
    {
        _sut.CalculateEarnings(_mockSystemClock.Object);
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile!.EarningsProfileId.Should().NotBeEmpty();
    }
}