# GreenLens

AI-powered cloud carbon footprint estimation for Azure infrastructure. Submit your resource usage — get CO2e estimates, per-resource breakdowns, and actionable reduction recommendations powered by Azure OpenAI.

**[Live Demo](https://greenlens-api.azurewebsites.net)** | **[Swagger API](https://greenlens-api.azurewebsites.net/swagger)**

[![CI](https://github.com/soneeee22000/GreenLens.dev/actions/workflows/ci.yml/badge.svg)](https://github.com/soneeee22000/GreenLens.dev/actions/workflows/ci.yml)
[![Backend Tests](https://img.shields.io/badge/backend_tests-25_passing-brightgreen)](https://github.com/soneeee22000/GreenLens.dev)
[![E2E Tests](https://img.shields.io/badge/e2e_tests-20_passing-brightgreen)](https://github.com/soneeee22000/GreenLens.dev)
[![Angular Tests](https://img.shields.io/badge/angular_tests-43_passing-brightgreen)](https://github.com/soneeee22000/GreenLens.dev)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-17-DD0031?logo=angular)](https://angular.io/)
[![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![TypeScript](https://img.shields.io/badge/TypeScript-strict-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Azure](https://img.shields.io/badge/Azure-AI_Search_%2B_OpenAI-0078D4?logo=microsoftazure)](https://azure.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[Live Demo](https://greenlens-api.azurewebsites.net) | [Architecture](#architecture) | [API Reference](#api-endpoints) | [Tech Stack](#tech-stack) | [Getting Started](#quick-start) | [Test Coverage](#test-coverage) | [Engineering Decisions](#key-engineering-decisions)

---

## Why GreenLens?

The EU Corporate Sustainability Reporting Directive (CSRD) mandates Scope 3 emissions reporting starting 2026. Cloud infrastructure is a growing portion of enterprise carbon footprint, yet existing tools are delayed, high-level, and lack actionable insights. GreenLens fills that gap with a sub-500ms API that returns precise CO2e estimates and AI-generated reduction strategies.

---

## Architecture

```mermaid
graph TB
    subgraph Frontend["Angular 17 SPA"]
        UI[Angular Material UI]
        Charts[Chart.js Visualizations]
        Forms[Reactive Forms]
    end

    subgraph API["ASP.NET Core 8 Web API"]
        Controllers[REST Controllers]
        Middleware["Auth | Rate Limit | Error Handler"]
        Cache[In-Memory Cache]
    end

    subgraph Core["Domain Layer"]
        Engine[Estimation Engine]
        Models[Domain Models]
        Interfaces[Service Interfaces]
    end

    subgraph Infrastructure["Infrastructure Layer"]
        EF[EF Core + SQLite]
        SearchSvc[Azure AI Search Client]
        OpenAISvc[Azure OpenAI Client]
    end

    subgraph Azure["Azure Cloud Services"]
        AISearch[(Azure AI Search<br/>Emission Factors Index)]
        OpenAI[Azure OpenAI<br/>GPT-4o-mini]
    end

    Frontend -->|HTTP + API Key| API
    API --> Core
    Core --> Infrastructure
    Infrastructure --> Azure

    style Frontend fill:#1976d2,color:#fff
    style API fill:#388e3c,color:#fff
    style Core fill:#f57c00,color:#fff
    style Infrastructure fill:#7b1fa2,color:#fff
    style Azure fill:#0078d4,color:#fff
```

**Clean Architecture** with strict dependency inversion: `Api -> Core <- Infrastructure`. The domain layer owns interfaces; infrastructure implements them. No Azure SDK leaks into business logic.

---

## Request Flow

```mermaid
sequenceDiagram
    participant Client as DevOps Engineer
    participant API as GreenLens API
    participant Engine as Estimation Engine
    participant Search as Azure AI Search
    participant OpenAI as Azure OpenAI
    participant DB as SQLite

    Client->>API: POST /api/v1/estimates
    API->>Engine: Calculate CO2e
    Engine->>Search: Query emission factors
    Search-->>Engine: Matched factors (kgCO2e/unit)
    Engine->>Engine: Compute per-resource breakdown
    Engine->>DB: Persist estimate
    DB-->>API: Estimate ID
    API-->>Client: 201 Created (total CO2e + breakdown)

    Client->>API: GET /estimates/{id}/recommendations
    API->>OpenAI: Generate reduction strategies
    OpenAI-->>API: Structured recommendations
    API->>API: Cache (1-hour TTL)
    API-->>Client: 200 OK (recommendations)
```

---

## Tech Stack

| Layer         | Technology                                                  | Purpose                                                     |
| ------------- | ----------------------------------------------------------- | ----------------------------------------------------------- |
| **Frontend**  | Angular 17, TypeScript (strict), Angular Material, Chart.js | SPA with dashboard, forms, and data visualization           |
| **Backend**   | ASP.NET Core 8, C# 12                                       | REST API with middleware pipeline                           |
| **Domain**    | Clean Architecture, CQRS-lite                               | Business logic isolation                                    |
| **AI/Search** | Azure AI Search, Azure OpenAI (GPT-4o-mini)                 | Semantic emission factor lookup + reduction recommendations |
| **Database**  | EF Core 8 + SQLite (dev) / Azure SQL (prod)                 | Estimate persistence with migrations                        |
| **Testing**   | xUnit, Moq, Jasmine/Karma, Playwright                       | 88 tests across unit, integration, and E2E                  |
| **DevOps**    | Docker, GitHub Actions, Swagger/OpenAPI                     | Containerized builds, CI pipeline, auto-generated API docs  |

---

## API Endpoints

| Method | Endpoint                                 | Purpose                                          | Auth    |
| ------ | ---------------------------------------- | ------------------------------------------------ | ------- |
| `GET`  | `/health`                                | Health check                                     | None    |
| `GET`  | `/api/v1/regions`                        | List 15 Azure regions with grid carbon intensity | None    |
| `POST` | `/api/v1/estimates`                      | Create carbon footprint estimate                 | API Key |
| `GET`  | `/api/v1/estimates`                      | List estimates (paginated)                       | API Key |
| `GET`  | `/api/v1/estimates/{id}`                 | Get estimate with per-resource breakdown         | API Key |
| `GET`  | `/api/v1/estimates/{id}/recommendations` | AI-powered reduction recommendations             | API Key |
| `GET`  | `/api/v1/emission-factors/search`        | Semantic search across emission factors          | API Key |

### Example: Create Estimate

```bash
curl -X POST https://localhost:5001/api/v1/estimates \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: your-key" \
  -d '{
    "resources": [
      { "resourceType": "Standard_D4s_v3", "region": "westeurope", "quantity": 2, "hours": 720 },
      { "resourceType": "BlobStorage", "region": "westeurope", "quantity": 500, "hours": 720 }
    ]
  }'
```

### Example Response

```json
{
  "data": {
    "estimateId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "totalCo2eKg": 12.35,
    "createdAt": "2026-03-05T10:00:00Z",
    "breakdown": [
      {
        "resourceType": "Standard_D4s_v3",
        "region": "westeurope",
        "quantity": 2,
        "hours": 720,
        "co2eKg": 8.12,
        "co2ePerUnit": 0.00564,
        "unit": "kgCO2e/hour"
      },
      {
        "resourceType": "BlobStorage",
        "region": "westeurope",
        "quantity": 500,
        "hours": 720,
        "co2eKg": 4.23,
        "co2ePerUnit": 0.00586,
        "unit": "kgCO2e/GB-month"
      }
    ]
  },
  "error": null
}
```

---

## Project Structure

```
GreenLens/
├── src/
│   ├── GreenLens.Api/              # Controllers, middleware, DI configuration
│   ├── GreenLens.Core/             # Domain models, interfaces, estimation engine
│   ├── GreenLens.Infrastructure/   # EF Core repos, Azure AI Search, Azure OpenAI
│   ├── GreenLens.Shared/           # DTOs, API contracts, constants
│   └── greenlens-ui/               # Angular 17 SPA
│       ├── src/app/components/     # Dashboard, EstimateForm, EstimateDetail, Search
│       ├── src/app/services/       # Typed HTTP client for all API endpoints
│       └── e2e/                    # Playwright E2E tests (20 tests)
├── tests/
│   ├── GreenLens.Core.Tests/       # 11 unit tests (estimation engine, domain logic)
│   ├── GreenLens.Api.Tests/        # 8 integration tests (endpoints, middleware)
│   └── GreenLens.Infrastructure.Tests/ # 6 tests (repositories, Azure service mocks)
├── tools/
│   └── GreenLens.Seed/             # CLI to seed emission factors into Azure AI Search
├── docs/
│   └── PRD.md                      # Product requirements document
├── .github/workflows/ci.yml        # CI: build, test, format, Docker
├── Dockerfile                      # Multi-stage build, non-root user
├── docker-compose.yml              # Local dev orchestration
└── GreenLens.sln
```

---

## Test Coverage

```mermaid
pie title 88 Tests Across All Layers
    "Angular Unit Tests" : 43
    "Playwright E2E Tests" : 20
    "Core Domain Tests" : 11
    "API Integration Tests" : 8
    "Infrastructure Tests" : 6
```

| Layer          | Framework                     | Count | Covers                                           |
| -------------- | ----------------------------- | ----- | ------------------------------------------------ |
| Domain         | xUnit + Moq                   | 11    | Estimation engine, CO2e calculations, edge cases |
| API            | xUnit + WebApplicationFactory | 8     | Endpoint contracts, auth, error responses        |
| Infrastructure | xUnit + Moq                   | 6     | Repository CRUD, Azure service integration       |
| Frontend       | Jasmine + Karma               | 43    | All components, services, form validation        |
| E2E            | Playwright                    | 20    | Full user flows with API mocking                 |

---

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) (for Angular frontend)
- Azure account (for AI Search + OpenAI services)

### Backend

```bash
git clone git@github.com:soneeee22000/GreenLens.dev.git
cd GreenLens
cp .env.example .env    # Add your Azure credentials
dotnet restore
dotnet ef database update --project src/GreenLens.Infrastructure --startup-project src/GreenLens.Api
dotnet run --project src/GreenLens.Api
```

API at `https://localhost:5001` | Swagger at `https://localhost:5001/swagger`

### Frontend

```bash
cd src/greenlens-ui
npm install
ng serve
```

Angular app at `http://localhost:4200`

### Docker

```bash
docker compose up --build
```

### Run All Tests

```bash
# Backend (25 tests)
dotnet test

# Frontend unit tests (43 tests)
cd src/greenlens-ui && ng test --watch=false --browsers=ChromeHeadless

# E2E tests (20 tests)
cd src/greenlens-ui && npm run e2e
```

---

## Key Engineering Decisions

| Decision                                     | Rationale                                                               |
| -------------------------------------------- | ----------------------------------------------------------------------- |
| Clean Architecture over MVC                  | Domain logic is testable in isolation; Azure services are swappable     |
| Azure AI Search for emission factors         | Semantic search over 50+ factors vs. hardcoded lookup tables            |
| GPT-4o-mini over GPT-4                       | 10x cheaper, sufficient for structured recommendation generation        |
| SQLite for dev, Azure SQL for prod           | Zero-config local development; same EF Core migrations for both         |
| API key auth over JWT                        | Simpler for DevOps tool/pipeline integration; no user sessions needed   |
| Angular Material over custom CSS             | Enterprise-standard components; accessibility built-in                  |
| Playwright over Cypress                      | Native multi-browser support; better API mocking via route interception |
| In-memory cache (1h TTL) for recommendations | Avoids redundant OpenAI calls; recommendations don't change frequently  |

---

## Environment Variables

```bash
# Azure AI Search
AZURE_SEARCH_ENDPOINT=https://<name>.search.windows.net
AZURE_SEARCH_API_KEY=<key>
AZURE_SEARCH_INDEX_NAME=emission-factors

# Azure OpenAI
AZURE_OPENAI_ENDPOINT=https://<name>.openai.azure.com/
AZURE_OPENAI_API_KEY=<key>
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o-mini

# API Security
API_KEY=<generate-a-strong-key>

# Database
DATABASE_CONNECTION_STRING=Data Source=greenlens.db
```

---

## License

MIT

---

Built by [Pyae Sone (Seon)](https://github.com/soneeee22000)
