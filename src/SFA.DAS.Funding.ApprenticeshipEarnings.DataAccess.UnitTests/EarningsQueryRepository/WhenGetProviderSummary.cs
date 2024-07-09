﻿using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.UnitTests.EarningsQueryRepository
{
    public class WhenGetProviderSummary
    {
        private ApprenticeshipEarningsDataContext _dbContext;
        private Repositories.EarningsQueryRepository _sut;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<ApprenticeshipEarningsDataContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new ApprenticeshipEarningsDataContext(options);
            _sut = new Repositories.EarningsQueryRepository(new Lazy<ApprenticeshipEarningsDataContext>(_dbContext), Mock.Of<ISystemClockService>());
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task ThenCurrentAcademicYearTotalEarningsIsReturned()
        {
            var providerId = _fixture.Create<long>();
            short currentAcademicYear = 2223;

            var earnings = new Earning[]
            {
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1001).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2010).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear + 1).With(x => x.Amount, 1100).Create(), //Exclude - wrong academic year
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2500).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId + 1).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1009).Create() //Exclude - wrong provider
            };

            await _dbContext.AddRangeAsync(earnings);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetProviderSummary(providerId, currentAcademicYear);

            // Assert
            result.TotalEarningsForCurrentAcademicYear.Should().Be(5511);
        }

        [Test]
        public async Task ThenCurrentAcademicYearLevyEarningsIsReturnedIncludingTransfers()
        {
            var providerId = _fixture.Create<long>();
            short currentAcademicYear = 2223;

            var earnings = new Earning[]
            {
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1001).With(x => x.FundingType, FundingType.Levy).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2010).With(x => x.FundingType, FundingType.NonLevy).Create(), //Exclude - non levy
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear + 1).With(x => x.Amount, 1100).With(x => x.FundingType, FundingType.Levy).Create(), //Exclude - wrong academic year
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2500).With(x => x.FundingType, FundingType.Transfer).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId + 1).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1009).With(x => x.FundingType, FundingType.Levy).Create() //Exclude - wrong provider
            };

            await _dbContext.AddRangeAsync(earnings);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetProviderSummary(providerId, currentAcademicYear);

            // Assert
            result.TotalLevyEarningsForCurrentAcademicYear.Should().Be(3501);
        }

        [Test]
        public async Task ThenCurrentAcademicYearNonLevyEarningsIsReturned()
        {
            var providerId = _fixture.Create<long>();
            short currentAcademicYear = 2223;

            var earnings = new Earning[]
            {
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1001).With(x => x.FundingType, FundingType.Levy).Create(), //Exclude - levy
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2010).With(x => x.FundingType, FundingType.NonLevy).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear + 1).With(x => x.Amount, 1100).With(x => x.FundingType, FundingType.NonLevy).Create(), //Exclude - wrong academic year
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2500).With(x => x.FundingType, FundingType.Transfer).Create(), //Exclude - transfer
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId + 1).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1009).With(x => x.FundingType, FundingType.NonLevy).Create() //Exclude - wrong provider
            };

            await _dbContext.AddRangeAsync(earnings);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetProviderSummary(providerId, currentAcademicYear);

            // Assert
            result.TotalNonLevyEarningsForCurrentAcademicYear.Should().Be(2010);
        }

        [Test]
        public async Task ThenTotalNonLevyEarningsForCurrentAcademicYearGovernmentIsReturned()
        {
            var providerId = _fixture.Create<long>();
            short currentAcademicYear = 2223;
            decimal nonLevyLearnings = 2010;
            decimal expectedGovernmentContribution = 1909.5m;

            var earnings = new Earning[]
            {
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1001).With(x => x.FundingType, FundingType.Levy).Create(), //Exclude - levy
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, nonLevyLearnings).With(x => x.FundingType, FundingType.NonLevy).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear + 1).With(x => x.Amount, 1100).With(x => x.FundingType, FundingType.NonLevy).Create(), //Exclude - wrong academic year
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2500).With(x => x.FundingType, FundingType.Transfer).Create(), //Exclude - transfer
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId + 1).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1009).With(x => x.FundingType, FundingType.NonLevy).Create() //Exclude - wrong provider
            };

            await _dbContext.AddRangeAsync(earnings);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetProviderSummary(providerId, currentAcademicYear);

            // Assert
            result.TotalNonLevyEarningsForCurrentAcademicYearGovernment.Should().Be(expectedGovernmentContribution);
        }

        [Test]
        public async Task ThenTotalNonLevyEarningsForCurrentAcademicYearEmployerIsReturned()
        {
            var providerId = _fixture.Create<long>();
            short currentAcademicYear = 2223;
            decimal nonLevyLearnings = 2010;
            decimal expectedEmployerContribution = 100.5m;

            var earnings = new Earning[]
            {
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1001).With(x => x.FundingType, FundingType.Levy).Create(), //Exclude - levy
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, nonLevyLearnings).With(x => x.FundingType, FundingType.NonLevy).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear + 1).With(x => x.Amount, 1100).With(x => x.FundingType, FundingType.NonLevy).Create(), //Exclude - wrong academic year
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2500).With(x => x.FundingType, FundingType.Transfer).Create(), //Exclude - transfer
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId + 1).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1009).With(x => x.FundingType, FundingType.NonLevy).Create() //Exclude - wrong provider
            };

            await _dbContext.AddRangeAsync(earnings);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetProviderSummary(providerId, currentAcademicYear);

            // Assert
            result.TotalNonLevyEarningsForCurrentAcademicYearEmployer.Should().Be(expectedEmployerContribution);
        }
    }
}
