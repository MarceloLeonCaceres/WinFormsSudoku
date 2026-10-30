using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Sudoku.WinForms.Tests.TestDoubles;

internal sealed class InMemorySettingsRepository(AppSettings? initial = null) : ISettingsRepository
{
    public AppSettings Settings { get; private set; } = initial ?? AppSettings.Default;
    public int SaveCount { get; private set; }
    public CorruptSaveException? LoadException { get; set; }

    public AppSettings Load() => LoadException is null ? Settings : throw LoadException;

    public void Save(AppSettings settings)
    {
        Settings = settings;
        SaveCount++;
    }
}
