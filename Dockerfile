# Stage 1: Build Angular frontend
FROM node:20-alpine AS frontend
WORKDIR /app
COPY src/greenlens-ui/package.json src/greenlens-ui/package-lock.json ./
RUN npm ci
COPY src/greenlens-ui/ .
RUN npx ng build --configuration production

# Stage 2: Build .NET backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY GreenLens.sln .
COPY src/GreenLens.Api/GreenLens.Api.csproj src/GreenLens.Api/
COPY src/GreenLens.Core/GreenLens.Core.csproj src/GreenLens.Core/
COPY src/GreenLens.Infrastructure/GreenLens.Infrastructure.csproj src/GreenLens.Infrastructure/
COPY src/GreenLens.Shared/GreenLens.Shared.csproj src/GreenLens.Shared/
COPY tools/GreenLens.Seed/GreenLens.Seed.csproj tools/GreenLens.Seed/
COPY tests/GreenLens.Core.Tests/GreenLens.Core.Tests.csproj tests/GreenLens.Core.Tests/
COPY tests/GreenLens.Api.Tests/GreenLens.Api.Tests.csproj tests/GreenLens.Api.Tests/
COPY tests/GreenLens.Infrastructure.Tests/GreenLens.Infrastructure.Tests.csproj tests/GreenLens.Infrastructure.Tests/

RUN dotnet restore

# Copy everything and build
COPY . .
RUN dotnet publish src/GreenLens.Api -c Release -o /app/publish --no-restore

# Copy Angular build output into wwwroot
COPY --from=frontend /app/dist/greenlens-ui/browser/ /app/publish/wwwroot/

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r greenlens && useradd -r -g greenlens greenlens

COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run as non-root
USER greenlens

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "GreenLens.Api.dll"]
