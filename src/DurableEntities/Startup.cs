﻿using System;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;

[assembly: FunctionsStartup(typeof(SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Startup))]
namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;

public class Startup : FunctionsStartup
{
    public IConfiguration Configuration { get; set; }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        var serviceProvider = builder.Services.BuildServiceProvider();
            
        var configuration = serviceProvider.GetService<IConfiguration>();

        var configBuilder = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables()
            .AddJsonFile("local.settings.json", true);

        if (!configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase))
        {
            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });
        }

        Configuration = configBuilder.Build();

        var applicationSettings = new ApplicationSettings();
        Configuration.Bind(nameof(ApplicationSettings), applicationSettings);
        EnsureConfig(applicationSettings);
        Environment.SetEnvironmentVariable("NServiceBusConnectionString", applicationSettings.NServiceBusConnectionString);
       
        builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), Configuration));
        builder.Services.AddSingleton<ApplicationSettings>(x => applicationSettings);

        builder.Services.AddNServiceBus(applicationSettings);

        builder.Services.AddSingleton<IAdjustedPriceProcessor, AdjustedPriceProcessor>();
        builder.Services.AddSingleton<IInstallmentsGenerator, InstallmentsGenerator>();
        builder.Services.AddSingleton<IEarningsProfileGenerator, EarningsProfileGenerator>();
        builder.Services.AddSingleton<IEarningsGeneratedEventBuilder, EarningsGeneratedEventBuilder>();
    }

    private static void EnsureConfig(ApplicationSettings applicationSettings)
    {
        if (string.IsNullOrWhiteSpace(applicationSettings.NServiceBusConnectionString))
            throw new Exception("NServiceBusConnectionString in ApplicationSettings should not be null.");
    }
}