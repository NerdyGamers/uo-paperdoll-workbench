using Newtonsoft.Json;

namespace UOGumpClassifier.Models;

public class Pass1Result
{
    [JsonProperty("file_id")]              public string       FileId             { get; set; } = "";
    [JsonProperty("asset_class")]          public string       AssetClass         { get; set; } = "GumpUI";
    [JsonProperty("gump_type")]            public string       GumpType           { get; set; } = "Unknown";
    [JsonProperty("shape")]                public string       Shape              { get; set; } = "Unknown";
    [JsonProperty("style")]                public List<string> Style              { get; set; } = new();
    [JsonProperty("function_guess")]       public string       FunctionGuess      { get; set; } = "Unknown";
    [JsonProperty("has_text")]             public bool         HasText            { get; set; }
    [JsonProperty("visible_text")]         public string       VisibleText        { get; set; } = "";
    [JsonProperty("is_paperdoll_related")] public bool         IsPaperdollRelated { get; set; }
    [JsonProperty("paperdoll_reason")]     public string       PaperdollReason    { get; set; } = "";
    [JsonProperty("confidence")]           public double       Confidence         { get; set; }
    [JsonProperty("short_description")]    public string       ShortDescription   { get; set; } = "";
    [JsonProperty("tags")]                 public List<string> Tags               { get; set; } = new();

    // Non-AI pipeline fields
    public string FileName { get; set; } = "";
    public string Sha256   { get; set; } = "";
    public int    Width    { get; set; }
    public int    Height   { get; set; }
    public bool   HasAlpha { get; set; }
}
