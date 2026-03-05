# Project: GreenLens

## Quick Reference

- **Stack:** ASP.NET Core 8 (C#) + Angular 17 + Azure AI Search + Azure OpenAI + SQLite
- **.NET version:** .NET 8.0
- **Node version:** Node 20+ (for Angular)
- **Package manager:** dotnet (NuGet) + npm (Angular)
- **PRD:** See `docs/PRD.md` -- this is the source of truth for all features

## Commands

```bash
# Backend (.NET)
dotnet build src/GreenLens.Api              # Build API project
dotnet test                                  # Run all tests
dotnet run --project src/GreenLens.Api       # Start API dev server (https://localhost:5001)
dotnet format                                # Format code
dotnet ef migrations add <Name> --project src/GreenLens.Infrastructure --startup-project src/GreenLens.Api

# Frontend (Angular)
cd src/greenlens-ui && npm install           # Install frontend deps
cd src/greenlens-ui && ng serve              # Start Angular dev server (http://localhost:4200)
cd src/greenlens-ui && ng test               # Run frontend tests
cd src/greenlens-ui && ng build              # Production build

# Azure Resources
dotnet run --project tools/GreenLens.Seed    # Seed emission factors to Azure AI Search
```

## Project Structure

```
GreenLens/
├── src/
│   ├── GreenLens.Api/              # ASP.NET Core 8 Web API (controllers, middleware, config)
│   ├── GreenLens.Core/             # Domain models, interfaces, business logic (estimation engine)
│   ├── GreenLens.Infrastructure/   # Azure AI Search, Azure OpenAI, EF Core, external services
│   ├── GreenLens.Shared/           # DTOs, constants, API contracts
│   └── greenlens-ui/               # Angular 17 SPA (dashboard, forms, charts)
├── tests/
│   ├── GreenLens.Api.Tests/        # Integration tests for API endpoints
│   ├── GreenLens.Core.Tests/       # Unit tests for business logic
│   └── GreenLens.Infrastructure.Tests/ # Integration tests for Azure services
├── tools/
│   └── GreenLens.Seed/             # CLI tool to seed emission factor data
├── data/
│   └── emission-factors/           # CSV seed data (EPA, cloud provider factors)
├── docs/
│   ├── PRD.md                      # Product requirements (source of truth)
│   └── ADR/                        # Architecture decision records
├── .env.example                    # Required environment variables
├── .gitignore
├── GreenLens.sln                   # .NET solution file
└── README.md
```

---

## BUILD RULES (Claude MUST follow these)

### Rule 1: PRD Is Law

- Every feature, endpoint, and component MUST trace back to a user story in `docs/PRD.md`
- If a feature is not in the PRD, DO NOT build it -- ask first
- If requirements are ambiguous, STOP and ask for clarification before writing code

### Rule 2: Build in Order

- Follow the milestone phases in the PRD strictly
- Complete Phase N before starting Phase N+1
- Each phase has a quality gate -- ALL gate criteria must pass before proceeding

### Rule 3: Test Before Implement

- Write the test file FIRST for every new module/component
- The test should initially fail (red)
- Then write the minimum code to make it pass (green)
- Then refactor if needed
- NO exceptions -- untested code is unshipped code

### Rule 4: One Thing at a Time

- Implement ONE user story at a time
- Each story follows: test -> implement -> verify -> commit
- Do NOT start the next story until the current one passes all tests
- Commit after each completed story with a descriptive conventional commit

### Rule 5: Error Handling Is Not Optional

- Every API endpoint must handle: validation errors, auth errors, not found, server errors
- Every Angular component must handle: loading, success, error, empty states
- Every Azure service call must handle: timeout, rate limit, unavailable
- Use the error scenarios table in the PRD as a checklist

### Rule 6: No Dead Code, No TODOs in Commits

- Remove all `Console.WriteLine` debug statements before committing
- No `// TODO` comments -- either implement it now or add it to the PRD backlog
- No commented-out code blocks
- No unused imports or variables

### Rule 7: Types Are Mandatory

- C#: All methods must have explicit return types, no `dynamic` or `object` when avoidable
- TypeScript: strict mode, no `any` type, no `@ts-ignore`
- All API request/response bodies must have typed DTOs (C# records / Angular interfaces)
- Shared types go in GreenLens.Shared

---

## CODING PATTERNS (Use these, not alternatives)

### API Response Format (Backend)

```csharp
// All endpoints return ApiResponse<T>
public record ApiResponse<T>(T? Data, ApiError? Error, ApiMeta? Meta = null);
public record ApiError(string Code, string Message, string[]? Details = null);
public record ApiMeta(int? Page = null, int? Total = null);

// Success: { "data": { ... }, "error": null }
// Error:   { "data": null, "error": { "code": "VALIDATION_ERROR", "message": "..." } }
```

### Error Handling (Backend)

```csharp
// Custom exception -> global exception handler -> consistent API response
public class AppException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }
    public AppException(string code, string message, int statusCode = 400) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
```

### Angular Component Pattern

```typescript
// Every component follows this structure:
// 1. Interface/types at top
// 2. Component with OnInit
// 3. Loading state handled
// 4. Error state handled
// 5. Empty state handled
// 6. Success state (the actual UI)
```

### Database Queries

- ALWAYS use EF Core with LINQ -- no raw SQL
- ALWAYS use async/await for database operations
- ALWAYS add indexes for columns used in WHERE/ORDER BY

---

## QUALITY GATES

### Before Each Commit

- [ ] `dotnet build` succeeds with zero warnings
- [ ] `dotnet test` -- all tests pass
- [ ] No `Console.WriteLine` debug statements
- [ ] Conventional commit message (feat/fix/refactor/test/docs/chore)

### Before Phase Transition

- [ ] All stories in current phase have passing tests
- [ ] Integration tests pass for the phase's features
- [ ] No P0 bugs open

### Before Shipping

- [ ] ALL PRD acceptance criteria checked off
- [ ] 80%+ test coverage on business logic
- [ ] E2E tests pass for critical user flows
- [ ] Error handling tested for all scenarios in PRD
- [ ] Environment variables documented in .env.example
- [ ] README updated with setup instructions

---

## ENVIRONMENT VARIABLES

```bash
# Azure AI Search
AZURE_SEARCH_ENDPOINT=https://<name>.search.windows.net
AZURE_SEARCH_API_KEY=<key>
AZURE_SEARCH_INDEX_NAME=emission-factors

# Azure OpenAI
AZURE_OPENAI_ENDPOINT=https://safegen-openai-pyae.openai.azure.com/
AZURE_OPENAI_API_KEY=<key>
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o-mini

# Azure Blob Storage
AZURE_STORAGE_CONNECTION_STRING=<connection-string>
AZURE_STORAGE_CONTAINER_NAME=emission-data

# API Security
API_KEY=<generate-a-strong-key>

# Database
DATABASE_CONNECTION_STRING=Data Source=greenlens.db
```

---

## KNOWN PATTERNS & DECISIONS

- Using Clean Architecture: Api -> Core <- Infrastructure (dependency inversion)
- Azure AI Search Free tier: 50MB storage, 10K documents, 3 indexes max
- SQLite for local dev to avoid Azure SQL costs during MVP
- API key auth (not JWT) -- simpler for DevOps tool integration
- Angular Material for UI components -- enterprise standard

## PROGRESS

- **Phase 1 (Foundation):** COMPLETED -- Solution scaffolding, EF Core + SQLite, health check, auth middleware, Swagger, rate limiting, CI pipeline
- **Phase 2 (Core API):** COMPLETED -- Estimation engine, all CRUD endpoints, Azure AI Search integration, seed tool
- **Phase 3 (AI Recommendations):** COMPLETED -- Azure OpenAI integration, recommendations endpoint with 1-hour cache, 503+Retry-After error handling, fallback recommendations
- **Phase 4 (Angular Frontend):** COMPLETED -- All components built, 43 unit tests + 20 E2E tests passing
- **Current test count:** 88 tests passing (11 Core + 6 Infra + 8 API + 43 Angular unit + 20 Playwright E2E)
