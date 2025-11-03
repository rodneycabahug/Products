# .NET Framework 4.5.2 to .NET 9 Migration Plan

## Executive Summary

This document outlines the comprehensive migration strategy for upgrading the Products API from .NET Framework 4.5.2 (ASP.NET Web API) to .NET 9 (ASP.NET Core). This is a major architectural shift that requires replacing legacy frameworks, tools, and patterns with modern .NET equivalents.

**Current State**: .NET Framework 4.5.2 with ASP.NET Web API  
**Target State**: .NET 9 with ASP.NET Core  
**Migration Type**: Full replatforming (not in-place upgrade)  

---

## Current Architecture Analysis

### Technology Stack Inventory

#### Web Framework
- **Current**: ASP.NET Web API 5.2.3 (System.Web-based)
- **Target**: ASP.NET Core Minimal APIs with file-per-endpoint pattern (.NET 9)
- **Impact**: Complete framework replacement, move from controllers to minimal API endpoints

#### Hosting Model
- **Current**: IIS/IIS Express with System.Web
- **Target**: Kestrel standalone web server
- **Impact**: Global.asax → Program.cs patterns, self-hosted application

#### Dependency Injection
- **Current**: Autofac 4.6.0 with Autofac.WebApi2
- **Target**: Built-in Microsoft.Extensions.DependencyInjection
- **Impact**: Container registration syntax changes, remove Autofac dependency

#### Object Mapping
- **Current**: AutoMapper 6.1.1 (legacy static API)
- **Target**: Manual mapping with extension methods
- **Impact**: Remove AutoMapper dependency, implement explicit mapping logic

#### Logging
- **Current**: NLog 5.0.0-beta09 with custom configuration
- **Target**: Serilog with Microsoft.Extensions.Logging integration
- **Impact**: Replace NLog with Serilog, modernize logging configuration

#### Configuration
- **Current**: Web.config (XML-based)
- **Target**: appsettings.json with configuration providers
- **Impact**: Complete configuration restructuring

#### Data Access
- **Current**: ADO.NET with SqlCommand and stored procedures
- **Target**: ADO.NET with SqlCommand and stored procedures (retained)
- **Impact**: Minimal changes, update connection management and async patterns

#### API Versioning
- **Current**: Microsoft.AspNet.WebApi.Versioning 2.1.0 (URL path based)
- **Target**: Asp.Versioning.Http 8.x with URL path versioning (/api/v1/, /api/v2/)
- **Impact**: Minimal changes, maintain existing URL structure

#### Testing
- **Current**: MSTest with Moq
- **Target**: TUnit (modern source-generated testing framework) with Moq
- **Impact**: Test framework migration, leverage source generation for performance

#### Project System
- **Current**: Legacy .csproj (XML verbose format)
- **Target**: SDK-style .csproj (standard minimal format with automatic file inclusion)
- **Impact**: Complete project file restructuring, simplified package management

---

## Migration Strategy

### Approach: Parallel Development with Feature Parity

We will follow a **ground-up rebuild** approach rather than incremental migration due to the fundamental architectural differences between ASP.NET Web API and ASP.NET Core.

### Phases

#### Phase 1: Foundation Setup
- Create new .NET 9 solution structure
- Configure SDK-style projects
- Establish baseline project dependencies
- Set up modern build pipeline

#### Phase 2: Core Infrastructure
- Implement dependency injection container
- Configure logging infrastructure
- Set up configuration management
- Implement middleware pipeline

#### Phase 3: Data Layer Modernization
- Migrate Entity models
- Modernize Repository layer with improved ADO.NET patterns
- Update connection management with async/await patterns
- Add CancellationToken support throughout

#### Phase 4: Business Layer Migration
- Migrate Service layer interfaces
- Implement service logic with modern patterns
- Implement manual mapping logic with extension methods
- Update exception handling

#### Phase 5: API Layer Implementation
- Create minimal API endpoints with file-per-endpoint pattern
- Implement API versioning (URL path based)
- Configure routing and OpenAPI
- Implement request/response DTOs

#### Phase 6: Cross-Cutting Concerns
- Implement global exception handling
- Configure CORS policies
- Set up authentication/authorization framework
- Implement filters and middleware

#### Phase 7: Testing Infrastructure
- Set up TUnit test framework
- Migrate existing tests to TUnit
- Implement integration tests with WebApplicationFactory
- Perform end-to-end testing with .http files

#### Phase 8: Deployment Preparation
- Configure production settings
- Set up logging and monitoring
- Performance testing
- Security hardening

---

## Key Architectural Changes

### 1. Project Structure Transformation

**Before (Framework)**:
```
Products.sln
├── Products.API (Web Application)
├── Products.Service (Class Library)
├── Products.Repository (Class Library)
├── Products.Entity (Class Library)
├── Products.Model (Class Library)
└── Products.Test (MSTest)
```

**After (.NET 9)**:
```
Products.sln
├── src/
│   ├── Products.API (ASP.NET Core Web API)
│   ├── Products.Application (Business Logic)
│   ├── Products.Domain (Entities & Interfaces)
│   └── Products.Infrastructure (Data Access)
├── tests/
│   ├── Products.API.Tests (Integration Tests)
│   ├── Products.Application.Tests (Unit Tests)
│   └── Products.Infrastructure.Tests (Unit Tests)
└── .http/ (API Test Files)
```

### 2. Dependency Injection Pattern

**Before**:
```csharp
// Autofac with static container
public static class AutofacConfig
{
    public static IContainer Container;
    public static void Initialize(HttpConfiguration config)
    {
        var builder = new ContainerBuilder();
        builder.RegisterApiControllers(typeof(ProductsController).Assembly);
        Container = builder.Build();
        config.DependencyResolver = new AutofacWebApiDependencyResolver(Container);
    }
}
```

**After**:
```csharp
// Built-in DI with extension methods
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductOptionService, ProductOptionService>();
        return services;
    }
    
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductOptionRepository, ProductOptionRepository>();
        services.AddScoped<IConnectionManager, ConnectionManager>();
        return services;
    }
}
```

### 3. Configuration Management

**Before (Web.config)**:
```xml
<appSettings>
    <add key="Versioning.Default" value="1.0" />
</appSettings>
<connectionStrings>
    <add name="ProductsDB" connectionString="..." />
</connectionStrings>
```

**After (appsettings.json)**:
```json
{
  "ApiVersioning": {
    "DefaultVersion": "1.0",
    "ParameterName": "api-version"
  },
  "ConnectionStrings": {
    "ProductsDB": "..."
  }
}
```

### 4. Application Startup

**Before (Global.asax.cs)**:
```csharp
public class WebApiApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        GlobalConfiguration.Configure(WebApiConfig.Register);
        GlobalConfiguration.Configure(AutofacConfig.Initialize);
        AutoMapperConfig.Configure();
    }
}
```

**After (Program.cs)**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Configure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

// Map minimal API endpoints
app.MapProductsEndpoints();
app.MapProductOptionsEndpoints();

app.Run();
```

### 5. Minimal API Endpoint Pattern

**Before**:
```csharp
[ApiVersion("1.0")]
[RoutePrefix("products")]
public class ProductsController : ApiController
{
    private readonly IProductService _productService;
    
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    [Route("")]
    [HttpGet]
    public async Task<IHttpActionResult> RetrieveProducts()
    {
        var products = await _productService.RetrieveAsync();
        return Ok(products);
    }
}
```

**After (Endpoints/v1/ProductsEndpoints.cs)**:
```csharp
public static class ProductsEndpointsV1
{
    public static RouteGroupBuilder MapProductsV1Endpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/products")
            .WithTags("Products v1.0")
            .WithOpenApi();

        group.MapGet("", GetAllProducts)
            .Produces<CollectionDTO<ProductDTO>>(StatusCodes.Status200OK);

        group.MapGet("{id:guid}", GetProductById)
            .Produces<ProductDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateProduct)
            .Produces<ProductDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("{id:guid}", UpdateProduct)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("{id:guid}", DeleteProduct)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAllProducts(
        IProductService productService,
        [FromQuery] string? name = null)
    {
        var products = string.IsNullOrEmpty(name)
            ? await productService.RetrieveAsync()
            : await productService.RetrieveByNameAsync(name);

        return Results.Ok(products);
    }

    private static async Task<IResult> GetProductById(
        Guid id,
        IProductService productService)
    {
        var product = await productService.RetrieveByIdAsync(id);
        return product != null
            ? Results.Ok(product)
            : Results.NotFound();
    }

    // Additional endpoint implementations...
}
```

### 6. Data Access Pattern (Retained ADO.NET)

**Before**:
```csharp
public async Task<ProductEntity> RetrieveByIdAsync(Guid id)
{
    using (var connection = await _connectionManager.GetConnectionAsync())
    {
        var command = new SqlCommand("RetrieveProductById", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@Id", id);
        
        using (var reader = await command.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                return new ProductEntity
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    // ...
                };
            }
        }
    }
    return null;
}
```

**After (Modernized ADO.NET)**:
```csharp
public async Task<ProductEntity?> RetrieveByIdAsync(Guid id, CancellationToken cancellationToken = default)
{
    await using var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
    await using var command = new SqlCommand("RetrieveProductById", connection)
    {
        CommandType = CommandType.StoredProcedure
    };
    
    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = id });
    
    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    
    if (await reader.ReadAsync(cancellationToken))
    {
        return new ProductEntity
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                ? null 
                : reader.GetString(reader.GetOrdinal("Description")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            DeliveryPrice = reader.GetDecimal(reader.GetOrdinal("DeliveryPrice"))
        };
    }
    
    return null;
}
```

**Key Improvements**:
- Use `await using` for proper async disposal
- Named parameters with explicit SqlDbType
- Column name-based ordinals for maintainability
- Nullable reference types support
- CancellationToken support

---

## Package Migration Mapping

| Framework Package | Version | .NET 9 Equivalent | Version | Notes |
|------------------|---------|-------------------|---------|-------|
| Microsoft.AspNet.WebApi.Core | 5.2.3 | - | Built-in | Part of ASP.NET Core |
| Microsoft.AspNet.WebApi.WebHost | 5.2.3 | - | Built-in | Part of ASP.NET Core |
| Autofac | 4.6.0 | - | Removed | Using built-in DI |
| Autofac.WebApi2 | 4.0.1 | - | Removed | Using built-in DI |
| AutoMapper | 6.1.1 | - | Removed | Using manual mapping |
| NLog | 5.0.0-beta | Serilog | 4.1.x | Structured logging |
| - | - | Serilog.AspNetCore | 8.x | ASP.NET Core integration |
| - | - | Serilog.Sinks.Console | 6.x | Console output |
| - | - | Serilog.Sinks.File | 6.x | File output |
| Newtonsoft.Json | 10.0.3 | System.Text.Json | Built-in | Default serializer |
| Microsoft.AspNet.WebApi.Versioning | 2.1.0 | Asp.Versioning.Http | 8.x | URL path versioning |
| - | - | Asp.Versioning.Mvc.ApiExplorer | 8.x | Swagger integration |
| MSTest.TestFramework | 1.2.0-beta | TUnit | Latest | Modern test framework |
| MSTest.TestAdapter | 1.2.0-beta | - | Removed | TUnit has built-in runner |
| Moq | 4.7.63 | Moq | 4.20.x | Compatible |
| - | - | Microsoft.AspNetCore.Mvc.Testing | Built-in | Integration testing |
| System.Data.SqlClient | Built-in | Microsoft.Data.SqlClient | 5.x | Modern SQL client |

---

## Database Considerations

### Current State
- LocalDB with .mdf file attached
- Stored procedures for all operations
- ADO.NET with SqlCommand

### Target State

**Retained Approach: LocalDB + Stored Procedures + ADO.NET**
- Continue using LocalDB for development
- Keep all existing stored procedures
- Modernize ADO.NET usage patterns:
  - Use `await using` for proper async disposal
  - Add CancellationToken support
  - Use named parameters with explicit SqlDbType
  - Improve null handling with nullable reference types
  - Use column name-based ordinals

---

## Breaking Changes & Compatibility Concerns

### 1. No System.Web Dependency
- `HttpContext` → `HttpContext` (different namespace)
- `HttpConfiguration` → Service registration in `Program.cs`
- `IHttpActionResult` → `ActionResult<T>`

### 2. Routing Changes
- `[RoutePrefix]` → `[Route]` at controller level
- Route templates include API version
- Action method routing uses `[Http*]` attributes

### 3. Model Binding
- `[FromBody]` is implicit for complex types
- `[FromQuery]` for query parameters (explicit)
- Model validation triggers automatically

### 4. Exception Handling
- No more `ExceptionFilterAttribute` (Web API style)
- Use Middleware or `IExceptionHandler` interface
- Problem Details RFC 7807 support

### 5. Async Patterns
- All async methods should return `Task<ActionResult<T>>`
- Consider `ValueTask<T>` for performance-critical paths

### 6. JSON Serialization
- Default is `System.Text.Json` (not Newtonsoft.Json)
- Different casing rules (camelCase default)
- Some serialization behaviors differ

---

## Testing Strategy

### Unit Tests
- Framework: TUnit (latest version)
- Mocking: Moq 4.20.x
- Coverage: Services, Repositories, Endpoints
- Pattern: Source-generated tests with `[Test]` attribute

### Integration Tests
- Framework: Microsoft.AspNetCore.Mvc.Testing with TUnit
- Test Database: In-memory or test container
- Scope: Full HTTP request/response cycle

### API Tests
- Tool: httpYac (VS Code extension)
- Files: .http files for all endpoints
- Coverage: All CRUD operations, versioned endpoints

### Test Environment
```
tests/
├── Products.Application.Tests/
│   ├── Services/
│   │   ├── ProductServiceTests.cs
│   │   └── ProductOptionServiceTests.cs
├── Products.Infrastructure.Tests/
│   ├── Repositories/
│   │   ├── ProductRepositoryTests.cs
│   │   └── ProductOptionRepositoryTests.cs
├── Products.API.Tests/
│   ├── Endpoints/
│   │   ├── ProductsEndpointsTests.cs
│   │   └── ProductOptionsEndpointsTests.cs
│   └── Integration/
│       └── ProductsApiIntegrationTests.cs
```

---

## Risk Assessment

### High Risk Items
1. **Minimal API Pattern**: Moving from controllers to file-per-endpoint requires new patterns
2. **Manual Mapping Implementation**: Replacing AutoMapper requires explicit mapping code
3. **Configuration Migration**: All settings must be accounted for
4. **API Versioning**: Ensure backward compatibility maintained with URL path versioning

### Medium Risk Items
1. **Serilog Configuration**: Replacing NLog requires new logging setup
2. **Dependency Injection Refactoring**: Autofac removal requires careful service registration
3. **Exception Handling**: Different middleware pipeline approach

### Low Risk Items
1. **Entity Models**: Minimal changes required
2. **Business Logic**: Core algorithms unchanged
3. **Database Schema**: No changes required
4. **Stored Procedures**: Retained as-is
5. **ADO.NET Pattern**: Modernized but fundamentally the same

### Mitigation Strategies
- Comprehensive test coverage before migration
- Side-by-side comparison testing
- Incremental rollout with feature flags
- Extensive API testing with .http files

---

## Success Criteria

### Functional Requirements
- ✅ All existing endpoints functional with identical contracts
- ✅ API versioning (v1.0 and v2.0) working correctly
- ✅ All business logic producing same results
- ✅ Database operations functioning correctly

### Non-Functional Requirements
- ✅ Performance equal or better than Framework version
- ✅ All unit tests passing (90%+ coverage)
- ✅ All integration tests passing
- ✅ All API tests passing

### Quality Requirements
- ✅ Code follows .NET 9 best practices
- ✅ Dependency injection properly configured
- ✅ Logging properly implemented
- ✅ Exception handling comprehensive
- ✅ API documentation (Swagger) functional

---

## Timeline Estimate

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 1: Foundation Setup | 1 day | None |
| Phase 2: Core Infrastructure | 2 days | Phase 1 |
| Phase 3: Data Layer | 3 days | Phase 2 |
| Phase 4: Business Layer | 2 days | Phase 3 |
| Phase 5: API Layer | 3 days | Phase 4 |
| Phase 6: Cross-Cutting Concerns | 2 days | Phase 5 |
| Phase 7: Testing | 3 days | Phase 6 |
| Phase 8: Deployment | 1 day | Phase 7 |
| **Total** | **17 days** | |

---

## Post-Migration Enhancements

Once migration is complete, consider these modernization opportunities:

1. **OpenAPI/Swagger**: Full API documentation
2. **Health Checks**: `/health` endpoint with detailed checks
3. **Metrics**: Prometheus/OpenTelemetry integration
4. **Docker Support**: Containerization with docker-compose
5. **CI/CD Pipeline**: GitHub Actions or Azure DevOps
6. **Authentication**: JWT bearer token support
7. **Rate Limiting**: Built-in rate limiting middleware
8. **CORS**: Proper CORS policy configuration
9. **Minimal APIs**: Consider for simple endpoints
10. **Observability**: Application Insights or similar

---

## Appendix: Key Documentation References

- [Migrate from ASP.NET Web API to ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/migration/webapi)
- [.NET 9 What's New](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [ASP.NET Core Fundamentals](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/)
- [Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [API Versioning in ASP.NET Core](https://github.com/dotnet/aspnet-api-versioning)

---

**Document Version**: 1.0  
**Last Updated**: November 3, 2025  
**Author**: Migration Team
