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
AzureServiceBus__FullyQualifiedNamespace
AzureServiceBus__TopicName
```

Azure Service Bus uses `DefaultAzureCredential`. In Azure, assign the API's managed identity the
`Azure Service Bus Data Sender` role on the configured namespace. Local development can authenticate
with the Azure CLI or a supported developer credential; no Service Bus connection string is stored.

## Run

```powershell
dotnet restore
dotnet run --project src/CarRental.API
```

Health endpoints are available at `/alive` (process liveness) and `/health` (dependency readiness). Swagger UI is enabled only in Development.

## Notification Function

`CarRental.Notification.Function` is a .NET 9 isolated Azure Functions worker. It consumes the
`BookingCreatedIntegrationEvent` from the configured Service Bus topic/subscription and sends booking
confirmation email through SendGrid.

Before deployment:

1. Apply [`deploy/sql/notification-inbox.sql`](deploy/sql/notification-inbox.sql) to the Car Rental database.
2. Create the `notifications` subscription on the `car-rental-events` topic and configure an appropriate
   `MaxDeliveryCount` (for example, 10). Azure Service Bus automatically moves exhausted transient deliveries
   to the subscription's dead-letter queue.
3. Assign the Function managed identity `Azure Service Bus Data Receiver` and the required Azure SQL role.
4. Configure `ServiceBusConnection__fullyQualifiedNamespace`, `NotificationsTopicName`,
   `NotificationsSubscriptionName`, `ConnectionStrings__CarRentalDatabase`, `SendGrid__ApiKey`,
   `SendGrid__FromEmail`, and `SendGrid__FromName` as Function App settings or Key Vault references.

For local development, copy `local.settings.example.json` to `local.settings.json` and supply secrets outside
source control. Malformed messages, missing customer recipients, and permanent SendGrid rejections are explicitly
dead-lettered. Transient failures are thrown for Service Bus redelivery. The SQL inbox prevents repeat sends for
messages already marked completed and coordinates concurrent scaled-out workers.
