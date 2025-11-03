# Database Setup Guide

## Prerequisites
- Docker Desktop (recommended)
- Or SQL Server 2019+ for Windows

## ⭐ Option 1: Using Docker (Recommended - Cross-Platform)

Docker SQL Server works on Windows, macOS, and Linux.

### Step 1: Start SQL Server with Docker Compose
```bash
# From repository root
docker-compose up -d sqlserver
```

Or use the automated setup script:
```bash
./setup-database.sh
```

### Step 2: Verify Connection
```bash
docker exec -it products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C -Q "SELECT @@VERSION"
```

### Step 3: Connection String
Already configured in `src/Products.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "ProductsDatabase": "Server=localhost,1433;Database=ProductsDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
  }
}
```

For containerized API (Production):
```json
{
  "ConnectionStrings": {
    "ProductsDatabase": "Server=sqlserver,1433;Database=ProductsDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
  }
}
```

---

## Option 2: Using LocalDB (Windows Only - Deprecated)

**⚠️ LocalDB is deprecated. Use Docker instead for cross-platform compatibility.**

LocalDB is a lightweight version of SQL Server Express (Windows-only).

### Step 1: Verify LocalDB Installation
```bash
sqllocaldb info
```

If not installed, install SQL Server Express with LocalDB from:
https://www.microsoft.com/en-us/sql-server/sql-server-downloads

### Step 2: Create LocalDB Instance
```bash
sqllocaldb create ProductsDB
sqllocaldb start ProductsDB
sqllocaldb info ProductsDB
```

### Step 3: Update Connection String
In `src/Products.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "ProductsDatabase": "Server=(localdb)\\ProductsDB;Database=ProductsDB;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

### Step 4: Run Database Setup Script
```bash
# From repository root
sqlcmd -S "(localdb)\ProductsDB" -i src/Products.Database/Setup-Database.sql
```

Or use SQL Server Management Studio (SSMS):
1. Connect to `(localdb)\ProductsDB`
2. Open `src/Products.Database/Setup-Database.sql`
3. Execute (F5)

---

## Option 3: Using SQL Server Express (Windows)

### Step 1: Install SQL Server Express
Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

### Step 2: Update Connection String
In `src/Products.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "ProductsDatabase": "Server=localhost\\SQLEXPRESS;Database=ProductsDB;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

### Step 3: Run Database Setup Script
```bash
# From repository root
sqlcmd -S "localhost\SQLEXPRESS" -i src/Products.Database/Setup-Database.sql
```

---

## Option 3: Using Full SQL Server

### Update Connection String
In `src/Products.API/appsettings.Development.json`:

**Windows Authentication:**
```json
{
  "ConnectionStrings": {
    "ProductsDatabase": "Server=localhost;Database=ProductsDB;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

**SQL Server Authentication:**
```json
{
  "ConnectionStrings": {
    "ProductsDatabase": "Server=localhost;Database=ProductsDB;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
  }
}
```

### Run Database Setup Script
```bash
sqlcmd -S "localhost" -i src/Products.Database/Setup-Database.sql
```

---

## Database Setup Script Contents

The `Setup-Database.sql` script will:
1. Create the `ProductsDB` database
2. Create `Product` table
3. Create `ProductOption` table with foreign key
4. Create all 12 stored procedures:
   - `RetrieveProducts`
   - `RetrieveProductById`
   - `RetrieveProductsByName`
   - `CreateProduct`
   - `UpdateProduct`
   - `DeleteProduct`
   - `RetrieveProductOptions`
   - `RetrieveProductOptionsById`
   - `RetrieveProductOptionsByProductId`
   - `CreateProductOption`
   - `UpdateProductOption`
   - `DeleteProductOption`
5. Insert sample data (optional)

---

## Verify Database Setup

### Using sqlcmd
```bash
sqlcmd -S "(localdb)\ProductsDB" -d ProductsDB -Q "SELECT * FROM Product"
```

### Using SQL Server Management Studio
1. Connect to your SQL Server instance
2. Navigate to Databases → ProductsDB
3. Run query:
```sql
-- Check tables
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'

-- Check stored procedures
SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'

-- Check sample data
SELECT * FROM Product
SELECT * FROM ProductOption
```

---

## Test the API with Database

### 1. Start the API
```bash
cd src/Products.API
dotnet run
```

### 2. Access Swagger UI
Open browser: `https://localhost:{port}/swagger`

### 3. Test Endpoints
Use the Swagger UI or .http files in `.http/` directory

### 4. Quick Test with curl
```bash
# Health check
curl https://localhost:{port}/health

# Get all products
curl https://localhost:{port}/api/v1/products

# Create a product
curl -X POST https://localhost:{port}/api/v1/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Product",
    "description": "Test Description",
    "price": 99.99,
    "deliveryPrice": 5.99
  }'
```

---

## Troubleshooting

### Connection String Issues

**Error: "Connection string 'ProductsDatabase' not found"**
- Ensure `appsettings.Development.json` exists with the ConnectionStrings section
- Check that the file is in `src/Products.API/` directory

**Error: "Cannot open database 'ProductsDB'"**
- Database doesn't exist, run Setup-Database.sql
- Check SQL Server instance is running: `sqlcmd -S "(localdb)\ProductsDB" -Q "SELECT @@VERSION"`

**Error: "Login failed for user"**
- For Windows Authentication: Ensure your Windows user has access
- For SQL Authentication: Verify username/password are correct

**Error: "A network-related or instance-specific error"**
- SQL Server instance is not running
- Check instance name matches connection string
- For LocalDB: `sqllocaldb start ProductsDB`
- For SQL Express: Check SQL Server service is running

### Stored Procedure Issues

**Error: "Could not find stored procedure 'RetrieveProducts'"**
- Stored procedures not created, run Setup-Database.sql
- Verify with: `SELECT * FROM sys.procedures WHERE name LIKE '%Product%'`

### Permission Issues

**Error: "The CREATE DATABASE statement is not allowed"**
- User lacks permissions, use an account with db_creator role
- Or manually create database first, then run table/procedure scripts

---

## Alternative: Use Individual SQL Files

If you prefer to run scripts individually:

```bash
```bash
# Create database manually first, then:
sqlcmd -S "(localdb)\ProductsDB" -d ProductsDB -i src/Products.Database/Table.Product.sql
sqlcmd -S "(localdb)\ProductsDB" -d ProductsDB -i src/Products.Database/Table.ProductOption.sql

# Create all stored procedures
sqlcmd -S "(localdb)\ProductsDB" -d ProductsDB -i src/Products.Database/Procedure.CreateProduct.sql
sqlcmd -S "(localdb)\ProductsDB" -d ProductsDB -i src/Products.Database/Procedure.RetrieveProducts.sql
```
# ... (repeat for all procedures)
```

---

## Production Configuration

For production, use environment variables or user secrets:

### Using User Secrets (Development)
```bash
cd src/Products.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:ProductsDatabase" "your-connection-string"
```

### Using Environment Variables (Production)
```bash
export ConnectionStrings__ProductsDatabase="Server=prod-server;Database=ProductsDB;..."
```

Or in `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "ProductsDatabase": "Server=prod-server;Database=ProductsDB;User Id=app_user;Password=xxx;TrustServerCertificate=true;Encrypt=true"
  }
}
```

---

## Next Steps

Once database is configured:
1. ✅ Run `dotnet run --project src/Products.API/Products.API.csproj`
2. ✅ Test endpoints via Swagger UI
3. ✅ Run integration tests: `dotnet test`
4. ✅ Use .http files for comprehensive API testing

---

*For more information, see the main documentation in `/docs`*
