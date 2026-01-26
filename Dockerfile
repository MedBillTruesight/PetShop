# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files (in dependency order for better layer caching)
COPY PetShop.Domain/PetShop.Domain.csproj PetShop.Domain/
COPY PetShop.Application/PetShop.Application.csproj PetShop.Application/
COPY PetShop.Infrastructure/PetShop.Infrastructure.csproj PetShop.Infrastructure/
COPY PetShop.Api/PetShop.Api.csproj PetShop.Api/

# Restore dependencies for API project (will restore all dependencies)
WORKDIR /src/PetShop.Api
RUN dotnet restore

# Copy all source files
COPY PetShop.Domain/ ../PetShop.Domain/
COPY PetShop.Application/ ../PetShop.Application/
COPY PetShop.Infrastructure/ ../PetShop.Infrastructure/
COPY PetShop.Api/ .

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET 8.0 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application from build stage
COPY --from=publish /app/publish .

# Create a non-root user and change ownership of /app
RUN groupadd -r appuser && useradd -r -g appuser appuser && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port 7052 (can be overridden via environment variable)
EXPOSE 7052

# Set environment variables
ENV ASPNETCORE_URLS=http://+:7052
ENV ASPNETCORE_ENVIRONMENT=Production

# Set the entry point
ENTRYPOINT ["dotnet", "PetShop.Api.dll"]