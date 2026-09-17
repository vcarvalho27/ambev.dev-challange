# Ambev Developer Evaluation

Backend API for managing sales, built with .NET 8, PostgreSQL, MongoDB and Redis.

## Requirements

- Docker Desktop with Docker Compose
- .NET SDK 8.0 (only required to run the solution or tests outside Docker)

## Run with Docker

The Docker Compose files are located in `template/backend`.

```bash
cd template/backend
docker compose up --build
```

The first startup builds the Web API image and starts the following services:

- Web API
- PostgreSQL
- MongoDB
- Redis

The API container listens on ports `8080` (HTTP) and `8081` (HTTPS). The Compose file publishes container ports dynamically, so retrieve the host port with:

```bash
docker compose port ambev.developerevaluation.webapi 8080
```

Open the displayed address followed by `/swagger`, for example `http://localhost:<host-port>/swagger`.
Swagger is enabled because the Compose environment is `Development`.

To stop the services, press `Ctrl+C` or run:

```bash
docker compose down
```

To stop the services and remove their containers and volumes created by Compose:

```bash
docker compose down --volumes
```

If the API starts before PostgreSQL is ready and the logs show a database connection error, restart only the API after the database container is running:

```bash
docker compose restart ambev.developerevaluation.webapi
```

View service status and logs with:

```bash
docker compose ps
docker compose logs -f ambev.developerevaluation.webapi
```

## Tests

Restore dependencies and run all test projects from `template/backend`:

```bash
dotnet restore Ambev.DeveloperEvaluation.sln
dotnet test Ambev.DeveloperEvaluation.sln
```

The solution contains unit, integration and functional test projects under `template/backend/tests`.

To run a specific test project:

```bash
dotnet test tests/Ambev.DeveloperEvaluation.Unit
dotnet test tests/Ambev.DeveloperEvaluation.Integration
dotnet test tests/Ambev.DeveloperEvaluation.Functional
```

To collect coverage using the Coverlet collector included in the test projects:

```bash
dotnet test Ambev.DeveloperEvaluation.sln --collect:"XPlat Code Coverage"
```

The repository also includes `coverage-report.bat` and `coverage-report.sh`, which generate an HTML report when the required .NET global tools are available.

## Project structure

```text
template/backend/
	src/
		Ambev.DeveloperEvaluation.WebApi/       HTTP API and Swagger
		Ambev.DeveloperEvaluation.Application/  Use cases and handlers
		Ambev.DeveloperEvaluation.Domain/       Domain entities and rules
		Ambev.DeveloperEvaluation.ORM/          EF Core, PostgreSQL and repositories
		Ambev.DeveloperEvaluation.IoC/          Dependency injection configuration
		Ambev.DeveloperEvaluation.Common/       Cross-cutting concerns
	tests/
		Ambev.DeveloperEvaluation.Unit/
		Ambev.DeveloperEvaluation.Integration/
		Ambev.DeveloperEvaluation.Functional/
```

## Business rules

- Four or more identical items receive a 10% discount.
- Ten to twenty identical items receive a 20% discount.
- More than twenty identical items cannot be sold.
- Quantities below four items do not receive a discount.

## Notes

Since its a sample project, I didn`t do a lot of implementations that I would do on a real case scenario, so I am going to list some of them:

- Sales should have Authentication and Authorization (if user is a customer, it can only handle his data)
- Idempotency: Avoid duplicated requests
- It has not clear, but probably it need to have a CRUD for Cart (temporary holder) and another for effective Sale
- Check ProductId and BranchId on respective APIs to validate products prices and availability
- Caching with Redis