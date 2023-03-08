using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Scourge.AspNetCore.Information.Models;

namespace Scourge.AspNetCore.Information;

internal static class SystemEndpoints
{
    public static RouteGroupBuilder MapSystemApi(this RouteGroupBuilder group)
    {
        group.WithTags("System");

        group.MapGet("", () => new SystemInfo(Environment.MachineName, GC.GetGCMemoryInfo().TotalAvailableMemoryBytes, Environment.ProcessorCount))
            .WithOpenApi(operation =>
            {
                operation.Description = "Returns what info about the metal";
                operation.Summary = "system information.";
                return operation;
            });

        return group;
    }
}

