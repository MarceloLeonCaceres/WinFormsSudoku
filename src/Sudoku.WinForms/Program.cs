using Microsoft.Extensions.DependencyInjection;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var provider = CompositionRoot.Build();
        // Program.cs: resolver los presenters los instancia y mantiene vivas sus suscripciones;
        // el contenedor los libera al hacer Dispose.
        var form = provider.GetRequiredService<MainForm>();
        _ = provider.GetRequiredService<BoardPresenter>();
        _ = provider.GetRequiredService<MainPresenter>();
        System.Windows.Forms.Application.Run(form);
    }
}