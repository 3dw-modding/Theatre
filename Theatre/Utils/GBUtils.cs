using System.Text.Json;
using System.Text.Json.Nodes;

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
        return $"https://api.gamebanana.com/Core/Item/Data?itemtype=Mod&itemid={id}&fields=name,Files().aFiles()";
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

public static class GBUtils
{
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
    public static (string, AFile[]) GetSubmissionData(ulong id)
    {
        List<AFile> files = new();
        using var client = new HttpClient();
        var data = client.GetStringAsync(GBLinks.GetSubmissionDataLink(id)).GetAwaiter().GetResult();
        var doc = JsonDocument.Parse(data);
        var arr = doc.Deserialize<JsonArray>() ?? new JsonArray();
        var name = arr[0]?.Deserialize<string>() ?? string.Empty;
        foreach (var item in arr.Skip(1))
        {
            var inner = item?.AsObject();
            if (inner is not null)
                foreach (var (_, val) in inner)
                    if (val is not null)
                    {
                        var o = val.AsObject();
                        AFile file = new();
                        file.Parse(o);
                        files.Add(file);
                    }
        }
        return (name, files.ToArray());
    }
    public static Dictionary<ulong, (string, AFile[])> GetAllSubmissons(GBGame game, ulong page = 1)
    {
        return GetSubmissions(game, page).Select(x => (x, GetSubmissionData(x)))
            .ToDictionary(x => x.x, x => x.Item2);
    }
    public static Dictionary<ulong, (string, AFile[])> GetEverySingleSubmisson(GBGame game)
    {
        Dictionary<ulong, (string, AFile[])> result = new();
        ulong page = 1;
        var subs = GetAllSubmissons(game, page++);
        while (subs.Count != 0)
        {
            foreach (var (key, value) in subs)
                result.TryAdd(key, value);
            subs = GetAllSubmissons(game, page++);
        }
        return result;
    }
}