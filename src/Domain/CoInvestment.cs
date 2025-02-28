﻿namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public class CoInvestment
{
    private const decimal GovernmentContributionPercentage = 0.95m;

    public decimal GovernmentContribution { get; }
    public decimal EmployerContribution { get; }

    public static CoInvestment Calculate(bool isNoneLevyFullyFunded, decimal amount)
    {
        var governmentContribution = isNoneLevyFullyFunded ? amount : amount * GovernmentContributionPercentage;
        var employerContribution = amount - governmentContribution;
        return new CoInvestment(governmentContribution, employerContribution);
    }

    private CoInvestment(decimal governmentContribution, decimal employerContribution)
    {
        GovernmentContribution = governmentContribution;
        EmployerContribution = employerContribution;
    }
}