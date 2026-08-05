# TMS API

A RESTful **Training Management System** backend built with **ASP.NET Core 10** and **Entity Framework Core** on **PostgreSQL**.

The API handles the core domain of a training academy: courses, students, and enrollments. It is developed incrementally across training modules, so the codebase doubles as a learning record covering production-grade API patterns (see [Learning roadmap](#learning-roadmap)).

## Highlights

- **RESTful API** with pagination, search, sorting, and HATEOAS links
- **Entity Framework Core** with PostgreSQL, code-first migrations, and seed data
- **Optimistic concurrency** via the PostgreSQL `xmin` system column
- **Soft delete** with query filters and bulk archive operations
- **Structured error responses** (RFC 7807 `ProblemDetails`)
- **Request logging middleware** with correlation IDs
- **Custom authentication scheme** (header-based) + global audit logging filter
- **OpenAPI + Scalar** interactive documentation (Development only)
- **Options pattern** with validation for app configuration

## Tech Stack

| Area | Technology |
| ---- | ---------- |
| Runtime | .NET 10 |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL (Npgsql) |
| API Docs | Scalar + Microsoft.AspNetCore.OpenApi |

## Getting Started

### Prerequisites

- [.NET SDK 10+](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) 13+ running locally

### 1. Configure the database

Update the connection string in `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "TmsDatabase": "Host=localhost;Database=TmsDb;Username=postgres;Password=your_password"
}
```

> Prefer [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables over hardcoded credentials outside development.

### 2. Run the API

```bash
dotnet restore
dotnet run
```

In Development, migrations are applied and the database is seeded automatically on startup.

### 3. Browse the API docs

Open [http://localhost:5158/scalar/v1](http://localhost:5158/scalar/v1) in your browser.

### 4. Test with the request file

The repository ships a `TmsApi.http` file with ready-made requests for **REST Client** (VS Code) or JetBrains HTTP Client.

## Endpoints

| Method | Route | Description |
| ------ | ----- | ----------- |
| `GET` | `/api/courses?page=&pageSize=&search=&orderBy=` | List courses (paginated, searchable, sortable) |
| `GET` | `/api/courses/{id}` | Get course details with HATEOAS links |
| `POST` | `/api/courses` | Create a course (409 on duplicate code) |
| `GET` | `/api/courses/{courseId}/enrollments` | List enrollments for a course |
| `GET` | `/api/courses/{courseId}/enrollments/{id}` | Get one enrollment |
| `POST` | `/api/courses/{courseId}/enrollments` | Enroll a student (409 when course is full) |

Protected routes require the `X-Training-User` header:

```http
GET /api/courses HTTP/1.1
X-Training-User: student@tms.com
```

## Project Structure

```
TmsApi/
├── Configurations/   # EF Core entity configurations (mapping, constraints, indexes)
├── Controllers/      # API controllers
├── Data/             # DbContext, migrations, seed data
├── Entities/         # Domain entities
├── Filters/          # Global MVC filters (audit logging)
├── Migrations/       # Code-first EF Core migrations
├── Models/           # Request/response DTOs, pagination, HATEOAS links
├── Services/         # Business logic (courses, enrollments)
├── Program.cs        # Host setup, DI, middleware pipeline
└── *.cs              # Cross-cutting concerns (middleware, auth, options, exceptions)
```

## Learning Roadmap

This project was built module by module, and each area maps to a production skill:

| Module | Covered |
| ------ | ------- |
| Core EF Core | DbContext, configurations, deferred execution, query translation, N+1 fixes |
| Querying | Pagination, aggregates, projections, JOINs |
| Concurrency | Optimistic concurrency with `xmin`, conflict handling |
| Data lifecycle | Soft delete, bulk archive, migrations |
| API design | DTOs, validation, ProblemDetails, HATEOAS |
| Cross-cutting | Auth schemes, middleware, options pattern, background workers, audit filters |

## License

[MIT](LICENSE)
