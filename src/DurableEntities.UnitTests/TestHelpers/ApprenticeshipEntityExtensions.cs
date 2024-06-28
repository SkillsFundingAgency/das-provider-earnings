﻿using Microsoft.Extensions.Internal;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests.TestHelpers;

internal static class ApprenticeshipEntityExtensions
{
    internal static ApprenticeshipEpisodeModel GetCurrentEpisode(this ApprenticeshipEntity apprenticeship, ISystemClock systemClock)
    {
        var episode = apprenticeship.Model.ApprenticeshipEpisodes.FirstOrDefault(x => x.ActualStartDate <= systemClock.UtcNow && x.PlannedEndDate >= systemClock.UtcNow);

        if (episode == null)
        {
            // if no episode is active for the current date, then there could be a episode for the apprenticeship that is yet to start
            episode = apprenticeship.Model.ApprenticeshipEpisodes.Single(x => x.ActualStartDate >= systemClock.UtcNow);
        }

        if (episode == null)
            throw new InvalidOperationException("No current episode found");

        return episode!;
    }
}