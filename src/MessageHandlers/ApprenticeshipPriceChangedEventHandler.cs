using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class ApprenticeshipPriceChangedEventHandler
{
    private readonly ICommandHandler<ProcessEpisodeUpdatedCommand> _processEpisodeUpdatedCommandHandler;
    private readonly ILogger<ApprenticeshipPriceChangedEventHandler> _logger;

    public ApprenticeshipPriceChangedEventHandler(
        ICommandHandler<ProcessEpisodeUpdatedCommand> processEpisodeUpdatedCommandHandler,
        ILogger<ApprenticeshipPriceChangedEventHandler> logger)
    {
        _processEpisodeUpdatedCommandHandler = processEpisodeUpdatedCommandHandler;
        _logger = logger;
    }

    [Function(nameof(PriceChangeApprovedEventServiceBusTrigger))]
    public async Task PriceChangeApprovedEventServiceBusTrigger(
        [ServiceBusTrigger(QueueNames.PriceChangeApproved)] ApprenticeshipPriceChangedEvent apprenticeshipPriceChangedEvent)
    {
        _logger.LogInformation($"{nameof(PriceChangeApprovedEventServiceBusTrigger)} processing...");
        _logger.LogInformation("ApprenticeshipKey: {key} Received {eventName}: {eventJson}",
            apprenticeshipPriceChangedEvent.ApprenticeshipKey,
            nameof(ApprenticeshipPriceChangedEvent),
            JsonSerializer.Serialize(apprenticeshipPriceChangedEvent, new JsonSerializerOptions { WriteIndented = true }));

        await _processEpisodeUpdatedCommandHandler.Handle(new ProcessEpisodeUpdatedCommand(apprenticeshipPriceChangedEvent));
    }
}