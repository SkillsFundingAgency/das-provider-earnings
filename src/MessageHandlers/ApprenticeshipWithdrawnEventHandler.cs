using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class ApprenticeshipWithdrawnEventHandler
{
    private readonly ICommandHandler<ProcessWithdrawnApprenticeshipCommand> _processWithdrawnApprenticeshipCommandHandler;
    private readonly ILogger<ApprenticeshipWithdrawnEventHandler> _logger;

    public ApprenticeshipWithdrawnEventHandler(
        ICommandHandler<ProcessWithdrawnApprenticeshipCommand> processWithdrawnApprenticeshipCommandHandler,
        ILogger<ApprenticeshipWithdrawnEventHandler> logger)
    {
        _processWithdrawnApprenticeshipCommandHandler = processWithdrawnApprenticeshipCommandHandler;
        _logger = logger;
    }

    [Function(nameof(ApprenticeshipWithdrawnEventServiceBusTrigger))]
    public async Task ApprenticeshipWithdrawnEventServiceBusTrigger(
        [ServiceBusTrigger(QueueNames.ApprenticeshipWithdrawn)] ApprenticeshipWithdrawnEvent apprenticeshipWithdrawnEvent)
    {
        _logger.LogInformation($"{nameof(ApprenticeshipWithdrawnEventServiceBusTrigger)} processing...");
        _logger.LogInformation("ApprenticeshipKey: {key} Received {eventName}: {eventJson}",
            apprenticeshipWithdrawnEvent.ApprenticeshipKey,
            nameof(ApprenticeshipWithdrawnEvent),
            JsonSerializer.Serialize(apprenticeshipWithdrawnEvent, new JsonSerializerOptions { WriteIndented = true }));

        await _processWithdrawnApprenticeshipCommandHandler.Handle(new ProcessWithdrawnApprenticeshipCommand(apprenticeshipWithdrawnEvent));
    }
}