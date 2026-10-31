namespace Sudoku.WinForms.Formatting;

/// <summary>Única fuente del formato de tiempos en la UI (mm:ss, o h:mm:ss a partir de una hora).</summary>
public static class DurationFormat
{
    public static string Format(TimeSpan duration) =>
        duration.TotalHours >= 1
            ? (int)duration.TotalHours + ":" + duration.ToString(@"mm\:ss")
            : duration.ToString(@"mm\:ss");

    public static string Format(TimeSpan? duration) => duration is { } value ? Format(value) : "—";
}
