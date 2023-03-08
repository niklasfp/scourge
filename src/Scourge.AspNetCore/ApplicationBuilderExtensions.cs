using Microsoft.AspNetCore.Routing;
using Scourge.AspNetCore.DynamicData;
using Scourge.AspNetCore.GarbageCollector;
using Scourge.AspNetCore.Hurt;
using Scourge.AspNetCore.Information;
using Scourge.AspNetCore.Logging;
using Scourge.AspNetCore.Memory;
using Scourge.AspNetCore.Throttling;
using Scourge.AspNetCore.Throttling.Middleware;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseScourgeThrottling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ThrottlingMiddleware>();
        return app;
    }

    public static IEndpointRouteBuilder MapScourge(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string prefix)
    {
        prefix = AddTrailingSlash(prefix);

        // Memory end points
        endpoints.MapAllocatorApi(prefix + "mem");

        // GC
        var garbageCollectorGroup = endpoints.MapGroup(prefix + "gc");
        garbageCollectorGroup.MapGarbageCollectorApi();

        // Hurt apis
        var hurtGroup = endpoints.MapGroup(prefix + "hurt");
        hurtGroup.MapHurtApi();

        // System apis
        var systemGroup = endpoints.MapGroup(prefix + "sys");
        systemGroup.MapSystemApi();

        // Process apis
        var processGroup = endpoints.MapGroup(prefix + "proc");
        processGroup.MapProcessApi();

        // Process apis
        var loggingGroup = endpoints.MapGroup(prefix + "log");
        loggingGroup.MapLogApi();

        var throttleGroup = endpoints.MapGroup(prefix + "throttle");
        throttleGroup.MapThrottlingApi();

        var dynamicDataGroup = endpoints.MapGroup(prefix + "data");
        dynamicDataGroup.MapDynamicDataApi();

        return endpoints;
    }

    private static string AddTrailingSlash(string str)
        => str.EndsWith('/') ? str : str + "/";

}
