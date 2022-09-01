using System.Text.RegularExpressions;
namespace LazyStack.Utils;

public interface IMessages
{
    public void ReplaceVars();
    public Dictionary<string, string> Msgs { get; set; }
    public string Msg(string key);
}

public class Messages : IMessages
{
    const string keyPattern = "__.*__";

    public void ReplaceVars()
    {
        for (var i = 0; i < Msgs.Count; i++)
        {
            // TODO: optimize
            var msg = Msgs.ElementAt(i).Value;
            var key = Msgs.ElementAt(i).Key;
            MatchCollection matches;
            while((matches = Regex.Matches(msg, keyPattern)).Count > 0)
                foreach(Match match in matches)
                    if(Msgs.TryGetValue(match.Value[2..^2], out string? replacement))
                        msg = msg.Replace(match.Value, replacement);
                    else
                        throw new Exception($"Msgs[{match.Value[2..^2]}] not found.");
            Msgs[key] = msg;
        }
    }
    public Dictionary<string, string> Msgs { get; set; } = new();

    public string Msg(string key)
    {
        if (key == null)
            return "";

        if (key == "Nothing")
            return "";

        if (Msgs.TryGetValue(key, out string? value)) { 
            return string.IsNullOrEmpty(value) ? key : value;
        }
        else
            return $"{key} not found";
    }
}
