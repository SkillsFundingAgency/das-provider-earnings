﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Mappers;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Repositories;

public class EarningsQueryRepository : IEarningsQueryRepository
{
    private readonly Lazy<ApprenticeshipEarningsDataContext> _lazyContext;
    private readonly ISystemClockService _systemClockService;
    private readonly ILogger<EarningsQueryRepository> _logger;

    private ApprenticeshipEarningsDataContext DbContext => _lazyContext.Value;

    public EarningsQueryRepository(Lazy<ApprenticeshipEarningsDataContext> dbContext, ISystemClockService systemClockService, ILogger<EarningsQueryRepository> logger)
    {
        _lazyContext = dbContext;
        _systemClockService = systemClockService;
        _logger = logger;
    }

    public async Task Add(Apprenticeship apprenticeship)
    {
        var earningsReadModels = apprenticeship.ToEarningsReadModels(_systemClockService);
        if (earningsReadModels != null)
        {
            await DbContext.AddRangeAsync(earningsReadModels);
            await DbContext.SaveChangesAsync();
        }
    }

    public async Task Replace(Apprenticeship apprenticeship)
    {
        var earningsToBeRemoved = await DbContext.Earning.Where(x => x.ApprenticeshipKey == apprenticeship.ApprenticeshipKey).ToListAsync();
        DbContext.RemoveRange(earningsToBeRemoved);
        await Add(apprenticeship);
    }

    public async Task<ProviderEarningsSummary> GetProviderSummary(long ukprn, short academicYear)
    {
        _logger.LogInformation("Getting provider earnings summary for ukprn: {ukprn} {academicYear}", ukprn, academicYear);

        var dbResponse = new
        {
            levyEarnings = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear && x.FundingType == FundingType.Levy).SumAsync(x => x.Amount),
            nonLevyEarnings = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear && x.FundingType == FundingType.NonLevy).SumAsync(x => x.Amount),
            transferEarnings = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear && x.FundingType == FundingType.Transfer).SumAsync(x => x.Amount)
        };

        var summary = new ProviderEarningsSummary
        {
            TotalLevyEarningsForCurrentAcademicYear = dbResponse.levyEarnings + dbResponse.transferEarnings,
            TotalNonLevyEarningsForCurrentAcademicYear = dbResponse.nonLevyEarnings
        };

        summary.TotalEarningsForCurrentAcademicYear = summary.TotalLevyEarningsForCurrentAcademicYear + summary.TotalNonLevyEarningsForCurrentAcademicYear;
        summary.TotalNonLevyEarningsForCurrentAcademicYearGovernment = summary.TotalNonLevyEarningsForCurrentAcademicYear * Constants.GovernmentContribution;
        summary.TotalNonLevyEarningsForCurrentAcademicYearEmployer = summary.TotalNonLevyEarningsForCurrentAcademicYear * Constants.EmployerContribution;

        _logger.LogInformation("Returning provider earnings summary for ukprn: {ukprn} {academicYear}", ukprn, academicYear);
        return summary;
    }

    public async Task<AcademicYearEarnings> GetAcademicYearEarnings(long ukprn, short academicYear)
    {
        var earnings = DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear).GroupBy(x => x.Uln);
        var result = new AcademicYearEarnings
        (
            await earnings.Select(x => new Learner
            (
                x.Key,
                x.First().FundingType,
                x.Select(y => new OnProgrammeEarning
                {
                    AcademicYear = y.AcademicYear,
                    DeliveryPeriod = y.DeliveryPeriod,
                    Amount = y.Amount
                }).ToList(),
                x.Sum(y => y.Amount)
            )).ToListAsync()
        );

        return result;
    }


}
