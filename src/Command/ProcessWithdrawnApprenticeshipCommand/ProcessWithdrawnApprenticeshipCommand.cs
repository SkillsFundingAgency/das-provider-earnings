using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

public class ProcessWithdrawnApprenticeshipCommand : ICommand
{
    public ProcessWithdrawnApprenticeshipCommand(ApprenticeshipWithdrawnEvent apprenticeshipWithdrawnEvent)
    {
        ApprenticeshipKey = apprenticeshipWithdrawnEvent.ApprenticeshipKey;
        ApprenticeshipId = apprenticeshipWithdrawnEvent.ApprenticeshipId;
        Reason = apprenticeshipWithdrawnEvent.Reason;
        LastDayOfLearning = apprenticeshipWithdrawnEvent.LastDayOfLearning;
    }

    public Guid ApprenticeshipKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public string Reason { get; set; }
    public DateTime LastDayOfLearning { get; set; }
}