using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System.Collections.ObjectModel;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class Apprenticeship : AggregateRoot
{
    public Apprenticeship(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        _model = new ApprenticeshipModel
        {
            ApprovalsApprenticeshipId = apprenticeshipCreatedEvent.ApprovalsApprenticeshipId,
            Key = apprenticeshipCreatedEvent.ApprenticeshipKey,
            Uln = apprenticeshipCreatedEvent.Uln,
            Episodes = new List<EpisodeModel> { new EpisodeModel(apprenticeshipCreatedEvent.ApprenticeshipKey, apprenticeshipCreatedEvent.Episode) }
        };
        _episodes = _model.Episodes.Select(ApprenticeshipEpisode.Get).ToList();
    }

    private Apprenticeship(ApprenticeshipModel model)
    {
        _model = model;
        _episodes = _model.Episodes.Select(ApprenticeshipEpisode.Get).ToList();
    }

    private ApprenticeshipModel _model;
    private readonly List<ApprenticeshipEpisode> _episodes;

    public Guid ApprenticeshipKey => _model.Key;
    public long ApprovalsApprenticeshipId => _model.ApprovalsApprenticeshipId;
    public string Uln => _model.Uln;

    public IReadOnlyCollection<ApprenticeshipEpisode> ApprenticeshipEpisodes => new ReadOnlyCollection<ApprenticeshipEpisode>(_episodes);
    
    public static Apprenticeship Get(ApprenticeshipModel entity)
    {
        return new Apprenticeship(entity);
    }

    public ApprenticeshipModel GetModel()
    {
        return _model;
    }

    public void CalculateEarnings(ISystemClockService systemClock, IEarningsCalculator earningsCalculator)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.CalculateEpisodeEarnings(systemClock, earningsCalculator);
        AddEvent(new EarningsCalculatedEvent(this));
    }

    public void RecalculateEarnings(ApprenticeshipEvent apprenticeshipEvent, ISystemClockService systemClock, IEarningsCalculator earningsCalculator)
    {
        var episode = ApprenticeshipEpisodes.Single(x => x.ApprenticeshipEpisodeKey == apprenticeshipEvent.Episode.Key);
        episode.Update(apprenticeshipEvent.Episode);
        episode.CalculateEpisodeEarnings(systemClock, earningsCalculator);
        AddEvent(new EarningsRecalculatedEvent(this));
    }

    public void RemovalEarningsFollowingWithdrawal(DateTime lastDayOfLearning, ISystemClockService systemClock)
    {
        foreach (var episode in ApprenticeshipEpisodes)
        {
            episode.RemoveEarningsAfter(lastDayOfLearning, systemClock);
        }
        AddEvent(new EarningsRecalculatedEvent(this));
    }
}