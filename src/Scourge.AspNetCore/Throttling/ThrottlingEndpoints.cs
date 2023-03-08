using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Scourge.AspNetCore.Throttling.Middleware;
using Scourge.AspNetCore.Throttling.Models;

namespace Scourge.AspNetCore.Throttling;


internal static class ThrottlingEndpoints
{
    public static RouteGroupBuilder MapThrottlingApi(this RouteGroupBuilder group)
    {
        group.WithTags("Throttling");

        group.MapPost("enable",  EnableThrottling)
            .WithOpenApi(operation =>
            {
                operation.Description = "Enables throttling of responses";
                operation.Summary = "Enable throttling";
                return operation;
            });

        group.MapGet("", GetThrottling)
            .WithOpenApi(operation =>
            {
                operation.Description = "Gets the throttling settings";
                operation.Summary = "Throttling details";
                return operation;
            });

        group.MapPost("disable", DisableThrottling)
            .WithOpenApi(operation =>
            {
                operation.Description = "Disable throttling of responses";
                operation.Summary = "Disable throttling.";
                return operation;
            });

        
        return group;
    }

    private static ThrottlingInfo GetThrottling(HttpContext context, ThrottlingMiddlewareConfigService configService)
    {
        return new ThrottlingInfo(configService.Enabled, configService.MaxBytesPerSecond, configService.MaxBytesPerSecond.ToFormattedSpeed());
    }

    private static Ok DisableThrottling(HttpContext context, ThrottlingMiddlewareConfigService configService)
    {
        configService.Enabled = true;
        return TypedResults.Ok();
    }

    private static Ok EnableThrottling(HttpContext context, ThrottlingMiddlewareConfigService configService)
    {
        configService.Enabled = true;
        return TypedResults.Ok();
    }
}

