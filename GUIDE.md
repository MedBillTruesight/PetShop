
# PetShop API - Quick Start Guide

## 🚀 Quick Start Options

### Docker
The project has been dockerized and can be run using docker compose command as below (requires docker to be installed)

```bash
# 1. Clone and navigate to project
git clone <repository-url>
cd {FolderName}

# 3. Build and run with Docker Compose
docker-compose up --build

# 4. Access the API at: http://localhost:5144
```

## 📊 Testing the API

### Using Postman
**Postman Collection**: [Click here to access](https://marville-001.postman.co/workspace/TRD~7c9c9c32-e597-4949-92b4-a9e5309d2d3f/collection/9272590-85df55ee-3793-4bc1-a822-21c997814ed9?action=share&creator=9272590)

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal
```

## 📁 Project Structure Overview

```
PetShop/
├── PetShop.API/         # 🎯 Web API (Controllers, Startup)
├── PetShop.Application/ # 🔧 Business Logic (Services, DTOs)
├── PetShop.Domain/      # 💎 Core Entities (Models, Interfaces)
├── PetShop.Infrastructure/ # 🔌 Data Access (Repositories, DbContext)
└── PetShop.Test/        # 🧪 Unit & Integration Tests
```

- Use the Postman collection for comprehensive API testing
- The application uses an in-memory database that resets on restart

POSTMAN_Collection = https://marville-001.postman.co/workspace/TRD~7c9c9c32-e597-4949-92b4-a9e5309d2d3f/collection/9272590-85df55ee-3793-4bc1-a822-21c997814ed9?action=share&creator=9272590