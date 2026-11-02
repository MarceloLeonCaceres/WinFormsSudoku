using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

public sealed class MemorySettingsRepository : ISettingsRepository
{
    public AppSettings Stored { get; set; } = AppSettings.Default;
    public Exception? LoadFailure { get; set; }
    public Exception? SaveFailure { get; set; }

    public AppSettings Load() => LoadFailure is null ? Stored : throw LoadFailure;

    public void Save(AppSettings settings)
    {
        if (SaveFailure is not null)
            throw SaveFailure;

        Stored = settings;
    }
}
