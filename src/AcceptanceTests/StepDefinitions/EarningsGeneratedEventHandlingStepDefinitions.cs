using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Acceptance.Handlers;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Acceptance.StepDefinitions;

[Binding]
public class EarningsGeneratedEventHandlingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private static IEndpointInstance _endpointInstance;

    public EarningsGeneratedEventHandlingStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static async Task StartEndpoint()
    {
        var endpointConfiguration = new EndpointConfiguration(QueueNames.EarningsGenerated);
        endpointConfiguration.AssemblyScanner().ThrowExceptions = false;
        endpointConfiguration.UseNewtonsoftJsonSerializer();

        var transport = endpointConfiguration.UseTransport<LearningTransport>();
        transport.StorageDirectory(Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\.learningtransport"));

        _endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
    }

    [AfterTestRun]
    public static async Task StopEndpoint()
    {
        await _endpointInstance.Stop()
            .ConfigureAwait(false);
    }

    [Then(@"An earnings generated event is published with the correct learning amounts")]
    public async Task AssertEarningsGeneratedEvent()
    {
        await WaitHelper.WaitForIt(() => EarningsGeneratedEventHandler.ReceivedEvents.Any(EventMatchesExpectation), "Failed to find published EarningsGenerated event");
    }

    private bool EventMatchesExpectation(EarningsGeneratedEvent earningsGeneratedEvent)
    {
        return earningsGeneratedEvent.FundingPeriods.Count == 1 
               && earningsGeneratedEvent.FundingPeriods.First().DeliveryPeriods.Count == (int)_scenarioContext["expectedDeliveryPeriodCount"]
               && earningsGeneratedEvent.FundingPeriods.First().DeliveryPeriods.All(x => x.LearningAmount == (int)_scenarioContext["expectedDeliveryPeriodLearningAmount"]);
    }
}