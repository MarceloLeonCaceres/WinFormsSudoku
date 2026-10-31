using Sudoku.Application.UseCases;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;

/// <summary>Abre el diálogo de estadísticas. Si el archivo está dañado lo informa y no lo modifica.</summary>
public sealed class StatisticsPresenter : IDisposable
{
    private readonly IGameControlsView _view;
    private readonly GetStatistics _getStatistics;
    private readonly IStatisticsDialog _dialog;
    private readonly IDialogService _dialogs;

    public StatisticsPresenter(IGameControlsView view, GetStatistics getStatistics, IStatisticsDialog dialog, IDialogService dialogs)
    {
        _view = view;
        _getStatistics = getStatistics;
        _dialog = dialog;
        _dialogs = dialogs;

        _view.StatisticsRequested += OnStatisticsRequested;
    }

    public void Dispose() => _view.StatisticsRequested -= OnStatisticsRequested;

    private void OnStatisticsRequested(object? sender, EventArgs e)
    {
        try
        {
            _dialog.Show(_getStatistics.Execute());
        }
        catch (Exception ex) when (PersistenceErrors.IsRecoverable(ex))
        {
            _dialogs.ShowError("Estadísticas", $"No se pudieron cargar las estadísticas (el archivo no se modificó): {ex.Message}");
        }
    }
}
