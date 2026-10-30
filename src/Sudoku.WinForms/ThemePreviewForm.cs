using Sudoku.Application.Games;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms;

internal sealed class ThemePreviewForm : Form
{
    private readonly IThemeService _themes;
    private readonly Button _toggle = new() { Text = "Cambiar tema", Dock = DockStyle.Top, Height = 40 };
    private readonly FlowLayoutPanel _swatches = new() { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };

    public ThemePreviewForm(IThemeService themes)
    {
        _themes = themes;
        Text = "Sudoku — vista previa de tema";
        ClientSize = new Size(560, 440);

        Controls.Add(_swatches);
        Controls.Add(_toggle);

        _toggle.Click += (_, _) => Toggle();
        _themes.ThemeChanged += OnThemeChanged;
        Rebuild();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _themes.ThemeChanged -= OnThemeChanged;
        base.OnFormClosed(e);
    }

    private void OnThemeChanged(object? sender, EventArgs e) => Rebuild();

    private void Toggle()
    {
        var next = _themes.Current.Kind == ThemeKind.Light ? ThemeKind.Dark : ThemeKind.Light;
        try
        {
            _themes.SetTheme(next);
        }
        catch (CorruptSaveException)
        {
            MessageBox.Show("Los ajustes guardados están dañados; no se pudo guardar el tema.",
                "Sudoku", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void Rebuild()
    {
        var theme = _themes.Current;

        _swatches.SuspendLayout();
        _swatches.Controls.Clear();
        foreach (var property in typeof(Theme).GetProperties().Where(p => p.PropertyType == typeof(Color)))
        {
            _swatches.Controls.Add(new Label
            {
                Text = property.Name,
                Tag = (Color)property.GetValue(theme)!,
                Size = new Size(160, 44),
                TextAlign = ContentAlignment.MiddleCenter,
            });
        }

        ThemeApplier.Apply(theme, this);

        foreach (Label swatch in _swatches.Controls)
        {
            var color = (Color)swatch.Tag!;
            swatch.BackColor = color;
            swatch.ForeColor = color.GetBrightness() > 0.5f ? Color.Black : Color.White;
        }
        _swatches.ResumeLayout();
    }
}
