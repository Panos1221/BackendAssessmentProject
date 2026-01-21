# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency restoration
COPY backend/BackendProject.Domain/BackendProject.Domain.csproj backend/BackendProject.Domain/
COPY backend/BackendProject.Application/BackendProject.Application.csproj backend/BackendProject.Application/
COPY backend/BackendProject.Infrastructure/BackendProject.Infrastructure.csproj backend/BackendProject.Infrastructure/
COPY backend/BackendProject.API/BackendProject.API.csproj backend/BackendProject.API/

# Restore dependencies
WORKDIR /src/backend/BackendProject.API
RUN dotnet restore

# Copy source code (excluding obj and bin folders)
COPY backend/BackendProject.Domain/ ./backend/BackendProject.Domain/
COPY backend/BackendProject.Application/ ./backend/BackendProject.Application/
COPY backend/BackendProject.Infrastructure/ ./backend/BackendProject.Infrastructure/
COPY backend/BackendProject.API/ ./backend/BackendProject.API/

# Build and publish
WORKDIR /src/backend/BackendProject.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "BackendProject.API.dll"]
