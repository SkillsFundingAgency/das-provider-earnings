using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class StartDateChangedEventHandler
{
    private readonly ICommandHandler<ProcessEpisodeUpdatedCommand> _processEpisodeUpdatedCommandHandler;
    private readonly ILogger<StartDateChangedEventHandler> _logger;

    public StartDateChangedEventHandler(
        ICommandHandler<ProcessEpisodeUpdatedCommand> processEpisodeUpdatedCommandHandler,
        ILogger<StartDateChangedEventHandler> logger)
    {
        _processEpisodeUpdatedCommandHandler = processEpisodeUpdatedCommandHandler;
        _logger = logger;
    }

    [Function(nameof(StartDateChangedEventServiceBusTrigger))]
    public async Task StartDateChangedEventServiceBusTrigger(
        [ServiceBusTrigger(QueueNames.StartDateChangeApproved)] ApprenticeshipStartDateChangedEvent startDateChangedEvent)
    {
        _logger.LogInformation("{functionName} processing...", nameof(StartDateChangedEventServiceBusTrigger));
        _logger.LogInformation("ApprenticeshipKey: {key} Received {eventName}: {eventJson}",
            startDateChangedEvent.ApprenticeshipKey,
            nameof(ApprenticeshipStartDateChangedEvent),
            JsonSerializer.Serialize(startDateChangedEvent, new JsonSerializerOptions { WriteIndented = true }));

        await _processEpisodeUpdatedCommandHandler.Handle(new ProcessEpisodeUpdatedCommand(startDateChangedEvent));
    }
}