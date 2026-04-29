using Azure;
using Azure.Data.Tables;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Text.Json;

namespace DemoFunctionApp;

public class InsertCourses
{
    private readonly ILogger<InsertCourses> _logger;
    private readonly TableClient _table;

    // Aapke pehle code ki tarah TableClient yahan pass ho raha hai (Dependency Injection)
    public InsertCourses(ILogger<InsertCourses> logger, TableClient table)
    {
        _logger = logger;
        _table = table;
    }

    [Function("InsertCourses")]
    public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
    FunctionContext executionContext)
    {
        // Read request body
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        // Validate/parse JSON
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (!root.TryGetProperty("courseID", out var courseIdEl) ||
            !root.TryGetProperty("courseName", out var courseNameEl) ||
            !root.TryGetProperty("rating", out var ratingEl))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Request must include courseID, courseName, and rating.");
            return bad;
        }

        // Safe parsing (Number aur String dono ke liye)
        string courseID = courseIdEl.ValueKind == JsonValueKind.Number
                          ? courseIdEl.GetInt32().ToString()
                          : courseIdEl.GetString()!;

        string courseName = courseNameEl.GetString()!;

        string rating = ratingEl.ValueKind == JsonValueKind.Number
                        ? ratingEl.GetRawText()
                        : ratingEl.GetString()!;

        var entity = new CourseEntity
        {
            PartitionKey = rating.ToString(),  // PK = rating
            RowKey = courseID,                 // RK = courseID
            CourseID = courseID,
            CourseName = courseName,
            Rating = rating
        };

        await _table.UpsertEntityAsync(entity, TableUpdateMode.Merge);

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteStringAsync($"Upserted course '{courseID}' in partition '{rating}'.");
        return ok;

    }
}