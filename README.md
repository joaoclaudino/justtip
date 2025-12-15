# JustTip — Tipping Platform (API + CLI)

JustTip is a .NET solution that models a tipping platform for hospitality businesses.
It provides a REST API (with Swagger/OpenAPI) and a CLI to manage businesses, employees,
daily rosters (worked hours) and proportional tip distribution.

## Features

- Businesses and Employees management
- Daily rosters with worked hours (upsert entries)
- Tip distribution proportional to worked hours (2-decimal rounding with exact total allocation)
- Swagger / OpenAPI documentation
- Unit tests for core tip distribution rules
- SQLite persistence (EF Core)

## Tech Stack

- .NET
- ASP.NET Core Minimal API
- EF Core + SQLite
- Swagger (Swashbuckle)
- xUnit

## Running the API

Run:
dotnet run --project JustTip.Api

Open:
- Swagger UI: https://localhost:7035/swagger
- Health check: https://localhost:7035/health

Note: the port can vary depending on your local launch profile. Check the console output for the actual URL.

## CLI

The CLI talks to the API via HTTP.

Help:
dotnet run --project JustTip.Cli -- --help

Example flow:

1) Create a business
dotnet run --project JustTip.Cli -- --base-url https://localhost:7035 businesses create --name "Cafe A"

2) List businesses
dotnet run --project JustTip.Cli -- --base-url https://localhost:7035 businesses list

3) Add employees
dotnet run --project JustTip.Cli -- --base-url https://localhost:7035 employees add --business-id <BUSINESS_ID> --name "John"
dotnet run --project JustTip.Cli -- --base-url https://localhost:7035 employees add --business-id <BUSINESS_ID> --name "Mary"

4) Ensure roster for a date
dotnet run --project JustTip.Cli -- --base-url https://localhost:7035 rosters ensure --business-id <BUSINESS_ID> --date 2025-12-14

5) Upsert worked hours
dotnet run --project JustTip.Cli -- --base-url https://localhost:7035 rosters upsert-entry --business-id <BUSINESS_ID> --date 2025-12-14 --employee-id <EMPLOYEE_ID_1> --hours 8
dotnet run --project JustTip.Cli -- --base-url https://localhost:7035 rosters upsert-entry --business-id <BUSINESS_ID> --date 2025-12-14 --employee-id <EMPLOYEE_ID_2> --hours 4

6) Distribute tips
dotnet run --project JustTip.Cli -- --base-url https://localhost:7035 tips distribute --business-id <BUSINESS_ID> --date 2025-12-14 --total 120

## Tests

Run:
dotnet test

## Project Structure

JustTip.Api              REST API (Minimal API, Swagger, endpoints)
JustTip.Application      Business rules (tip distribution service)
JustTip.Domain           Domain entities
JustTip.Infrastructure   EF Core + SQLite persistence
JustTip.Cli              Command-line client (HTTP)
JustTip.UnitTests        Unit tests

## License

MIT
