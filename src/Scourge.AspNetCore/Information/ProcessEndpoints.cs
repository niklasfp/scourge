using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Scourge.AspNetCore.Information.Models;
using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Scourge.AspNetCore.Information;

internal static class ProcessEndpoints
{
    public static RouteGroupBuilder MapProcessApi(this RouteGroupBuilder group)
    {
        group.WithTags("Process");

        group.MapGet("/{processId?}", (int? processId) => GetProcessInfo(processId ?? -1))
            .WithOpenApi(operation =>
            {
                operation.Description = "Returns a list of all environment variables set";
                operation.Summary = "Environment variables.";
                return operation;
            });

        group.MapGet("/env", GetEnvironment)
            .WithOpenApi(operation =>
            {
                operation.Description = "Returns a list of all environment variables set";
                operation.Summary = "Environment variables.";
                return operation;
            });

        group.MapGet("/config", GetConfiguration)
            .WithOpenApi(operation =>
            {
                operation.Description = "Returns a list of all configuration settings";
                operation.Summary = "Configuration...a lot of configuration.";
                return operation;
            });

        group.MapGet("memory", Stats)
            .WithOpenApi(operation =>
            {
                operation.Description = "Get memory information such as working set size.";
                operation.Summary = "Application memory information.";
                return operation;
            });


        group.MapGet("list", GetProcessList)
            .WithOpenApi(operation =>
            {
                operation.Description = "Get the list of processes running on the system.";
                operation.Summary = "Application memory information.";
                return operation;
            });

        return group;
    }

    private static Results<Ok<ProcessInfo>, NotFound> GetProcessInfo(int processId)
    {
        try
        {
            var process = processId != -1 ? Process.GetProcessById(processId) : Process.GetCurrentProcess();
            return TypedResults.Ok(new ProcessInfo(process));
        }
        catch (ArgumentException e)
        {
            return TypedResults.NotFound();
        }
    }

    private static List<ProcessInfo> GetProcessList()
    {
        return Process.GetProcesses().Select(p => new ProcessInfo(p)).ToList();
    }

    private static Dictionary<string, string?> GetConfiguration(HttpContext context, IConfiguration config)
    {
        return new Dictionary<string, string?>(config.AsEnumerable());
    }

    private static Dictionary<string, string> GetEnvironment(HttpContext context)
    {
        return Environment.GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .ToDictionary(entryKey => entryKey.Key!.ToString(), entryVal => entryVal.Value!.ToString());
    }

    private static MemoryInfo Stats()
    {
        using var process = Process.GetCurrentProcess();
        return MemoryInfo.FromProcess(process);
    }
}
