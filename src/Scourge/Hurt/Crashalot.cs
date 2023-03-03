using System.Diagnostics;

namespace Scourge.Hurt;

public static class Crashalot
{
    public static void StackOverflow()
    {
        while (true)
        {
            StackOverflow();
        }

        // ReSharper disable once FunctionNeverReturns - absolutely right..it hurts.
    }

    [StackTraceHidden]
    public static void ThrowException(Type exceptionType, string? message = default)
    {
        if (!typeof(Exception).IsAssignableFrom(exceptionType))
        {
            // Throwing an exception in the throw exception method :D
            throw new ArgumentException($"{exceptionType} is not an exception type.");
        }

        var exceptionObj = (Exception)Activator.CreateInstance(exceptionType, message ?? "It hurts.")!;
        throw exceptionObj;
    }

    [StackTraceHidden]
    public static async void AsyncVoidThrow()
    {
        await Task.Run(() => throw new InvalidOperationException("Feels bad right"));
    }
}
