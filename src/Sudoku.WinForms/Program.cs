using Microsoft.Extensions.DependencyInjection;
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
        System.Windows.Forms.Application.Run(provider.GetRequiredService<MainForm>());
    }
}