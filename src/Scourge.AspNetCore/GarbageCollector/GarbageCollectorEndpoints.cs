using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Scourge.AspNetCore.GarbageCollector.Models;

namespace Scourge.AspNetCore.GarbageCollector;

internal static class GarbageCollectorEndpoints
{
    public static RouteGroupBuilder MapGarbageCollectorApi(this RouteGroupBuilder group)
    {
        group.WithTags("Garbage Collector");

        // Managed
        group.MapGet("", () => TypedResults.Ok(Stats()))
            .WithOpenApi(operation =>
            {
                operation.Description = "Get statistics from the DotNet garbage collector.";
                operation.Summary = "DotNet Garbage Collector statistics";
                return operation;
            });

        group.MapPost("/collect/{generation?}", (int? generation) => Collect(generation ?? -1))
            .WithOpenApi(operation =>
            {
                operation.Description = "Get statistics from the DotNet garbage collector.";
                operation.Summary = "DotNet Garbage Collector statistics";
                return operation;
            });

        return group;
    }

    private static Results<Ok, BadRequest<string>> Collect(int generation)
    {
        switch (generation)
        {
            case > 3:
                {
                    var v = TypedResults.BadRequest("Invalid generation, valid generations are: 0..3");
                    return v;
                }
            case < 0:
                GC.Collect();
                break;
            default:
                GC.Collect(generation);
                break;
        }

        GC.WaitForPendingFinalizers();

        return TypedResults.Ok();
    }

    private static GarbageCollectorInfo Stats()
    {
        return new GarbageCollectorInfo(GC.GetGCMemoryInfo());
    }
}