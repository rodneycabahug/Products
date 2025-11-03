# Products API - Docker Guide

## Quick Start

### Production Mode
```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f products-api

# Stop all services
docker-compose down
```

### Development Mode (with hot reload)
```bash
# Build and start with development settings
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f products-api

# Stop all services
docker-compose -f docker-compose.dev.yml down
```

## Service Endpoints

- **API (Production)**: http://localhost:8080
- **API (Development)**: http://localhost:5291
- **Swagger UI (Production)**: http://localhost:8080/swagger
- **Swagger UI (Development)**: http://localhost:5291/swagger
- **Health Check**: http://localhost:8080/health
- **SQL Server**: localhost:1433

## Container Architecture

### Services

1. **sqlserver**
   - Image: `mcr.microsoft.com/mssql/server:2022-latest`
   - Port: 1433
   - Credentials: sa / YourStrong@Passw0rd
   - Database: ProductsDB
   - Persistent volume: `sqlserver-data`

2. **products-api**
   - Built from `src/Products.API/Dockerfile`
   - Port: 8080 (production) / 5291 (development)
   - Depends on: SQL Server (waits for health check)
   - Auto-restart: unless-stopped (production only)

## Database Initialization

### First Time Setup
```bash
# Start SQL Server
docker-compose up -d sqlserver

# Wait for SQL Server to be ready (check logs)
docker-compose logs -f sqlserver

# Run database setup script
docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
  < Products.Database/Setup-Database.sql

# Verify database
docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
  -Q "USE ProductsDB; SELECT COUNT(*) FROM Product"
```

### Using Setup Script
```bash
# Make script executable (first time only)
chmod +x setup-database.sh

# Run automated setup
./setup-database.sh
```

## Development Workflow

### Building the API
```bash
# Build API image only
docker-compose build products-api

# Build without cache (clean rebuild)
docker-compose build --no-cache products-api

# Build with specific Docker Compose file
docker-compose -f docker-compose.dev.yml build products-api
```

### Running Tests
```bash
# Run tests locally (outside Docker)
dotnet test

# Run tests in build stage
docker build --target build -t products-api-test -f src/Products.API/Dockerfile .
docker run --rm products-api-test dotnet test
```

### Viewing Logs
```bash
# All services
docker-compose logs -f

# API only
docker-compose logs -f products-api

# SQL Server only
docker-compose logs -f sqlserver

# Last 100 lines
docker-compose logs --tail=100 products-api
```

### Accessing Containers
```bash
# Open shell in API container
docker exec -it products-api /bin/bash

# Open SQL Server command line
docker exec -it products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C
```

## Configuration

### Environment Variables

#### SQL Server
- `ACCEPT_EULA`: Y (required)
- `SA_PASSWORD`: YourStrong@Passw0rd
- `MSSQL_PID`: Developer (free edition)

#### API (Production)
- `ASPNETCORE_ENVIRONMENT`: Production
- `ASPNETCORE_URLS`: http://+:8080
- `ConnectionStrings__ProductsDatabase`: Configured in appsettings.Production.json

#### API (Development)
- `ASPNETCORE_ENVIRONMENT`: Development
- `ASPNETCORE_URLS`: http://+:8080
- Hot reload enabled via `dotnet watch`

### Connection Strings

#### From Host Machine
```
Server=localhost,1433;Database=ProductsDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
```

#### From API Container (Production)
```
Server=sqlserver,1433;Database=ProductsDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
```

## Health Checks

### API Health Check
```bash
# Production
curl http://localhost:8080/health

# Development
curl http://localhost:5291/health

# Expected response
{"status":"Healthy","timestamp":"2025-11-03T06:40:34.796374Z"}
```

### SQL Server Health Check
```bash
# Check if SQL Server is ready
docker exec products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -Q "SELECT 1" -C
```

### Container Health Status
```bash
# View health status
docker ps

# Detailed health check history
docker inspect products-api --format='{{json .State.Health}}' | jq
```

## Troubleshooting

### API Won't Start
```bash
# Check API logs
docker-compose logs products-api

# Check if SQL Server is healthy
docker ps --filter name=sqlserver

# Restart API
docker-compose restart products-api
```

### Database Connection Issues
```bash
# Verify SQL Server is running
docker exec products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -Q "SELECT @@VERSION" -C

# Check network connectivity
docker network inspect products-network

# Verify connection string in container
docker exec products-api cat /app/appsettings.Production.json
```

### Port Conflicts
```bash
# Check what's using port 8080
lsof -i :8080

# Use different port (edit docker-compose.yml)
ports:
  - "9090:8080"  # Map to different host port
```

### Clean Rebuild
```bash
# Stop all containers
docker-compose down

# Remove containers, networks, and images
docker-compose down --rmi all --volumes

# Rebuild from scratch
docker-compose build --no-cache
docker-compose up -d
```

## Performance Optimization

### Connection Pooling
API uses optimized connection string:
- Min Pool Size: 5
- Max Pool Size: 100
- Connection Timeout: 30s

### Container Resources
```yaml
# Add resource limits (docker-compose.yml)
products-api:
  deploy:
    resources:
      limits:
        cpus: '2'
        memory: 1G
      reservations:
        cpus: '0.5'
        memory: 512M
```

## Security Considerations

### Production Deployment
1. Change default SA password
2. Use secrets management (Docker Secrets, Azure Key Vault)
3. Enable HTTPS with valid certificates
4. Configure firewall rules
5. Use read-only file system where possible
6. Scan images for vulnerabilities

### Example with Docker Secrets
```bash
# Create secret
echo "YourStrong@Passw0rd" | docker secret create sa_password -

# Update docker-compose.yml
secrets:
  sa_password:
    external: true
```

## Maintenance

### Backup Database
```bash
# Backup to host
docker exec products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
  -Q "BACKUP DATABASE ProductsDB TO DISK='/var/opt/mssql/backup/ProductsDB.bak'"

# Copy backup to host
docker cp products-sqlserver:/var/opt/mssql/backup/ProductsDB.bak ./backups/
```

### Update Images
```bash
# Pull latest base images
docker-compose pull

# Rebuild with new images
docker-compose up -d --build
```

### Clean Up
```bash
# Remove unused images
docker image prune -a

# Remove unused volumes
docker volume prune

# Remove unused networks
docker network prune
```

## CI/CD Integration

### GitHub Actions Example
```yaml
name: Build and Push Docker Image

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build image
        run: docker build -f src/Products.API/Dockerfile -t products-api .
      - name: Run tests
        run: docker run products-api dotnet test
```

## Monitoring

### Container Stats
```bash
# Real-time stats
docker stats products-api

# Resource usage
docker exec products-api ps aux
```

### Application Logs
API uses Serilog for structured logging. Logs are output to console and can be collected by:
- Docker logs
- Log aggregation tools (ELK, Seq, Application Insights)
- Cloud monitoring (Azure Monitor, AWS CloudWatch)
