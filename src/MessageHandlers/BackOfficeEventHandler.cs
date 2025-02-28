using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReReleaseEarningsGeneratedCommand;
using System;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System.Net.Http;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class BackOfficeEventHandler
{
    private readonly ICommandDispatcher _commandDispatcher;

    public BackOfficeEventHandler(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [Function(nameof(ReReleaseEarningsGenerated))]
    public async Task<IActionResult> ReReleaseEarningsGenerated([HttpTrigger(AuthorizationLevel.Function, "post", Route = "BackOffice/ReReleaseEarningsGenerated/{ukprn}")] HttpRequestMessage req,
                long ukprn,
                ILogger log)
    {
        try
        {
            log.LogInformation($"{nameof(ReReleaseEarningsGenerated)} processing...");

            if (ukprn == 0)
            {
                var noPrnProvided = "ukprn has not been provided";
                log.LogError(noPrnProvided);
                return new BadRequestObjectResult(noPrnProvided);
            }

            log.LogInformation("UkPrn: {0}");

            await _commandDispatcher.Send(new ReReleaseEarningsGeneratedCommand(ukprn));
            var message = String.Format($"ReRelease Earnings for ukprn {ukprn} successful");
            return new OkObjectResult(message);
        }
        catch (Exception ex)
        {
            log.LogError(ex, $"{nameof(ReReleaseEarningsGenerated)} threw exception.");
            throw;
        }
    }
}