using System;
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
public class WhenRemovingEarningsFollowingWithdrawalPriorToQualificationPeriodCompletion
{
    private readonly Fixture _fixture = new();
    private Apprenticeship.Apprenticeship _sut;
    private Mock<ISystemClockService> _mockSystemClock;

    [SetUp]
    public void SetUp()
    {
        SetupSystemClock(new DateTime(2024, 6, 24));

        var actualStartDate = new DateTime(2024, 1, 1);
        var plannedEndDate = new DateTime(2024, 12, 31);

        SetupApprenticeship(actualStartDate, plannedEndDate, 12000m);
    }

    [Test]
    public void ThenAllInstalmentsForThisAcademicYearAreRemoved()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 1, 31);

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.Instalments.Count.Should().Be(0);
    }

    [Test]
    public void ThenAllInstalmentsForPreviousAcademicYearArePreserved()
    {
        // Arrange
        SetupSystemClock(new DateTime(2024, 12, 1));
        var actualStartDate = new DateTime(2024, 7, 1); //started in last month of 23/24 ay
        var plannedEndDate = new DateTime(2025, 6, 30);
        SetupApprenticeship(actualStartDate, plannedEndDate, 12000m);

        var lastDayOfLearning = new DateTime(2024, 8, 11); //withdrawn beginning of 24/25 ay on last day of qualifying period

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.Instalments.Count.Should().Be(1);
        currentEpisode.EarningsProfile.Instalments.Should().OnlyContain(x => x.AcademicYear == 2324);
    }

    [Test]
    public void ThenEarningsRecalculatedEventIsCreated()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 1, 31);

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var events = _sut.FlushEvents();
        events.Should().ContainSingle(e => e is EarningsRecalculatedEvent);
    }

    [Test]
    public void ThenNewEarningsProfileIdIsGenerated()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 1, 31);

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }

    private void SetupSystemClock(DateTime currentDateTime)
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(currentDateTime);
    }

    private void SetupApprenticeship(DateTime actualStartDate, DateTime plannedEndDate, decimal agreedPrice)
    {
        _sut = _fixture.CreateApprenticeship(actualStartDate, plannedEndDate, agreedPrice);
        var episode = _sut.ApprenticeshipEpisodes.First();

        episode.CalculateEpisodeEarnings(_mockSystemClock.Object);
    }
}