using Domain;
using Sudoku.Application.Statistics;
using Sudoku.WinForms.Formatting;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Views;

/// <summary>
/// Diálogo modal de estadísticas. Usa solo Label/Panel/Button (que ThemeApplier ya pinta);
/// ListView y DataGridView dibujarían cabeceras claras con el tema oscuro.
/// </summary>
public sealed class StatisticsForm : Form
{
    private readonly Font _boldFont = new(DefaultFont, FontStyle.Bold);

    public StatisticsForm(IThemeService themes, GameStatistics statistics)
    {
        Text = "Estadísticas";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(620, 520);

        var close = new Button { Text = "Cerrar", DialogResult = DialogResult.Cancel, Dock = DockStyle.Bottom, Height = 36 };
        CancelButton = close;

        var content = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12)
        };

        content.Controls.Add(Cell("Por dificultad", bold: true));
        content.Controls.Add(BuildGrid(
            ["Dificultad", "Iniciadas", "Ganadas", "Perdidas", "Mejor tiempo", "Tiempo medio"],
            Enum.GetValues<Difficulty>().Select(d => DifficultyRow(d, statistics.For(d)))));

        content.Controls.Add(Cell("Últimas partidas", bold: true));
        content.Controls.Add(statistics.Recent.Count == 0
            ? Cell("Aún no hay partidas terminadas.")
            : BuildGrid(
                ["Fecha", "Dificultad", "Resultado", "Tiempo"],
                statistics.Recent.Select(RecordRow)));

        // El control con Dock.Fill se añade primero; los de borde después.
        Controls.Add(content);
        Controls.Add(close);

        ThemeApplier.Apply(themes.Current, this);
    }

    private static string[] DifficultyRow(Difficulty difficulty, DifficultyStatistics s) =>
    [
        DisplayNames.Of(difficulty),
        s.Started.ToString(),
        s.Won.ToString(),
        s.Lost.ToString(),
        DurationFormat.Format(s.BestTime),
        DurationFormat.Format(s.AverageWonTime)
    ];

    private static string[] RecordRow(GameRecord r) =>
    [
        r.FinishedAt.ToLocalTime().ToString("g"),
        DisplayNames.Of(r.Difficulty),
        DisplayNames.Of(r.Outcome),
        DurationFormat.Format(r.Elapsed)
    ];

    private TableLayoutPanel BuildGrid(string[] headers, IEnumerable<string[]> rows)
    {
        var grid = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = headers.Length,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows,
            Margin = new Padding(0, 4, 0, 16)
        };
        foreach (var header in headers)
            grid.Controls.Add(Cell(header, bold: true));
        foreach (var row in rows)
            foreach (var text in row)
                grid.Controls.Add(Cell(text));
        return grid;
    }

    private Label Cell(string text, bool bold = false) => new()
    {
        Text = text,
        AutoSize = true,
        Margin = new Padding(6, 3, 18, 3),
        Font = bold ? _boldFont : DefaultFont
    };

    private void InitializeComponent()
    {

    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _boldFont.Dispose();
        base.Dispose(disposing);
    }
}
