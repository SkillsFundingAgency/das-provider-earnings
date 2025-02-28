using AutoFixture;
using Moq;
using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ProcessWithdrawnApprenticeshipsCommandHandler;

[TestFixture]
public class WhenProcessWithdrawnApprenticeshipCommandHandled
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IMessageSession> _mockMessageSession = new();
    private readonly Mock<IApprenticeshipEarningsRecalculatedEventBuilder> _mockEventBuilder = new();
    private readonly Mock<ISystemClockService> _mockSystemClock = new();
    private readonly Mock<IApprenticeshipRepository> _mockRepository = new();

    private void SetupMocks()
    {
        _mockMessageSession.Reset();
        _mockEventBuilder.Reset();
        _mockRepository.Reset();

        _mockEventBuilder.Setup(x => x.Build(It.IsAny<Apprenticeship>()))
            .Returns(new ApprenticeshipEarningsRecalculatedEvent());
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 1));
    }

    [Test]
    public async Task ThenTheApprenticeshipIsWithdrawnAndEventIsPublished()
    {
        // Arrange
        var apprenticeshipModel = _fixture.Create<ApprenticeshipModel>();
        var apprenticeship = Apprenticeship.Get(apprenticeshipModel);
        SetupMocks();
        var command = BuildCommand(apprenticeship);

        _mockRepository
            .Setup(x => x.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        var sut = new ProcessWithdrawnApprenticeshipCommandHandler(
            _mockRepository.Object,
            _mockMessageSession.Object,
            _mockEventBuilder.Object,
            _mockSystemClock.Object
        );

        // Act
        await sut.Handle(command);

        // Assert
        _mockRepository.Verify(x => x.Get(command.ApprenticeshipKey), Times.Once);
        _mockEventBuilder.Verify(x => x.Build(It.IsAny<Apprenticeship>()), Times.Once);
        _mockMessageSession.Verify(x => x.Publish(It.IsAny<ApprenticeshipEarningsRecalculatedEvent>(), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.Update(It.IsAny<Apprenticeship>()), Times.Once);
    }

    private ProcessWithdrawnApprenticeshipCommand.ProcessWithdrawnApprenticeshipCommand BuildCommand(Apprenticeship apprenticeship)
    {
        var apprenticeshipWithdrawnEvent = new ApprenticeshipWithdrawnEvent
        {
            ApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            Reason = _fixture.Create<string>(),
            LastDayOfLearning = new DateTime(2024, 11, 30)
        };

        return new ProcessWithdrawnApprenticeshipCommand.ProcessWithdrawnApprenticeshipCommand(apprenticeshipWithdrawnEvent);
    }
}