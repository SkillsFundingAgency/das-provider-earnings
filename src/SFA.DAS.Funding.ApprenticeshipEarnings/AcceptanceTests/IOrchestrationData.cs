﻿using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Acceptance;

public interface IOrchestrationData
{
    DurableOrchestrationStatus Status { get; set; }
}