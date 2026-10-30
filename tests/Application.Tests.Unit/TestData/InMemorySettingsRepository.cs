using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Application.Tests.Unit.TestData;

internal sealed class InMemorySettingsRepository : ISettingsRepository
{
    public AppSettings Current { get; private set; } = AppSettings.Default;
    public int SaveCount { get; private set; }

    public AppSettings Load() => Current;

    public void Save(AppSettings settings)
    {
        Current = settings;
        SaveCount++;
    }
}
