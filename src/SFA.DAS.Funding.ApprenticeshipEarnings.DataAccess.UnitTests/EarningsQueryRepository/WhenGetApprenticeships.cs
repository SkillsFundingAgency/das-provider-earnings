using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.UnitTests.EarningsQueryRepository
{
    public class WhenGetApprenticeships
    {
        private ApprenticeshipEarningsDataContext _dbContext;
        private Domain.Repositories.EarningsQueryRepository _sut;
        private Mock<ISystemClockService> _mockSystemClockService;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            _mockSystemClockService = new Mock<ISystemClockService>();
            var options = new DbContextOptionsBuilder<ApprenticeshipEarningsDataContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new ApprenticeshipEarningsDataContext(options);
            _sut = new Domain.Repositories.EarningsQueryRepository(new Lazy<ApprenticeshipEarningsDataContext>(_dbContext), _mockSystemClockService.Object);
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public void GetApprenticeships_NoApprenticeships_ReturnsNull()
        {
            // Arrange
            var ukprn = _fixture.Create<long>();

            // Act
            var result = _sut.GetApprenticeships(ukprn);

            // Assert
            result.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task GetApprenticeships_ApprenticeshipsExistButNoneMatchUKPRN_ReturnsNull()
        {
            // Arrange
            var ukprn = _fixture.Create<long>();
            var apprenticeships = _fixture.Create<List<ApprenticeshipModel>>();
            await PopulateDb(apprenticeships);

            // Act
            var result = _sut.GetApprenticeships(ukprn);

            // Assert
            result.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task GetApprenticeships_CurrentApprenticeshipsMatchUKPRN_ReturnsList()
        {
            // Arrange
            var ukprn = _fixture.Create<long>();
            var testDateTime = _fixture.Create<DateTime>();
            var apprenticeships = _fixture.Create<List<ApprenticeshipModel>>();

            _mockSystemClockService.Setup(x=>x.UtcNow).Returns(testDateTime);

            foreach (var episode in apprenticeships.SelectMany(x => x.Episodes))
            {
                episode.Ukprn = ukprn;
                episode.Prices.First().StartDate = testDateTime.AddDays(-60);
                episode.Prices.First().EndDate = testDateTime.AddDays(60);
            }
            await PopulateDb(apprenticeships);

            // Act
            var result = _sut.GetApprenticeships(ukprn);

            // Assert
            result.Count.Should().Be(apprenticeships.Count);
        }

        private async Task PopulateDb(List<ApprenticeshipModel> apprenticeshipModels)
        {
            await _dbContext.AddRangeAsync(apprenticeshipModels);
            await _dbContext.SaveChangesAsync();
        }
    }
}