using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class IncentivePayments
{
    public static List<IncentivePayment> Generate16to18IncentivePayments(int ageAtStartOfApprenticeship, DateTime apprenticeshipStartDate, DateTime apprenticeshipEndDate)
    {
        var incentivePayments = new List<IncentivePayment>();

        if (ageAtStartOfApprenticeship is < 16 or > 18)
        {
            return incentivePayments;
        }

        var incentiveDate = apprenticeshipStartDate.AddDays(89);
        incentivePayments.Add(new IncentivePayment
        {
            AcademicYear = incentiveDate.ToAcademicYear(),
            DueDate = incentiveDate,
            Amount = 500,
            DeliveryPeriod = incentiveDate.ToDeliveryPeriod(),
            IncentiveType = "ProviderIncentive"
        });

        incentivePayments.Add(new IncentivePayment
        {
            AcademicYear = incentiveDate.ToAcademicYear(),
            DueDate = incentiveDate,
            Amount = 500,
            DeliveryPeriod = incentiveDate.ToDeliveryPeriod(),
            IncentiveType = "EmployerIncentive"
        });

        var duration = apprenticeshipEndDate - apprenticeshipStartDate;
        if (duration.Days < 365)
        {
            return incentivePayments;
        }

        incentiveDate = apprenticeshipStartDate.AddDays(364);
        incentivePayments.Add(new IncentivePayment
        {
            AcademicYear = incentiveDate.ToAcademicYear(),
            DueDate = incentiveDate,
            Amount = 500,
            DeliveryPeriod = incentiveDate.ToDeliveryPeriod(),
            IncentiveType = "ProviderIncentive"
        });

        incentivePayments.Add(new IncentivePayment
        {
            AcademicYear = incentiveDate.ToAcademicYear(),
            DueDate = incentiveDate,
            Amount = 500,
            DeliveryPeriod = incentiveDate.ToDeliveryPeriod(),
            IncentiveType = "EmployerIncentive"
        });

        return incentivePayments;
    }
}
