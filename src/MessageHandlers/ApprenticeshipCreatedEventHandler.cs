using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class ApprenticeshipCreatedEventHandler
{
    private readonly ICommandHandler<CreateApprenticeshipCommand, Apprenticeship> _createApprenticeshipCommandHandler;
    private readonly ILogger<ApprenticeshipCreatedEventHandler> _logger;

    public ApprenticeshipCreatedEventHandler(
        ICommandHandler<CreateApprenticeshipCommand, Apprenticeship> createApprenticeshipCommandHandler,
        ILogger<ApprenticeshipCreatedEventHandler> logger)
    {
        _createApprenticeshipCommandHandler = createApprenticeshipCommandHandler;
        _logger = logger;
    }

    [Function(nameof(ApprenticeshipLearnerEventServiceBusTrigger))]
    public async Task ApprenticeshipLearnerEventServiceBusTrigger(
        [ServiceBusTrigger(QueueNames.ApprovalCreated)] ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        try
        {
            _logger.LogInformation($"{nameof(ApprenticeshipLearnerEventServiceBusTrigger)} processing...");

            _logger.LogInformation("ApprenticeshipKey: {0} Received ApprenticeshipCreatedEvent: {1}",
                apprenticeshipCreatedEvent.ApprenticeshipKey,
                JsonSerializer.Serialize(apprenticeshipCreatedEvent, new JsonSerializerOptions { WriteIndented = true }));

            if (!(apprenticeshipCreatedEvent.Episode.FundingPlatform.HasValue && Enum.Parse<FundingPlatform>(apprenticeshipCreatedEvent.Episode.FundingPlatform.Value.ToString()) == FundingPlatform.DAS))
            {
                _logger.LogInformation($"{nameof(ApprenticeshipLearnerEventServiceBusTrigger)} - Not generating earnings for non pilot apprenticeship with ApprenticeshipKey = {apprenticeshipCreatedEvent.ApprenticeshipKey}");
                return;
            }

            var command = new CreateApprenticeshipCommand(apprenticeshipCreatedEvent);
            await _createApprenticeshipCommandHandler.Handle(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(CreateApprenticeshipCommand)} threw exception.");
            throw;
        }
    }
}