namespace sos100_bibliotek.Helpers;

public static class SwedenTime
{
    private static readonly Lazy<TimeZoneInfo> StockholmTz = new(() =>
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
    });

    /// <summary>
    /// ReservationAPI sparar UTC. JSON ger ofta <see cref="DateTimeKind.Unspecified"/> — behandlas som UTC.
    /// </summary>
    public static DateTime ToStockholm(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => TimeZoneInfo.ConvertTimeToUtc(value, TimeZoneInfo.Local),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utc, StockholmTz.Value);
    }

    public static string FormatStockholm(DateTime value, string format = "yyyy-MM-dd HH:mm")
        => ToStockholm(value).ToString(format);
}
