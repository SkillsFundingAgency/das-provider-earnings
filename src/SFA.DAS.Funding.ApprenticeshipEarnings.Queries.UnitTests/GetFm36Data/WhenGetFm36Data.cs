using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.UnitTests.GetFm36Data;

[TestFixture]
public class WhenGetFm36Data
{
    private Fixture _fixture;
    private Mock<IEarningsQueryRepository> _mockEarningsQueryRepository;
    private Mock<ISystemClockService> _mockSystemClockService;
    private GetFm36DataQueryHandler _queryHandler;
    private DateTime _testTime;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _mockEarningsQueryRepository = new Mock<IEarningsQueryRepository>();

        _mockSystemClockService = new Mock<ISystemClockService>();
        _testTime = _fixture.Create<DateTime>();
        _mockSystemClockService.Setup(x=>x.UtcNow).Returns(_testTime);

        _queryHandler = new GetFm36DataQueryHandler(_mockEarningsQueryRepository.Object, _mockSystemClockService.Object, Mock.Of<ILogger<GetFm36DataQueryHandler>>());
    }

    [Test]
    public async Task Handle_NoApprenticeships_ReturnsEmptyResponse()
    {
        // Arrange
        var query = _fixture.Create<GetFm36DataRequest>();
        _mockEarningsQueryRepository.Setup(x => x.GetApprenticeships(query.Ukprn)).Returns((List<Domain.Apprenticeship.Apprenticeship>)null);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_ApprenticeshipsExist_ReturnsMappedResponse()
    {
        // Arrange
        var query = _fixture.Create<GetFm36DataRequest>();
        var currentEpisode = CreateCurrentEpisode();
        var expectedApprenticeship = CreateApprenticeship(query.Ukprn, currentEpisode);
        var domainApprenticeships = new List<Domain.Apprenticeship.Apprenticeship> { expectedApprenticeship };

        _mockEarningsQueryRepository.Setup(x => x.GetApprenticeships(query.Ukprn)).Returns(domainApprenticeships);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        var apprenticeship = result.First();
        apprenticeship.Key.Should().Be(expectedApprenticeship.ApprenticeshipKey);
        apprenticeship.Ukprn.Should().Be(query.Ukprn);
        apprenticeship.FundingLineType.Should().Be(currentEpisode.AgeAtStartOfApprenticeship < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)");
        apprenticeship.Episodes.Should().ContainSingle();

        var episodeResult = apprenticeship.Episodes.First();
        episodeResult.Key.Should().Be(currentEpisode.Key);
        episodeResult.CompletionPayment.Should().Be(currentEpisode.EarningsProfile.CompletionPayment);
        episodeResult.OnProgramTotal.Should().Be(currentEpisode.EarningsProfile.OnProgramTotal);
        episodeResult.Instalments.Should().HaveCount(currentEpisode.EarningsProfile.Instalments.Count);
        episodeResult.AdditionalPayments.Should().HaveCount(currentEpisode.EarningsProfile.AdditionalPayments.Count);

        foreach(var expectedInstalment in currentEpisode.EarningsProfile.Instalments)
        {
            var domainInstalment = episodeResult.Instalments.Single(x => 
                x.AcademicYear == expectedInstalment.AcademicYear && 
                x.DeliveryPeriod == expectedInstalment.DeliveryPeriod &&
                x.Amount == expectedInstalment.Amount);
        }

        foreach (var expectedAdditionalPayment in currentEpisode.EarningsProfile.AdditionalPayments)
        {
            var additionalPayment = episodeResult.AdditionalPayments.Single(x =>
                x.AcademicYear == expectedAdditionalPayment.AcademicYear &&
                x.DeliveryPeriod == expectedAdditionalPayment.DeliveryPeriod &&
                x.Amount == expectedAdditionalPayment.Amount &&
                x.AdditionalPaymentType == expectedAdditionalPayment.AdditionalPaymentType &&
                x.DueDate == expectedAdditionalPayment.DueDate);
        }

    }

    /// <summary>
    /// Creates an episode with a date range that will result in it being the current episode
    /// </summary>
    private EpisodeModel CreateCurrentEpisode()
    {
        var episode = _fixture.Create<EpisodeModel>();

        episode.Prices.First().StartDate = _testTime.AddDays(-60);
        episode.Prices.First().EndDate = _testTime.AddDays(60);

        return episode;
    }

    private Domain.Apprenticeship.Apprenticeship CreateApprenticeship(long ukprn, EpisodeModel episode)
    {
        var domainApprenticeship = _fixture.Create<ApprenticeshipModel>();

        var episodes = new List<EpisodeModel>();

        episode.Ukprn = ukprn;

        episodes.Add(episode);
        domainApprenticeship.Episodes = episodes;

        return Domain.Apprenticeship.Apprenticeship.Get(domainApprenticeship);
    }
}
