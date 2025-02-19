using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class OnProgramPayments
{
    public static List<Earning> GenerateEarningsForEpisodePrices(IEnumerable<Price> prices, out decimal onProgramTotal, out decimal completionPayment)
    {
        var earnings = new List<Earning>();
        onProgramTotal = 0;
        completionPayment = 0;

        var orderedPrices = prices.OrderBy(x => x.StartDate).ToList();
        var apprenticeshipEndDate = orderedPrices.Last().EndDate;

        foreach (var price in orderedPrices)
        {
            var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(
                price.AgreedPrice,
                price.FundingBandMaximum);
            onProgramTotal = apprenticeshipFunding.OnProgramTotal;
            completionPayment = apprenticeshipFunding.CompletionPayment;

            var remainingCostToDistribute = onProgramTotal - earnings.Sum(x => x.Amount);
            var earningsForPricePeriod = GenerateEarningsForPeriod(
                remainingCostToDistribute,
                price.StartDate,
                price.EndDate,
                apprenticeshipEndDate);
            earnings.AddRange(earningsForPricePeriod);
        }

        return earnings;
    }

    private static IEnumerable<Earning> GenerateEarningsForPeriod(decimal total, DateTime periodStartDate, DateTime periodEndDate, DateTime apprenticeshipEndDate)
    {
        var periodInstalmentCount = CalculateInstalmentCount(periodStartDate, periodEndDate);
        var remainingInstalmentCount = CalculateInstalmentCount(periodStartDate, apprenticeshipEndDate);
        var instalmentAmount = decimal.Round(total / remainingInstalmentCount, 5);

        var earnings = new List<Earning>();
        var currentMonth = new DateTime(periodStartDate.Year, periodStartDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i < periodInstalmentCount; i++)
        {
            var earning = new Earning
            {
                DeliveryPeriod = currentMonth.ToDeliveryPeriod(),
                AcademicYear = currentMonth.ToAcademicYear(),
                Amount = instalmentAmount
            };

            earnings.Add(earning);
            currentMonth = currentMonth.AddMonths(1);
        }

        return earnings;
    }

    private static int CalculateInstalmentCount(DateTime startDate, DateTime endDate)
    {
        var startDateMonth = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDateMonth = GetLastPaymentMonth(endDate);
        var instalmentCount = (endDateMonth.Year - startDateMonth.Year) * 12 +
            endDateMonth.Month -
            startDateMonth.Month + 1;
        return instalmentCount;
    }

    private static DateTime GetLastPaymentMonth(DateTime endDate)
    {
        var isLastDayOfMonth = endDate.Day == DateTime.DaysInMonth(endDate.Year, endDate.Month);
        return isLastDayOfMonth
            ? new DateTime(endDate.Year, endDate.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            : new DateTime(endDate.Year, endDate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
    }
}
