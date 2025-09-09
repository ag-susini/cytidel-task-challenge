# Tasker API - .NET 8 Web API Backend

## 📋 Description

The Tasker API is a modern .NET 8 Web API built with Clean Architecture principles and a custom CQRS implementation. It provides comprehensive task management capabilities with real-time updates, advanced validation, and file-based audit logging for critical operations.

### Architecture Highlights
- **Custom CQRS Pattern**: In-house command/query dispatcher (not MediatR)
- **Clean Architecture**: Clear separation between Domain, Application, Infrastructure, and API layers
- **Domain-Driven Design**: Rich domain models with business logic encapsulation
- **Repository Pattern**: Abstract data access for testability
- **Event-Driven**: Domain events for critical task changes
- **Audit Trail**: File-based critical event logging via Serilog

## 🛠️ Prerequisites

- **.NET 8 SDK** or later - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker & Docker Compose** - For running PostgreSQL
- **IDE**: Visual Studio 2022, VS Code with C# extension, or JetBrains Rider

## 🚀 Setup Instructions

### 1. Infrastructure Dependencies

Start the required database using Docker Compose:

```bash
cd ../deploy
docker compose -f docker-compose.infra.yml up -d
```

This starts:
- **PostgreSQL**: Port 5432 (main data storage)
- **PgAdmin**: Port 5050 (optional database GUI)

### 2. Install Dependencies

```bash
cd api
dotnet restore
```

### 3. Configure Application

The API uses `appsettings.Development.json` for local development. Key configurations:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Database=tasker;Username=tasker;Password=tasker"
  },
  "Jwt": {
    "SigningKey": "your-secret-key-at-least-32-characters-long",
    "Issuer": "TaskerAPI",
    "Audience": "TaskerClient",
    "ExpiryMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    }
  }
}
```

### 4. Run the API

```bash
dotnet run --project src/Tasker.Api
```

The API will:
1. Apply database migrations automatically on startup
2. Seed initial data if configured
3. Start listening on http://localhost:5080
4. Swagger UI available at http://localhost:5080/swagger

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and receive JWT tokens
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout and invalidate refresh token

### Tasks (Protected - Requires Authentication)
- `GET /api/tasks` - Get paginated task list with filtering
  - Query params: `page`, `pageSize`, `status`, `priority`, `search`, `sort`
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update existing task
- `DELETE /api/tasks/{id}` - Delete task (hard delete)
- `GET /api/tasks/stats` - Get task statistics

### Real-time Hub
- `/hubs/tasks` - SignalR hub for real-time notifications

## 🏗️ Project Structure

```
api/
├── src/
│   ├── Tasker.Api/                    # Web API Layer
│   │   ├── Controllers/               # REST endpoints
│   │   ├── Hubs/                      # SignalR hubs
│   │   ├── Middleware/                # Custom middleware
│   │   ├── Extensions/                # Service extensions
│   │   ├── Services/                  # API-specific services
│   │   └── Tasker.Api.http            # HTTP test file for API testing
│   │
│   ├── Tasker.Application/            # Application Layer
│   │   ├── Commands/                  # Command definitions
│   │   │   └── Handlers/              # Command handlers
│   │   ├── Queries/                   # Query definitions
│   │   ├── DTOs/                      # Data transfer objects
│   │   ├── Validators/                # FluentValidation validators
│   │   ├── EventHandlers/             # Domain event handlers
│   │   └── Services/                  # Application services
│   │
│   ├── Tasker.Domain/                 # Domain Layer
│   │   ├── Entities/                  # Domain entities
│   │   ├── Enums/                     # Domain enumerations
│   │   ├── Events/                    # Domain events
│   │   └── Factories/                 # Entity factories
│   │
│   ├── Tasker.Infrastructure/         # Infrastructure Layer
│   │   ├── Persistence/               # EF Core implementation
│   │   │   ├── Configurations/        # Entity configurations
│   │   │   ├── Migrations/            # Database migrations
│   │   │   └── Queries/Handlers/      # Query handlers
│   │   ├── Repositories/              # Repository implementations
│   │   ├── Adapters/                  # External service adapters
│   │   └── Extensions/                # Infrastructure extensions
│   │
│   ├── Tasker.Shared/                 # Shared Implementation
│   │   ├── Commands/                  # Command dispatcher
│   │   └── Queries/                   # Query dispatcher
│   │
│   └── Tasker.Shared.Abstractions/    # Shared Contracts
│       ├── Commands/                  # Command interfaces
│       └── Queries/                   # Query interfaces
│
└── tests/
    └── Tasker.Application.Tests/      # Unit tests
```

## 🔧 Special Features

### Custom CQRS Implementation
The API uses a custom-built CQRS pattern with:
- `InMemoryCommandDispatcher` - Routes commands to handlers
- `InMemoryQueryDispatcher` - Routes queries to handlers
- Pure CQRS: Commands return void, Queries return data
- Validation pipeline behavior using FluentValidation

### Middleware Pipeline
1. **Request Logging Middleware** - Logs all HTTP requests with timing
2. **Exception Handler** - Global exception handling with ProblemDetails
3. **Rate Limiting** - Per-IP rate limiting to prevent abuse
4. **Authentication/Authorization** - JWT bearer token validation

### Critical Event Logging
When high-priority tasks (High/Critical) are created or updated:
1. Domain event `HighPriorityTaskChanged` is raised
2. Event handler logs to file via Serilog
3. SignalR notification sent for real-time updates
4. Structured log written to `logs/critical.log`

### Database Features
- **PostgreSQL**: Main data storage with Entity Framework Core
- **Automatic Migrations**: Applied on startup
- **Shadow Properties**: `CreatedAt`, `UpdatedAt` auto-managed
- **Indexes**: Optimized for common query patterns
- **Hard Delete**: Tasks are permanently deleted (no soft delete)

## 🧪 Testing

### Run Unit Tests
```bash
cd api/tests
dotnet test
```

### Test Coverage
- Command handlers with validation
- Query handlers with filtering
- Priority change detection
- Validation behaviors

### API Testing with HTTP Files
Use the built-in `Tasker.Api.http` file in VS Code, Visual Studio, or JetBrains Rider:

1. Open `src/Tasker.Api/Tasker.Api.http`
2. Follow the testing flow:
   - Register a new user
   - Login to get tokens
   - Create, update, and delete tasks
   - Test various scenarios

The file includes:
- Complete authentication flow
- All CRUD operations
- Test scenarios for validation
- Critical event triggering examples

## 📝 Development Notes

### Adding New Features

1. **New Command/Query**:
   - Define in `Tasker.Application/Commands` or `Queries`
   - Create handler in respective `Handlers` folder
   - Add validator in `Validators` folder
   - Register in DI container

2. **New Entity**:
   - Create in `Tasker.Domain/Entities`
   - Add configuration in `Infrastructure/Persistence/Configurations`
   - Create repository interface and implementation
   - Add migration: `dotnet ef migrations add [Name]`

3. **New Endpoint**:
   - Add controller action in `Tasker.Api/Controllers`
   - Dispatch command/query using injected dispatchers
   - Add Swagger annotations for documentation

### Common Commands

```bash
# Add migration
dotnet ef migrations add [MigrationName] -p src/Tasker.Infrastructure -s src/Tasker.Api

# Update database
dotnet ef database update -p src/Tasker.Infrastructure -s src/Tasker.Api

# Run with watch mode
dotnet watch run --project src/Tasker.Api

# Build for production
dotnet publish src/Tasker.Api -c Release -o ./publish
```

## 🐛 Troubleshooting

### Database Connection Issues
- Ensure PostgreSQL is running: `docker ps`
- Check connection string in `appsettings.Development.json`
- Verify network connectivity to localhost:5432

### Migration Issues
- Drop database and recreate: `dotnet ef database drop`
- Remove migrations folder and regenerate

### JWT Authentication Issues
- Ensure SigningKey is at least 32 characters
- Check token expiry settings
- Verify CORS configuration for frontend origin

### Critical Event Logging
- Check `logs/critical.log` exists and is writable
- Verify Serilog configuration in `appsettings.json`
- High/Critical priority tasks trigger logging

## 📦 Docker Deployment

Build and run the API in Docker:

```bash
cd api/src/Tasker.Api
docker build -t tasker-api .
docker run -p 5080:8080 tasker-api
```

Or use Docker Compose from the deploy folder for the complete stack.

## 🔗 Related Documentation

- [Main Project README](../README.md)
- [Task Implementation Plan](./src/TASKS.md)
- [Deployment Guide](../deploy/README.md)

---

Built with .NET 8, Clean Architecture, and Custom CQRS