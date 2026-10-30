using Sudoku.Application.Settings;

namespace Sudoku.WinForms.Theming;

/// <summary>Paleta inmutable con nombres semánticos. Los controles leen de aquí y nunca usan colores fijos.</summary>
public sealed record Theme
{
    public required ThemeKind Kind { get; init; }

    // Ventana y barra lateral
    public required Color Background { get; init; }
    public required Color Surface { get; init; }
    public required Color Text { get; init; }
    public required Color MutedText { get; init; }
    public required Color Accent { get; init; }

    // Botones
    public required Color Control { get; init; }
    public required Color ControlText { get; init; }
    public required Color ControlBorder { get; init; }

    // Tablero
    public required Color BoardBackground { get; init; }
    public required Color BoardLine { get; init; }
    public required Color BoxLine { get; init; }
    public required Color GivenText { get; init; }
    public required Color UserText { get; init; }
    public required Color SelectedCell { get; init; }
    public required Color RelatedCell { get; init; }
    public required Color MatchingValue { get; init; }
    public required Color ConflictText { get; init; }
    public required Color NoteText { get; init; }
}