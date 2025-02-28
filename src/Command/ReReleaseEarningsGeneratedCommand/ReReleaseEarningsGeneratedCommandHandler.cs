using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReReleaseEarningsGeneratedCommand;

public class ReReleaseEarningsGeneratedCommandHandler : ICommandHandler<ReReleaseEarningsGeneratedCommand>
{
    private ILogger<ReReleaseEarningsGeneratedCommandHandler> _logger;
    private readonly IEarningsQueryRepository _earningsQueryRepository;
    private readonly IEarningsGeneratedEventBuilder _earningsGeneratedEventBuilder;
    private readonly IMessageSession _messageSession;

    public ReReleaseEarningsGeneratedCommandHandler(
        ILogger<ReReleaseEarningsGeneratedCommandHandler> logger,
        IEarningsQueryRepository earningsQueryRepository,
        IEarningsGeneratedEventBuilder earningsGeneratedEventBuilder,
        IMessageSession messageSession
        )
    {
        _logger = logger;
        _earningsQueryRepository = earningsQueryRepository;
        _earningsGeneratedEventBuilder = earningsGeneratedEventBuilder;
        _messageSession = messageSession;
    }

    public async Task Handle(ReReleaseEarningsGeneratedCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling ReReleaseEarningsGeneratedCommand for Ukprn: {ukprn}", command.Ukprn);

        var domainApprenticeships = _earningsQueryRepository.GetApprenticeships(command.Ukprn);

        if (domainApprenticeships == null || !domainApprenticeships.Any())
        {
            _logger.LogInformation("No apprenticeships found for Ukprn: {ukprn}", command.Ukprn);
            return;
        }

        foreach (var domainApprenticeship in domainApprenticeships)
        {
            var eventMessage = _earningsGeneratedEventBuilder.ReGenerate(domainApprenticeship);
            await _messageSession.Publish(eventMessage);
            _logger.LogInformation("Re-released earnings for apprenticeship: {apprenticeshipKey}", eventMessage.ApprenticeshipKey);
        }

        _logger.LogInformation("Re-released earnings for Ukprn: {ukprn} completed", command.Ukprn);
    }
}
