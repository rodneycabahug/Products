using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Products.API.Tests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public IntegrationTestWebAppFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Find and remove the existing IConnectionManager registration
            var connectionManagerDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(Products.Infrastructure.Data.IConnectionManager));
            if (connectionManagerDescriptor != null)
            {
                services.Remove(connectionManagerDescriptor);
            }

            // Register a new IConnectionManager with the test connection string
            services.AddSingleton<Products.Infrastructure.Data.IConnectionManager>(sp =>
                new Products.Infrastructure.Data.ConnectionManager(_connectionString));
        });
    }
}
