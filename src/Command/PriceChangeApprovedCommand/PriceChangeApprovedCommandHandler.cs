﻿using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.PriceChangeApprovedCommand;

public class PriceChangeApprovedCommandHandler : IPriceChangeApprovedCommandHandler
{
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;

    public PriceChangeApprovedCommandHandler(IMessageSession messageSession, IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder)
    {
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
    }

    public async Task<Apprenticeship> RecalculateEarnings(PriceChangeApprovedCommand command)
    {
        var apprenticeship = Parse(command);
        var existingEarnings = MapModelToEarningsProfile(command.ApprenticeshipEntity.EarningsProfile);
        apprenticeship.RecalculateEarnings(existingEarnings, command.PriceChangeDetails.EffectiveFromDate);
        await _messageSession.Publish(_eventBuilder.Build(apprenticeship));
        return apprenticeship;
    }

    private Apprenticeship Parse(PriceChangeApprovedCommand entityModel)
    {
        var newAgreedPrice = entityModel.PriceChangeDetails.TrainingPrice + entityModel.PriceChangeDetails.AssessmentPrice;
        return new Apprenticeship(
            entityModel.ApprenticeshipEntity.ApprenticeshipKey,
            entityModel.ApprenticeshipEntity.ApprovalsApprenticeshipId,
            entityModel.ApprenticeshipEntity.Uln,
            entityModel.ApprenticeshipEntity.UKPRN,
            entityModel.ApprenticeshipEntity.EmployerAccountId,
            entityModel.ApprenticeshipEntity.LegalEntityName,
            entityModel.ApprenticeshipEntity.ActualStartDate,
            entityModel.ApprenticeshipEntity.PlannedEndDate,
            newAgreedPrice,
            entityModel.ApprenticeshipEntity.TrainingCode,
            entityModel.ApprenticeshipEntity.FundingEmployerAccountId,
            entityModel.ApprenticeshipEntity.FundingType,
            entityModel.ApprenticeshipEntity.FundingBandMaximum,
            entityModel.ApprenticeshipEntity.AgeAtStartOfApprenticeship
        );
    }

    private EarningsProfile MapModelToEarningsProfile(EarningsProfileEntityModel model)
    {
        var instalments = model.Instalments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList();
        return new EarningsProfile(model.AdjustedPrice, instalments, model.CompletionPayment);
    }

}
