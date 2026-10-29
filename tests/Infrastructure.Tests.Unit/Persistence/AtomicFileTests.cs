using Infrastructure.Persistence;
using Xunit;

namespace Infrastructure.Tests.Persistence;

public sealed class AtomicFileTests : IDisposable
{
    private readonly TempDirectory _temp = new();

    public void Dispose() => _temp.Dispose();

    [Fact]
    public void WriteAllText_CreatesMissingDirectories()
    {
        var path = Path.Combine(_temp.Path, "a", "b", "file.json");

        AtomicFile.WriteAllText(path, "hola");

        Assert.Equal("hola", File.ReadAllText(path));
    }

    [Fact]
    public void WriteAllText_ReplacesExistingContent()
    {
        var path = Path.Combine(_temp.Path, "file.json");
        AtomicFile.WriteAllText(path, "primero");

        AtomicFile.WriteAllText(path, "segundo");

        Assert.Equal("segundo", File.ReadAllText(path));
    }

    [Fact]
    public void WriteAllText_LeavesNoTemporaryFiles()
    {
        var path = Path.Combine(_temp.Path, "file.json");
        AtomicFile.WriteAllText(path, "uno");
        AtomicFile.WriteAllText(path, "dos");

        Assert.Equal([path], Directory.GetFiles(_temp.Path));
    }

    [Fact]
    public void WriteAllText_WritesUtf8WithoutBom()
    {
        var path = Path.Combine(_temp.Path, "file.json");

        AtomicFile.WriteAllText(path, "ñ");

        Assert.Equal([0xC3, 0xB1], File.ReadAllBytes(path));
    }
}
