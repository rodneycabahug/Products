# Database Configuration - Quick Start

## 🚀 Quick Setup (Recommended for macOS/Linux)

### Using Docker Compose

1. **Start SQL Server**
```bash
docker-compose up -d
```

2. **Wait for SQL Server to be ready** (about 10-15 seconds)
```bash
docker-compose logs -f sqlserver
# Wait until you see: "SQL Server is now ready for client connections"
# Press Ctrl+C to exit logs
```

3. **Run database setup script**
```bash
docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
  < src/Products.Database/Setup-Database.sql
```

4. **Update connection string** in `src/Products.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "ProductsDatabase": "Server=localhost,1433;Database=ProductsDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
  }
}
```

5. **Run the API**
```bash
cd src/Products.API
dotnet run
```

### Stop SQL Server
```bash
docker-compose down
```

### Stop and remove data
```bash
docker-compose down -v
```

---

## 💻 Alternative: Manual Docker Setup

```bash
# Start SQL Server
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourStrong@Passw0rd' \
  -p 1433:1433 --name products-sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest

# Wait 10 seconds for startup
sleep 10

# Run setup script
docker exec -i products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
  < src/Products.Database/Setup-Database.sql
```

---

## 🪟 Windows - LocalDB Setup (Deprecated)

**⚠️ LocalDB is deprecated. Use Docker SQL Server instead for cross-platform compatibility.**

If you must use LocalDB on Windows:

1. **Check LocalDB installation**
```powershell
sqllocaldb info
```

2. **Start LocalDB**
```powershell
sqllocaldb start MSSQLLocalDB
```

3. **Run setup script**
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -i src\Products.Database\Setup-Database.sql
```

4. **Update connection string** in `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "ProductsDatabase": "Server=(localdb)\\MSSQLLocalDB;Database=ProductsDB;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

**Note**: LocalDB is Windows-only and not available on macOS/Linux. Docker is the recommended approach.

---

## 🔍 Verify Database Setup

### Using Docker (Recommended)
```bash
docker exec -it products-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
  -Q "USE ProductsDB; SELECT COUNT(*) AS ProductCount FROM Product; SELECT COUNT(*) AS OptionCount FROM ProductOption;"
```

### Using sqlcmd (LocalDB - Deprecated)
```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d ProductsDB \
  -Q "SELECT COUNT(*) AS ProductCount FROM Product; SELECT COUNT(*) AS OptionCount FROM ProductOption;"
```

Expected output:
```
ProductCount
------------
3

OptionCount
-----------
12
```

---

## 🧪 Test the API

1. **Start the API**
```bash
cd src/Products.API
dotnet run
```

2. **Test health endpoint**
```bash
curl http://localhost:5000/health
```

3. **Get all products**
```bash
curl http://localhost:5000/api/v1/products
```

4. **Access Swagger UI**
```
http://localhost:5000/swagger
```

---

## 🔧 Troubleshooting

### Docker: "Cannot connect to SQL Server"
```bash
# Check if container is running
docker ps

# Check logs
docker logs products-sqlserver

# Restart container
docker restart products-sqlserver
```

### API: "Connection string not found"
- Ensure `appsettings.Development.json` has the `ConnectionStrings` section
- Check the connection string name matches: `ProductsDatabase`

### API: "Cannot open database ProductsDB"
- Database not created, run `Setup-Database.sql` script
- Verify with: `docker exec products-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -Q "SELECT name FROM sys.databases" -C`

### Docker: "Port 1433 already in use"
- Another SQL Server instance is running
- Stop it or change port in docker-compose.yml: `"1434:1433"`
- Update connection string: `Server=localhost,1434;...`

---

## 📝 Connection String Examples

### LocalDB (Windows)
```
Server=(localdb)\MSSQLLocalDB;Database=ProductsDB;Integrated Security=true;TrustServerCertificate=true
```

### Docker (macOS/Linux/Windows)
```
Server=localhost,1433;Database=ProductsDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
```

### SQL Server Express
```
Server=localhost\SQLEXPRESS;Database=ProductsDB;Integrated Security=true;TrustServerCertificate=true
```

### Azure SQL Database
```
Server=tcp:yourserver.database.windows.net,1433;Database=ProductsDB;User ID=yourusername;Password=yourpassword;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
```

---

## 🎯 Next Steps

Once database is configured and running:

1. ✅ Run the API: `dotnet run --project src/Products.API/Products.API.csproj`
2. ✅ Test endpoints via Swagger: `http://localhost:5000/swagger`
3. ✅ Run tests: `dotnet test`
4. ✅ Use .http files in `.http/` directory for comprehensive testing

---

*See `src/Products.Database/DATABASE-SETUP.md` for detailed documentation*
