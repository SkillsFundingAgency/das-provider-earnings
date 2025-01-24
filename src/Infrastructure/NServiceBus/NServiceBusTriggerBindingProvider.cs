﻿using Microsoft.Azure.WebJobs.Host.Triggers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Extensions;
using System.Reflection;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.NServiceBus
{
    public class NServiceBusTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly NServiceBusOptions _nServiceBusOptions;

        public NServiceBusTriggerBindingProvider(NServiceBusOptions nServiceBusOptions = null)
        {
            _nServiceBusOptions = nServiceBusOptions ?? new NServiceBusOptions();
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            var parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<NServiceBusTriggerAttribute>(false);

            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            if (string.IsNullOrEmpty(attribute.Connection))
            {
                attribute.Connection = EnvironmentVariables.NServiceBusConnectionString?.FormatConnectionString();
            }

            if (string.IsNullOrEmpty(attribute.LearningTransportStorageDirectory))
            {
                attribute.LearningTransportStorageDirectory = EnvironmentVariables.LearningTransportStorageDirectory;
            }

            return Task.FromResult<ITriggerBinding>(new NServiceBusTriggerBinding(parameter, attribute, _nServiceBusOptions));
        }
    }
}
