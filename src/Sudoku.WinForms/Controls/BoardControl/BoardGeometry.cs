using Domain;

namespace Sudoku.WinForms.Controls.BoardControl;

/// <summary>
/// Geometría del tablero. No depende de controles WinForms ni de GDI+.
/// </summary>
public readonly struct BoardGeometry
{
    public RectangleF Bounds { get; }
    public float CellSize { get; }

    private BoardGeometry(RectangleF bounds)
    {
        Bounds = bounds;
        CellSize = bounds.Width / SudokuConstants.Size;
    }

    /// <summary>
    /// Calcula un tablero cuadrado centrado dentro del espacio disponible.
    /// </summary>
    public static BoardGeometry Create(SizeF availableSize)
    {
        if (availableSize.Width <= 0 || availableSize.Height <= 0)
            return default;

        var size = Math.Min(availableSize.Width, availableSize.Height);

        var x = (availableSize.Width - size) / 2f;
        var y = (availableSize.Height - size) / 2f;

        return new BoardGeometry(new RectangleF(x, y, size, size));
    }

    /// <summary>
    /// Devuelve el rectángulo correspondiente a una celda.
    /// </summary>
    public RectangleF GetCellBounds(Position position)
    {
        if (CellSize <= 0)
            throw new InvalidOperationException("La geometría del tablero no está inicializada.");

        var x = Bounds.Left + position.Column * CellSize;
        var y = Bounds.Top + position.Row * CellSize;

        return new RectangleF(x, y, CellSize, CellSize);
    }

    /// <summary>
    /// Devuelve la posición de la celda que contiene el punto,
    /// o null si el punto está fuera del tablero.
    /// </summary>
    public Position? HitTest(PointF point)
    {
        if (CellSize <= 0)
            return null;

        if (point.X < Bounds.Left || point.X >= Bounds.Right ||
            point.Y < Bounds.Top || point.Y >= Bounds.Bottom)
        {
            return null;
        }

        var column = (int)((point.X - Bounds.Left) / CellSize);
        var row = (int)((point.Y - Bounds.Top) / CellSize);

        return new Position(row, column);
    }
}

