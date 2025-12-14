# JustTip — Tipping Platform (API + CLI)

JustTip is a .NET-based solution that models a tipping platform for hospitality businesses.
It provides a REST API (with Swagger/OpenAPI) and a command-line interface (CLI) to manage
businesses, employees, daily rosters, and proportional tip distribution.

The project is designed with clean architecture principles, focusing on clear separation
of concerns, testability, and maintainability.

## Tech Stack

- .NET (latest installed)
- ASP.NET Core Minimal API
- Entity Framework Core
- SQLite
- Swagger / OpenAPI (Swashbuckle)
- xUnit (unit and integration tests)
- GitHub Actions (CI)

## Getting Started

### Prerequisites

- .NET SDK installed
- (Optional) EF Core CLI tools: dotnet tool install --global dotnet-ef

### Running the API

Run the API using the .NET CLI:

dotnet run --project src/JustTip.Api

Once the application is running, open your browser at the following endpoints:

Swagger UI:
https://localhost:7035/swagger

Health check:
https://localhost:7035/health

The exact port may vary depending on your local configuration. Check the console output
for the actual listening address.

## Database

The application uses SQLite as its persistence layer.

The database file is created automatically when migrations are applied and the API starts.
By default, the database file is named:

justtip.db

This file is intentionally excluded from source control.

## Project Structure

src/
  JustTip.Api              REST API (Minimal API, Swagger, HTTP endpoints)
  JustTip.Domain           Domain entities and core business rules
  JustTip.Infrastructure   EF Core, SQLite, persistence and data access

## Development Notes

- The API follows REST principles and exposes clear, predictable endpoints.
- Business logic is isolated from infrastructure concerns.
- Entity Framework Core is used with LINQ for data access.
- The project favors explicit validation and clear error messages.
- Code readability and maintainability are prioritized over over-engineering.

## Roadmap

- Business and Employee CRUD endpoints
- Daily rosters and worked hours validation
- Tip distribution logic proportional to worked hours
- Command-line interface (CLI) consuming the API
- Unit and integration tests covering business rules
- CI pipeline with GitHub Actions

## License

MIT
