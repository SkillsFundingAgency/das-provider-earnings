using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Extensions;

public static class ConfigureNServiceBusExtension
{
    public static void ConfigureNServiceBusForSend(this IServiceCollection services, string fullyQualifiedNamespace)
    {
        var endpointConfiguration = new EndpointConfiguration("SFA.DAS.Funding.ApprenticeshipEarnings");
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.SendOnly();

        var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
        transport.CustomTokenCredential(fullyQualifiedNamespace, new DefaultAzureCredential());
        endpointConfiguration.Conventions().SetConventions();

        var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
        services.AddSingleton<IMessageSession>(endpointInstance);
    }
}
