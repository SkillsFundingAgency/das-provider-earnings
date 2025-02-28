using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

internal class TestStartup
{
    private readonly Startup _startUp;
    private readonly IEnumerable<QueueTriggeredFunction> _queueTriggeredFunctions;
    private readonly IMessageSession _messageSession;

    public TestStartup(
        TestContext testContext, 
        IEnumerable<QueueTriggeredFunction> queueTriggeredFunctions,
        IMessageSession messageSession)
    {
        _startUp = new Startup();
        _startUp.Configuration = GenerateConfiguration(testContext);
        _queueTriggeredFunctions = queueTriggeredFunctions;
        _messageSession = messageSession;
    }

    public void Configure()
    {
        // Intentionally left blank
    }

    public void ConfigureServices(IServiceCollection collection)
    {
        _startUp.SetupServices(collection);

        collection.AddSingleton<IMessageSession>(_messageSession);

        foreach (var queueTriggeredFunction in _queueTriggeredFunctions)
        {
            collection.AddTransient(queueTriggeredFunction.ClassType);
        }

        collection.AddSingleton<ISystemClockService, TestSystemClock>();// override DI in Startup
    }

    private static IConfigurationRoot GenerateConfiguration(TestContext testContext)
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new[]
            {
                new KeyValuePair<string, string>("EnvironmentName", "LOCAL_ACCEPTANCE_TESTS"),
                new KeyValuePair<string, string>("AzureWebJobsStorage", "UseDevelopmentStorage=true"),
                new KeyValuePair<string, string>("AzureWebJobsServiceBus", "UseDevelopmentStorage=true"),
                new KeyValuePair<string, string>("ApplicationSettings:NServiceBusConnectionString", "UseLearningEndpoint=true"),
                new KeyValuePair<string, string>("ApplicationSettings:DbConnectionString", testContext.SqlDatabase?.DatabaseInfo.ConnectionString!)
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);
        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}
