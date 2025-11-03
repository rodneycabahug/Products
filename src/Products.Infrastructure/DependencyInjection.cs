using Microsoft.Extensions.DependencyInjection;
using Products.Domain.Repositories;
using Products.Infrastructure.Data;
using Products.Infrastructure.Repositories;

namespace Products.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IConnectionManager>(new ConnectionManager(connectionString));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductOptionRepository, ProductOptionRepository>();

        return services;
    }
}
