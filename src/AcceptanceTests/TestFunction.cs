using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Testing.AzureStorageEmulator;
using System;
using System.Linq;
using System.Net;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

public class Settings
{
    public string AzureWebJobsStorage { get; set; }
    public string NServiceBusConnectionString { get; set; }
    public string TopicPath { get; set; }
    public string QueueName { get; set; }
}

public class TestFunction : IDisposable
{
    private readonly TestContext _testContext;
    private readonly TestServer _testServer;
    private readonly IEnumerable<QueueTriggeredFunction> _queueTriggeredFunctions;
    private bool _isDisposed;

    public string HubName { get; }

    public TestFunction(TestContext testContext, string hubName)
    {
        AzureStorageEmulatorManager.StartStorageEmulator();
        

        HubName = hubName;
        _testContext = testContext;
        var _ = new Startup();// This forces the AzureFunction assembly to load
        _queueTriggeredFunctions = QueueFunctionResolver.GetQueueTriggeredFunctions();

        _testServer = new TestServer(new WebHostBuilder()
            .UseEnvironment(Environments.Development)
            .UseStartup<TestStartup>((_) => new TestStartup(testContext, _queueTriggeredFunctions, _testContext.MessageSession)));

    }

    public async Task PublishEvent<T>(T eventObject)
    {
        var function = _queueTriggeredFunctions.FirstOrDefault(x => x.Endpoints.Where(e => e.EventType == typeof(T)).Any());
        var handler = _testServer.Services.GetService(function.ClassType);
        var method = function.Endpoints.FirstOrDefault(x => x.EventType == typeof(T)).MethodInfo;

        if (method.GetParameters().Length != 1)
        {
            throw new InvalidOperationException("To trigger events for functions with multiple parameters more development is required");
        }
        try
        {
            await (Task)method.Invoke(handler, new object[] { eventObject });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to invoke method {method.Name} on class {function.ClassType.Name}", ex);
        }
    }

    public async Task DisposeAsync()
    {
        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            // no components to dispose
        }

        _isDisposed = true;
    }

}