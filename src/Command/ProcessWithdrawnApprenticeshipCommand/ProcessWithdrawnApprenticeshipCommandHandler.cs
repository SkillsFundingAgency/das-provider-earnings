using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

public class ProcessWithdrawnApprenticeshipCommandHandler : ICommandHandler<ProcessWithdrawnApprenticeshipCommand>
{
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;
    private readonly ISystemClockService _systemClock;

    public ProcessWithdrawnApprenticeshipCommandHandler(IApprenticeshipRepository apprenticeshipRepository, IMessageSession messageSession, IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder, ISystemClockService systemClock)
    {
        _apprenticeshipRepository = apprenticeshipRepository;
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
        _systemClock = systemClock;
    }

    public async Task Handle(ProcessWithdrawnApprenticeshipCommand command, CancellationToken cancellationToken = default)
    {
        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.RemovalEarningsFollowingWithdrawal(command.LastDayOfLearning, _systemClock);

        await _messageSession.Publish(_eventBuilder.Build(apprenticeshipDomainModel));

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);
    }
}