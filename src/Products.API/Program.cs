using Serilog;
using Asp.Versioning;
using Products.Application;
using Products.Infrastructure;
using Products.API.Endpoints;
using Products.API.Endpoints.v1;
using Products.API.Endpoints.v2;

namespace Products.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
        builder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));

        // Get connection string
        var connectionString = builder.Configuration.GetConnectionString("ProductsDatabase")
            ?? throw new InvalidOperationException("Connection string 'ProductsDatabase' not found.");

        // Add services to the container
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(connectionString);
        
        // Add health checks
        builder.Services.AddHealthChecks()
            .AddSqlServer(
                connectionString: connectionString,
                name: "sql-server",
                timeout: TimeSpan.FromSeconds(5),
                tags: new[] { "db", "sql", "sqlserver" });
        
        builder.Services.AddExceptionHandler<Products.API.Middleware.GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        
        // Add validation
        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        
        builder.Services.AddAuthorization();

        // Configure API versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader());
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Enable automatic validation
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
        });

        // Configure OpenAPI/Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Products API",
                Version = "v1.0",
                Description = "Products API for managing product catalog"
            });
            options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Products API",
                Version = "v2.0",
                Description = "Products API for managing product catalog (v2)"
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline
        app.UseExceptionHandler();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Products API v1.0");
                options.SwaggerEndpoint("/swagger/v2/swagger.json", "Products API v2.0");
            });
        }

        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthorization();

        // Map health checks
        app.MapHealthChecks("/health");

        // Map endpoint groups
        app.MapHealthCheckEndpoints();
        
        var versionedApi = app.NewVersionedApi();
        versionedApi.MapProductsV1Endpoints();
        versionedApi.MapProductOptionsV1Endpoints();
        versionedApi.MapProductsV2Endpoints();
        versionedApi.MapProductOptionsV2Endpoints();

        app.Run();
    }
}

