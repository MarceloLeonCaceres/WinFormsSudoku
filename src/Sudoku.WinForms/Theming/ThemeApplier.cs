namespace Sudoku.WinForms.Theming;

/// <summary>
/// Pinta los controles estándar (formulario, paneles, etiquetas, botones) con el tema, de forma recursiva.
/// Los controles propios (p. ej. BoardControl) leen el tema directamente y no pasan por aquí.
/// </summary>
public static class ThemeApplier
{
    public static void Apply(Theme theme, Control root)
    {
        ArgumentNullException.ThrowIfNull(theme);
        ArgumentNullException.ThrowIfNull(root);

        ApplyTo(theme, root);
        foreach (Control child in root.Controls)
            Apply(theme, child);
    }

    private static void ApplyTo(Theme theme, Control control)
    {
        switch (control)
        {
            case Form:
                control.BackColor = theme.Background;
                control.ForeColor = theme.Text;
                break;
            case Button button:
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = theme.ControlBorder;
                button.BackColor = theme.Control;
                button.ForeColor = theme.ControlText;
                break;
            case CheckBox checkBox:
                checkBox.FlatStyle = FlatStyle.Flat;
                checkBox.FlatAppearance.BorderColor = theme.ControlBorder;
                checkBox.BackColor = theme.Control;
                checkBox.ForeColor = theme.ControlText;
                break;
            case ComboBox comboBox:
                comboBox.FlatStyle = FlatStyle.Flat;
                comboBox.BackColor = theme.Control;
                comboBox.ForeColor = theme.ControlText;
                break;
            case Label:
                control.BackColor = Color.Transparent;
                control.ForeColor = theme.Text;
                break;
            case Panel:
                control.BackColor = theme.Surface;
                control.ForeColor = theme.Text;
                break;
        }
    }
}
