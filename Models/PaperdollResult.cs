using Newtonsoft.Json;

namespace UOGumpClassifier.Models;

public class PaperdollMetadata
{
    [JsonProperty("offset_x")]          public int    OffsetX         { get; set; }
    [JsonProperty("offset_y")]          public int    OffsetY         { get; set; }
    [JsonProperty("anchor_confidence")] public double AnchorConfidence{ get; set; }
    [JsonProperty("has_alpha")]         public bool   HasAlpha        { get; set; }
}

public class PaperdollResult
{
    [JsonProperty("file_id")]           public string              FileId          { get; set; } = "";
    [JsonProperty("asset_class")]       public string              AssetClass      { get; set; } = "PaperdollWearable";
    [JsonProperty("base_name")]         public string              BaseName        { get; set; } = "Unknown";
    [JsonProperty("slot")]              public string              Slot            { get; set; } = "Unknown";
    [JsonProperty("material")]          public string              Material        { get; set; } = "Unknown";
    [JsonProperty("gender")]            public string              Gender          { get; set; } = "Unknown";
    [JsonProperty("body_type")]         public string              BodyType        { get; set; } = "Unknown";
    [JsonProperty("state")]             public string              State           { get; set; } = "Unknown";
    [JsonProperty("is_hueable")]        public bool                IsHueable       { get; set; }
    [JsonProperty("is_partial_piece")]  public bool                IsPartialPiece  { get; set; }
    [JsonProperty("covers_regions")]    public List<string>        CoversRegions   { get; set; } = new();
    [JsonProperty("theme")]             public string              Theme           { get; set; } = "Unknown";
    [JsonProperty("short_description")] public string              ShortDescription{ get; set; } = "";
    [JsonProperty("confidence")]        public double              Confidence      { get; set; }
    [JsonProperty("tags")]              public List<string>        Tags            { get; set; } = new();
    [JsonProperty("metadata")]          public PaperdollMetadata   Metadata        { get; set; } = new();

    // Non-AI pipeline fields
    public string FileName { get; set; } = "";
    public string Sha256   { get; set; } = "";
    public int    Width    { get; set; }
    public int    Height   { get; set; }
}
