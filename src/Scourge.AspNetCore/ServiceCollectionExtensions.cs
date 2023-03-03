// ReSharper disable once CheckNamespace

using Scourge.Hurt;
using Scourge.Memory;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScourge(this IServiceCollection services)
    {
        return services
            .AddSingleton<IWorkManager>(IWorkManager.CreateDefault())
            .AddSingleton<ManagedAllocator>()
            .AddSingleton<UnmanagedAllocator>();

    }
}