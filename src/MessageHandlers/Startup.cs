using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using NServiceBus;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.AppStart;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

[assembly: NServiceBusTriggerFunction("SFA.DAS.Funding.ApprenticeshipEarnings")]
namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

[ExcludeFromCodeCoverage]
public class Startup
{
    public IConfiguration Configuration { get; set; }

    private ApplicationSettings _applicationSettings;

    public ApplicationSettings ApplicationSettings
    {
        get
        {
            if (_applicationSettings == null)
            {
                _applicationSettings = new ApplicationSettings();
                Configuration.Bind(nameof(ApplicationSettings), _applicationSettings);
            }
            return _applicationSettings;
        }
    }

    public Startup()
    {
        ForceAssemblyLoad();
    }

    public void Configure(IHostBuilder builder)
    {
        builder
            .ConfigureAppConfiguration(PopulateConfig)
            .ConfigureNServiceBusForSubscribe()
            .ConfigureServices((c, s) =>
            {
                SetupServices(s);
                s.ConfigureNServiceBusForSend(ApplicationSettings.NServiceBusConnectionString.GetFullyQualifiedNamespace());
            });
    }

    private void PopulateConfig(IConfigurationBuilder configurationBuilder)
    {
        Environment.SetEnvironmentVariable("ENDPOINT_NAME", "SFA.DAS.Funding.ApprenticeshipEarnings");

        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", true);

        var configuration = configurationBuilder.Build();
        if (NotAcceptanceTests(configuration))// May not need this check, Fail PR if this comment is still here
        {
            configurationBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });
        }

        Configuration = configurationBuilder.Build();
        EnsureConfig(ApplicationSettings);
    }

    public void SetupServices(IServiceCollection services)
    {

        services.AddLogging(builder =>
        {
            builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
            builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);

            builder.AddFilter(typeof(Program).Namespace, LogLevel.Information);
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddConsole();

        });

        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();

        services.AddEntityFrameworkForApprenticeships(ApplicationSettings, NotLocal(Configuration));
        services.AddCommandServices().AddEventServices().AddCommandDependencies();

        services.AddSingleton<ISystemClockService, SystemClockService>();
    }

    private static void EnsureConfig(ApplicationSettings applicationSettings)
    {
        if (string.IsNullOrWhiteSpace(applicationSettings.NServiceBusConnectionString))
            throw new Exception("NServiceBusConnectionString in ApplicationSettings should not be null.");
    }

    private static bool NotAcceptanceTests(IConfiguration configuration)
    {
        return !configuration!["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool NotLocal(IConfiguration configuration)
    {
        var env = configuration!["EnvironmentName"];
        var isLocal = env.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase);
        var isLocalAcceptanceTests = env.Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
        return !isLocal && !isLocalAcceptanceTests;
    }

    /// <summary>
    /// This method is used to force the assembly to load so that the NServiceBus assembly scanner can find the events.
    /// This has to be called before builder configuration steps are called as these don't get executed until build() is called.
    /// </summary>
    private static void ForceAssemblyLoad()
    {
        var apprenticeshipEarningsTypes = new ApprenticeshipEarningsRecalculatedEvent();
    }
}