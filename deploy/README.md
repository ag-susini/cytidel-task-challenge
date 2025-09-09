# Docker Deployment Guide

## 📋 Description

This directory contains Docker Compose configurations for deploying the Tasker application. It supports both full-stack deployment and infrastructure-only setups for local development.

## 🚀 Quick Start - Full Application

Run the complete application stack (PostgreSQL + API + Frontend):

```bash
cd deploy

# Build images first (required on first run)
docker compose build

# Start all services
docker compose up -d
```

**First-time setup**: The `build` step is required to create the API and frontend Docker images from source code.

### Access Points
- **Frontend (Main Entry)**: http://localhost:3000
- **API Swagger**: http://localhost:5080/swagger
- **PostgreSQL**: localhost:5432 (for database tools)

## 🛠️ Infrastructure Only (Development Mode)

For local development with hot-reload, run only the database infrastructure:

```bash
cd deploy
docker compose -f docker-compose.infra.yml up -d
```

### Services Started
- **PostgreSQL**: localhost:5432
  - Database: `tasker`
  - Username: `tasker`
  - Password: `tasker`
- **PgAdmin**: http://localhost:5050 (optional database GUI)
  - Email: `admin@tasker.com`
  - Password: `admin`

Then run the API and Frontend locally:
```bash
# Terminal 1 - API
cd api
dotnet run --project src/Tasker.Api

# Terminal 2 - Frontend
cd web
npm run dev
```

## ⚙️ Configuration

### Docker Compose Files
- **docker-compose.yml**: Full application stack (PostgreSQL + API + Frontend)
- **docker-compose.infra.yml**: Infrastructure only (PostgreSQL + PgAdmin)

### Environment Variables

Create a `.env` file in the deploy directory (optional):

```env
# PostgreSQL Configuration
POSTGRES_USER=tasker
POSTGRES_PASSWORD=tasker
POSTGRES_DB=tasker

# PgAdmin Configuration (optional)
PGADMIN_DEFAULT_EMAIL=admin@tasker.com
PGADMIN_DEFAULT_PASSWORD=admin

# API Configuration
JWT_SECRET=your-secret-key-at-least-32-characters-long
ASPNETCORE_ENVIRONMENT=Development
```

## 🛑 Managing Services

### Stop Services
```bash
# Stop all services (preserves data)
docker compose down

# Stop infrastructure only
docker compose -f docker-compose.infra.yml down

# Stop and remove volumes (⚠️ WARNING: Deletes all data)
docker compose down -v
```

### View Logs
```bash
# View all logs
docker compose logs

# View specific service logs
docker compose logs api
docker compose logs postgres
docker compose logs frontend

# Follow logs in real-time
docker compose logs -f [service-name]
```

### Restart Services
```bash
# Restart all services
docker compose restart

# Restart specific service
docker compose restart api
```

## 🔄 Development Workflow

### Option 1: Infrastructure Only (Recommended for Development)
Best for active development with hot-reload:

1. Start infrastructure: `docker compose -f docker-compose.infra.yml up -d`
2. Run API locally: `dotnet run --project ../api/src/Tasker.Api`
3. Run frontend locally: `npm run dev --prefix ../web`

**Benefits:**
- Instant code changes without rebuilding containers
- Full debugging capabilities
- Faster iteration cycles

### Option 2: Full Stack Docker (Production-like)
Best for testing the complete deployment:

1. Build all images: `docker compose build`
2. Start everything: `docker compose up -d`
3. Access via http://localhost:3000

**Benefits:**
- Tests actual deployment configuration
- Validates Dockerfile builds
- Ensures all services integrate correctly

## 🏥 Health Checks

The deployment includes health checks for reliability:

| Service | Health Check | Interval | Timeout | Retries |
|---------|-------------|----------|---------|---------|
| PostgreSQL | `pg_isready -U tasker` | 10s | 5s | 5 |
| API | HTTP GET `/health` | 30s | 10s | 3 |
| Frontend | HTTP GET `/` | 30s | 10s | 3 |

## 🐛 Troubleshooting

### Common Issues

#### Port Conflicts
```bash
# Check what's using a port
lsof -i :5432  # PostgreSQL
lsof -i :5080  # API
lsof -i :3000  # Frontend

# Alternative: Change ports in docker-compose.yml
```

#### Build Failures
```bash
# Clean rebuild
docker compose build --no-cache

# Remove all containers and images
docker compose down --rmi all
```

#### Database Connection Issues
```bash
# Check PostgreSQL is running
docker compose ps postgres

# Test connection
docker compose exec postgres psql -U tasker -d tasker

# View PostgreSQL logs
docker compose logs postgres
```

#### Permission Issues
```bash
# Fix volume permissions
sudo chown -R $USER:$USER ./volumes
```

## 📊 Resource Management

### View Resource Usage
```bash
# Check container stats
docker stats

# Check disk usage
docker system df
```

### Cleanup Commands
```bash
# Remove stopped containers
docker container prune

# Remove unused images
docker image prune

# Remove unused volumes (⚠️ careful)
docker volume prune

# Complete cleanup (⚠️ removes everything unused)
docker system prune -a --volumes
```

## 🔒 Security Notes

### Production Deployment
For production environments, ensure you:

1. **Change default passwords** in `.env` file
2. **Use secrets management** instead of plain environment variables
3. **Enable HTTPS** for all services
4. **Restrict database access** to only necessary services
5. **Use specific image tags** instead of `latest`
6. **Implement proper backup strategies** for PostgreSQL

### Network Security
- All services communicate through the internal `tasker_net` bridge network
- Only necessary ports are exposed to the host
- Database is not exposed externally by default

## 📝 Notes

### Database Migrations
- API applies migrations automatically on startup
- No manual migration steps required
- Database schema is managed by Entity Framework Core

### Data Persistence
- PostgreSQL data stored in named volume `tasker_pgdata`
- Survives container restarts
- Backup regularly for production use

### Scaling
For horizontal scaling in production:
```bash
# Scale API instances
docker compose up -d --scale api=3
```

## 🔗 Related Documentation

- [Main Project README](../README.md)
- [API Documentation](../api/README.md)
- [Frontend Documentation](../web/README.md)

---

Orchestrated with Docker Compose