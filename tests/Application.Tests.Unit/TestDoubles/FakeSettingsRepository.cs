using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Sudoku.Application.Tests.TestDoubles;

public sealed class FakeSettingsRepository : ISettingsRepository
{
    public AppSettings Stored { get; set; } = AppSettings.Default;
    public int SaveCalls { get; private set; }

    public AppSettings Load() => Stored;

    public void Save(AppSettings settings)
    {
        Stored = settings;
        SaveCalls++;
    }
}
