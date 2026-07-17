# Car Rental Platform

Production-oriented .NET 9 modular-monolith backend using Clean Architecture.

## Projects

- `CarRental.API`: REST host and composition root.
- `CarRental.Application`: CQRS use cases, validation, and ports.
- `CarRental.Domain`: module-owned domain models and rules.
- `CarRental.Infrastructure`: EF Core and external adapter implementations.
- `CarRental.SharedKernel`: minimal domain and application abstractions shared across modules.

Dependencies point inward. Modules (`Customer`, `Booking`, `Inventory`, and `Reporting`) own their business logic inside the Domain and Application layers.

## Local configuration

Do not commit secrets. Override the placeholder Auth0 and Azure SQL values with environment variables, user secrets, or Azure App Configuration/Key Vault:

```text
Auth0__Authority
Auth0__Audience
Auth0__RoleClaimType
ConnectionStrings__CarRentalDatabase
```

## Run

```powershell
dotnet restore
dotnet run --project src/CarRental.API
```

Health endpoints are available at `/alive` (process liveness) and `/health` (dependency readiness). Swagger UI is enabled only in Development.
