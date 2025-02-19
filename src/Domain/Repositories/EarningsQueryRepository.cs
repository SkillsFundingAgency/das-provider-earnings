using Microsoft.EntityFrameworkCore;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Mappers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public class EarningsQueryRepository : IEarningsQueryRepository
{
    private readonly Lazy<ApprenticeshipEarningsDataContext> _lazyContext;
    private readonly ISystemClockService _systemClockService;

    private ApprenticeshipEarningsDataContext DbContext => _lazyContext.Value;

    public EarningsQueryRepository(Lazy<ApprenticeshipEarningsDataContext> dbContext, ISystemClockService systemClockService)
    {
        _lazyContext = dbContext;
        _systemClockService = systemClockService;
    }

    public async Task Add(Apprenticeship.Apprenticeship apprenticeship)
    {
        var earningsReadModels = apprenticeship.ToEarningsReadModels(_systemClockService);
        if (earningsReadModels != null)
        {
            await DbContext.AddRangeAsync(earningsReadModels);
            await DbContext.SaveChangesAsync();
        }
    }

    public async Task Replace(Apprenticeship.Apprenticeship apprenticeship)
    {
        var earningsToBeRemoved = await DbContext.Earning.Where(x => x.ApprenticeshipKey == apprenticeship.ApprenticeshipKey).ToListAsync();
        DbContext.RemoveRange(earningsToBeRemoved);
        await Add(apprenticeship);
    }

    public async Task<ProviderEarningsSummary> GetProviderSummary(long ukprn, short academicYear)
    {
        var dbResponse = new
        {
            levyEarnings = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear && x.FundingType == FundingType.Levy).SumAsync(x => x.Amount),
            coinvestedNonLevyEarnings = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear && x.FundingType == FundingType.NonLevy && !x.IsNonLevyFullyFunded).SumAsync(x => x.Amount),
            transferEarnings = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear && x.FundingType == FundingType.Transfer).SumAsync(x => x.Amount),
            fullyFundedNonLevyEarnings = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear && x.FundingType == FundingType.NonLevy && x.IsNonLevyFullyFunded).SumAsync(x => x.Amount),
        };

        var summary = new ProviderEarningsSummary
        {
            TotalLevyEarningsForCurrentAcademicYear = dbResponse.levyEarnings + dbResponse.transferEarnings,
            TotalNonLevyEarningsForCurrentAcademicYear = dbResponse.coinvestedNonLevyEarnings + dbResponse.fullyFundedNonLevyEarnings
        };

        summary.TotalEarningsForCurrentAcademicYear = summary.TotalLevyEarningsForCurrentAcademicYear + summary.TotalNonLevyEarningsForCurrentAcademicYear;
        summary.TotalNonLevyEarningsForCurrentAcademicYearGovernment = dbResponse.fullyFundedNonLevyEarnings + (dbResponse.coinvestedNonLevyEarnings * Constants.GovernmentContribution);
        summary.TotalNonLevyEarningsForCurrentAcademicYearEmployer = dbResponse.coinvestedNonLevyEarnings * Constants.EmployerContribution;

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
                x.Sum(y => y.Amount),
                x.First().IsNonLevyFullyFunded
            )).ToListAsync()
        );

        return result;
    }

    public List<Apprenticeship.Apprenticeship>? GetApprenticeships(long ukprn)
    {
        // first get any apprenticeships which belonged to the ukprn, spliting this query will improve performance
        var apprenticeships = DbContext.Apprenticeships
            .Where(x => x.Episodes.Any(y => y.Ukprn == ukprn))
            .Include(x => x.Episodes)
            .ThenInclude(x => x.Prices)
            .Include(x => x.Episodes)
            .ThenInclude(x => x.EarningsProfile)
            .ThenInclude(x => x.Instalments)
            .Include(x => x.Episodes)
            .ThenInclude(x => x.EarningsProfile)
            .ThenInclude(x => x.AdditionalPayments)
            .Select(z => Apprenticeship.Apprenticeship.Get(z))
            .ToList();

        if (apprenticeships == null || !apprenticeships.Any())
            return null;

        // now get apprenticeships which currently belong to the ukprn
        var currentApprenticeships = apprenticeships.Where(x => x.GetCurrentEpisode(_systemClockService).UKPRN == ukprn).ToList();

        return currentApprenticeships;
    }
}
