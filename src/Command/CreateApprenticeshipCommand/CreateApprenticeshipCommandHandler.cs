﻿using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand
{
    public class CreateApprenticeshipCommandHandler : ICreateApprenticeshipCommandHandler
    {
        private readonly IApprenticeshipFactory _apprenticeshipFactory;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IMessageSession _messageSession;
        private readonly IEarningsGeneratedEventBuilder _earningsGeneratedEventBuilder; 
        private readonly ISystemClockService _systemClock;

        public CreateApprenticeshipCommandHandler(IApprenticeshipFactory apprenticeshipFactory, IApprenticeshipRepository apprenticeshipRepository, IMessageSession messageSession, IEarningsGeneratedEventBuilder earningsGeneratedEventBuilder, ISystemClockService systemClock)
        {
            _apprenticeshipFactory = apprenticeshipFactory;
            _apprenticeshipRepository = apprenticeshipRepository;
            _messageSession = messageSession;
            _earningsGeneratedEventBuilder = earningsGeneratedEventBuilder;
            _systemClock = systemClock;
        }

        public async Task<Apprenticeship> Create(CreateApprenticeshipCommand command)
        {
            var apprenticeship = _apprenticeshipFactory.CreateNew(command.ApprenticeshipCreatedEvent);
            apprenticeship.CalculateEarnings(_systemClock);
            await _apprenticeshipRepository.Add(apprenticeship);
            await _messageSession.Publish(_earningsGeneratedEventBuilder.Build(apprenticeship));
            return apprenticeship;
        }
    }
}
