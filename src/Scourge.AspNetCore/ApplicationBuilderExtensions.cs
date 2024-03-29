﻿
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using Scourge.AspNetCore;
using Scourge.AspNetCore.GarbageCollector;
using Scourge.AspNetCore.Hurt;
using Scourge.AspNetCore.Information;
using Scourge.AspNetCore.Log;
using Scourge.AspNetCore.Logging;
using Scourge.AspNetCore.Memory;
using Scourge.Memory;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
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

        return endpoints;
    }

    private static string AddTrailingSlash(string str)
        => str.EndsWith('/') ? str : str + "/";

}
