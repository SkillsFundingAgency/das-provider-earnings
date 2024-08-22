using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Text.Json;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;

public class ApprenticeshipPriceChangedEventHandler
{
    private readonly IProcessEpisodeUpdatedCommandHandler _processEpisodeUpdatedCommandHandler;

    public ApprenticeshipPriceChangedEventHandler(IProcessEpisodeUpdatedCommandHandler processEpisodeUpdatedCommandHandler)
    {
        _processEpisodeUpdatedCommandHandler = processEpisodeUpdatedCommandHandler;
    }

    [FunctionName(nameof(PriceChangeApprovedEventServiceBusTrigger))]
    public async Task PriceChangeApprovedEventServiceBusTrigger(
        [NServiceBusTrigger(Endpoint = QueueNames.PriceChangeApproved)] ApprenticeshipPriceChangedEvent apprenticeshipPriceChangedEvent,
        [DurableClient] IDurableEntityClient client,
        ILogger log)
    {
        log.LogInformation($"{nameof(PriceChangeApprovedEventServiceBusTrigger)} processing...");
        log.LogInformation("ApprenticeshipKey: {key} Received {eventName}: {eventJson}",
            apprenticeshipPriceChangedEvent.ApprenticeshipKey,
            nameof(ApprenticeshipPriceChangedEvent),
            JsonSerializer.Serialize(apprenticeshipPriceChangedEvent, new JsonSerializerOptions { WriteIndented = true }));

        await _processEpisodeUpdatedCommandHandler.RecalculateEarnings(new ProcessEpisodeUpdatedCommand(apprenticeshipPriceChangedEvent));
    }
}