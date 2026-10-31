using Sudoku.Application.UseCases;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;

/// <summary>
/// Coordina la ventana principal: nueva partida (generación en segundo plano, cancelable) y pausa.
/// Solo se usa desde el hilo de UI.
/// </summary>
public sealed class MainPresenter : IDisposable
{
    private readonly IMainView _view;
    private readonly StartNewGame _startNewGame;
    private readonly CurrentGame _current;
    private readonly IDialogService _dialogs;
    private CancellationTokenSource? _generation;
    private bool _disposed;

    public MainPresenter(IMainView view, StartNewGame startNewGame, CurrentGame current, IDialogService dialogs)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _startNewGame = startNewGame ?? throw new ArgumentNullException(nameof(startNewGame));
        _current = current ?? throw new ArgumentNullException(nameof(current));
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));

        _view.NewGameRequested += OnNewGameRequested;
        _view.CancelRequested += OnCancelRequested;
        _view.PauseRequested += OnPauseRequested;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _view.NewGameRequested -= OnNewGameRequested;
        _view.CancelRequested -= OnCancelRequested;
        _view.PauseRequested -= OnPauseRequested;
        _generation?.Cancel();
    }

    /// <summary>Genera una partida nueva. Si ya había una generación en curso, la cancela. No lanza excepciones.</summary>
    public async Task StartNewGameAsync()
    {
        _generation?.Cancel();
        var generation = new CancellationTokenSource();
        _generation = generation;

        var difficulty = _view.SelectedDifficulty;
        _view.SetBusy(true);
        _view.SetStatus("Generando partida…");

        try
        {
            var token = generation.Token;
            var session = await Task.Run(() => _startNewGame.Execute(difficulty, token), token);
            if (token.IsCancellationRequested) return;

            session.Start();
            _current.Set(session);
            _view.SetBoard(session.Board);
            _view.SetDifficulty(session.Difficulty);   // <- faltaba
            _view.SetPaused(false);
            _view.SetStatus("Partida en curso");
        }
        catch (OperationCanceledException)
        {
            if (IsCurrent(generation)) _view.SetStatus("Generación cancelada");
        }
        catch (Exception ex)
        {
            if (IsCurrent(generation))
            {
                _dialogs.ShowError("No se pudo generar la partida", ex.Message);
                _view.SetStatus("Error al generar la partida");
            }
        }
        finally
        {
            // Una generación reemplazada no debe apagar el "ocupado" de la nueva.
            if (IsCurrent(generation))
            {
                _generation = null;
                _view.SetBusy(false);
            }
            generation.Dispose();
        }
    }

    private bool IsCurrent(CancellationTokenSource generation) => ReferenceEquals(_generation, generation);

    private async void OnNewGameRequested(object? sender, EventArgs e) => await StartNewGameAsync();

    private void OnCancelRequested(object? sender, EventArgs e) => _generation?.Cancel();

    private void OnPauseRequested(object? sender, EventArgs e)
    {
        if (_generation is not null) return;                 // ocupado generando
        if (_current.Session is not { } session) return;

        if (session.IsRunning)
        {
            session.Pause();
            _view.SetPaused(true);
            _view.SetStatus("Partida en pausa");
        }
        else
        {
            session.Start();
            _view.SetPaused(false);
            _view.SetStatus("Partida en curso");
        }
    }
}

// Version anterior, antes de extraer IGameControlsView. Se mantiene para referencia histórica y pruebas.
//public sealed class MainPresenter : IDisposable
//{
//    private readonly IMainView _view;
//    private readonly StartNewGame _startNewGame;
//    private readonly CurrentGame _current;
//    private readonly IDialogService _dialogs;
//    private CancellationTokenSource? _generation;

//    public MainPresenter(IMainView view, StartNewGame startNewGame, CurrentGame current, IDialogService dialogs)
//    {
//        _view = view;
//        _startNewGame = startNewGame;
//        _current = current;
//        _dialogs = dialogs;

//        _view.NewGameRequested += OnNewGameRequested;
//    }

//    public void Dispose()
//    {
//        _view.NewGameRequested -= OnNewGameRequested;

//        var generation = _generation;
//        _generation = null;   // la continuación pendiente ya no toca la vista
//        generation?.Cancel();
//    }

//    // Único async void: es un handler de evento. Todo el trabajo y los errores están en StartNewGameAsync.
//    private async void OnNewGameRequested(object? sender, EventArgs e) => await StartNewGameAsync();

//    /// <summary>Genera una partida nueva. Si ya había una generación en curso, la cancela. No lanza excepciones.</summary>
//    public async Task StartNewGameAsync()
//    {
//        _generation?.Cancel();
//        var cts = new CancellationTokenSource();
//        _generation = cts;

//        var difficulty = _view.SelectedDifficulty;
//        _view.SetBusy(true);
//        _view.SetStatus("Generando partida…");

//        try
//        {
//            var session = await Task.Run(() => _startNewGame.Execute(difficulty, cts.Token), cts.Token);

//            if (cts.IsCancellationRequested)
//                return; // reemplazada por una petición más nueva (o presenter liberado)

//            session.Start(); // las sesiones nacen en pausa; sin esto el tablero no acepta entrada
//            _view.SetPaused(false);
//            _current.Set(session);
//            _view.SetBoard(session.Board);
//            _view.SetDifficulty(session.Difficulty);
//            _view.SetStatus("Partida nueva");
//        }
//        catch (OperationCanceledException)
//        {
//            if (ReferenceEquals(_generation, cts))
//                _view.SetStatus("Generación cancelada");
//        }
//        catch (Exception ex)
//        {
//            if (ReferenceEquals(_generation, cts))
//            {
//                _view.SetStatus("No se pudo generar la partida");
//                _dialogs.ShowError("Nueva partida", $"No se pudo generar la partida: {ex.Message}");
//            }
//        }
//        finally
//        {
//            if (ReferenceEquals(_generation, cts))
//            {
//                _generation = null;
//                _view.SetBusy(false);
//            }
//            cts.Dispose();
//        }
//    }
//}
