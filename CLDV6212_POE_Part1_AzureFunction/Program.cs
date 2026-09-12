using Azure.Monitor.OpenTelemetry.Exporter;
using CLDV6212_POE_Part1_AzureFunction.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

// Enables the ASP.NET Core HTTP integration, so functions can bind
// HttpRequest and return IActionResult rather than HttpResponseData.
builder.ConfigureFunctionsWebApplication();

// Registered as a singleton: BlobContainerClient is thread-safe and designed
// to be long-lived, so creating one per request would waste connections.
// DocumentFunctions receives it through constructor injection.
builder.Services.AddSingleton<IDocumentRepository, BlobDocumentRepository>();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
