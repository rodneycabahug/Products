# .NET 9 Migration - Implementation Status

## ✅ Completed

### Phase 1: Foundation Setup (COMPLETE)
- [x] Created new .NET 9 solution structure
- [x] Created 4 src projects (API, Application, Domain, Infrastructure)
- [x] Created 3 test projects (API.Tests, Application.Tests, Infrastructure.Tests)
- [x] Added all projects to solution
- [x] Configured project references
- [x] Added NuGet packages:
  - Serilog.AspNetCore (9.0.0) to Products.API
  - Asp.Versioning.Http (8.1.0) to Products.API
  - Asp.Versioning.Mvc.ApiExplorer (8.1.0) to Products.API
  - Microsoft.Data.SqlClient (6.1.2) to Products.Infrastructure
  - TUnit (0.90.6) to all test projects
  - Moq (4.20.72) to all test projects
  - Microsoft.AspNetCore.Mvc.Testing (9.0.10) to Products.API.Tests

### Phase 2: Core Infrastructure (IN PROGRESS)
- [x] Updated appsettings.json with Serilog configuration
- [x] Updated Program.cs with:
  - Serilog integration
  - API versioning configuration
  - Swagger/OpenAPI setup for v1 and v2
  - Request logging middleware
- [ ] Create DependencyInjection extension methods
- [ ] Create configuration options classes
- [ ] Create global exception handler

---

## 📋 Next Steps - Phase 2 Completion

### Files to Create

#### 1. Products.Application/DependencyInjection.cs
```bash
cd /Users/rodney.cabahug/Projects/Personal/Products/src/Products.Application
```

Create this file with service registration for application services.

#### 2. Products.Infrastructure/DependencyInjection.cs
```bash
cd /Users/rodney.cabahug/Projects/Personal/Products/src/Products.Infrastructure
```

Create this file with service registration for repositories and connection manager.

#### 3. Products.API/Extensions/ServiceCollectionExtensions.cs
Optional helper extensions for API-specific services.

---

## 📋 Phase 3: Data Layer Modernization

### Files to Migrate from Legacy Project

From `Products.Entity`:
1. **BaseEntity.cs** → `Products.Domain/Entities/BaseEntity.cs`
2. **ProductEntity.cs** → `Products.Domain/Entities/ProductEntity.cs`
3. **ProductOptionEntity.cs** → `Products.Domain/Entities/ProductOptionEntity.cs`

From `Products.Model`:
1. **ProductModel.cs** → `Products.Domain/Models/ProductModel.cs`
2. **ProductOptionModel.cs** → `Products.Domain/Models/ProductOptionModel.cs`

From `Products.Repository`:
1. **IRepository.cs** → `Products.Domain/Repositories/IRepository.cs`
2. **IProductRepository.cs** → `Products.Domain/Repositories/IProductRepository.cs`
3. **IProductOptionRepository.cs** → `Products.Domain/Repositories/IProductOptionRepository.cs`
4. **ConnectionManager.cs** → `Products.Infrastructure/Data/ConnectionManager.cs` (modernize)
5. **ProductRepository.cs** → `Products.Infrastructure/Repositories/ProductRepository.cs` (modernize)
6. **ProductOptionRepository.cs** → `Products.Infrastructure/Repositories/ProductOptionRepository.cs` (modernize)

### Modernization Changes Required:
- Use `await using` for disposables
- Add `CancellationToken` parameters
- Use `Microsoft.Data.SqlClient` instead of `System.Data.SqlClient`
- Use named parameters with explicit SqlDbType
- Use column name-based ordinals
- Enable nullable reference types

---

## 📋 Phase 4: Business Layer Migration

### Files to Migrate from Legacy Project

From `Products.Service`:
1. **IProductService.cs** → `Products.Application/Services/IProductService.cs`
2. **IProductOptionService.cs** → `Products.Application/Services/IProductOptionService.cs`
3. **ProductService.cs** → `Products.Application/Services/ProductService.cs` (remove AutoMapper)
4. **ProductOptionService.cs** → `Products.Application/Services/ProductOptionService.cs` (remove AutoMapper)

### New Files to Create:
1. `Products.Application/Mappings/ProductMappingExtensions.cs` - Manual mapping for Product
2. `Products.Application/Mappings/ProductOptionMappingExtensions.cs` - Manual mapping for ProductOption
3. `Products.Application/DTOs/ProductDTO.cs` - Copy from Products.API/DTOs
4. `Products.Application/DTOs/ProductOptionDTO.cs` - Copy from Products.API/DTOs
5. `Products.Application/DTOs/CollectionDTO.cs` - Copy from Products.API/DTOs

---

## 📋 Phase 5: API Layer Implementation

### Minimal API Endpoints to Create

#### V1 Endpoints:
1. `Products.API/Endpoints/v1/ProductsEndpoints.cs`
   - GET /api/v1/products (all + search by name)
   - GET /api/v1/products/{id}
   - POST /api/v1/products
   - PUT /api/v1/products/{id}
   - DELETE /api/v1/products/{id}

2. `Products.API/Endpoints/v1/ProductOptionsEndpoints.cs`
   - GET /api/v1/products/{productId}/options
   - GET /api/v1/products/{productId}/options/{id}
   - POST /api/v1/products/{productId}/options
   - PUT /api/v1/products/{productId}/options/{id}
   - DELETE /api/v1/products/{productId}/options/{id}

#### V2 Endpoints:
3. `Products.API/Endpoints/v2/ProductsEndpoints.cs`
   - Same as v1 with enhanced features

4. `Products.API/Endpoints/v2/ProductOptionsEndpoints.cs`
   - Same as v1 with enhanced features

#### Extension Method:
5. `Products.API/Endpoints/EndpointExtensions.cs`
   - `MapProductsEndpoints()` method
   - `MapProductOptionsEndpoints()` method

---

## 📋 Phase 6: Cross-Cutting Concerns

### Files to Create:
1. `Products.API/Middleware/GlobalExceptionHandler.cs` - IExceptionHandler implementation
2. `Products.API/Middleware/RequestResponseLoggingMiddleware.cs` - Optional detailed logging
3. Update Program.cs to register exception handler
4. Configure CORS if needed

---

## 📋 Phase 7: Testing Infrastructure

### Test Files to Create:

#### Unit Tests - Application:
1. `Products.Application.Tests/Services/ProductServiceTests.cs`
2. `Products.Application.Tests/Services/ProductOptionServiceTests.cs`

#### Unit Tests - Infrastructure:
3. `Products.Infrastructure.Tests/Repositories/ProductRepositoryTests.cs`
4. `Products.Infrastructure.Tests/Repositories/ProductOptionRepositoryTests.cs`

#### Integration Tests - API:
5. `Products.API.Tests/Integration/ProductsApiTests.cs`
6. `Products.API.Tests/Integration/ProductOptionsApiTests.cs`
7. `Products.API.Tests/WebApplicationFactory/CustomWebApplicationFactory.cs`

---

## 📋 Phase 8: Deployment Preparation

1. Update `.gitignore` for .NET 9
2. Create `Dockerfile` for containerization
3. Create `docker-compose.yml` for local development
4. Configure production appsettings
5. Add health checks endpoint
6. Performance testing
7. Security review

---

## 🔧 Quick Commands Reference

### Build Solution
```bash
cd /Users/rodney.cabahug/Projects/Personal/Products
dotnet build
```

### Run API
```bash
cd /Users/rodney.cabahug/Projects/Personal/Products/src/Products.API
dotnet run
```

### Run Tests
```bash
cd /Users/rodney.cabahug/Projects/Personal/Products
dotnet test
```

### Add Package
```bash
dotnet add package <PackageName>
```

---

## 📁 Current Solution Structure

```
Products/
├── Products.sln (NEW)
├── docs/
│   ├── plan.md (UPDATED with confirmed preferences)
│   ├── tasks.md
│   ├── SUMMARY.md
│   └── IMPLEMENTATION_STATUS.md (THIS FILE)
├── .http/
│   ├── products-v1.http (75 test cases)
│   ├── products-v2.http
│   ├── http-client.env.json
│   └── README.md
├── src/
│   ├── Products.API/ (ASP.NET Core Web API - .NET 9)
│   │   ├── Program.cs (UPDATED - Serilog, API versioning, Swagger)
│   │   ├── appsettings.json (UPDATED - Serilog config)
│   │   ├── appsettings.Development.json
│   │   └── [TODO: Endpoints/, Middleware/, Extensions/]
│   ├── Products.Application/ (Business Logic - .NET 9)
│   │   └── [TODO: Services/, DTOs/, Mappings/, DependencyInjection.cs]
│   ├── Products.Domain/ (Entities & Interfaces - .NET 9)
│   │   └── [TODO: Entities/, Models/, Repositories/]
│   └── Products.Infrastructure/ (Data Access - .NET 9)
│       └── [TODO: Repositories/, Data/, DependencyInjection.cs]
└── tests/
    ├── Products.Application.Tests/ (TUnit - .NET 9)
    ├── Products.Infrastructure.Tests/ (TUnit - .NET 9)
    └── Products.API.Tests/ (TUnit + Integration - .NET 9)
```

---

## 🎯 Immediate Next Action

**Phase 2 Continuation**: Create the core Domain entities and interfaces.

Run these commands in sequence:
```bash
# Navigate to Domain project
cd /Users/rodney.cabahug/Projects/Personal/Products/src/Products.Domain

# Remove default Class1.cs
rm Class1.cs

# Create directory structure
mkdir -p Entities Models Repositories
```

Then copy and modernize files from the legacy projects:
1. Copy entity files from old `Products.Entity` project
2. Copy model files from old `Products.Model` project  
3. Copy repository interfaces from old `Products.Repository` project
4. Update namespaces to `Products.Domain.*`
5. Enable nullable reference types where appropriate

---

## 📝 Notes

- **Database**: The connection string points to LocalDB with the .mdf file. Ensure the database file is accessible.
- **Stored Procedures**: All existing stored procedures in `src/Products.Database/` folder will be retained and used as-is.
- **Testing**: Use TUnit with `[Test]` attribute. Global usings are configured automatically.
- **Logging**: Logs will be written to `logs/products-api-{Date}.txt` with 7-day retention.

---

**Last Updated**: Phase 1 Complete, Phase 2 In Progress  
**Next Milestone**: Complete Phase 2 (Core Infrastructure), then begin Phase 3 (Data Layer)
