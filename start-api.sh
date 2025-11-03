#!/bin/bash

# Products API - Quick Start Script
set -e

echo "🚀 Starting Products API with Docker..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Stop existing containers
echo "📦 Stopping existing containers..."
docker-compose down 2>/dev/null || true

# Start services
echo "🔨 Building and starting services..."
docker-compose up -d --build

# Wait for SQL Server to be healthy
echo "⏳ Waiting for SQL Server to be ready..."
timeout=60
elapsed=0
while [ $elapsed -lt $timeout ]; do
    if docker exec products-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -Q "SELECT 1" -C > /dev/null 2>&1; then
        echo "✅ SQL Server is ready!"
        break
    fi
    sleep 2
    elapsed=$((elapsed + 2))
    echo "   Still waiting... ($elapsed/$timeout seconds)"
done

if [ $elapsed -ge $timeout ]; then
    echo "❌ SQL Server failed to start within $timeout seconds"
    exit 1
fi

# Check if database exists
echo "🔍 Checking database..."
DB_EXISTS=$(docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -C -Q "SELECT COUNT(*) FROM sys.databases WHERE name = 'ProductsDB'" -h -1 2>/dev/null | tr -d ' ')

if [ "$DB_EXISTS" = "0" ]; then
    echo "📊 Setting up database..."
    docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -C < Products.Database/Setup-Database.sql
    
    # Create missing procedures if needed
    docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -C -d ProductsDB -Q "IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'RetrieveProducts') BEGIN CREATE PROCEDURE [dbo].[RetrieveProducts] AS BEGIN SET NOCOUNT ON; SELECT [Id], [Name], [Description], [Price], [DeliveryPrice] FROM [dbo].[Product] ORDER BY [Name] END END" > /dev/null 2>&1
    
    docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -C -d ProductsDB -Q "IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'RetrieveProductOptions') BEGIN CREATE PROCEDURE [dbo].[RetrieveProductOptions] AS BEGIN SET NOCOUNT ON; SELECT [Id], [ProductId], [Name], [Description] FROM [dbo].[ProductOption] ORDER BY [Name] END END" > /dev/null 2>&1
    
    echo "✅ Database setup complete!"
else
    echo "✅ Database already exists"
fi

# Wait for API to be ready
echo "⏳ Waiting for API to be ready..."
sleep 5
timeout=30
elapsed=0
while [ $elapsed -lt $timeout ]; do
    if curl -s http://localhost:8080/health > /dev/null 2>&1; then
        echo "✅ API is ready!"
        break
    fi
    sleep 2
    elapsed=$((elapsed + 2))
done

# Check container status
echo ""
echo "📊 Container Status:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" --filter "name=products"

# Test API endpoints
echo ""
echo "🧪 Testing API..."
HEALTH=$(curl -s http://localhost:8080/health | jq -r '.status' 2>/dev/null || echo "FAILED")
PRODUCTS=$(curl -s http://localhost:8080/api/v1/products | jq -r '.items | length' 2>/dev/null || echo "0")

echo "   Health: $HEALTH"
echo "   Products: $PRODUCTS items"

# Display URLs
echo ""
echo "🎉 Products API is running!"
echo ""
echo "📍 Endpoints:"
echo "   - API:         http://localhost:8080"
echo "   - Swagger:     http://localhost:8080/swagger"
echo "   - Health:      http://localhost:8080/health"
echo "   - SQL Server:  localhost:1433 (sa / YourStrong@Passw0rd)"
echo ""
echo "📚 Documentation:"
echo "   - Docker Guide:    ./DOCKER-GUIDE.md"
echo "   - API Docs:        http://localhost:8080/swagger"
echo ""
echo "🛠️  Useful Commands:"
echo "   - View logs:       docker-compose logs -f products-api"
echo "   - Stop services:   docker-compose down"
echo "   - Restart API:     docker-compose restart products-api"
echo ""
