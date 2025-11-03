#!/bin/bash
# Database Setup Script for Products API
# This script sets up the SQL Server database using Docker Compose

set -e  # Exit on error

echo "=========================================="
echo "Products API - Database Setup"
echo "=========================================="
echo ""

# Check if Docker is available
if ! command -v docker &> /dev/null; then
    echo "❌ Docker not found. Please install Docker Desktop."
    echo "Download from: https://www.docker.com/products/docker-desktop"
    exit 1
fi

echo "✓ Docker found"
echo ""

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
    echo "❌ docker-compose not found. Please install docker-compose."
    exit 1
fi

echo "✓ docker-compose found"
echo ""

# Start SQL Server using docker-compose
echo "Starting SQL Server container..."
docker-compose up -d sqlserver

echo ""
echo "Waiting for SQL Server to be ready..."
sleep 5

# Wait for SQL Server to be healthy
MAX_ATTEMPTS=30
ATTEMPT=0
while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
    if docker exec products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P 'YourStrong@Passw0rd' -Q "SELECT 1" -C &> /dev/null; then
        echo "✓ SQL Server is ready"
        break
    fi
    ATTEMPT=$((ATTEMPT + 1))
    echo "Waiting... (attempt $ATTEMPT/$MAX_ATTEMPTS)"
    sleep 2
done

if [ $ATTEMPT -eq $MAX_ATTEMPTS ]; then
    echo "❌ SQL Server failed to start after $MAX_ATTEMPTS attempts"
    exit 1
fi

echo ""
echo "Creating database and tables..."

# Create database if not exists
docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
    -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ProductsDB') CREATE DATABASE ProductsDB;"

# Run table creation scripts
echo "Creating Product table..."
docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P 'YourStrong@Passw0rd' -d ProductsDB -C \
    < Products.Database/Table.Product.sql

echo "Creating ProductOption table..."
docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P 'YourStrong@Passw0rd' -d ProductsDB -C \
    < Products.Database/Table.ProductOption.sql

# Run stored procedure scripts
echo "Creating stored procedures..."
for proc in Products.Database/Procedure.*.sql; do
    if [ -f "$proc" ]; then
        echo "  - $(basename "$proc")"
        docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P 'YourStrong@Passw0rd' -d ProductsDB -C \
            < "$proc"
    fi
done

echo ""
echo "=========================================="
echo "✅ Database setup complete!"
echo "=========================================="
echo ""
echo "Connection details:"
echo "  Server: localhost,1433"
echo "  Database: ProductsDB"
echo "  User: sa"
echo "  Password: YourStrong@Passw0rd"
echo ""
echo "You can now run the API with:"
echo "  ./start-api.sh"
echo ""
echo "Or run locally with:"
echo "  cd src/Products.API"
echo "  dotnet run"
echo ""
