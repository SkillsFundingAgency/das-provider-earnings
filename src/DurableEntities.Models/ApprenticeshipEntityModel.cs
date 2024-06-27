﻿using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApprenticeshipEntityModel
    {
        [JsonProperty] public Guid ApprenticeshipKey { get; set; }
        [JsonProperty] public long ApprovalsApprenticeshipId { get; set; }

        [JsonProperty] public string Uln { get; set; } = null!;
        [JsonProperty] public string LegalEntityName { get; set; } = null!;


        [JsonProperty] public string TrainingCode { get; set; } = null!;
        [JsonProperty] public long? FundingEmployerAccountId { get; set; }
        [JsonProperty] public FundingType FundingType { get; set; }


        [JsonProperty] public DateTime ActualStartDate { get; set; } // DO NOT APPROVE PR WITH THESE HERE
        [JsonProperty] public DateTime PlannedEndDate { get; set; } // DO NOT APPROVE PR WITH THESE HERE



        [JsonProperty] public EarningsProfileEntityModel EarningsProfile { get; set; } = null!;
        [JsonProperty] public List<HistoryRecord<EarningsProfileEntityModel>> EarningsProfileHistory { get; set; } = null!;
        [JsonProperty] public decimal FundingBandMaximum { get; set; }
        [JsonProperty] public List<ApprenticeshipEpisodeModel> ApprenticeshipEpisodes { get; set; } = null!;
        [JsonProperty] public int AgeAtStartOfApprenticeship { get; set; }
    }

    public class HistoryRecord<T> where T : class
    {
        public T Record { get; set; } = null!;
        public DateTime SupersededDate { get; set; }
    }
}
