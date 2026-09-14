/*I used these as part of learning how to make Azure functions. 
 * Reference List
 * Kumar, B. 2023. Azure FunctionsRest API Example C#. [online] 8 October 
 * Available at: <https://azurelessons.com/how-to-create-api-with-azure-functions/> [Date Accessed 11 September 2026]
 * 
 * Microsoft. 2026. Azure Functions C# HTTP Trigger using Azure Developer CLI. [online]
 * 25 January Available at: <https://learn.microsoft.com/en-us/samples/azure-samples/functions-quickstart-dotnet-azd/starter-http-trigger-csharp/> [Date Accessed 11 September 2026]
 */

using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
