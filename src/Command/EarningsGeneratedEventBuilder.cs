using System.Reflection.Metadata.Ecma335;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface IEarningsGeneratedEventBuilder
{
    EarningsGeneratedEvent Build(Apprenticeship apprenticeship);
    EarningsGeneratedEvent ReGenerate(Apprenticeship apprenticeship);
}

public class EarningsGeneratedEventBuilder : IEarningsGeneratedEventBuilder
{
    private readonly ISystemClockService _systemClock;

    public EarningsGeneratedEventBuilder(ISystemClockService systemClock)
    {
        _systemClock = systemClock;
    }

    public EarningsGeneratedEvent Build(Apprenticeship apprenticeship)
    {
        var currentEpisode = apprenticeship.GetCurrentEpisode(_systemClock);

        var result = new EarningsGeneratedEvent
        {
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            Uln = apprenticeship.Uln,
            EmployerId = currentEpisode.EmployerAccountId,
            ProviderId = currentEpisode.UKPRN,
            TransferSenderEmployerId = currentEpisode.FundingEmployerAccountId,
            AgreedPrice = currentEpisode.Prices!.First().AgreedPrice,
            StartDate = currentEpisode.Prices!.First().StartDate,
            TrainingCode = currentEpisode.TrainingCode,
            EmployerType = currentEpisode.FundingType.ToOutboundEventEmployerType(),
            DeliveryPeriods = currentEpisode.BuildDeliveryPeriods() ?? throw new ArgumentException("DeliveryPeriods"),
            EmployerAccountId = currentEpisode.EmployerAccountId,
            PlannedEndDate = currentEpisode.Prices!.First().EndDate,
            ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            EarningsProfileId = currentEpisode.EarningsProfile!.EarningsProfileId,
            AgeAtStartOfApprenticeship = currentEpisode.AgeAtStartOfApprenticeship
        };

        return result;
    }

    public EarningsGeneratedEvent ReGenerate(Apprenticeship apprenticeship)
    {
        var currentEpisode = apprenticeship.GetCurrentEpisode(_systemClock);
        var deliveryPeriods = currentEpisode.BuildDeliveryPeriods()?.OrderBy(r => r.CalenderYear).ThenBy(r => r.CalendarMonth).ToList();
        var latestPrice = currentEpisode.Prices.OrderBy(x => x.EndDate).Last();
        var firstPrice = currentEpisode.Prices.OrderBy(x => x.StartDate).First();

        return new EarningsGeneratedEvent
        {
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            Uln = apprenticeship.Uln,
            EmployerId = currentEpisode.EmployerAccountId,
            ProviderId = currentEpisode.UKPRN,
            TransferSenderEmployerId = currentEpisode.FundingEmployerAccountId,
            AgreedPrice = latestPrice.AgreedPrice,
            StartDate = firstPrice.StartDate,
            TrainingCode = currentEpisode.TrainingCode?.Trim(),
            EmployerType = currentEpisode.FundingType.ToOutboundEventEmployerType(),
            DeliveryPeriods = deliveryPeriods ?? throw new ArgumentException("DeliveryPeriods"),
            EmployerAccountId = currentEpisode.EmployerAccountId,
            PlannedEndDate = latestPrice.EndDate,
            ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            EarningsProfileId = currentEpisode.EarningsProfile!.EarningsProfileId,
            AgeAtStartOfApprenticeship = currentEpisode.AgeAtStartOfApprenticeship
        };
    }
}