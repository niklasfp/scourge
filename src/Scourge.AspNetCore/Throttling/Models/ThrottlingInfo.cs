namespace Scourge.AspNetCore.Throttling.Models
{
    public record ThrottlingInfo(bool Enabled, int BytesPerSecond, string BytesPerSecondInfo);
}
