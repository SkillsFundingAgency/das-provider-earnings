using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode
{

    private ApprenticeshipEpisode(EpisodeModel model)
    {
        _model = model;

        _prices = _model.Prices.Select(Price.Get).ToList();
        if (_model.EarningsProfile != null)
        {
            _earningsProfile = EarningsProfile.Get(_model.EarningsProfile);
        }
    }

    private readonly EpisodeModel _model;
    private List<Price> _prices;
    private EarningsProfile _earningsProfile;

    public Guid ApprenticeshipEpisodeKey => _model.Key;
    public long UKPRN => _model.Ukprn;
    public long EmployerAccountId => _model.EmployerAccountId;
    public int AgeAtStartOfApprenticeship => _model.AgeAtStartOfApprenticeship;
    public string TrainingCode => _model.TrainingCode;
    public FundingType FundingType => _model.FundingType;
    public string LegalEntityName => _model.LegalEntityName;
    public long? FundingEmployerAccountId => _model.FundingEmployerAccountId;
    public EarningsProfile? EarningsProfile => _earningsProfile;
    public IReadOnlyCollection<Price> Prices => new ReadOnlyCollection<Price>(_prices);
    public bool IsNonLevyFullyFunded => _model.FundingType == FundingType.NonLevy && _model.AgeAtStartOfApprenticeship < 22;

    public string FundingLineType =>
        AgeAtStartOfApprenticeship < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)";

    public static ApprenticeshipEpisode Get(EpisodeModel entity)
    {
        return new ApprenticeshipEpisode(entity);
    }

    public void CalculateEpisodeEarnings(ISystemClockService systemClock, IEarningsCalculator earningsCalculator)
    {
        var result = earningsCalculator.CalculateEarnings(this);
        
        //No more statics
        //var earnings = OnProgramPayments.GenerateEarningsForEpisodePrices(Prices, out var onProgramTotal, out var completionPayment);
        //var additionalPayments = IncentivePayments.Generate16to18IncentivePayments(AgeAtStartOfApprenticeship, _prices.Min(p => p.StartDate), _prices.Max(p => p.EndDate));
        
        UpdateEarningsProfile(result.Earnings, result.AdditionalPayments, systemClock, result.OnProgramTotal, result.CompletionPayment);
    }

    public void Update(Apprenticeships.Types.ApprenticeshipEpisode episodeUpdate)
    {
        UpdatePrices(episodeUpdate);

        _model.AgeAtStartOfApprenticeship = episodeUpdate.AgeAtStartOfApprenticeship;
        _model.EmployerAccountId = episodeUpdate.EmployerAccountId;
        _model.FundingEmployerAccountId = episodeUpdate.FundingEmployerAccountId;
        _model.FundingType = Enum.Parse<FundingType>(episodeUpdate.FundingType.ToString());
        _model.LegalEntityName = episodeUpdate.LegalEntityName;
        _model.TrainingCode = episodeUpdate.TrainingCode;
        _model.Ukprn = episodeUpdate.Ukprn;
    }

    public void RemoveEarningsAfter(DateTime lastDayOfLearning, ISystemClockService systemClock)
    {
        if (EarningsProfile != null)
        {
            var historyEntity = new EarningsProfileHistoryModel(EarningsProfile.GetModel(), systemClock!.UtcNow.Date);
            _model.EarningsProfileHistory.Add(historyEntity);
        }

        var earningsToKeep = GetEarningsToKeep(lastDayOfLearning);

        var additionalPayments = _model.EarningsProfile.AdditionalPayments
            .Select(x => new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.AdditionalPaymentType)).ToList();

        _earningsProfile = new EarningsProfile(_model.EarningsProfile.OnProgramTotal, earningsToKeep.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(),
            additionalPayments,
            _model.EarningsProfile.CompletionPayment, ApprenticeshipEpisodeKey);
        _model.EarningsProfile = _earningsProfile.GetModel();
    }

    private List<InstalmentModel> GetEarningsToKeep(DateTime lastDayOfLearning)
    {
        List<InstalmentModel> result;

        var academicYear = lastDayOfLearning.ToAcademicYear();
        var deliveryPeriod = lastDayOfLearning.ToDeliveryPeriod();

        var startDate = _model.Prices.Min(x => x.StartDate);
        var qualifyingDate = startDate.AddDays(Constants.QualifyingPeriod); //With shorter apprenticeships, this qualifying period will change
        if (lastDayOfLearning < qualifyingDate)
        {
            result = _model.EarningsProfile.Instalments.Where(x =>
                    x.AcademicYear < academicYear) //keep earnings from previous academic years
                .ToList();
        }
        else
        {
            result = _model.EarningsProfile.Instalments.Where(x =>
                    x.AcademicYear < academicYear //keep earnings from previous academic years
            || x.AcademicYear == academicYear && x.DeliveryPeriod < deliveryPeriod //keep earnings from previous delivery periods in the same academic year
            || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod && lastDayOfLearning.Day == DateTime.DaysInMonth(lastDayOfLearning.Year, lastDayOfLearning.Month)) //keep earnings in the last delivery period of learning if the learner is in learning on the census date
                .ToList();
        }

        return result;
    }

    private void UpdatePrices(Apprenticeships.Types.ApprenticeshipEpisode episodeUpdate)
    {
        foreach (var existingPrice in _prices.ToList())
        {
            var updatedPrice = episodeUpdate.Prices.SingleOrDefault(x => x.Key == existingPrice.PriceKey);
            if (updatedPrice != null)
            {
                existingPrice.Update(updatedPrice.StartDate, updatedPrice.EndDate, updatedPrice.TotalPrice, updatedPrice.FundingBandMaximum);
            }
            else
            {
                _prices.Remove(existingPrice);
                _model.Prices.Remove(existingPrice.GetModel());
            }
        }

        var newPrices = episodeUpdate.Prices
            .Where(x => _prices.All(y => y.PriceKey != x.Key))
            .Select(x => new Price(x.Key, x.StartDate, x.EndDate, x.TotalPrice, x.FundingBandMaximum))
            .ToList();
        _model.Prices.AddRange(newPrices.Select(x => x.GetModel()));
        _prices.AddRange(newPrices);
    }

    private void UpdateEarningsProfile(IEnumerable<Earning> earnings, IEnumerable<AdditionalPayment> additionalPaymentList, ISystemClockService systemClock, decimal onProgramTotal, decimal completionPayment)
    {
        if (EarningsProfile != null)
        {
            var historyEntity = new EarningsProfileHistoryModel(EarningsProfile.GetModel(), systemClock!.UtcNow.Date);
            _model.EarningsProfileHistory.Add(historyEntity);
        }

        var instalments = earnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList();

        var additionalPayments = additionalPaymentList.Select(x =>
            new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.AdditionalPaymentType)).ToList();

        _earningsProfile = new EarningsProfile(onProgramTotal, instalments, additionalPayments, completionPayment, ApprenticeshipEpisodeKey);
        _model.EarningsProfile = _earningsProfile.GetModel();
    }
}