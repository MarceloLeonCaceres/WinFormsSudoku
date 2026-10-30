using System.Text.Json.Nodes;

namespace Infrastructure.Tests.Unit.TestSupport;

internal static class JsonTamper
{
    public static void SetVersion(string path, int version)
    {
        var node = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var key = node.Select(p => p.Key).Single(k => k.Equals("version", StringComparison.OrdinalIgnoreCase));
        node[key] = version;
        File.WriteAllText(path, node.ToJsonString());
    }

    public static void ReplaceText(string path, string oldText, string newText) =>
        File.WriteAllText(path, File.ReadAllText(path).Replace(oldText, newText));
}
