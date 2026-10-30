using Domain;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Views;

/// <summary>Ventana principal. Solo presenta el estado y comunica las acciones del usuario.</summary>
public sealed class MainForm : Form, IMainView
{
    private const int SidebarWidth = 248;

    private readonly IThemeService _themeService;
    private readonly BoardControl _boardControl;
    private bool _isInitializing;
    private readonly Label _timerLabel = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 17F, FontStyle.Bold),
        Text = "00:00"
    };

    private readonly Label _statusLabel = new()
    {
        AutoSize = true,
        Text = "Listo para jugar"
    };

    private readonly ComboBox _difficultyComboBox = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 210
    };

    private readonly ComboBox _themeComboBox = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 210
    };

    private readonly Button _newGameButton = CreateButton("Nuevo juego");
    private readonly Button _undoButton = CreateButton("Deshacer");
    private readonly Button _redoButton = CreateButton("Rehacer");
    private readonly CheckBox _notesButton = new()
    {
        Text = "Modo notas",
        AutoSize = false,
        Height = 40,
        Width = 210,
        TextAlign = ContentAlignment.MiddleCenter,
        Appearance = Appearance.Button,
        FlatStyle = FlatStyle.Flat
    };
    private readonly Button _hintButton = CreateButton("Pista");
    private readonly Button _solveButton = CreateButton("Resolver");
    private readonly Button _pauseButton = CreateButton("Continuar");
    private readonly Button _statisticsButton = CreateButton("Estadísticas");

    private bool _isBusy;
    private bool _isPaused = true;
    private bool _canUndo;
    private bool _canRedo;
    private bool _updatingView;

    public MainForm(IThemeService themeService)
    {
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _boardControl = new BoardControl(_themeService)
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };

        ConfigureForm();
        BuildLayout();
        ConfigureSelectors();
        WireEvents();

        _themeService.ThemeChanged += OnThemeChanged;
        ApplyCurrentTheme();
        UpdateInteractionState();
    }

    public Difficulty SelectedDifficulty =>
        (_difficultyComboBox.SelectedItem as Choice<Difficulty>)?.Value ?? Difficulty.Easy;

    public ThemeKind SelectedTheme =>
        (_themeComboBox.SelectedItem as Choice<ThemeKind>)?.Value ?? ThemeKind.Dark;

    public bool NotesMode => _notesButton.Checked;

    public event EventHandler? NewGameRequested;
    public event EventHandler? DifficultyChanged;
    public event EventHandler? UndoRequested;
    public event EventHandler? RedoRequested;
    public event EventHandler? NotesModeChanged;
    public event EventHandler? HintRequested;
    public event EventHandler? SolveRequested;
    public event EventHandler? PauseRequested;
    public event EventHandler? StatisticsRequested;
    public event EventHandler? ThemeChangeRequested;
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<ValueEnteredEventArgs>? ValueEntered;
    public event EventHandler? ClearRequested;

    public void SetBoard(Board board) => _boardControl.SetBoard(board);

    public void SetDifficulty(Difficulty difficulty)
    {
        _updatingView = true;
        try { SelectChoice(_difficultyComboBox, difficulty); }
        finally { _updatingView = false; }
    }

    public void SetElapsedTime(TimeSpan elapsedTime)
    {
        var totalHours = (int)elapsedTime.TotalHours;
        _timerLabel.Text = totalHours > 0
            ? $"{totalHours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}"
            : $"{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}";
    }

    public void SetStatus(string message) => _statusLabel.Text = message ?? string.Empty;

    public void SetNotesMode(bool enabled)
    {
        _updatingView = true;
        try { _notesButton.Checked = enabled; }
        finally { _updatingView = false; }
        _boardControl.NotesMode = enabled;
    }

    public void SetPaused(bool paused)
    {
        _isPaused = paused;
        _pauseButton.Text = paused ? "Continuar" : "Pausar";
        UpdateInteractionState();
    }

    public void SetBusy(bool busy)
    {
        _isBusy = busy;
        UseWaitCursor = busy;
        UpdateInteractionState();
    }

    public void SetUndoRedoState(bool canUndo, bool canRedo)
    {
        _canUndo = canUndo;
        _canRedo = canRedo;
        UpdateInteractionState();
    }

    public void SetTheme(Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        _updatingView = true;
        try { SelectChoice(_themeComboBox, theme.Kind); }
        finally { _updatingView = false; }
        ThemeApplier.Apply(theme, this);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _themeService.ThemeChanged -= OnThemeChanged;
        base.OnFormClosed(e);
    }

    private void ConfigureForm()
    {
        Text = "Sudoku";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 780);
        MinimumSize = new Size(960, 700);
        FormBorderStyle = FormBorderStyle.Sizable;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        KeyPreview = false;
    }

    private void ConfigureSelectors()
    {
        _isInitializing = true;

        try
        {
            var difficulties = Enum.GetValues<Difficulty>()
                .Select(d => new Choice<Difficulty>(d, d.ToString()))
                .ToArray();

            _difficultyComboBox.DataSource = null;
            _difficultyComboBox.DisplayMember = nameof(Choice<Difficulty>.Text);
            _difficultyComboBox.DataSource = difficulties;

            var count = _difficultyComboBox.Items.Count;

            _difficultyComboBox.SelectedIndex = 0;

        }
        finally
        {
            _isInitializing = false;
        }
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(18, 12, 18, 10),
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildContent(), 0, 1);
        root.Controls.Add(BuildStatusBar(), 0, 2);
        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));

        var title = new Label
        {
            Text = "SUDOKU",
            AutoSize = true,
            Font = new Font("Segoe UI", 21F, FontStyle.Bold),
            Anchor = AnchorStyles.Left
        };
        var difficultyCaption = new Label
        {
            Text = "Dificultad",
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0, 0, 12, 0)
        };
        var difficultyHeader = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0, 0, 18, 0)
        };
        difficultyHeader.Controls.Add(difficultyCaption);
        difficultyHeader.Controls.Add(_difficultyComboBox);

        _timerLabel.Anchor = AnchorStyles.Right;
        _timerLabel.Margin = new Padding(0);

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(difficultyHeader, 1, 0);
        header.Controls.Add(_timerLabel, 2, 0);
        return header;
    }

    private Control BuildContent()
    {
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SidebarWidth));

        var boardPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 8, 20, 8),
            Margin = Padding.Empty
        };
        boardPanel.Controls.Add(_boardControl);

        var sidebar = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 12, 8, 8),
            Margin = Padding.Empty
        };
        sidebar.Controls.Add(BuildSidebar());

        content.Controls.Add(boardPanel, 0, 0);
        content.Controls.Add(sidebar, 1, 0);
        return content;
    }

    private Control BuildSidebar()
    {
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Margin = Padding.Empty
        };

        AddSectionTitle(layout, "PARTIDA");
        AddSidebarControl(layout, _newGameButton);
        AddSectionTitle(layout, "ACCIONES");
        AddSidebarControl(layout, _undoButton);
        AddSidebarControl(layout, _redoButton);
        AddSidebarControl(layout, _notesButton);
        AddSidebarControl(layout, _hintButton);
        AddSidebarControl(layout, _solveButton);
        AddSidebarControl(layout, _pauseButton);
        AddSectionTitle(layout, "OTROS");
        AddSidebarControl(layout, _statisticsButton);
        AddSectionTitle(layout, "APARIENCIA");
        AddSidebarControl(layout, _themeComboBox);
        return layout;
    }

    private Control BuildStatusBar()
    {
        var status = new Panel { Dock = DockStyle.Fill, Padding = new Padding(2, 7, 2, 0) };
        status.Controls.Add(_statusLabel);
        return status;
    }

    private void WireEvents()
    {
        _newGameButton.Click += (_, _) => NewGameRequested?.Invoke(this, EventArgs.Empty);
        _undoButton.Click += (_, _) => UndoRequested?.Invoke(this, EventArgs.Empty);
        _redoButton.Click += (_, _) => RedoRequested?.Invoke(this, EventArgs.Empty);
        _notesButton.CheckedChanged += (_, _) =>
        {
            _boardControl.NotesMode = _notesButton.Checked;
            if (!_updatingView)
                NotesModeChanged?.Invoke(this, EventArgs.Empty);
        };
        _hintButton.Click += (_, _) => HintRequested?.Invoke(this, EventArgs.Empty);
        _solveButton.Click += (_, _) => SolveRequested?.Invoke(this, EventArgs.Empty);
        _pauseButton.Click += (_, _) => PauseRequested?.Invoke(this, EventArgs.Empty);
        _statisticsButton.Click += (_, _) => StatisticsRequested?.Invoke(this, EventArgs.Empty);
        _difficultyComboBox.SelectedIndexChanged += (_, _) =>
        {
            if (!_updatingView)
                DifficultyChanged?.Invoke(this, EventArgs.Empty);
        };
        _themeComboBox.SelectedIndexChanged += (_, _) =>
        {
            if (!_updatingView)
                ThemeChangeRequested?.Invoke(this, EventArgs.Empty);
        };

        _boardControl.SelectionChanged += (_, e) => SelectionChanged?.Invoke(this, e);
        _boardControl.ValueEntered += (_, e) => ValueEntered?.Invoke(this, e);
        _boardControl.ClearRequested += (_, _) => ClearRequested?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateInteractionState()
    {
        var canPlay = !_isBusy && !_isPaused;

        _boardControl.Enabled = canPlay;
        _newGameButton.Enabled = !_isBusy;
        _difficultyComboBox.Enabled = !_isBusy;
        _undoButton.Enabled = canPlay && _canUndo;
        _redoButton.Enabled = canPlay && _canRedo;
        _notesButton.Enabled = canPlay;
        _hintButton.Enabled = canPlay;
        _solveButton.Enabled = canPlay;
        _pauseButton.Enabled = !_isBusy;
        _statisticsButton.Enabled = !_isBusy;
        _themeComboBox.Enabled = !_isBusy;
    }

    private void OnThemeChanged(object? sender, EventArgs e) => ApplyCurrentTheme();

    private void ApplyCurrentTheme() => SetTheme(_themeService.Current);

    private static Button CreateButton(string text) => new()
    {
        Text = text,
        Width = 210,
        Height = 40,
        Margin = new Padding(0, 3, 0, 3),
        UseVisualStyleBackColor = false
    };

    private static void AddSectionTitle(FlowLayoutPanel parent, string text)
    {
        parent.Controls.Add(new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Margin = new Padding(0, 13, 0, 5)
        });
    }

    private static void AddSidebarControl(FlowLayoutPanel parent, Control control)
    {
        control.Margin = new Padding(0, 2, 0, 2);
        parent.Controls.Add(control);
    }

    private static void SelectChoice<T>(ComboBox comboBox, T value)
    {
        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is Choice<T> choice &&
                EqualityComparer<T>.Default.Equals(choice.Value, value))
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private sealed record Choice<T>(T Value, string Text)
    {
        public override string ToString() => Text;
    }
}
