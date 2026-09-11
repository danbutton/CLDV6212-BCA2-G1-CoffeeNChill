using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Middleware;

/// <summary>
/// Converts unhandled exceptions into a consistent JSON error envelope so
/// clients never receive a raw stack trace.
/// </summary>
/// <remarks>OWNER: B — STUB. See issue [B] "add ExceptionHandlingMiddleware".</remarks>
public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        => _logger = logger;

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        // TODO(B): wrap in try/catch, map RequestFailedException status codes
        //          (404 -> NotFound, 409 -> Conflict, else 500), log with the
        //          function name, write a JSON envelope to the response.
        await next(context);
    }
}
