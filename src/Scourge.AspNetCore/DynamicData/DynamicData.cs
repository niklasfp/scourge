using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Net.Mime;

namespace Scourge.AspNetCore.DynamicData;

internal static class DynamicData
{
    private const string Tag = "Dynamic Data";

    public static RouteGroupBuilder MapDynamicDataApi(this RouteGroupBuilder group)
    {
        group.WithTags(Tag);

        group.MapGet("", ([FromQuery] int count, CancellationToken cancellationToken)
                => Stream(count, cancellationToken))
            .WithOpenApi(operation =>
            {
                operation.Description = $"Download a binary file consisting of 'count' random bytes";
                operation.Summary = "Download random data.";
                return operation;
            })
            .Produces(200, contentType: MediaTypeNames.Application.Octet)
            .Produces(400, contentType: MediaTypeNames.Text.Plain);


        return group;
    }

    private static Results<PushStreamHttpResult, BadRequest> Stream(int count, CancellationToken cancellationToken)
    {
        const int BufferSize = 4096;

        if (count <= 0)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Stream(async stream =>
        {
            var buffer = new byte[BufferSize];

            int bytesWritten = 0;

            do
            {
                var bytesToWrite = bytesWritten + BufferSize <= count ? BufferSize : count - bytesWritten;
                if (bytesToWrite <= 0)
                {
                    break;
                }

                Random.Shared.NextBytes(buffer);

                await stream.WriteAsync(buffer, 0, bytesToWrite, cancellationToken);
                bytesWritten += bytesToWrite;
            } while (bytesWritten > 0 && !cancellationToken.IsCancellationRequested);

        }, MediaTypeNames.Application.Octet, "AFile.bin");
    }
}