# Products API

A modern REST API built with .NET 9, showcasing clean architecture, minimal APIs, and containerization.

## 🚀 Quick Start

### Using Docker (Recommended)
```bash
# Start everything with one command
./start-api.sh

# Or manually
docker-compose up -d

# Access the API
open http://localhost:8080/swagger
```

### Local Development
```bash
# Start SQL Server
docker-compose up -d sqlserver

# Setup database
./setup-database.sh

# Run API
dotnet run --project src/Products.API/Products.API.csproj

# Access the API
open http://localhost:5291/swagger
```

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (required for SQL Server)
- SQL Server 2022 runs via Docker (included in docker-compose.yml)

## 🏗️ Architecture

```
Products/
├── src/
│   ├── Products.API/              # API endpoints, middleware, Swagger
│   ├── Products.Application/      # Business logic, services
│   ├── Products.Domain/           # Core entities, exceptions
│   └── Products.Infrastructure/   # Data access, repositories
├── tests/
│   └── Products.Tests/            # Unit tests (TUnit)
└── Products.Database/             # SQL scripts, migrations
```

### Key Technologies

- **.NET 9** - Latest framework with minimal APIs
- **SQL Server 2022** - Database with stored procedures
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **Docker** - Containerization
- **TUnit** - Modern testing framework

## 📡 API Endpoints

### Products
- `GET /api/v1/products` - Get all products
- `GET /api/v1/products/{id}` - Get product by ID
- `GET /api/v1/products/search?name={name}` - Search products
- `POST /api/v1/products` - Create product
- `PUT /api/v1/products/{id}` - Update product
- `DELETE /api/v1/products/{id}` - Delete product

### Product Options
- `GET /api/v1/products/{productId}/options` - Get all options for product
- `GET /api/v1/products/{productId}/options/{id}` - Get option by ID
- `POST /api/v1/products/{productId}/options` - Create option
- `PUT /api/v1/products/{productId}/options/{id}` - Update option
- `DELETE /api/v1/products/{productId}/options/{id}` - Delete option

### System
- `GET /health` - Health check
- `GET /swagger` - Interactive API documentation

## 🔧 Configuration

### Connection Strings

**Development** (appsettings.Development.json):
```json
"ConnectionStrings": {
  "ProductsDatabase": "Server=localhost,1433;Database=ProductsDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
}
```

**Production** (appsettings.Production.json):
```json
"ConnectionStrings": {
  "ProductsDatabase": "Server=sqlserver,1433;Database=ProductsDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
}
```

### Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Development|Production
ASPNETCORE_URLS=http://+:8080
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~ProductServiceTests"
```

## 🐳 Docker

### Production Deployment
```bash
# Build and start
docker-compose up -d

# View logs
docker-compose logs -f products-api

# Stop
docker-compose down
```

### Development with Hot Reload
```bash
# Start with development settings
docker-compose -f docker-compose.dev.yml up -d

# API will reload on file changes
docker-compose -f docker-compose.dev.yml logs -f products-api
```

### Container Details

**API Container:**
- Port: 8080 (production) / 5291 (development)
- Health check: `/health`
- Auto-restart: enabled
- Depends on: SQL Server

**SQL Server Container:**
- Port: 1433
- Credentials: `sa` / `YourStrong@Passw0rd`
- Database: `ProductsDB`
- Persistent volume: `sqlserver-data`

## 📚 Documentation

- [Docker Guide](./docs/DOCKER-GUIDE.md) - Detailed Docker commands and troubleshooting
- [Database Quickstart](./docs/DATABASE-QUICKSTART.md) - Database setup and configuration
- [HTTP Test Results](./docs/HTTP-TEST-RESULTS.md) - API test validation results
- [Containerization Guide](./docs/CONTAINERIZATION_COMPLETE.md) - Complete containerization details
- [httpYac Fix](./docs/HTTPYAC-FIX.md) - HTTP test variable extraction fix
- [API Documentation](http://localhost:8080/swagger) - Interactive Swagger UI

## 🛠️ Development Workflow

### Initial Setup
```bash
# Clone repository
git clone <repository-url>
cd Products

# Start services
./start-api.sh
```

### Making Changes
```bash
# Create feature branch
git checkout -b feature/my-feature

# Make changes and test
dotnet test

# Run locally
dotnet run --project src/Products.API/Products.API.csproj

# Commit and push
git add .
git commit -m "feat: add my feature"
git push origin feature/my-feature
```

### Database Changes
```bash
# 1. Update stored procedures in Products.Database/
# 2. Apply changes
docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
  < Products.Database/Procedure.YourProcedure.sql

# 3. Test changes
curl http://localhost:8080/api/v1/products
```

## 🔍 Troubleshooting

### API won't start
```bash
# Check logs
docker-compose logs products-api

# Verify SQL Server
docker ps --filter name=sqlserver

# Restart
docker-compose restart products-api
```

### Database connection issues
```bash
# Test SQL Server
docker exec products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -Q "SELECT 1" -C

# Verify network
docker network inspect products-network
```

### Port conflicts
```bash
# Check what's using port
lsof -i :8080

# Use different port (edit docker-compose.yml)
ports:
  - "9090:8080"
```

## 📊 Performance

- **Connection Pooling:** 5-100 connections
- **Request Timeout:** 30s
- **Health Check:** 30s interval
- **Startup Time:** ~5-10s

## 🔐 Security

- ✅ SQL injection prevention (parameterized queries)
- ✅ CORS configuration
- ✅ Exception handling with Problem Details (RFC 7807)
- ✅ Health checks
- ⚠️ Change default SA password in production
- ⚠️ Use secrets management (Azure Key Vault, Docker Secrets)
- ⚠️ Enable HTTPS with valid certificates

## 📈 Roadmap

- [ ] Integration tests with TestContainers
- [ ] OpenTelemetry for distributed tracing
- [ ] Redis caching
- [ ] Rate limiting
- [ ] JWT authentication
- [ ] CI/CD pipeline
- [ ] Kubernetes deployment

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📧 Support

For issues and questions, please open an issue on GitHub.

