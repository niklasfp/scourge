using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Scourge.Throttling;

namespace Scourge.AspNetCore.Throttling.Middleware;

internal sealed class ThrottlingMiddleware
{

    private readonly RequestDelegate _next;

    public ThrottlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // TODO: Maybe add a prefix where throttling is ignored, just to make sure you don't lock your self out :D
    public async Task InvokeAsync(HttpContext context)
    {
        // TODO: Consider scoping throttle config to per request
        var config = context.RequestServices.GetService<ThrottlingMiddlewareConfigService>();
        
        if (config is { Enabled: true })
        {
            var originalStream = context.Response.Body;

            await using var throttleStream = CreateThrottledWriteStream(originalStream, config.MaxBytesPerSecond);

            context.Response.Body = throttleStream;
            try
            {
                await _next(context);
            }
            finally
            {
                context.Response.Body = originalStream;
            }

            return;
        }

        await _next(context);
    }

    private static Stream CreateThrottledWriteStream(Stream baseStream, int maxBytesPerSecond)
    {
        return new ThrottledWriteStream(baseStream, new Throttler(maxBytesPerSecond), false);
    }
}