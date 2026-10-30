using Domain;
using Sudoku.WinForms.Theming;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Sudoku.WinForms.Controls.BoardControl;

internal static class BoardRenderer
{
    private const string FontFamily = "Segoe UI";

    public static void Draw(
        Graphics graphics,
        Rectangle clientBounds,
        BoardGeometry geometry,
        Board board,
        Position? selectedPosition,
        Theme theme)
    {
        var graphicsState = graphics.Save();

        try
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using var surfaceBrush = new SolidBrush(theme.Surface);
            graphics.FillRectangle(surfaceBrush, clientBounds);

            if (geometry.CellSize <= 0)
                return;

            DrawCells(graphics, geometry, board, selectedPosition, theme);
            DrawGrid(graphics, geometry, theme);
        }
        finally
        {
            graphics.Restore(graphicsState);
        }
    }

    private static void DrawCells(
        Graphics graphics,
        BoardGeometry geometry,
        Board board,
        Position? selectedPosition,
        Theme theme)
    {
        var relatedCells = GetRelatedCells(selectedPosition);
        var matchingCells = GetMatchingCells(board, selectedPosition);
        var conflicts = SudokuRules.FindConflicts(board);

        using var boardBrush = new SolidBrush(theme.BoardBackground);
        using var relatedBrush = new SolidBrush(theme.RelatedCell);
        using var matchingBrush = new SolidBrush(theme.MatchingValue);
        using var selectedBrush = new SolidBrush(theme.SelectedCell);

        graphics.FillRectangle(boardBrush, geometry.Bounds);

        var cellSize = geometry.CellSize;

        using var valueFont = new Font(
            FontFamily,
            cellSize * 0.50f,
            FontStyle.Regular,
            GraphicsUnit.Pixel);

        using var givenFont = new Font(
            FontFamily,
            cellSize * 0.50f,
            FontStyle.Bold,
            GraphicsUnit.Pixel);

        using var noteFont = new Font(
            FontFamily,
            cellSize * 0.20f,
            FontStyle.Regular,
            GraphicsUnit.Pixel);

        foreach (var position in Position.All)
        {
            var cellBounds = geometry.GetCellBounds(position);

            if (relatedCells.Contains(position))
                graphics.FillRectangle(relatedBrush, cellBounds);

            if (matchingCells.Contains(position))
                graphics.FillRectangle(matchingBrush, cellBounds);

            if (selectedPosition == position)
                graphics.FillRectangle(selectedBrush, cellBounds);

            DrawCellContent(
                graphics,
                cellBounds,
                board[position],
                conflicts.Contains(position),
                valueFont,
                givenFont,
                noteFont,
                theme);
        }
    }

    private static HashSet<Position> GetRelatedCells(Position? selectedPosition)
    {
        if (selectedPosition is not { } position)
            return [];

        return SudokuRules.Peers(position).ToHashSet();
    }

    private static HashSet<Position> GetMatchingCells(
        Board board,
        Position? selectedPosition)
    {
        if (selectedPosition is not { } position)
            return [];

        var selectedValue = board[position].Value;

        if (selectedValue is null)
            return [];

        return Position.All
            .Where(candidate => candidate != position)
            .Where(candidate => board[candidate].Value == selectedValue)
            .ToHashSet();
    }

    private static void DrawCellContent(
        Graphics graphics,
        RectangleF bounds,
        Cell cell,
        bool hasConflict,
        Font valueFont,
        Font givenFont,
        Font noteFont,
        Theme theme)
    {
        if (cell.Value is int value)
        {
            var color = hasConflict
                ? theme.ConflictText
                : cell.IsGiven
                    ? theme.GivenText
                    : theme.UserText;

            var font = cell.IsGiven ? givenFont : valueFont;

            DrawCenteredText(
                graphics,
                value.ToString(),
                font,
                color,
                bounds);

            return;
        }

        if (!cell.Notes.IsEmpty)
            DrawNotes(graphics, bounds, cell.Notes, noteFont, theme.NoteText);
    }

    private static void DrawNotes(
        Graphics graphics,
        RectangleF cellBounds,
        CandidateSet notes,
        Font font,
        Color color)
    {
        var slotWidth = cellBounds.Width / SudokuConstants.BoxSize;
        var slotHeight = cellBounds.Height / SudokuConstants.BoxSize;

        foreach (var value in notes)
        {
            var index = value - 1;
            var row = index / SudokuConstants.BoxSize;
            var column = index % SudokuConstants.BoxSize;

            var noteBounds = new RectangleF(
                cellBounds.Left + column * slotWidth,
                cellBounds.Top + row * slotHeight,
                slotWidth,
                slotHeight);

            DrawCenteredText(
                graphics,
                value.ToString(),
                font,
                color,
                noteBounds);
        }
    }

    private static void DrawCenteredText(
        Graphics graphics,
        string text,
        Font font,
        Color color,
        RectangleF bounds)
    {
        var rectangle = Rectangle.FromLTRB(
            (int)Math.Floor(bounds.Left),
            (int)Math.Floor(bounds.Top),
            (int)Math.Ceiling(bounds.Right),
            (int)Math.Ceiling(bounds.Bottom));

        TextRenderer.DrawText(
            graphics,
            text,
            font,
            rectangle,
            color,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.NoPadding |
            TextFormatFlags.SingleLine);
    }

    private static void DrawGrid(
        Graphics graphics,
        BoardGeometry geometry,
        Theme theme)
    {
        var bounds = geometry.Bounds;
        var cellSize = geometry.CellSize;

        using var regularPen = new Pen(
            theme.BoardLine,
            Math.Clamp(cellSize * 0.012f, 1f, 2f));

        using var boxPen = new Pen(
            theme.BoxLine,
            Math.Clamp(cellSize * 0.035f, 1.5f, 4f));

        for (var index = 0; index <= SudokuConstants.Size; index++)
        {
            var isBoxLine = index % SudokuConstants.BoxSize == 0;
            var pen = isBoxLine ? boxPen : regularPen;

            var x = bounds.Left + index * cellSize;
            var y = bounds.Top + index * cellSize;

            graphics.DrawLine(pen, x, bounds.Top, x, bounds.Bottom);
            graphics.DrawLine(pen, bounds.Left, y, bounds.Right, y);
        }
    }
}

