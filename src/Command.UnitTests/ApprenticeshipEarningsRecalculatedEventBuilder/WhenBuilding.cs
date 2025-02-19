using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ApprenticeshipEarningsRecalculatedEventBuilder;

[TestFixture]
public class WhenBuilding
{
    private Fixture _fixture;
    private Mock<ISystemClockService> _mockSystemClockService;
    private Apprenticeship? _apprenticeship;
    private ApprenticeshipModel? _apprenticeshipEntityModel;
    private Command.ApprenticeshipEarningsRecalculatedEventBuilder _builder;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _mockSystemClockService = new Mock<ISystemClockService>();
        _builder = new Command.ApprenticeshipEarningsRecalculatedEventBuilder(_mockSystemClockService.Object);
    }

    [Test]
    public void ThenTheApprenticeshipEarningsRecalculatedEventIsCorrectlyBuilt()
    {
        // Arrange
        BuildApprenticeship();
        _mockSystemClockService.Setup(x => x.UtcNow).Returns(DateTimeOffset.UtcNow);

        // Act
        var result = _builder.Build(_apprenticeship);

        // Assert
        var currentEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClockService.Object);

        result.ApprenticeshipKey.Should().Be(_apprenticeship.ApprenticeshipKey);
        result.DeliveryPeriods.Should().BeEquivalentTo(currentEpisode.BuildDeliveryPeriods());
        result.EarningsProfileId.Should().Be(currentEpisode.EarningsProfile!.EarningsProfileId);
        result.StartDate.Should().Be(currentEpisode.Prices!.OrderBy(x => x.StartDate).First().StartDate);
        result.PlannedEndDate.Should().Be(currentEpisode.Prices!.OrderBy(x => x.StartDate).Last().EndDate);
        result.AgeAtStartOfApprenticeship.Should().Be(currentEpisode.AgeAtStartOfApprenticeship);
    }

    private void BuildApprenticeship()
    {
        var episodeModel = _fixture
            .Build<EpisodeModel>()
            .With(x => x.Prices, new List<EpisodePriceModel>{ _fixture.Build<EpisodePriceModel>()
                .With(x => x.StartDate, DateTime.UtcNow.AddMonths(-10))
                .With(x => x.EndDate, DateTime.UtcNow.AddMonths(10))
                .Create() })
            .With(x => x.EarningsProfile, _fixture
                .Build<EarningsProfileModel>()
                .With(x => x.Instalments, new List<InstalmentModel>())
                .With(x => x.AdditionalPayments, new List<AdditionalPaymentModel>())
                .Create())
            .Create();
        _apprenticeshipEntityModel = _fixture
            .Build<ApprenticeshipModel>()
            .With(x => x.Episodes, new List<EpisodeModel>{ episodeModel })
            .Create();
        
        _apprenticeship = Apprenticeship.Get(_apprenticeshipEntityModel);
    }
}