# .NET 9 Migration Implementation Tasks

## Task Tracking Legend
- ⬜ Not Started
- 🟦 In Progress  
- ✅ Completed
- ❌ Blocked

---

## Phase 1: Foundation Setup

### Task 1.1: Create New Solution Structure ⬜
**Priority**: Critical  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Create new `src/` directory
- [ ] Create new `tests/` directory
- [ ] Create new solution file or update existing
- [ ] Set up .gitignore for .NET 9
- [ ] Create README updates for new structure

**Acceptance Criteria**:
- Solution builds successfully
- Directory structure follows .NET 9 conventions
- Git ignores appropriate build artifacts

**Commands**:
```bash
dotnet new sln -n Products -o .
mkdir -p src tests .http docs
```

---

### Task 1.2: Create SDK-Style Projects ⬜
**Priority**: Critical  
**Estimated Time**: 3 hours

**Subtasks**:
- [ ] Create `Products.Domain` project (net9.0)
- [ ] Create `Products.Application` project (net9.0)
- [ ] Create `Products.Infrastructure` project (net9.0)
- [ ] Create `Products.API` project (net9.0 Web API)
- [ ] Add projects to solution
- [ ] Set up project references

**Acceptance Criteria**:
- All projects use SDK-style format
- Target framework is `net9.0`
- Project references are correct
- Solution builds without errors

**Commands**:
```bash
cd src
dotnet new classlib -n Products.Domain -f net9.0
dotnet new classlib -n Products.Application -f net9.0
dotnet new classlib -n Products.Infrastructure -f net9.0
dotnet new webapi -n Products.API -f net9.0
cd ..
dotnet sln add src/**/*.csproj
```

---

### Task 1.3: Configure Base Dependencies ⬜
**Priority**: Critical  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Install core NuGet packages
- [ ] Configure nullable reference types
- [ ] Set up global usings
- [ ] Configure analyzers

**Packages to Install**:
- Products.API:
  - `Asp.Versioning.Http` (8.x)
  - `Asp.Versioning.Mvc.ApiExplorer` (8.x)
  - `Swashbuckle.AspNetCore` (6.x)
  - `NLog.Web.AspNetCore` (5.3.x)
- Products.Application:
  - `AutoMapper` (13.x)
  - `AutoMapper.Extensions.Microsoft.DependencyInjection` (13.x)
  - `FluentValidation` (11.x)
- Products.Infrastructure:
  - `Dapper` (2.1.x)
  - `Microsoft.Data.SqlClient` (5.x)

**Acceptance Criteria**:
- All packages restored successfully
- No package conflicts
- Project builds with warnings addressed

---

## Phase 2: Core Infrastructure

### Task 2.1: Set Up Dependency Injection ⬜
**Priority**: Critical  
**Estimated Time**: 3 hours

**Subtasks**:
- [ ] Create `DependencyInjection.cs` in each project
- [ ] Register services with appropriate lifetimes
- [ ] Configure options pattern for settings
- [ ] Wire up DI in `Program.cs`

**Files to Create**:
- `src/Products.Domain/DependencyInjection.cs`
- `src/Products.Application/DependencyInjection.cs`
- `src/Products.Infrastructure/DependencyInjection.cs`
- `src/Products.API/Program.cs` (modify)

**Acceptance Criteria**:
- Services resolve correctly from DI container
- No circular dependencies
- Appropriate service lifetimes configured
- Startup completes without errors

---

### Task 2.2: Configure Logging Infrastructure ⬜
**Priority**: High  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Create `nlog.config` in API project
- [ ] Configure NLog provider in `Program.cs`
- [ ] Set up logging categories and levels
- [ ] Create structured logging helpers
- [ ] Test logging output

**Files to Create/Modify**:
- `src/Products.API/nlog.config`
- `src/Products.API/Program.cs` (configure logging)

**Acceptance Criteria**:
- Logs written to file correctly
- Console logging works in development
- Structured logging captures context
- Log levels filter appropriately

---

### Task 2.3: Set Up Configuration Management ⬜
**Priority**: Critical  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Create `appsettings.json`
- [ ] Create `appsettings.Development.json`
- [ ] Create configuration classes (strongly-typed)
- [ ] Migrate all Web.config settings
- [ ] Configure options pattern

**Files to Create**:
- `src/Products.API/appsettings.json`
- `src/Products.API/appsettings.Development.json`
- `src/Products.API/Configuration/ApiVersioningOptions.cs`
- `src/Products.API/Configuration/DatabaseOptions.cs`

**Settings to Migrate**:
```json
{
  "ConnectionStrings": {
    "ProductsDB": "Data Source=(LocalDB)\\MSSQLLocalDB;..."
  },
  "ApiVersioning": {
    "DefaultVersion": "1.0",
    "ParameterName": "api-version"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**Acceptance Criteria**:
- All settings migrated from Web.config
- Strongly-typed configuration classes work
- Environment-specific settings load correctly
- Connection strings accessible

---

### Task 2.4: Implement Middleware Pipeline ⬜
**Priority**: High  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Configure exception handling middleware
- [ ] Set up CORS policy
- [ ] Configure request/response logging
- [ ] Add compression middleware
- [ ] Configure HTTPS redirection

**Files to Modify**:
- `src/Products.API/Program.cs`

**Middleware to Configure**:
```csharp
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**Acceptance Criteria**:
- Middleware executes in correct order
- Exceptions handled gracefully
- CORS configured for development
- HTTPS redirection works

---

## Phase 3: Data Layer Modernization

### Task 3.1: Migrate Entity Models ⬜
**Priority**: Critical  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Copy `ProductEntity.cs` to Domain project
- [ ] Copy `ProductOptionEntity.cs` to Domain project
- [ ] Copy `BaseEntity.cs` to Domain project
- [ ] Remove `System.Web` dependencies
- [ ] Add nullable annotations
- [ ] Update namespaces

**Files to Create**:
- `src/Products.Domain/Entities/BaseEntity.cs`
- `src/Products.Domain/Entities/ProductEntity.cs`
- `src/Products.Domain/Entities/ProductOptionEntity.cs`
- `src/Products.Domain/Extensions/BaseEntityExtensions.cs`

**Acceptance Criteria**:
- All entities compile without errors
- No System.Web dependencies
- Nullable reference types enabled
- Extensions work correctly

---

### Task 3.2: Create Repository Interfaces ⬜
**Priority**: Critical  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Create `IRepository<T>` interface
- [ ] Create `IProductRepository` interface
- [ ] Create `IProductOptionRepository` interface
- [ ] Define async contract methods

**Files to Create**:
- `src/Products.Domain/Interfaces/IRepository.cs`
- `src/Products.Domain/Interfaces/IProductRepository.cs`
- `src/Products.Domain/Interfaces/IProductOptionRepository.cs`

**Acceptance Criteria**:
- Interfaces define clear contracts
- All methods return Task<T>
- Consistent naming conventions
- XML documentation comments added

---

### Task 3.3: Implement Connection Management with Dapper ⬜
**Priority**: Critical  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Create `IDbConnectionFactory` interface
- [ ] Implement `SqlConnectionFactory`
- [ ] Register factory in DI
- [ ] Add Dapper package
- [ ] Create base repository with Dapper

**Files to Create**:
- `src/Products.Infrastructure/Data/IDbConnectionFactory.cs`
- `src/Products.Infrastructure/Data/SqlConnectionFactory.cs`
- `src/Products.Infrastructure/Data/BaseRepository.cs`

**Acceptance Criteria**:
- Connection factory creates connections correctly
- Connections disposed properly
- Dapper integrated successfully
- DI registration works

---

### Task 3.4: Implement Product Repository ⬜
**Priority**: Critical  
**Estimated Time**: 3 hours

**Subtasks**:
- [ ] Migrate `RetrieveAsync()` method
- [ ] Migrate `RetrieveByIdAsync()` method
- [ ] Migrate `RetrieveByNameAsync()` method
- [ ] Migrate `CreateAsync()` method
- [ ] Migrate `UpdateAsync()` method
- [ ] Migrate `DeleteAsync()` method
- [ ] Add Dapper query methods
- [ ] Handle null reference properly

**Files to Create**:
- `src/Products.Infrastructure/Repositories/ProductRepository.cs`

**Dapper Pattern Example**:
```csharp
public async Task<ProductEntity?> RetrieveByIdAsync(Guid id)
{
    using var connection = _connectionFactory.CreateConnection();
    return await connection.QuerySingleOrDefaultAsync<ProductEntity>(
        "RetrieveProductById",
        new { Id = id },
        commandType: CommandType.StoredProcedure
    );
}
```

**Acceptance Criteria**:
- All methods use Dapper correctly
- Stored procedures called properly
- Async/await used consistently
- Null handling with nullable types
- Repository registered in DI

---

### Task 3.5: Implement ProductOption Repository ⬜
**Priority**: Critical  
**Estimated Time**: 3 hours

**Subtasks**:
- [ ] Migrate all CRUD methods
- [ ] Implement `RetrieveByProductIdAsync()`
- [ ] Implement `RetrieveByProductIdAndIdAsync()`
- [ ] Add Dapper mappings
- [ ] Handle relationships correctly

**Files to Create**:
- `src/Products.Infrastructure/Repositories/ProductOptionRepository.cs`

**Acceptance Criteria**:
- All methods functional with Dapper
- Product relationships handled
- Repository registered in DI
- Error handling implemented

---

### Task 3.6: Update Database Scripts (Optional) ⬜
**Priority**: Low  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Review stored procedures compatibility
- [ ] Update any SQL Server syntax if needed
- [ ] Create migration script if required
- [ ] Document database setup

**Files to Review**:
- All files in `Products.Database/` directory

**Acceptance Criteria**:
- Stored procedures work with .NET 9
- Database setup documented
- Connection strings updated

---

## Phase 4: Business Layer Migration

### Task 4.1: Migrate Business Models ⬜
**Priority**: Critical  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Copy `ProductModel.cs` to Application project
- [ ] Copy `ProductOptionModel.cs` to Application project
- [ ] Update namespaces
- [ ] Add data annotations if needed
- [ ] Add nullable annotations

**Files to Create**:
- `src/Products.Application/Models/ProductModel.cs`
- `src/Products.Application/Models/ProductOptionModel.cs`

**Acceptance Criteria**:
- Models compile without errors
- No legacy dependencies
- Proper namespace structure

---

### Task 4.2: Create Service Interfaces ⬜
**Priority**: Critical  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Create `IProductService` interface
- [ ] Create `IProductOptionService` interface
- [ ] Define async method signatures
- [ ] Add XML documentation

**Files to Create**:
- `src/Products.Application/Interfaces/IProductService.cs`
- `src/Products.Application/Interfaces/IProductOptionService.cs`

**Acceptance Criteria**:
- Interfaces match existing contracts
- All methods async
- Documentation complete

---

### Task 4.3: Configure AutoMapper ⬜
**Priority**: Critical  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Create `EntityToModelProfile`
- [ ] Create `ModelToEntityProfile`
- [ ] Register AutoMapper in DI
- [ ] Test mapping configurations
- [ ] Add custom value converters if needed

**Files to Create**:
- `src/Products.Application/Mapping/EntityToModelProfile.cs`
- `src/Products.Application/Mapping/ModelToEntityProfile.cs`

**Modern AutoMapper Pattern**:
```csharp
public class EntityToModelProfile : Profile
{
    public EntityToModelProfile()
    {
        CreateMap<ProductEntity, ProductModel>();
        CreateMap<ProductOptionEntity, ProductOptionModel>();
    }
}

// In DI
services.AddAutoMapper(typeof(EntityToModelProfile).Assembly);
```

**Acceptance Criteria**:
- AutoMapper configured with DI
- All mappings defined
- Mapping tests pass
- No static Mapper.Map calls

---

### Task 4.4: Implement Product Service ⬜
**Priority**: Critical  
**Estimated Time**: 3 hours

**Subtasks**:
- [ ] Inject repository and mapper
- [ ] Migrate `RetrieveAsync()` method
- [ ] Migrate `RetrieveByIdAsync()` method
- [ ] Migrate `RetrieveByNameAsync()` method
- [ ] Migrate `CreateAsync()` method
- [ ] Migrate `UpdateAsync()` method
- [ ] Migrate `DeleteAsync()` method
- [ ] Update exception handling

**Files to Create**:
- `src/Products.Application/Services/ProductService.cs`

**Acceptance Criteria**:
- All business logic preserved
- AutoMapper used via DI
- Repository accessed via DI
- Exceptions handled appropriately
- Service registered in DI

---

### Task 4.5: Implement ProductOption Service ⬜
**Priority**: Critical  
**Estimated Time**: 3 hours

**Subtasks**:
- [ ] Inject dependencies
- [ ] Migrate all CRUD operations
- [ ] Handle product relationship validation
- [ ] Implement exception handling

**Files to Create**:
- `src/Products.Application/Services/ProductOptionService.cs`

**Acceptance Criteria**:
- All operations functional
- Business rules enforced
- Service registered in DI

---

### Task 4.6: Create Custom Exceptions ⬜
**Priority**: High  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Migrate `NotFoundException`
- [ ] Create additional exceptions if needed
- [ ] Add proper exception constructors

**Files to Create**:
- `src/Products.Application/Exceptions/NotFoundException.cs`
- `src/Products.Application/Exceptions/ValidationException.cs` (optional)

**Acceptance Criteria**:
- Exceptions serialize properly
- Constructors follow conventions
- Used consistently in services

---

## Phase 5: API Layer Implementation

### Task 5.1: Create DTOs ⬜
**Priority**: Critical  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Copy `BaseDTO.cs`
- [ ] Copy `CollectionDTO.cs`
- [ ] Copy `ProductDTO.cs`
- [ ] Copy `ProductOptionDTO.cs`
- [ ] Add validation attributes
- [ ] Update namespaces

**Files to Create**:
- `src/Products.API/DTOs/BaseDTO.cs`
- `src/Products.API/DTOs/CollectionDTO.cs`
- `src/Products.API/DTOs/ProductDTO.cs`
- `src/Products.API/DTOs/ProductOptionDTO.cs`

**Acceptance Criteria**:
- DTOs compile successfully
- Validation attributes added
- JSON serialization works
- Nullable types configured

---

### Task 5.2: Configure AutoMapper for DTOs ⬜
**Priority**: Critical  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Create `ModelToDTOProfile`
- [ ] Create `DTOToModelProfile`
- [ ] Register in API project
- [ ] Test mappings

**Files to Create**:
- `src/Products.API/Mapping/ModelToDTOProfile.cs`
- `src/Products.API/Mapping/DTOToModelProfile.cs`

**Acceptance Criteria**:
- Mappings registered in DI
- No static Mapper usage
- All conversions work

---

### Task 5.3: Configure API Versioning ⬜
**Priority**: High  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Install Asp.Versioning packages
- [ ] Configure versioning in Program.cs
- [ ] Set default version
- [ ] Configure version reader (query string)
- [ ] Test versioning

**Configuration**:
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

**Acceptance Criteria**:
- Versioning configured correctly
- Query string reader works
- Default version set
- Swagger shows versions

---

### Task 5.4: Implement Products Controller v1.0 ⬜
**Priority**: Critical  
**Estimated Time**: 4 hours

**Subtasks**:
- [ ] Create controller with ControllerBase
- [ ] Add API version attribute
- [ ] Implement GET /products
- [ ] Implement GET /products?name={name}
- [ ] Implement GET /products/{id}
- [ ] Implement POST /products
- [ ] Implement PUT /products/{id}
- [ ] Implement DELETE /products/{id}
- [ ] Implement GET /products/{id}/options
- [ ] Implement GET /products/{id}/options/{optionId}
- [ ] Implement POST /products/{id}/options
- [ ] Implement PUT /products/{id}/options/{optionId}
- [ ] Implement DELETE /products/{id}/options/{optionId}
- [ ] Add ProducesResponseType attributes
- [ ] Add proper status codes

**Files to Create**:
- `src/Products.API/Controllers/v1/ProductsController.cs`

**Modern Controller Pattern**:
```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;

    [HttpGet]
    [ProducesResponseType(typeof(CollectionDTO<ProductDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionDTO<ProductDTO>>> GetProducts()
    {
        var products = await _productService.RetrieveAsync();
        return Ok(_mapper.Map<CollectionDTO<ProductDTO>>(products));
    }
}
```

**Acceptance Criteria**:
- All endpoints functional
- Status codes correct
- DTOs mapped properly
- Routing works correctly
- Validation working

---

### Task 5.5: Implement Products Controller v2.0 ⬜
**Priority**: Medium  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Copy v1 controller
- [ ] Update API version to 2.0
- [ ] Implement any v2-specific features
- [ ] Update route

**Files to Create**:
- `src/Products.API/Controllers/v2/ProductsController.cs`

**Acceptance Criteria**:
- v2.0 endpoints work
- Version routing correct
- Both versions coexist

---

### Task 5.6: Configure Swagger/OpenAPI ⬜
**Priority**: High  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Install Swashbuckle.AspNetCore
- [ ] Configure Swagger generator
- [ ] Add API versioning to Swagger
- [ ] Configure XML documentation
- [ ] Add authorization if needed
- [ ] Test Swagger UI

**Configuration**:
```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Products API", Version = "v1.0" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "Products API", Version = "v2.0" });
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
```

**Acceptance Criteria**:
- Swagger UI accessible
- All endpoints documented
- Versions shown separately
- Try-it-out works

---

## Phase 6: Cross-Cutting Concerns

### Task 6.1: Implement Global Exception Handler ⬜
**Priority**: Critical  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Create exception handler middleware
- [ ] Map exceptions to HTTP status codes
- [ ] Implement ProblemDetails response
- [ ] Log exceptions appropriately
- [ ] Test exception scenarios

**Files to Create**:
- `src/Products.API/Middleware/GlobalExceptionHandler.cs`

**Modern Exception Handling**:
```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        var problemDetails = exception switch
        {
            NotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = exception.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error"
            }
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true;
    }
}
```

**Acceptance Criteria**:
- Exceptions handled globally
- Proper status codes returned
- ProblemDetails format used
- No sensitive data leaked
- Exceptions logged

---

### Task 6.2: Implement Action Filters ⬜
**Priority**: Medium  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Migrate ModelStateCheckFilter
- [ ] Migrate NullResourceCheckFilter
- [ ] Migrate SameIdCheckFilter
- [ ] Update filter registration
- [ ] Test filters

**Files to Create**:
- `src/Products.API/Filters/ValidationFilter.cs`
- `src/Products.API/Filters/ResourceCheckFilter.cs`

**Modern Filter Pattern**:
```csharp
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
```

**Acceptance Criteria**:
- Filters work as attributes
- Validation happens automatically
- Filter order correct
- Tests pass

---

### Task 6.3: Configure CORS Policy ⬜
**Priority**: Medium  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Define CORS policy in Program.cs
- [ ] Configure allowed origins
- [ ] Set allowed methods
- [ ] Configure credentials
- [ ] Test CORS headers

**Configuration**:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

**Acceptance Criteria**:
- CORS headers present
- Preflight requests work
- Policy restrictive for production

---

### Task 6.4: Add Health Checks ⬜
**Priority**: Low  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Install health checks packages
- [ ] Add database health check
- [ ] Configure /health endpoint
- [ ] Test health endpoint

**Configuration**:
```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("ProductsDB")!);

app.MapHealthChecks("/health");
```

**Acceptance Criteria**:
- /health returns 200 when healthy
- Database check works
- Detailed checks available

---

## Phase 7: Testing Infrastructure

### Task 7.1: Set Up Test Projects ⬜
**Priority**: Critical  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Create Products.Application.Tests (xUnit)
- [ ] Create Products.Infrastructure.Tests (xUnit)
- [ ] Create Products.API.Tests (xUnit + WebApplicationFactory)
- [ ] Add test projects to solution
- [ ] Install test dependencies

**Projects to Create**:
```bash
cd tests
dotnet new xunit -n Products.Application.Tests -f net9.0
dotnet new xunit -n Products.Infrastructure.Tests -f net9.0
dotnet new xunit -n Products.API.Tests -f net9.0
```

**Packages to Install**:
- xUnit (2.9.x)
- xunit.runner.visualstudio (2.8.x)
- Moq (4.20.x)
- FluentAssertions (6.x)
- Microsoft.AspNetCore.Mvc.Testing (for integration tests)

**Acceptance Criteria**:
- Test projects build
- Test runner discovers tests
- Dependencies installed

---

### Task 7.2: Migrate Unit Tests - Services ⬜
**Priority**: High  
**Estimated Time**: 4 hours

**Subtasks**:
- [ ] Migrate ProductServiceTests
- [ ] Migrate ProductOptionServiceTests
- [ ] Update MSTest → xUnit syntax
- [ ] Update assertions
- [ ] Add FluentAssertions
- [ ] Ensure all tests pass

**Files to Create**:
- `tests/Products.Application.Tests/Services/ProductServiceTests.cs`
- `tests/Products.Application.Tests/Services/ProductOptionServiceTests.cs`

**xUnit Pattern**:
```csharp
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockMapper = new Mock<IMapper>();
        _sut = new ProductService(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task RetrieveByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var entity = new ProductEntity { Id = Guid.NewGuid() };
        var model = new ProductModel { Id = entity.Id };
        _mockRepository.Setup(x => x.RetrieveByIdAsync(entity.Id))
            .ReturnsAsync(entity);
        _mockMapper.Setup(x => x.Map<ProductModel>(entity))
            .Returns(model);

        // Act
        var result = await _sut.RetrieveByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
    }
}
```

**Acceptance Criteria**:
- All service tests migrated
- Tests use xUnit attributes
- FluentAssertions used
- All tests pass
- Code coverage maintained

---

### Task 7.3: Create Integration Tests ⬜
**Priority**: High  
**Estimated Time**: 4 hours

**Subtasks**:
- [ ] Create WebApplicationFactory setup
- [ ] Configure test database
- [ ] Write integration tests for all endpoints
- [ ] Test authentication/authorization
- [ ] Test error scenarios

**Files to Create**:
- `tests/Products.API.Tests/Integration/CustomWebApplicationFactory.cs`
- `tests/Products.API.Tests/Integration/ProductsApiTests.cs`

**Integration Test Pattern**:
```csharp
public class ProductsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/v1.0/products");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
    }
}
```

**Acceptance Criteria**:
- Integration tests run successfully
- Test database isolated
- All endpoints tested
- Error cases covered

---

### Task 7.4: Create Repository Unit Tests ⬜
**Priority**: Medium  
**Estimated Time**: 3 hours

**Subtasks**:
- [ ] Create repository tests with mocked connections
- [ ] Test Dapper queries
- [ ] Test exception handling
- [ ] Verify SQL parameters

**Files to Create**:
- `tests/Products.Infrastructure.Tests/Repositories/ProductRepositoryTests.cs`
- `tests/Products.Infrastructure.Tests/Repositories/ProductOptionRepositoryTests.cs`

**Acceptance Criteria**:
- Repository methods tested
- Dapper integration verified
- Exception paths tested

---

### Task 7.5: Create HTTP Test Files ⬜
**Priority**: Critical  
**Estimated Time**: 3 hours

**Subtasks**:
- [ ] Install httpYac VS Code extension
- [ ] Create environment variables file
- [ ] Create products.http for v1.0 endpoints
- [ ] Create products-v2.http for v2.0 endpoints
- [ ] Create test data setup file
- [ ] Document usage

**Files to Create**:
- `.http/http-client.env.json`
- `.http/products-v1.http`
- `.http/products-v2.http`
- `.http/README.md`

**Sample products-v1.http**:
```http
### Variables
@baseUrl = http://localhost:5000
@apiVersion = 1.0

### Get All Products
GET {{baseUrl}}/api/v{{apiVersion}}/products
Accept: application/json

### Get Product by Name
GET {{baseUrl}}/api/v{{apiVersion}}/products?name=iPhone
Accept: application/json

### Get Product by ID
@productId = {{productIdFromCreate}}
GET {{baseUrl}}/api/v{{apiVersion}}/products/{{productId}}
Accept: application/json

### Create Product
# @name createProduct
POST {{baseUrl}}/api/v{{apiVersion}}/products
Content-Type: application/json

{
  "name": "Test Product",
  "description": "Test Description",
  "price": 99.99,
  "deliveryPrice": 9.99
}

### Extract Product ID
@productIdFromCreate = {{createProduct.response.body.$.id}}

### Update Product
PUT {{baseUrl}}/api/v{{apiVersion}}/products/{{productId}}
Content-Type: application/json

{
  "id": "{{productId}}",
  "name": "Updated Product",
  "description": "Updated Description",
  "price": 149.99,
  "deliveryPrice": 14.99
}

### Delete Product
DELETE {{baseUrl}}/api/v{{apiVersion}}/products/{{productId}}

### Get Product Options
GET {{baseUrl}}/api/v{{apiVersion}}/products/{{productId}}/options
Accept: application/json

### Create Product Option
# @name createOption
POST {{baseUrl}}/api/v{{apiVersion}}/products/{{productId}}/options
Content-Type: application/json

{
  "name": "Color Option",
  "description": "Red"
}

### Get Product Option by ID
@optionId = {{createOption.response.body.$.id}}
GET {{baseUrl}}/api/v{{apiVersion}}/products/{{productId}}/options/{{optionId}}
Accept: application/json

### Update Product Option
PUT {{baseUrl}}/api/v{{apiVersion}}/products/{{productId}}/options/{{optionId}}
Content-Type: application/json

{
  "id": "{{optionId}}",
  "name": "Updated Color",
  "description": "Blue"
}

### Delete Product Option
DELETE {{baseUrl}}/api/v{{apiVersion}}/products/{{productId}}/options/{{optionId}}
```

**Acceptance Criteria**:
- All endpoints have HTTP tests
- Variables properly configured
- Tests can run sequentially
- Response extraction works
- Documentation complete

---

## Phase 8: Deployment Preparation

### Task 8.1: Configure Production Settings ⬜
**Priority**: High  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Create `appsettings.Production.json`
- [ ] Configure production connection string
- [ ] Set up environment variables
- [ ] Configure logging for production
- [ ] Review security settings

**Files to Create**:
- `src/Products.API/appsettings.Production.json`

**Production Configuration**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Acceptance Criteria**:
- Production config doesn't contain secrets
- Environment variables documented
- Logging appropriate for production
- Connection strings externalized

---

### Task 8.2: Add Application Insights (Optional) ⬜
**Priority**: Low  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Install Application Insights SDK
- [ ] Configure instrumentation key
- [ ] Add telemetry
- [ ] Test monitoring

**Acceptance Criteria**:
- Telemetry flowing to App Insights
- Custom events tracked
- Performance monitored

---

### Task 8.3: Create Docker Support ⬜
**Priority**: Low  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Create Dockerfile
- [ ] Create .dockerignore
- [ ] Create docker-compose.yml
- [ ] Test containerization
- [ ] Document Docker setup

**Files to Create**:
- `src/Products.API/Dockerfile`
- `.dockerignore`
- `docker-compose.yml`

**Sample Dockerfile**:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Products.API/Products.API.csproj", "Products.API/"]
RUN dotnet restore "Products.API/Products.API.csproj"
COPY . .
WORKDIR "/src/Products.API"
RUN dotnet build "Products.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Products.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Products.API.dll"]
```

**Acceptance Criteria**:
- Docker image builds successfully
- Container runs API correctly
- docker-compose starts all services
- Ports exposed correctly

---

### Task 8.4: Update Documentation ⬜
**Priority**: Medium  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Update README.md
- [ ] Document API endpoints
- [ ] Add setup instructions
- [ ] Document configuration
- [ ] Add troubleshooting guide

**Files to Update**:
- `README.md`
- `docs/API.md` (create)
- `docs/SETUP.md` (create)

**Acceptance Criteria**:
- README reflects .NET 9 setup
- API documented with examples
- Setup steps clear
- Configuration documented

---

### Task 8.5: Performance Testing ⬜
**Priority**: Medium  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Install benchmarking tools
- [ ] Create performance tests
- [ ] Compare with old API
- [ ] Document results
- [ ] Optimize if needed

**Tools**:
- BenchmarkDotNet
- k6 or Apache Bench

**Acceptance Criteria**:
- Performance comparable or better
- No regressions identified
- Results documented

---

### Task 8.6: Security Review ⬜
**Priority**: High  
**Estimated Time**: 2 hours

**Subtasks**:
- [ ] Review HTTPS configuration
- [ ] Check for hardcoded secrets
- [ ] Review CORS policy
- [ ] Enable security headers
- [ ] Run security scan

**Security Headers**:
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

**Acceptance Criteria**:
- No secrets in source code
- Security headers added
- HTTPS enforced
- Dependencies scanned

---

## Post-Migration Tasks

### Task 9.1: Cleanup Legacy Code ⬜
**Priority**: Low  
**Estimated Time**: 1 hour

**Subtasks**:
- [ ] Archive old .NET Framework projects
- [ ] Remove unused packages
- [ ] Clean up bin/obj folders
- [ ] Update .gitignore

---

### Task 9.2: CI/CD Pipeline Setup ⬜
**Priority**: Medium  
**Estimated Time**: 4 hours

**Subtasks**:
- [ ] Create GitHub Actions workflow
- [ ] Configure build pipeline
- [ ] Configure test execution
- [ ] Set up deployment pipeline
- [ ] Configure environment secrets

---

### Task 9.3: Monitoring and Observability ⬜
**Priority**: Medium  
**Estimated Time**: 3 hours

**Subtasks**:
- [ ] Set up structured logging
- [ ] Configure metrics collection
- [ ] Add distributed tracing
- [ ] Create dashboards

---

## Summary

**Total Estimated Time**: ~17 days (136 hours)

**Critical Path Tasks**:
1. Foundation Setup (Phase 1)
2. Data Layer (Phase 3)
3. Business Layer (Phase 4)
4. API Layer (Phase 5)
5. Testing (Phase 7)

**Dependencies**:
- Phase 2 requires Phase 1
- Phase 3 requires Phase 2
- Phase 4 requires Phase 3
- Phase 5 requires Phase 4
- Phase 7 requires Phase 6

**Risk Mitigation**:
- Complete testing at each phase
- Run .http tests frequently
- Compare responses with old API
- Keep old API running until verified

---

**Document Version**: 1.0  
**Last Updated**: November 3, 2025
