# Azure Learning Repository

This repository contains two .NET 8 projects used to practice Azure integrations:

- **MyApi** – ASP.NET Core Web API for order operations, queue publishing, and Service Bus publishing.
- **MyFunctionApp** – Azure Functions app that processes queue/service bus messages and blob events.

## Repository Structure

- `/MyApi` – Web API project (`MyApi.csproj`)
- `/MyFunctionApp` – Azure Functions isolated worker project (`MyFunctionApp.csproj`)
- `/azure.sln` – solution for `MyApi`
- `/MyFunctionApp/MyFunctionApp.sln` – solution for `MyFunctionApp`

## Prerequisites

- .NET SDK 8.0+
- SQL Server (or LocalDB)
- Azure Storage account (for Queue/Blob triggers)
- Azure Service Bus (optional, for topic/subscription flow)
- Azure Cosmos DB (optional, for event persistence)

## Configuration

Set configuration in:

- `/MyApi/appsettings.json`
- Function App local settings/environment variables (for local function runtime)

Important keys:

- `ConnectionStrings:DefaultConnection`
- `Storage:ConnectionString`
- `ServiceBus:ConnectionString`
- `CosmosDbConnection`
- `APPLICATIONINSIGHTS_CONNECTION_STRING`
- `AzureFunctions:BaseUrl`
- `AzureFunctions:FunctionKey`

## Build & Test

From repository root:

```bash
dotnet build azure.sln
dotnet build MyFunctionApp/MyFunctionApp.sln
dotnet test azure.sln
dotnet test MyFunctionApp/MyFunctionApp.sln
```

## Run Locally

### API

```bash
dotnet run --project MyApi/MyApi.csproj
```

Swagger UI is enabled in the API app.

### Azure Functions

```bash
dotnet run --project MyFunctionApp/MyFunctionApp.csproj
```

## Main Endpoints / Triggers

### API

- `GET /api/orders`
- `GET /api/orders/{id}`
- `POST /api/orders`
- `POST /api/orders/bus`
- `POST /api/queue/test`
- `POST /api/test/post`

### Functions

- `SampleGet`, `SamplePost`, `SamplePut`, `SampleDelete`, `SamplePatch` (HTTP triggers)
- `ProcessOrderQueueV2` (Queue trigger)
- `ProcessOrderTopic` (Service Bus topic/subscription trigger)
- `BlobProcessor` (Blob trigger + blob output)

