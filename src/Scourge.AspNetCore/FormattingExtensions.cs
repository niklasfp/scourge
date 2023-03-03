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