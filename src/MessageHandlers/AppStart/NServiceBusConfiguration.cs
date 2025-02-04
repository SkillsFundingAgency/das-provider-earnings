﻿using Azure.Identity;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.AppStart;

internal static class NServiceBusConfiguration
{
    internal static IHostBuilder ConfigureNServiceBusForSubscribe(this IHostBuilder hostBuilder)
    {

        hostBuilder.UseNServiceBus((config, endpointConfiguration) =>
        {
            endpointConfiguration.AdvancedConfiguration.Conventions().SetConventions(); 

            var value = config["ApplicationSettings:NServiceBusLicense"];
            if (!string.IsNullOrEmpty(value))
            {
                var decodedLicence = WebUtility.HtmlDecode(value);
                endpointConfiguration.AdvancedConfiguration.License(decodedLicence);
            }

            CheckCreateQueues(config);
        });

        return hostBuilder;
    }

    /// <summary>
    /// Check if the queues exist and create them if they don't
    /// </summary>
    private static void CheckCreateQueues(IConfiguration configuration)
    {
        var queueTriggers = GetQueueTriggers();

        var connectionString = configuration["ApplicationSettings:NServiceBusConnectionString"];
        var fullyQualifiedNamespace = connectionString.GetFullyQualifiedNamespace();
        var adminClient = new ServiceBusAdministrationClient(fullyQualifiedNamespace, new DefaultAzureCredential());

        foreach (var queueTrigger in queueTriggers)
        {
            if (!adminClient.QueueExistsAsync(queueTrigger.QueueName).GetAwaiter().GetResult())
            {
                var errorQueue = $"{queueTrigger.QueueName}-error";

                adminClient.CreateQueueAsync(errorQueue).GetAwaiter().GetResult();

                var queue = new CreateQueueOptions(queueTrigger.QueueName)
                {
                    ForwardDeadLetteredMessagesTo = errorQueue,
                };

                adminClient.CreateQueueAsync(queue).GetAwaiter().GetResult();
            }
        }
    }

    private static IEnumerable<QueueTriggerAttribute> GetQueueTriggers()
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetName().FullName.Contains("SFA.DAS"));

        var queueTriggers = allAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .SelectMany(type => type.GetMethods())
            .SelectMany(method => method.GetParameters())
            .SelectMany(parameter => parameter.GetCustomAttributes(typeof(QueueTriggerAttribute), false))
            .Cast<QueueTriggerAttribute>();

        return queueTriggers;
    }
}
