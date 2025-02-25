using AutoFixture;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using FundingType = SFA.DAS.Apprenticeships.Enums.FundingType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;

internal static class FixtureExtensions
{
    internal static Apprenticeship.Apprenticeship CreateApprenticeship(this Fixture fixture, 
        DateTime startDate, DateTime endDate, decimal agreedPrice, FundingType? fundingType = null)
    {
        var apprenticeshipEntityModel = fixture.Create<ApprenticeshipCreatedEvent>();

        apprenticeshipEntityModel.Episode = new SFA.DAS.Apprenticeships.Types.ApprenticeshipEpisode()
        {
            Key = Guid.NewGuid(),
            Ukprn = 10000001, 
            EmployerAccountId = 10000001, 
            FundingType = fundingType == null ? fixture.Create<FundingType>() : fundingType.Value,
            Prices = new List<ApprenticeshipEpisodePrice>{ new()
                {
                    Key = Guid.NewGuid(),
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPrice = agreedPrice,
                    FundingBandMaximum = (int)agreedPrice + 1
                }
            }
        };

        return new Apprenticeship.Apprenticeship(apprenticeshipEntityModel);
    }


    internal static Apprenticeship.Apprenticeship CreateUpdatedApprenticeship(
        this Fixture fixture, 
        Apprenticeship.Apprenticeship apprenticeship, 
        decimal? newPrice = null, 
        DateTime? newStartDate = null, 
        bool withMissingEarningsProfile = false)
    {
        var apprenticeshipEntityModel = fixture.Create<ApprenticeshipModel>();

        apprenticeshipEntityModel.Key = apprenticeship.ApprenticeshipKey;
        apprenticeshipEntityModel.ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        apprenticeshipEntityModel.Uln = apprenticeship.Uln;

        apprenticeshipEntityModel.Episodes = apprenticeship.ApprenticeshipEpisodes.Select(x => new EpisodeModel
        {
            Ukprn = x.UKPRN,
            EmployerAccountId = x.EmployerAccountId,
            AgeAtStartOfApprenticeship = x.AgeAtStartOfApprenticeship,
            TrainingCode = x.TrainingCode,
            FundingType = x.FundingType,
            LegalEntityName = x.LegalEntityName,
            EarningsProfile = withMissingEarningsProfile ? null : MapEarningsProfileToModel(x.EarningsProfile),
            FundingEmployerAccountId = x.FundingEmployerAccountId,
            Prices = MapPricesToModel(x.Prices, newPrice == null ? apprenticeship.ApprenticeshipEpisodes.Single().Prices.Single().AgreedPrice + 1 : newPrice.Value + 1, newStartDate),
            Key = x.ApprenticeshipEpisodeKey
        }).ToList();

        return Apprenticeship.Apprenticeship.Get(apprenticeshipEntityModel);
    }

    internal static EarningsProfileModel MapEarningsProfileToModel(EarningsProfile earningsProfile)
    {
        var instalments = earningsProfile.Instalments.Select(i => new InstalmentModel
        {
            AcademicYear = i.AcademicYear,
            DeliveryPeriod = i.DeliveryPeriod,
            Amount = i.Amount
        }).ToList();

        var additionalPayments = earningsProfile.AdditionalPayments.Select(p => new AdditionalPaymentModel
            {
                AcademicYear = p.AcademicYear,
                DeliveryPeriod = p.DeliveryPeriod,
                Amount = p.Amount,
                DueDate = p.DueDate,
                AdditionalPaymentType = p.AdditionalPaymentType
            }).ToList();

        return new EarningsProfileModel
        {
            OnProgramTotal = earningsProfile.OnProgramTotal,
            Instalments = instalments,
            AdditionalPayments = additionalPayments,
            CompletionPayment = earningsProfile.CompletionPayment,
            EarningsProfileId = earningsProfile.EarningsProfileId
        };
    }

    internal static List<EpisodePriceModel>? MapPricesToModel(IReadOnlyCollection<Price>? prices, decimal fundingBandMaximum, DateTime? newStartDate)
    {
        return prices?.Select(x => new EpisodePriceModel
        {
            Key = x.PriceKey,
            FundingBandMaximum = fundingBandMaximum,
            StartDate = newStartDate ?? x.StartDate,
            AgreedPrice = x.AgreedPrice,
            EndDate = x.EndDate
        }).ToList();
    }
}
