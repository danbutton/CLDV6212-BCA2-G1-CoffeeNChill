using CoffeeNChill.Functions.Middleware;
using CoffeeNChill.Functions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ── SHARED FILE ─────────────────────────────────────────────────────────────
// All DI registrations were added during the contract sprint so nobody has to
// edit this file while implementing their own layer.
//
// If you genuinely need a change here: announce it in the group chat, make the
// change alone in a one-line PR, merge immediately, tell everyone to pull.
// ────────────────────────────────────────────────────────────────────────────

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker =>
    {
        worker.UseMiddleware<ExceptionHandlingMiddleware>();          // owner: B
    })
    .ConfigureServices(services =>
    {
        // Singletons: TableClient and ShareClient are thread-safe and designed
        // to be long-lived, so one per request would waste connections.
        services.AddSingleton<IMenuRepository, MenuRepository>();                    // owner: A
        services.AddSingleton<IDocumentRepository, BlobDocumentRepository>();   // owner: C
    })
    .Build();

host.Run();
