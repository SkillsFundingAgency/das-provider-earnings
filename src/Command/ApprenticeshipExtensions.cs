using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public static class ApprenticeshipExtensions
{
    public static List<DeliveryPeriod> BuildDeliveryPeriods(this ApprenticeshipEpisode currentEpisode)
    {
        var deliveryPeriods = new List<DeliveryPeriod>();

        if (currentEpisode.EarningsProfile != null)
        {
            deliveryPeriods.AddRange(currentEpisode.EarningsProfile.Instalments.Select(instalment => new DeliveryPeriod
            (
                instalment.DeliveryPeriod.ToCalendarMonth(),
                instalment.AcademicYear.ToCalendarYear(instalment.DeliveryPeriod),
                instalment.DeliveryPeriod,
                instalment.AcademicYear,
                instalment.Amount,
                currentEpisode.FundingLineType,
                "OnProgramme"
            )));

            //deliveryPeriods.AddRange(currentEpisode.EarningsProfile.AdditionalPayments.Select(additionalPayment => new DeliveryPeriod(
            //    additionalPayment.DeliveryPeriod.ToCalendarMonth(),
            //    additionalPayment.AcademicYear.ToCalendarYear(additionalPayment.DeliveryPeriod),
            //    additionalPayment.DeliveryPeriod,
            //    additionalPayment.AcademicYear,
            //    additionalPayment.Amount,
            //    currentEpisode.FundingLineType,
            //    additionalPayment.AdditionalPaymentType)));
        }

        return deliveryPeriods;
    }
}