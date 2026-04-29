# Event-Driven Notification System

A fully serverless, event-driven message pipeline built on Azure. An Azure Function App (HTTP Trigger) publishes messages to Azure Service Bus, which triggers downstream Function App functions to process business logic and persist outputs to Blob Storage. Azure Logic Apps automate email notifications on blob upload events, delivering an end-to-end serverless workflow monitored via Application Insights.

---

## Architecture Overview

```
[Azure Function App — HTTP Trigger (GetCourses / Publisher)]
        |
        | Publishes message
        v
[Azure Service Bus Queue / Topic]          [Azure Queue Storage (saurabhqueue)]
        |                                           |
        | Triggers (SBTopic.cs)                     | Triggers (QueueTrigger.cs)
        v                                           v
[Azure Function App — Service Bus Trigger] [Azure Function App — Queue Trigger]
        |
        |-- Processes business logic
        |-- Persists output to Blob Storage (SAS token / Key Vault)
        |-- Writes to Azure Table Storage
        v
[Azure Blob Storage (saurabhblob)]
        |
        | Upload event triggers (BlobTrigger.cs)
        v
[Azure Logic App — No-Code Workflow]
        |
        | Sends automated email notification
        v
[End User / Subscriber]

[Application Insights] <-- monitors all Function App executions and message flow
```

---

## Key Features

- **Event-Driven Architecture** — Azure Function App (HTTP Trigger) publishes messages to Azure Service Bus Queue/Topic and Azure Queue Storage, decoupling producers from consumers
- **Service Bus Trigger** — Azure Function App listens to Service Bus messages and executes business logic on each received message
- **Blob Storage Persistence** — Processed outputs are stored in Azure Blob Storage, secured via SAS tokens and Azure Key Vault
- **Automated Email Notifications** — Azure Logic App monitors Blob Storage for upload events and automatically sends email notifications (no-code workflow)
- **Topic-Based Fan-Out** — Replaced Queue pattern with Service Bus Topic + Subscription model to support multiple consumers receiving the same message
- **Observability** — Full message flow and Function App execution tracked via Application Insights with live metrics and sampling

---

## Azure Services Used

| Service | Purpose |
|---|---|
| **Azure Service Bus (Queue)** | Initial message ingestion from the HTTP-triggered Function App |
| **Azure Service Bus (Topic + Subscription)** | Fan-out pattern — multiple subscribers consume the same message |
| **Azure Queue Storage** | Lightweight message queue (`saurabhqueue`) processed by Queue Trigger function |
| **Azure Function App** | Serverless compute; triggered by Service Bus, Blob, Queue, and HTTP events |
| **Azure Blob Storage** | Persists processed outputs from the Function App |
| **Azure Table Storage** | Stores structured course/entity data |
| **Azure Key Vault** | Secures Blob Storage SAS tokens and connection strings |
| **Azure Logic Apps** | No-code workflow; triggers automated email on Blob upload event |
| **Application Insights** | Monitors Function App execution, logs message flow, live metrics |

---

## Project Structure

```
DemoFunctionApp/
├── Program.cs                  # Entry point, DI configuration, App Insights setup
├── DemoFunctionApp.csproj      # Project file (.NET 8, isolated worker model)
├── host.json                   # Azure Functions host config (App Insights sampling)
├── local.settings.json         # Local dev settings (connection strings)
├── BlobTrigger.cs              # Function triggered on Blob Storage upload
├── QueueTrigger.cs             # Function triggered on Azure Storage Queue message
├── SBTopic.cs                  # Function triggered on Service Bus Topic subscription
├── GetCourses.cs               # HTTP-triggered function; reads Courses from SQL Server
├── Courses.cs                  # POCO model for course data
├── CourseEntity.cs             # Azure Table Storage entity model
└── Properties/
    ├── serviceDependencies.json
    └── launchSettings.json
```

---

## Function App Triggers

### 1. Service Bus Topic Trigger — `SBTopic.cs`
Listens to the `saurabhtopics` Service Bus topic via the `saurabhSub` subscription. Logs message ID, body, and content type, then completes the message.

```
Topic:        saurabhtopics
Subscription: saurabhSub
Connection:   saurabhSAS (Shared Access Policy)
```

### 2. Blob Storage Trigger — `BlobTrigger.cs`
Fires when a new file is uploaded to the `saurabhblob` container. Reads the blob stream and logs the blob name and size.

```
Container:  saurabhblob/{name}
Connection: blobconstr
```

### 3. Queue Trigger — `QueueTrigger.cs`
Processes messages from the `saurabhqueue` Azure Storage Queue.

```
Queue:      saurabhqueue
Connection: blobconstr
```

### 4. HTTP Trigger — `GetCourses.cs`
Exposes an HTTP GET endpoint that queries a SQL Server `Courses` table and returns the results as JSON.

---

## Message Flow (End-to-End)

1. **Publish** — HTTP-triggered Function App sends a message to Azure Service Bus Queue
2. **Process** — Service Bus trigger fires the Function App; business logic executes
3. **Persist** — Function App writes output to Azure Blob Storage (secured via SAS / Key Vault)
4. **Notify** — Logic App detects the Blob upload event and sends an automated email
5. **Fan-Out** — Service Bus Topic distributes the same message to multiple subscriptions simultaneously, enabling multi-consumer scenarios

---

## Observability

Application Insights is configured for full telemetry:
- Function execution tracking (start, duration, success/failure)
- Live metrics stream
- Sampling enabled (Request logs excluded from sampling to capture all invocations)
- Dependency tracking across Service Bus, Blob Storage, and Table Storage calls

```json
// host.json
{
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      },
      "enableLiveMetricsFilters": true
    }
  }
}
```

---

## Technology Stack

- **.NET 8** — Isolated worker model
- **Azure Functions v4**
- **Azure Service Bus** — `Azure.Messaging.ServiceBus` v7.20.1
- **Azure Blob & Queue Storage** — Function worker extensions
- **Azure Table Storage** — `Azure.Data.Tables` v12.11.0
- **Application Insights** — `Microsoft.ApplicationInsights.WorkerService` v2.23.0
- **SQL Server** — `Microsoft.Data.SqlClient` v7.0.0
- **Azure Logic Apps** — No-code email notification workflow
- **Azure Key Vault** — Secret management for SAS tokens

---

## Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd DemoFunctionApp
   ```

2. **Install prerequisites**
   - [.NET 8 SDK](https://dotnet.microsoft.com/download)
   - [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
   - [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (local storage emulator)

3. **Configure `local.settings.json`**
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "saurabhSAS": "<your-service-bus-connection-string>",
       "AZURE_TABLES_CONNECTION_STRING": "<your-storage-account-connection-string>",
       "TABLES_TABLE_NAME": "Courses",
       "blobconstr": "<your-blob-storage-connection-string>"
     }
   }
   ```
   > **Note:** Never commit real connection strings to source control. Use Azure Key Vault or environment variables in production.

4. **Run locally**
   ```bash
   func start
   ```

---

## Design Decisions

| Decision | Reason |
|---|---|
| Service Bus Topic over Queue | Enables fan-out — multiple subscriptions can consume the same message independently |
| Isolated worker model (.NET 8) | Full control over middleware, DI, and startup pipeline; better performance |
| SAS tokens + Key Vault | Avoids embedding storage keys in code; enables secret rotation |
| Logic Apps for email | No-code integration; decouples notification logic from Function App code |
| Application Insights sampling | Reduces telemetry cost while retaining full Request-level tracing |
