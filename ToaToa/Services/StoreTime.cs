namespace ToaToa.Services;

/// <summary>
/// Fuso horário da loja (Mato Grosso, America/Cuiaba, UTC-4). Os dados são gravados em UTC;
/// aqui convertemos para exibição e para filtrar "o dia" corretamente, independente do
/// fuso do servidor.
/// </summary>
public static class StoreTime
{
    private static readonly TimeZoneInfo Tz = Resolver();

    private static TimeZoneInfo Resolver()
    {
        foreach (var id in new[] { "America/Cuiaba", "America/Sao_Paulo", "E. South America Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch { /* tenta o próximo */ }
        }
        return TimeZoneInfo.CreateCustomTimeZone("BRT-4", TimeSpan.FromHours(-4), "Horário MT", "Horário MT");
    }

    public static DateTime ToLocal(DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Tz);

    public static DateTime Today => ToLocal(DateTime.UtcNow).Date;

    /// <summary>Início (inclusive) e fim (exclusivo) de um dia local, já em UTC, para filtros.</summary>
    public static (DateTime inicioUtc, DateTime fimUtc) DiaUtc(DateTime dataLocal)
    {
        var inicioLocal = DateTime.SpecifyKind(dataLocal.Date, DateTimeKind.Unspecified);
        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(inicioLocal, Tz);
        return (inicioUtc, inicioUtc.AddDays(1));
    }
}
