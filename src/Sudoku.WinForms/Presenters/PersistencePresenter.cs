using Sudoku.Application.Games;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;

/// <summary>Botones «Guardar» y «Continuar». Un guardado dañado se informa y se deja intacto (decisión del Hito 5).</summary>
public sealed class PersistencePresenter : IDisposable
{
    private readonly IMainView _view;
    private readonly CurrentGame _current;
    private readonly SaveGame _saveGame;
    private readonly LoadGame _loadGame;
    private readonly IDialogService _dialogs;

    public PersistencePresenter(IMainView view, CurrentGame current, SaveGame saveGame, LoadGame loadGame, IDialogService dialogs)
    {
        _view = view;
        _current = current;
        _saveGame = saveGame;
        _loadGame = loadGame;
        _dialogs = dialogs;

        _view.SaveRequested += OnSaveRequested;
        _view.ContinueRequested += OnContinueRequested;
    }

    public void Dispose()
    {
        _view.SaveRequested -= OnSaveRequested;
        _view.ContinueRequested -= OnContinueRequested;
    }

    private void OnSaveRequested(object? sender, EventArgs e)
    {
        if (_current.Session is not { } session)
        {
            _view.SetStatus("No hay ninguna partida que guardar.");
            return;
        }

        if (_current.IsFinished)
        {
            _view.SetStatus("La partida ya terminó.");
            return;
        }

        try
        {
            _saveGame.Execute(session);
            _current.MarkSaved();
            _view.SetStatus("Partida guardada");
        }
        catch (Exception ex) when (PersistenceErrors.IsRecoverable(ex))
        {
            _view.SetStatus("No se pudo guardar la partida");
            _dialogs.ShowError("Guardar", $"No se pudo guardar la partida: {ex.Message}");
        }
    }

    private void OnContinueRequested(object? sender, EventArgs e)
    {
        GameSession? loaded;
        try
        {
            loaded = _loadGame.Execute();
        }
        catch (Exception ex) when (PersistenceErrors.IsRecoverable(ex))
        {
            _view.SetStatus("No se pudo cargar la partida");
            _dialogs.ShowError("Continuar", $"No se pudo cargar la partida guardada (el archivo no se modificó): {ex.Message}");
            return;
        }

        if (loaded is null)
        {
            _dialogs.ShowInfo("Continuar", "No hay ninguna partida guardada.");
            return;
        }

        if (_current.Session is not null && !_current.IsFinished &&
            !_dialogs.Confirm("Continuar", "Hay una partida en curso. Si continúas la guardada, la actual se contará como perdida. ¿Continuar?"))
            return;

        loaded.Start();
        _current.Set(loaded, isResumed: true);
        _view.SetPaused(false);
        _view.SetBoard(loaded.Board);
        _view.SetDifficulty(loaded.Difficulty);
        _view.SetStatus("Partida reanudada");
    }
}
