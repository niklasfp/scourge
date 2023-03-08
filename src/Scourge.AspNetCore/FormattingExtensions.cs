namespace Scourge.AspNetCore;

internal static class FormattingExtensions
{
    public static string ToFormattedSize(this long size)
    {
        return size switch
        {
            >= 1048576000 => $"{size / 1073741824:f2} GiB",
            >= 1024000 => $"{size / 1048576:f2} MiB",
            >= 1000 => $"{size / 1024:f2} KiB",
            _ => $"{size}"
        };
    }

    public static string ToFormattedSpeed(this int bytesPerSecond)
    {
        return bytesPerSecond switch
        {
            >= 1048576000 => $"{bytesPerSecond / 1073741824:f2} GB/s",
            >= 1024000 => $"{bytesPerSecond / 1048576:f2} MB/s",
            >= 1000 => $"{bytesPerSecond / 1024:f2} KB/s",
            _ => $"{bytesPerSecond} Bytes/s"
        };
    }

    public static string ToFormattedDuration(this TimeSpan span)
    {
        return span.TotalSeconds switch
        {
            >= 3600 => $"{span.TotalHours:F2} Hours",
            >= 60 => $"{span.TotalMinutes:F2} Minutes",
            > 0 and < 60 => $"{span.TotalSeconds:F2} Seconds",
            _ => $"{span.TotalMilliseconds:F2} Ms"
        };
    }
}