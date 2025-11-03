# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Products.sln", "./"]
COPY ["src/Products.API/Products.API.csproj", "src/Products.API/"]
COPY ["src/Products.Application/Products.Application.csproj", "src/Products.Application/"]
COPY ["src/Products.Domain/Products.Domain.csproj", "src/Products.Domain/"]
COPY ["src/Products.Infrastructure/Products.Infrastructure.csproj", "src/Products.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Products.API/Products.API.csproj"

# Copy remaining source code
COPY . .

# Build the application
WORKDIR "/src/src/Products.API"
RUN dotnet build "Products.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Products.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Products.API.dll"]
