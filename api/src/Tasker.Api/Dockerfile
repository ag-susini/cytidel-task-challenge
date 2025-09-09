# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy only the project files first for better layer caching
COPY ["Tasker.Api/Tasker.Api.csproj", "Tasker.Api/"]
COPY ["Tasker.Application/Tasker.Application.csproj", "Tasker.Application/"]
COPY ["Tasker.Domain/Tasker.Domain.csproj", "Tasker.Domain/"]
COPY ["Tasker.Infrastructure/Tasker.Infrastructure.csproj", "Tasker.Infrastructure/"]
COPY ["Tasker.Shared/Tasker.Shared.csproj", "Tasker.Shared/"]
COPY ["Tasker.Shared.Abstractions/Tasker.Shared.Abstractions.csproj", "Tasker.Shared.Abstractions/"]

# Restore dependencies
RUN dotnet restore "Tasker.Api/Tasker.Api.csproj"

# Copy source code
COPY . .

# Build stage
WORKDIR "/src/Tasker.Api"
RUN dotnet build "Tasker.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Tasker.Api.csproj" -c Release -o /app/publish

# Runtime stage - use minimal alpine image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# Create non-root user for security
RUN addgroup -g 1000 -S appgroup && \
    adduser -u 1000 -S appuser -G appgroup

# Install curl for health checks (optional)
RUN apk add --no-cache curl

WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app
USER appuser

# Configure environment
EXPOSE 5080
ENV ASPNETCORE_URLS=http://+:5080
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Tasker.Api.dll"]