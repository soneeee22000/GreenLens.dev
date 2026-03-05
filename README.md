# GreenLens - Carbon Footprint Intelligence API

A .NET 8 / C# Web API that estimates the carbon footprint of Azure cloud infrastructure and provides AI-powered reduction recommendations using Azure AI Search and Azure OpenAI.

## Architecture

```
Angular 17 SPA --> ASP.NET Core 8 Web API --> Azure AI Search (emission factors)
                                          --> Azure OpenAI GPT-4o-mini (recommendations)
                                          --> SQLite / Azure SQL (estimate history)
```

**Clean Architecture:** `Api -> Core <- Infrastructure` with dependency inversion.

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (for Azure resources)
- [Docker](https://docs.docker.com/get-docker/) (optional)

### Setup

```bash
# Clone and restore
git clone <repo-url>
cd GreenLens
dotnet restore

# Configure environment
cp .env.example .env
# Edit .env with your Azure credentials

# Run database migration
dotnet ef database update --project src/GreenLens.Infrastructure --startup-project src/GreenLens.Api

# Start the API
dotnet run --project src/GreenLens.Api
```

The API will be available at `https://localhost:5001` with Swagger UI at `https://localhost:5001/swagger`.

### Docker

```bash
docker compose up --build
```

API available at `http://localhost:8080`.

## Commands

```bash
dotnet build                                # Build all projects
dotnet test                                 # Run all tests (19 tests)
dotnet run --project src/GreenLens.Api      # Start API server
dotnet format                               # Format code
```

## API Endpoints

| Method | Endpoint                                 | Purpose                 | Auth    |
| ------ | ---------------------------------------- | ----------------------- | ------- |
| GET    | `/health`                                | Health check            | None    |
| GET    | `/api/v1/regions`                        | List supported regions  | None    |
| POST   | `/api/v1/estimates`                      | Create carbon estimate  | API Key |
| GET    | `/api/v1/estimates`                      | List estimates          | API Key |
| GET    | `/api/v1/estimates/{id}`                 | Get estimate detail     | API Key |
| GET    | `/api/v1/estimates/{id}/recommendations` | AI recommendations      | API Key |
| GET    | `/api/v1/emission-factors/search`        | Search emission factors | API Key |

### Authentication

Pass your API key in the `X-Api-Key` header:

```bash
curl -H "X-Api-Key: your-key" https://localhost:5001/api/v1/estimates
```

## Project Structure

```
GreenLens/
├── src/
│   ├── GreenLens.Api/              # ASP.NET Core 8 controllers, middleware
│   ├── GreenLens.Core/             # Domain models, interfaces, business logic
│   ├── GreenLens.Infrastructure/   # EF Core, Azure services
│   └── GreenLens.Shared/           # DTOs, constants
├── tests/
│   ├── GreenLens.Core.Tests/       # Unit tests (8 tests)
│   ├── GreenLens.Api.Tests/        # Integration tests (5 tests)
│   └── GreenLens.Infrastructure.Tests/  # Repository tests (6 tests)
├── tools/
│   └── GreenLens.Seed/             # CLI to seed emission factor data
├── docs/
│   └── PRD.md                      # Product requirements
├── Dockerfile
├── docker-compose.yml
└── .github/workflows/ci.yml        # CI pipeline
```

## Tech Stack

- **Backend:** ASP.NET Core 8, C#, Entity Framework Core
- **Search:** Azure AI Search
- **AI:** Azure OpenAI (GPT-4o-mini)
- **Database:** SQLite (dev) / Azure SQL (prod)
- **Frontend:** Angular 17 + Angular Material (Phase 4)
- **CI/CD:** GitHub Actions
- **Container:** Docker

## License

MIT
