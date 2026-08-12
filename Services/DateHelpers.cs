namespace IFormQualityApp.Services;

public static class DateHelpers
{
    // Npgsql stores DateTime as timestamptz, which requires a set DateTimeKind.
    // Form-bound dates come in as Unspecified, so normalize them to UTC before save.
    public static DateTime? ToUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        var dt = value.Value;
        return dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
    }
}
