using Azure;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Diagnostics;
using Microsoft.AspNetCore.TestHost;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Azure;
using Google.Protobuf.WellKnownTypes;
using SFA.DAS.Apprenticeships.Types;
using Castle.Components.DictionaryAdapter.Xml;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class HostingStepDefinitions
{
    private readonly TestContext _testContext;
    private readonly FeatureContext _featureContext;
    private static Startup _startUp;

    public HostingStepDefinitions(TestContext testContext, FeatureContext featureContext)
    {
        _testContext = testContext;
        _featureContext = featureContext;
    }

    [BeforeScenario(Order = 3)]
    public async Task StartFunctionApp()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _testContext.TestFunction = new TestFunction(_testContext, $"TEST{_featureContext.FeatureInfo.Title.Replace(" ", "")}");
        stopwatch.Stop();
        Console.WriteLine($"Time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
    }

    [AfterScenario(Order = 101)]
    public async Task CleanupAfterTestHarness()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await _testContext.TestFunction.DisposeAsync();
        stopwatch.Stop();
        Console.WriteLine($"Time it took to Cleanup  FunctionsHost: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
    }
}