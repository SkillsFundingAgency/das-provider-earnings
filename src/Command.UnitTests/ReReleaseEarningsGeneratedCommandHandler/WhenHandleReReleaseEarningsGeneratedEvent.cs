using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprenticeship = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ReReleaseEarningsGeneratedCommandHandler;

[TestFixture]
public class WhenHandleReReleaseEarningsGeneratedEvent
{
    private Fixture _fixture;
    private Mock<ILogger<ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommandHandler>> _loggerMock;
    private Mock<IEarningsQueryRepository> _earningsQueryRepositoryMock;
    private Mock<IEarningsGeneratedEventBuilder> _earningsGeneratedEventBuilderMock;
    private Mock<IMessageSession> _messageSessionMock;
    private ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _loggerMock = new Mock<ILogger<ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommandHandler>>();
        _earningsQueryRepositoryMock = new Mock<IEarningsQueryRepository>();
        _earningsGeneratedEventBuilderMock = new Mock<IEarningsGeneratedEventBuilder>();
        _messageSessionMock = new Mock<IMessageSession>();

        _handler = new ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommandHandler(
            _loggerMock.Object,
            _earningsQueryRepositoryMock.Object,
            _earningsGeneratedEventBuilderMock.Object,
            _messageSessionMock.Object);
    }

    [Test]
    public async Task Handle_ShouldNotPublishWhenNoApprenticeshipFound()
    {
        // Arrange
        var command = new ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommand(12345);
        _earningsQueryRepositoryMock.Setup(x => x.GetApprenticeships(command.Ukprn)).Returns((List<Apprenticeship>)null);

        // Act
        await _handler.Handle(command);

        // Assert
        _messageSessionMock.Verify(x => x.Publish(It.IsAny<EarningsGeneratedEvent>(), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ShouldPublishEvent_ForEachApprenticeship()
    {
        // Arrange
        var command = new ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommand(12345);
        var apprenticeships = _fixture.Create<List<Apprenticeship>>();
        var eventMessage = new EarningsGeneratedEvent();
        _earningsQueryRepositoryMock.Setup(x => x.GetApprenticeships(command.Ukprn)).Returns(apprenticeships);
        _earningsGeneratedEventBuilderMock.Setup(x => x.ReGenerate(It.IsAny<Apprenticeship>())).Returns(eventMessage);

        // Act
        await _handler.Handle(command);

        // Assert
        _messageSessionMock.Verify(x => x.Publish(eventMessage, It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(apprenticeships.Count));
    }
}
