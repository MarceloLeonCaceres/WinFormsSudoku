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
        var form = provider.GetRequiredService<MainForm>();
        CompositionRoot.ActivatePresenters(provider);
        System.Windows.Forms.Application.Run(form);
    }
}