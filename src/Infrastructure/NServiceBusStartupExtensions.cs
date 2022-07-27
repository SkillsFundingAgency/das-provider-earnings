﻿using System.Text.RegularExpressions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;

public static class NServiceBusStartupExtensions
{
    public static IServiceCollection AddNServiceBus(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
    {
        var webBuilder = serviceCollection.AddWebJobs(x => { });
        webBuilder.AddExecutionContextBinding();
        webBuilder.AddExtension(new NServiceBusExtensionConfigProvider());

        var endpointConfiguration = new EndpointConfiguration("SFA.DAS.Funding.ApprenticeshipEarnings")
            .UseMessageConventions()
            .UseNewtonsoftJsonSerializer();

        endpointConfiguration.SendOnly();

        if (configuration["NServiceBusConnectionString"].Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
        {
            var learningTransportFolder =
                Path.Combine(
                    Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)],
                    @"src\.learningtransport");
            endpointConfiguration
                .UseTransport<LearningTransport>()
                .StorageDirectory(learningTransportFolder);
            endpointConfiguration.UseLearningTransport(s => s.AddRouting());
            Environment.SetEnvironmentVariable("LearningTransportStorageDirectory", learningTransportFolder, EnvironmentVariableTarget.Process);
        }
        else
        {
            endpointConfiguration
                .UseAzureServiceBusTransport(configuration["ApplicationSettings:NServiceBusConnectionString"], r => r.AddRouting());
        }

        if (!string.IsNullOrEmpty(configuration["ApplicationSettings:NServiceBusLicense"]))
        {
            endpointConfiguration.License(configuration["ApplicationSettings:NServiceBusLicense"]);
        }

        ExcludeTestAssemblies(endpointConfiguration.AssemblyScanner());

        var endpointWithExternallyManagedServiceProvider = EndpointWithExternallyManagedServiceProvider.Create(endpointConfiguration, serviceCollection);
        endpointWithExternallyManagedServiceProvider.Start(new UpdateableServiceProvider(serviceCollection));
        serviceCollection.AddSingleton(p => endpointWithExternallyManagedServiceProvider.MessageSession.Value);

        return serviceCollection;
    }

    private static void ExcludeTestAssemblies(AssemblyScannerConfiguration scanner)
    {
        var excludeRegexs = new List<string>
        {
            @"nunit.*.dll"
        };

        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        foreach (var fileName in Directory.EnumerateFiles(baseDirectory, "*.dll")
                     .Select(Path.GetFileName))
        {
            foreach (var pattern in excludeRegexs)
            {
                if (Regex.IsMatch(fileName, pattern, RegexOptions.IgnoreCase))
                {
                    scanner.ExcludeAssemblies(fileName);
                    break;
                }
            }
        }
    }
}