# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy solution and projects
COPY *.sln ./
COPY PetShop.API/*.csproj ./PetShop.API/
COPY PetShop.Application/*.csproj ./PetShop.Application/
COPY PetShop.Domain/*.csproj ./PetShop.Domain/
COPY PetShop.Infrastructure/*.csproj ./PetShop.Infrastructure/
COPY PetShop.Test/*.csproj ./PetShop.Test/

# Restore
RUN dotnet restore

# Copy everything
COPY . ./

# Publish
RUN dotnet publish PetShop.API/PetShop.API.csproj -c Release -o /app/out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published app
COPY --from=build-env /app/out ./

# Create Images directory
RUN mkdir -p /app/Images

# Optional: curl for healthchecks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "PetShop.API.dll"]