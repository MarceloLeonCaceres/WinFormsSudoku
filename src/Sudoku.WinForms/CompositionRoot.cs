using Infrastructure.Generation;
using Infrastructure.Persistence;
using Infrastructure.Solving;
using Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Settings;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Theming;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms;

/// <summary>Único lugar que conoce a Infrastructure y configura el contenedor de DI.</summary>
internal static class CompositionRoot
{
    public static ServiceProvider Build()
    {
        var services = new ServiceCollection();
        AddInfrastructure(services);
        AddUseCases(services);
        AddTheming(services);
        AddForms(services);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
    }

    private static void AddInfrastructure(IServiceCollection services)
    {
        services.AddSingleton<IAppDataPaths>(_ => AppDataPaths.ForCurrentUser());
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ISolver, BacktrackingSolver>();
        services.AddSingleton<IPuzzleGenerator, PuzzleGenerator>();
        services.AddSingleton<IGameRepository, JsonGameRepository>();
        services.AddSingleton<IStatisticsRepository, JsonStatisticsRepository>();
        services.AddSingleton<ISettingsRepository, JsonSettingsRepository>();
    }

    private static void AddUseCases(IServiceCollection services)
    {
        services.AddSingleton<StartNewGame>();
        services.AddSingleton<PlaceValue>();
        services.AddSingleton<ClearCell>();
        services.AddSingleton<ToggleNote>();
        services.AddSingleton<Undo>();
        services.AddSingleton<Redo>();
        services.AddSingleton<GetHint>();
        services.AddSingleton<ApplyHint>();
        services.AddSingleton<SaveGame>();
        services.AddSingleton<LoadGame>();
        services.AddSingleton<GetStatistics>();
        services.AddSingleton<RecordGameStarted>();
        services.AddSingleton<RecordGameFinished>();
        services.AddSingleton<GetSettings>();
        services.AddSingleton<ChangeTheme>();

        services.AddSingleton<SolveGame>();
    }

    private static void AddTheming(IServiceCollection services)
    {
        services.AddSingleton<IThemeService>(sp => new ThemeService(
            sp.GetRequiredService<ChangeTheme>(),
            ResolveInitialTheme(sp.GetRequiredService<GetSettings>())));
    }

    private static void AddForms(IServiceCollection services)
    {
        services.AddSingleton<MainForm>();
        services.AddSingleton<CurrentGame>();
        services.AddSingleton<IDialogService, MessageBoxDialogService>();
        services.AddSingleton<IUiTimer, WinFormsUiTimer>();
        services.AddSingleton<ISolutionDialog, SolutionDialogService>();
        services.AddSingleton<IStatisticsDialog, StatisticsDialogService>();
        
        services.AddSingleton<IMainView>(sp => sp.GetRequiredService<MainForm>());
        services.AddSingleton<IBoardView>(sp => sp.GetRequiredService<MainForm>());

        services.AddSingleton<BoardPresenter>();
        services.AddSingleton<MainPresenter>();

        services.AddSingleton<GameplayPresenter>();
        services.AddSingleton<ClockPresenter>();
        services.AddSingleton<GameOutcomePresenter>();
        services.AddSingleton<PersistencePresenter>();
        services.AddSingleton<SolvePresenter>();
        services.AddSingleton<StatisticsPresenter>();
        services.AddSingleton<ThemePresenter>();

        // 2. Rediriges IGameControlsView para que use la misma instancia de IMainView
        services.AddSingleton<IGameControlsView>(proveedor => proveedor.GetRequiredService<IMainView>());

        // 3. (Opcional) Haz lo mismo para el resto de interfaces heredadas si otros presentadores las requieren
        services.AddSingleton<IBoardView>(proveedor => proveedor.GetRequiredService<IMainView>());
        services.AddSingleton<IWindowLifecycleView>(proveedor => proveedor.GetRequiredService<IMainView>());
    }

    private static ThemeKind ResolveInitialTheme(GetSettings getSettings)
    {
        try
        {
            return getSettings.Execute().Theme;
        }
        catch (CorruptSaveException)
        {
            // El archivo no se toca; se usa el tema por defecto.
            MessageBox.Show(
                "No se pudieron leer los ajustes guardados. Se usará el tema por defecto.",
                "Sudoku",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return AppSettings.Default.Theme;
        }
    }
}
