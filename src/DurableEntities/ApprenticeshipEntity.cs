﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApprenticeshipEntity
    {
        [JsonProperty] public ApprenticeshipEntityModel Model { get; set; }

        private readonly ICreateApprenticeshipCommandHandler _createApprenticeshipCommandHandler;

        public ApprenticeshipEntity(ICreateApprenticeshipCommandHandler createApprenticeshipCommandHandler)
        {
            _createApprenticeshipCommandHandler = createApprenticeshipCommandHandler;
        }

        public async Task HandleApprenticeshipLearnerEvent(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            MapApprenticeshipLearnerEventProperties(apprenticeshipCreatedEvent);
            var apprenticeship = await _createApprenticeshipCommandHandler.Create(new CreateApprenticeshipCommand(Model));
            Model.EarningsProfile = MapEarningsProfileToModel(apprenticeship.EarningsProfile);
        }

        [FunctionName(nameof(ApprenticeshipEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<ApprenticeshipEntity>();

        private void MapApprenticeshipLearnerEventProperties(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            Model = new ApprenticeshipEntityModel
            {
                ApprenticeshipKey = apprenticeshipCreatedEvent.ApprenticeshipKey,
                Uln = apprenticeshipCreatedEvent.Uln,
                UKPRN = apprenticeshipCreatedEvent.UKPRN,
                EmployerAccountId = apprenticeshipCreatedEvent.EmployerAccountId,
                ActualStartDate = apprenticeshipCreatedEvent.ActualStartDate.Value,
                PlannedEndDate = apprenticeshipCreatedEvent.PlannedEndDate.Value,
                AgreedPrice = apprenticeshipCreatedEvent.AgreedPrice,
                TrainingCode = apprenticeshipCreatedEvent.TrainingCode,
                FundingEmployerAccountId = apprenticeshipCreatedEvent.FundingEmployerAccountId,
                FundingType = apprenticeshipCreatedEvent.FundingType,
                ApprovalsApprenticeshipId = apprenticeshipCreatedEvent.ApprovalsApprenticeshipId,
                LegalEntityName = apprenticeshipCreatedEvent.LegalEntityName,
                AgeAtStartOfApprenticeship = apprenticeshipCreatedEvent.AgeAtStartOfApprenticeship
            };
        }

        private EarningsProfileEntityModel MapEarningsProfileToModel(EarningsProfile earningsProfile)
        {
            return new EarningsProfileEntityModel
            {
                AdjustedPrice = earningsProfile.AdjustedPrice,
                CompletionPayment = earningsProfile.CompletionPayment,
                Instalments = MapInstalmentsToModel(earningsProfile.Instalments),
            };
        }

        private List<InstalmentEntityModel> MapInstalmentsToModel(List<Instalment> instalments)
        {
            return instalments.Select(x => new InstalmentEntityModel
            {
                AcademicYear = x.AcademicYear,
                DeliveryPeriod = x.DeliveryPeriod,
                Amount = x.Amount
            }).ToList();
        }
    }
}
