using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Data.Tables;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
// ? Register TableClient properly
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var tableName = config["TABLES_TABLE_NAME"] ?? "Courses";
    var conn = config["AZURE_TABLES_CONNECTION_STRING"]
        ?? throw new InvalidOperationException("AZURE_TABLES_CONNECTION_STRING is missing.");

    var tableClient = new TableClient(conn, tableName);
    tableClient.CreateIfNotExists();

    Console.WriteLine($"[Startup] TableClient registered for table '{tableName}'.");
    return tableClient;
});

builder.Build().Run();
