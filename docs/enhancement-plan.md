# Enhancement Plan

## Overview
Post-migration improvements identified for the Products API .NET 9 project before production deployment.

**Priority Levels:**
- 🔴 **Critical** - Must fix before commit
- 🟡 **High** - Should fix before production
- 🟢 **Medium** - Nice to have

---

## 🔴 Critical Issues

### 1. Connection String Validation
**Status:** ✅ Implemented  
**Impact:** Silent failures on startup if SQL Server unavailable

**Tasks:**
- [x] Add SQL Server health check in `Program.cs`
- [x] Test connection on startup before accepting requests
- [x] Return HTTP 503 if database unreachable
- [x] Log connection attempts for debugging

**Implementation:**
```csharp
// src/Products.API/Program.cs
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        name: "sql-server",
        timeout: TimeSpan.FromSeconds(5));

app.MapHealthChecks("/health");
```

**Estimate:** 30 minutes

---

### 2. Basic Input Validation
**Status:** ✅ Implemented  
**Impact:** Invalid data can corrupt database, cause errors

**Tasks:**
- [x] Add validation attributes to DTOs
- [x] Add FluentValidation or built-in validators
- [x] Return 400 Bad Request with validation errors
- [x] Add validation middleware

**Example Issues:**
- Product name can be empty or null
- Price can be negative
- No max length on strings (SQL truncation risk)

**Implementation:**
```csharp
public class ProductDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal DeliveryPrice { get; set; }
}
```

**Estimate:** 1 hour

---

### 3. Example Unit Tests
**Status:** ✅ Implemented  
**Impact:** No confidence in code correctness, no CI/CD baseline

**Tasks:**
- [x] Add ProductService unit test (with mocked repository)
- [x] Add ProductRepository integration test (with test database)
- [x] Add Products API endpoint test
- [x] Document test patterns for team

**Target Coverage:**
- At least 1 test per test project
- Demonstrate mocking pattern
- Demonstrate integration test pattern

**Example:**
```csharp
// tests/Products.Application.Tests/Services/ProductServiceTests.cs
public class ProductServiceTests
{
    [Test]
    public async Task GetProduct_ExistingId_ReturnsProduct()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        mockRepo.GetByIdAsync(1).Returns(new Product { Id = 1 });
        var service = new ProductService(mockRepo);

        // Act
        var result = await service.GetProductAsync(1);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Id).IsEqualTo(1);
    }
}
```

**Estimate:** 2 hours

---

## 🟡 High Priority

### 4. Error Handling Standardization
**Status:** Inconsistent exception handling  
**Impact:** Poor error messages, hard to debug

**Tasks:**
- [ ] Create domain-specific exceptions (e.g., `ProductNotFoundException`)
- [ ] Wrap `SqlException` in domain exceptions
- [ ] Add global exception handler middleware
- [ ] Return consistent error response format

**Implementation:**
```csharp
// src/Products.Domain/Exceptions/ProductNotFoundException.cs
public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(int id) 
        : base($"Product with ID {id} not found") { }
}

// src/Products.API/Middleware/GlobalExceptionHandler.cs
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            ProductNotFoundException => (404, exception.Message),
            ValidationException => (400, exception.Message),
            _ => (500, "An error occurred")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);
        return true;
    }
}
```

**Estimate:** 1.5 hours

---

### 5. CI/CD Pipeline
**Status:** Not implemented  
**Impact:** No automated builds, no deployment pipeline

**Tasks:**
- [ ] Create `.github/workflows/build.yml`
- [ ] Add automated test runs on PR
- [ ] Add Docker image build and push
- [ ] Add code quality checks (linting, security scan)
- [ ] Add version tagging on release

**Implementation:**
```yaml
# .github/workflows/build.yml
name: Build and Test

on:
  push:
    branches: [ master, develop ]
  pull_request:
    branches: [ master ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: YourStrong@Passw0rd
        ports:
          - 1433:1433
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 9.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Build Docker image
      run: docker build -t products-api:${{ github.sha }} .
```

**Estimate:** 2 hours

---

### 6. Environment Configuration
**Status:** Only Development config exists  
**Impact:** Can't deploy to Staging/Production safely

**Tasks:**
- [ ] Create `appsettings.Staging.json`
- [ ] Create `appsettings.Production.json` (with secrets redacted)
- [ ] Document secret management strategy (Azure Key Vault, AWS Secrets Manager, etc.)
- [ ] Add environment-specific connection strings
- [ ] Configure logging levels per environment

**Implementation:**
```json
// src/Products.API/appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-sql-server;Database=ProductsDB;User Id=prod_user;Password=***;TrustServerCertificate=true"
  },
  "AllowedHosts": "api.products.com"
}
```

**Documentation needed:**
- How to use Azure Key Vault / environment variables
- Secret rotation procedures
- Connection string format for each environment

**Estimate:** 1 hour

---

## 🟢 Medium Priority

### 7. Enhanced .gitignore
**Status:** Missing modern patterns  
**Impact:** Could accidentally commit sensitive files

**Tasks:**
- [ ] Add database files: `*.db`, `*.db-shm`, `*.db-wal`
- [ ] Add IDE files: `.vscode/settings.json`, `.idea/`
- [ ] Add Docker volumes: `docker/volumes/`
- [ ] Add test results: `TestResults/`, `*.trx`
- [ ] Add secrets: `.env`, `*.secrets.json`

**Estimate:** 15 minutes

---

### 8. API Documentation Improvements
**Status:** Basic Swagger exists  
**Impact:** Hard for consumers to understand API

**Tasks:**
- [ ] Add XML comments to controllers
- [ ] Add example responses in Swagger
- [ ] Add authentication documentation (if applicable)
- [ ] Add versioning strategy documentation
- [ ] Create Postman/Insomnia collection

**Example:**
```csharp
/// <summary>
/// Retrieves a product by its unique identifier
/// </summary>
/// <param name="id">The product ID</param>
/// <returns>The product details</returns>
/// <response code="200">Returns the product</response>
/// <response code="404">Product not found</response>
[HttpGet("{id}")]
[ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetProduct(int id)
```

**Estimate:** 1 hour

---

### 9. Logging Enhancements
**Status:** Basic logging exists  
**Impact:** Hard to troubleshoot production issues

**Tasks:**
- [ ] Add structured logging with Serilog
- [ ] Add correlation IDs for request tracking
- [ ] Add performance logging (execution time)
- [ ] Configure log sinks (file, Application Insights, etc.)
- [ ] Add sensitive data masking

**Estimate:** 1.5 hours

---

### 10. Database Migration Strategy
**Status:** Manual SQL scripts  
**Impact:** Hard to track schema changes over time

**Tasks:**
- [ ] Evaluate EF Core Migrations vs. DbUp vs. FluentMigrator
- [ ] Create migration tracking table
- [ ] Version all database changes
- [ ] Create rollback scripts
- [ ] Document migration process

**Note:** Current ADO.NET approach works but consider migration tooling for production.

**Estimate:** 3 hours (research + implementation)

---

## Implementation Timeline

### Phase 1: Pre-Commit (Must Do)
**Time Required:** ~3.5 hours
1. Connection string validation (30 min)
2. Basic input validation (1 hour)
3. Example unit tests (2 hours)

### Phase 2: Pre-Production (Should Do)
**Time Required:** ~5.5 hours
4. Error handling standardization (1.5 hours)
5. CI/CD pipeline (2 hours)
6. Environment configuration (1 hour)
7. Enhanced .gitignore (15 min)
8. API documentation (1 hour)

### Phase 3: Production Readiness (Nice to Have)
**Time Required:** ~4.5 hours
9. Logging enhancements (1.5 hours)
10. Database migration strategy (3 hours)

**Total Estimated Time:** ~13.5 hours

---

## Success Criteria

### Before Commit
- ✅ Application starts successfully with valid connection
- ✅ Application fails gracefully with invalid connection (health check endpoint)
- ✅ Invalid input returns 400 with clear error message (validation implemented)
- ✅ At least 1 passing test per test project (13 + 3 + 7 = 23 tests passing)

### Before Production
- ✅ CI/CD pipeline runs successfully
- ✅ All tests pass in CI
- ✅ Docker image builds and runs
- ✅ Staging environment configured
- ✅ Error responses are consistent
- ✅ Health check endpoint returns status

### Production Readiness
- ✅ Structured logging in place
- ✅ Correlation IDs tracked across requests
- ✅ Database migrations automated
- ✅ Secrets stored securely
- ✅ Monitoring/alerting configured

---

## Notes

**Migration Completeness:**
The .NET 9 migration is functionally complete. All old .NET Framework 4.5.2 code has been removed. The API works and all HTTP tests pass. These enhancements address production-readiness gaps, not migration defects.

**Current State:**
- ✅ .NET 9 migration complete
- ✅ Docker containerization complete
- ✅ SQL Server database migration complete
- ✅ 75/75 HTTP tests passing
- ✅ Documentation organized
- ✅ Critical enhancements implemented (health checks, validation, unit tests)
- ⚠️ Optional production-readiness gaps remain (see High/Medium priority items)

**Team Review:**
Before implementing, review priorities with team to ensure alignment on what "production-ready" means for your context.
