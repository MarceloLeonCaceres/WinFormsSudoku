using Domain;
using Sudoku.WinForms.Theming;
using System.ComponentModel;

namespace Sudoku.WinForms.Controls.BoardControl;

public sealed class BoardControl : Control
{
    private readonly IThemeService _themeService;

    private Board _board = Board.Empty;
    private Position? _selectedPosition;
    private bool _notesMode;

    public BoardControl(IThemeService themeService)
    {
        _themeService = themeService
            ?? throw new ArgumentNullException(nameof(themeService));

        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable,
            true);

        TabStop = true;

        _themeService.ThemeChanged += OnThemeChanged;
    }

    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    public event EventHandler<ValueEnteredEventArgs>? ValueEntered;

    public event EventHandler? ClearRequested;

    /// <summary>
    /// Celda seleccionada. Puede ser null cuando no hay selección.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Position? SelectedPosition
    {
        get => _selectedPosition;
        set
        {
            if (_selectedPosition == value)
                return;

            _selectedPosition = value;
            Invalidate();

            // Solo notificamos cuando se selecciona una celda.
            if (value is { } position)
            {
                SelectionChanged?.Invoke(
                    this,
                    new SelectionChangedEventArgs(position));
            }
        }
    }

    /// <summary>
    /// Indica si el jugador está utilizando el modo notas.
    /// La decisión sobre qué operación ejecutar corresponde al Presenter.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool NotesMode
    {
        get => _notesMode;
        set
        {
            if (_notesMode == value)
                return;

            _notesMode = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Actualiza el tablero y solicita un nuevo dibujo.
    /// </summary>
    public void SetBoard(Board board)
    {
        ArgumentNullException.ThrowIfNull(board);

        _board = board;
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // BoardRenderer pinta toda el área del control.
        // Evitamos el pintado de fondo adicional de WinForms.
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var geometry = BoardGeometry.Create(ClientSize);

        BoardRenderer.Draw(
            e.Graphics,
            ClientRectangle,
            geometry,
            _board,
            _selectedPosition,
            _themeService.Current);

        base.OnPaint(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left)
            return;

        Focus();

        var geometry = BoardGeometry.Create(ClientSize);
        var position = geometry.HitTest(e.Location);

        // Un clic fuera del tablero conserva la selección actual.
        if (position is not null)
            SelectedPosition = position;
    }
    protected override bool IsInputKey(Keys keyData) =>
    (keyData & Keys.KeyCode) switch
    {
        Keys.Left or Keys.Right or Keys.Up or Keys.Down => true,
        _ => base.IsInputKey(keyData)
    };
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (_selectedPosition is not { } selected || e.Modifiers != Keys.None)
            return;

        var handled = true;
        switch (e.KeyCode)
        {
            case Keys.Left: MoveSelection(selected.Row, selected.Column - 1); break;
            case Keys.Right: MoveSelection(selected.Row, selected.Column + 1); break;
            case Keys.Up: MoveSelection(selected.Row - 1, selected.Column); break;
            case Keys.Down: MoveSelection(selected.Row + 1, selected.Column); break;
            case Keys.Delete:
            case Keys.Back: ClearRequested?.Invoke(this, EventArgs.Empty); break;
            default:
                if (GetEnteredValue(e.KeyCode) is { } value)
                    ValueEntered?.Invoke(this, new ValueEnteredEventArgs(value));
                else
                    handled = false;
                break;
        }

        if (handled)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
    }

    private void MoveSelection(int row, int column)
    {
        // No hacemos wrap: la selección se detiene en los bordes.
        if (row < 0 || row >= SudokuConstants.Size ||
            column < 0 || column >= SudokuConstants.Size)
        {
            return;
        }

        SelectedPosition = new Position(row, column);
    }

    private static int? GetEnteredValue(Keys key)
    {
        if (key >= Keys.D1 && key <= Keys.D9)
            return (int)key - (int)Keys.D0;

        if (key >= Keys.NumPad1 && key <= Keys.NumPad9)
            return (int)key - (int)Keys.NumPad0;

        return null;
    }

    private static void HandleKey(KeyEventArgs e)
    {
        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Refresh();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _themeService.ThemeChanged -= OnThemeChanged;

        base.Dispose(disposing);
    }
}

