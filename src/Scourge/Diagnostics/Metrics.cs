using System.Diagnostics.Metrics;

namespace Scourge.Diagnostics;

internal static class Metrics
{
    private static readonly Meter Meter = new Meter("Scourge", "1.0.0");

    private static ObservableInstrument<long> _unmanagedInstrument;
    private static ObservableInstrument<long> _managedInstrument;

    private static long _unmanagedAllocated;
    private static long _managedAllocated;

    static Metrics()
    {
        _managedInstrument = Meter.CreateObservableUpDownCounter<long>(
            "Managed",
            () => _unmanagedAllocated,
            "bytes",
            "Total number of bytes allocated by Managed Allocators.");

        _unmanagedInstrument = Meter.CreateObservableUpDownCounter<long>(
            "Unmanaged",
            () => _unmanagedAllocated,
            "bytes",
            "Total number of bytes allocated by Unmanaged Allocators.");
    }

    public static void AddManagedAllocatedOrDeallocated(long numberOfBytes)
    {
        Interlocked.Add(ref _managedAllocated, numberOfBytes);
    }

    public static void AddUnManagedAllocatedOrDeallocated(long bytesAllocated)
    {
        Interlocked.Add(ref _unmanagedAllocated, bytesAllocated);
    }
}