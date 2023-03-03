using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Scourge.AspNetCore.Memory.Models;
using Scourge.Memory;

namespace Scourge.AspNetCore.Memory;

internal static class AllocEndpoints
{
    public static IEndpointRouteBuilder MapAllocatorApi(this IEndpointRouteBuilder endpoints, string prefix)
    {
        // Memory Info
        var allocatorGroup = endpoints.MapGroup(prefix).WithTags("Memory");

        // Managed
        var managedAllocator = (ManagedAllocator)endpoints.ServiceProvider.GetService(typeof(ManagedAllocator))!;
        new AllocatorEndpoints<ManagedAllocatorEntry>(managedAllocator).RegisterEndpoints(allocatorGroup, "managed");

        // Unmanaged
        var unmanagedAllocator = (UnmanagedAllocator)endpoints.ServiceProvider.GetService(typeof(UnmanagedAllocator))!;
        new AllocatorEndpoints<UnmanagedAllocatorEntry>(unmanagedAllocator).RegisterEndpoints(allocatorGroup,
            "unmanaged");

        allocatorGroup.MapGet("", (ManagedAllocator managed, UnmanagedAllocator unmanaged) =>
        {
            var managedStats = managed.GetStatistics();
            var unmanagedStats = unmanaged.GetStatistics();

            return AllAllocatorsInfo.Map(
                managed: managed.GetStatistics(),
                unmanaged: unmanaged.GetStatistics());
        });

        return endpoints;
    }
}