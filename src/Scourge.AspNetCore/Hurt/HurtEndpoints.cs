using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Scourge.AspNetCore.Hurt.Models;
using Scourge.Hurt;

namespace Scourge.AspNetCore.Hurt;

internal static class HurtEndpoints
{
    public static RouteGroupBuilder MapHurtApi(this RouteGroupBuilder group)
    {
        group.WithTags("Stuff that hurts");

        // Managed
        group.MapPost("/stackoverflow", Crashalot.StackOverflow)
            .WithOpenApi(operation =>
            {
                operation.Description = "Causes a stack overflow, no one will help you now.";
                operation.Summary = "Stack overflow";
                return operation;
            });

        // TODO: Make throw accept a type name, or maybe expose an enum of exceptions we want to support.
        group.MapPost("/throw", () => Crashalot.ThrowException(typeof(ArgumentException)))
            .WithOpenApi(operation =>
            {
                operation.Description = "Throws an exception. for now ONLY ArgumentException is thrown.";
                operation.Summary = "Throw whatever.";
                return operation;
            });

        group.MapPost("/throwasyncvoid", Crashalot.AsyncVoidThrow)
            .WithOpenApi(operation =>
            {
                operation.Description = "Throws an exception in an async method with void return.";
                operation.Summary = "Throw async void";
                return operation;
            });

        group.MapPost("/startThreadWork", (IWorkManager workManager, int? numberOfThreads) => StartWork(workManager, numberOfThreads ?? 1))
            .WithOpenApi(operation =>
            {
                operation.Description = "Starts a number of threads.";
                operation.Summary = "Starts threads";
                return operation;
            });

        group.MapPost("/stopWork", (IWorkManager workManager, [FromQuery] string workId) => StopWork(workManager, workId))
            .WithOpenApi(operation =>
            {
                operation.Description = "Stops a job by a given id.";
                operation.Summary = "Stops work";
                return operation;
            });

        group.MapGet("/listWork", GetWork)
            .WithOpenApi(operation =>
            {
                operation.Description = "List work currently running.";
                operation.Summary = "Lists registered work.";
                return operation;
            });


        return group;
    }

    private static IEnumerable<Work> GetWork(IWorkManager workManager)
    {
        return workManager.GetActiveWork()
            .Select(work => new Work(work.Id, true, work.StartTime));
    }

    private static Ok StopWork(IWorkManager workManager, string workId)
    {
        workManager.StopWork(workId);
        return TypedResults.Ok();
    }

    private static Work StartWork(IWorkManager workManager, int threadCount = 1)
    {
        var work = workManager.StartWork(cancel => ThreadWhipper.DoNothingWork(threadCount, cancel));
        return new Work(work.Id, true, work.StartTime);
    }
}