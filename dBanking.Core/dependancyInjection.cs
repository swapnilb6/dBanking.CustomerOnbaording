using dBanking.Core.Repository_Contracts;
using dBanking.Core.ServiceContracts;
using dBanking.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace dBanking.Core;

public static class dependancyInjection
{
    // Extension method to add core services to the IServiceCollection
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Register core services here
        // e.g., services.AddTransient<IMyService, MyService>();
        // Services
        services.AddTransient<ICustomerService, CustomerService>();

        return services;
    }
}

