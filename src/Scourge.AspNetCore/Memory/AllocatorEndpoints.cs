﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Scourge.AspNetCore.Memory.Models;
using Scourge.Memory;

namespace Scourge.AspNetCore.Memory;

internal class AllocatorEndpoints<T> where T : struct, IAllocatorEntry
{
    private readonly IAllocator<T> _allocator;

    public AllocatorEndpoints(IAllocator<T> allocator)
    {
        _allocator = allocator;
    }

    public RouteGroupBuilder RegisterEndpoints(RouteGroupBuilder group, string prefix)
    {
        var name = prefix;

        group.MapPost("/{size}/" + name, Alloc)
            .WithOpenApi(operation =>
            {
                operation.Description =
                    "Allocates the given number of bytes and returns and id of the allocated memory.";
                operation.Summary = "Allocates memory.";
                return operation;
            });

        group.MapDelete("/{key:int}/" + name, Free)
            .WithOpenApi(operation =>
            {
                operation.Description = "Frees the memory belonging to the specified id";
                operation.Summary = "Frees previously allocated memory.";
                operation.Parameters[0].Description = "The id associated with the allocation.";

                return operation;
            });

        group.MapGet(name, Statistics)
            .WithOpenApi(operation =>
            {
                operation.Description = "Gets the detailed allocator statistics.";
                operation.Summary = "Statistics";

                return operation;
            });

        group.MapDelete("" + name, Clear)
            .WithOpenApi(operation =>
            {
                operation.Description = "Free all allocated memory";
                operation.Summary = "Statistics";

                return operation;
            });

        return group;
    }

    private Ok Clear()
    {
        _allocator.Clear();
        return TypedResults.Ok();
    }

    private AllocatorInfo Statistics()
    {
        return AllocatorInfo.Map(_allocator.GetStatistics());
    }

    private Ok<MemoryEntryModel> Alloc(uint size)
    {
        var entry = _allocator.Alloc(size);
        return TypedResults.Ok(MemoryEntryModel.Map(entry));
    }

    private Results<Ok<uint>, NotFound> Free(int key)
    {
        var freedBytes = _allocator.Free(new EntryKey((uint)key));
        if (freedBytes == 0)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(freedBytes);
    }
}