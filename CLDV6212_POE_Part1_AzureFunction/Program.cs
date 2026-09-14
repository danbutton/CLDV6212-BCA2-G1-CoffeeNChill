/*I used these as part of learning how to make Azure functions. 
 * Reference List
 * Kumar, B. 2023. Azure FunctionsRest API Example C#. [online] 8 October 
 * Available at: <https://azurelessons.com/how-to-create-api-with-azure-functions/> [Date Accessed 11 September 2026]
 * 
 * Microsoft. 2026. Azure Functions C# HTTP Trigger using Azure Developer CLI. [online]
 * 25 January Available at: <https://learn.microsoft.com/en-us/samples/azure-samples/functions-quickstart-dotnet-azd/starter-http-trigger-csharp/> [Date Accessed 11 September 2026]
 */

using Azure.Monitor.OpenTelemetry.Exporter;
using CoffeeNChill.Services;
using CLDV6212_POE_Part1_AzureFunction.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

// Enables the ASP.NET Core HTTP integration so functions can bind
// HttpRequest and return IActionResult.
builder.ConfigureFunctionsWebApplication();

// Services are registered as singletons because BlobContainerClient and
// TableClient are thread-safe and designed to be long-lived; the Functions
// host resolves them into each function via constructor injection [6].
builder.Services.AddSingleton<IDocumentRepository, BlobDocumentRepository>();
builder.Services.AddSingleton<MenuItemService>();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
