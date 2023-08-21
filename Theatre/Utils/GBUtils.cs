using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.VisualBasic.FileIO;

namespace Theatre.Utils;

public enum GBGame : ulong
{
    WiiU = 5872,
    Switch = 8482
}


static class GBLinks
{
    public static string GetSubmissionsLink(GBGame game, ulong page = 1)
    {
        page = Math.Clamp(page, 1, ulong.MaxValue);
        var gameid = (ulong)game;
        return $"https://api.gamebanana.com/Core/List/New?gameid={gameid}&page={page}&itemtype=Mod";
    }
    public static string GetSubmissionDataLink(ulong id)
    {
        return $"https://api.gamebanana.com/Core/Item/Data?itemtype=Mod&itemid={id}&fields=name,Owner().name,Files().aFiles()";
    }
}

public sealed class GbModInfo
{
    public string Name = string.Empty;
    public string Owner = string.Empty;
    public List<AFile> Files = new();
    public static GbModInfo Parse(JsonArray array)
    {
        GbModInfo result = new()
        {
            Name = array[0]?.Deserialize<string>() ?? string.Empty,
            Owner = array[1]?.Deserialize<string>() ?? string.Empty
        };
        foreach (var item in array.Skip(2))
        {
            var inner = item?.AsObject();
            if (inner is not null)
                foreach (var (_, value) in inner)
                    if (value is not null)
                    {
                        var o = value.AsObject();
                        AFile file = new();
                        file.Parse(o);
                        result.Files.Add(file);
                    }
        }
        return result;
    }
    public static implicit operator GbModInfo(JsonArray array) => Parse(array);
    public void Deconstruct(out string name, out string owner, out AFile[] files)
    {
        name = Name;
        owner = Owner;
        files = Files.ToArray();
    }
    public GbModInfo() { }
    public GbModInfo(string name, string owner, IEnumerable<AFile> files)
    {
        Name = name;
        Owner = owner;
        Files = new(files);
    }
}

public sealed class AFile
{
    public ulong idRow;
    public string sFile = string.Empty;
    public ulong nFilesize;
    public string sDescription = string.Empty;
    // Could be a DateTime??? Unsure. (Lord-G)
    public ulong tsDateAdded;
    public ulong nDownloadCount;
    public string sAnalysisState = string.Empty;
    public string sDownloadUrl = string.Empty;
    public string sMd5Checksum = string.Empty;
    public string sClamAvResult = string.Empty;
    public string sAnalysisResult = string.Empty;
    public bool bContainsExe;
    // Deserialize refused to work on AFile itself, wrote this hack instead.
    public void Parse(JsonObject obj)
    {
        foreach (var field in GetType().GetFields())
        {
            object? o = obj['_' + field.Name].Deserialize(field.FieldType);
            field.SetValue(this, o);
        }
    }
}

[Obsolete("For removal, moving to a separate private project for this.")]
public static class GBUtils
{
    [Obsolete("For removal, moving to a separate private project for this.")]
    public static ulong[] GetSubmissions(GBGame game, ulong page = 1)
    {
        List<ulong> items = new();
        using var client = new HttpClient();
        var data = client.GetStringAsync(GBLinks.GetSubmissionsLink(game, page)).GetAwaiter().GetResult();
        JsonDocument doc = JsonDocument.Parse(data);
        var arr = doc.Deserialize<JsonArray>() ?? new JsonArray();
        foreach (var item in arr)
        {
            var inner = item?.AsArray();
            var id = inner?[1];
            if (id is not null)
                items.Add(id.Deserialize<ulong>());
        }
        return items.ToArray();
    }

    [Obsolete("For removal, moving to a separate private project for this.")]
    public static GbModInfo GetSubmissionData(ulong id)
    {
        using var client = new HttpClient();
        var data = client.GetStringAsync(GBLinks.GetSubmissionDataLink(id)).GetAwaiter().GetResult();
        var doc = JsonDocument.Parse(data);
        var arr = doc.Deserialize<JsonArray>() ?? new JsonArray();
        return arr;
    }

    [Obsolete("For removal, moving to a separate private project for this.")]
    public static Dictionary<ulong, GbModInfo> GetAllSubmissions(GBGame game, ulong page = 1)
    {
        return GetSubmissions(game, page).Select(x => (x, GetSubmissionData(x)))
            .ToDictionary(x => x.x, x => x.Item2);
    }

    [Obsolete("For removal, moving to a separate private project for this.")]
    public static Dictionary<ulong, GbModInfo> GetEverySingleSubmission(GBGame game)
    {
        Dictionary<ulong, GbModInfo> result = new();
        ulong page = 1;
        var subs = GetAllSubmissions(game, page++);
        while (subs.Count != 0)
        {
            foreach (var (key, value) in subs)
                result.TryAdd(key, value);
            subs = GetAllSubmissions(game, page++);
        }
        return result;
    }
}

public static class CachedGBMods
{
    public static Dictionary<ulong, GbModInfo> AllWiiUModsCached = new();
    public static Dictionary<ulong, GbModInfo> AllSwitchModsCached = new();
}