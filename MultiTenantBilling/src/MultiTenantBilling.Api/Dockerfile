# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the solution and project files
COPY ["MultiTenantBilling.sln", "./"]
COPY ["src/MultiTenantBilling.Api/MultiTenantBilling.Api.csproj", "src/MultiTenantBilling.Api/"]
COPY ["src/MultiTenantBilling.Application/MultiTenantBilling.Application.csproj", "src/MultiTenantBilling.Application/"]
COPY ["src/MultiTenantBilling.Domain/MultiTenantBilling.Domain.csproj", "src/MultiTenantBilling.Domain/"]
COPY ["src/MultiTenantBilling.Infrastructure/MultiTenantBilling.Infrastructure.csproj", "src/MultiTenantBilling.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/MultiTenantBilling.Api/MultiTenantBilling.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet publish "src/MultiTenantBilling.Api/MultiTenantBilling.Api.csproj" -c Release -o /app/publish

# Use the official .NET 9.0 runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser
USER appuser

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "MultiTenantBilling.Api.dll"]