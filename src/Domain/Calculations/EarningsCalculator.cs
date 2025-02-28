using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using Earning = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding.Earning;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations
{
#region Toplevel Earnings Calc and result

    public interface IEarningsCalculator
    {
        EarningsCalculationResult CalculateEarnings(ApprenticeshipEpisode episode);
    }

    public class EarningsCalculator(
        IOnProgramEarningsCalculator onProgramEarningsCalculator,
        IAdditionalPaymentsCalculator additionalPaymentsCalculator)
        : IEarningsCalculator
    {
        public EarningsCalculationResult CalculateEarnings(ApprenticeshipEpisode episode)
        {
            var earnings =
                onProgramEarningsCalculator.CalculateEarnings(episode, out var onProgramTotal, out var completionPayment);

            var result = new EarningsCalculationResult
            {
                Earnings = earnings,
                OnProgramTotal = onProgramTotal,
                CompletionPayment = completionPayment,
                AdditionalPayments = additionalPaymentsCalculator.CalculateAdditionalPayments(episode)
            };

            return result;
        }
    }

    public class EarningsCalculationResult
    {
        public List<Earning> Earnings { get; set; }
        public List<AdditionalPayment> AdditionalPayments { get; set; }
        public decimal OnProgramTotal { get; set; }
        public decimal CompletionPayment { get; set; }
    }

    #endregion

#region OnProg Earnings Calc interface and implementation


    public interface IOnProgramEarningsCalculator
    {
        List<Earning> CalculateEarnings(ApprenticeshipEpisode episode, out decimal onProgramTotal, out decimal completionPayment);
    }

    public class OnProgramEarningsCalculator : IOnProgramEarningsCalculator
    {
        public List<Earning> CalculateEarnings(ApprenticeshipEpisode episode, out decimal onProgramTotal, out decimal completionPayment)
        {
            //Here we'd put the actual logic for on-prog earnings
            onProgramTotal = 0;
            completionPayment = 0;
            return new List<Earning>();
        }
    }

    #endregion

#region Additional Payments Calc interface and implementation

    public interface IAdditionalPaymentsCalculator
    {
        List<AdditionalPayment> CalculateAdditionalPayments(ApprenticeshipEpisode episode);
    }

    public class AdditionalPaymentsCalculator(IIncentivesFor16to18Calculator incentivesFor16To18Calculator)
        : IAdditionalPaymentsCalculator
    {
        public List<AdditionalPayment> CalculateAdditionalPayments(ApprenticeshipEpisode episode)
        {
            var result = new List<AdditionalPayment>();

            //For each type of incentive payment, there'd be an additional call out here to the service that generates them
            result.AddRange(incentivesFor16To18Calculator.CalculateAdditionalPayments(episode));

            return result;
        }
    }

    #endregion

#region Incenctive calc

    public interface IIncentivesFor16to18Calculator
    {
        List<AdditionalPayment> CalculateAdditionalPayments(ApprenticeshipEpisode episode);
    }

    public class IncentivesFor16To18Calculator : IIncentivesFor16to18Calculator
    {
        public List<AdditionalPayment> CalculateAdditionalPayments(ApprenticeshipEpisode episode)
        {
            //Here we'd put the actual logic for this specific incentive payment
            return new List<AdditionalPayment>();
        }
    }

    #endregion
}
