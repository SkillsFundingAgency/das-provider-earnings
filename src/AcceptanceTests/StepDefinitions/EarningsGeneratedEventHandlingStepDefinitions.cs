using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class EarningsGeneratedEventHandlingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public EarningsGeneratedEventHandlingStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Then(@"Earnings are generated with the correct learning amounts")]
    public async Task AssertEarningsGeneratedEvent()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(EventMatchesExpectation), "Failed to find published EarningsGenerated event");
    }

    [Then(@"Earnings are not generated for that apprenticeship")]
    public async Task AssertNoEarningsGeneratedEvent()
    {
        await WaitHelper.WaitForUnexpected(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => x.Uln == _scenarioContext[ContextKeys.ExpectedUln].ToString()), "Found published EarningsGenerated event when expecting no earnings to be generated", TimeSpan.FromSeconds(10));
    }

    [Then(@"the funding line type 16-18 must be used in the calculation")]
    public async Task ThenThe16To18FundingLineTypeIsUsed()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => EventMatchesExpectation(x, "16-18 Apprenticeship (Employer on App Service)")), "Failed to find published EarningsGenerated event");
    }

    [Then(@"the funding line type 19 plus must be used in the calculation")]
    public async Task ThenThe19AndOverFundingLineTypeIsUsed()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => EventMatchesExpectation(x, "19+ Apprenticeship (Employer on App Service)")), "Failed to find published EarningsGenerated event");
    }

    private bool EventMatchesExpectation(EarningsGeneratedEvent earningsGeneratedEvent)
    {
        return earningsGeneratedEvent.DeliveryPeriods.Count == (int)_scenarioContext[ContextKeys.ExpectedDeliveryPeriodCount]
            && earningsGeneratedEvent.DeliveryPeriods.All(x => x.LearningAmount == (int)_scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount]
            && earningsGeneratedEvent.Uln == _scenarioContext[ContextKeys.ExpectedUln].ToString());
    }

    private bool EventMatchesExpectation(EarningsGeneratedEvent earningsGeneratedEvent, string expectedFundingLineType)
    {
        return earningsGeneratedEvent.DeliveryPeriods.All(y => y.FundingLineType == expectedFundingLineType) &&
               earningsGeneratedEvent.Uln == _scenarioContext[ContextKeys.ExpectedUln].ToString() &&
               earningsGeneratedEvent.EarningsProfileId != Guid.Empty;
    }
}