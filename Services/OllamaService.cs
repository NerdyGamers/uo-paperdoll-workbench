using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using UOGumpClassifier.Models;

namespace UOGumpClassifier.Services;

public class OllamaService
{
    private readonly HttpClient _http;
    private readonly string _model;
    private const string BaseUrl = "http://localhost:11434";

    private const string Pass1SystemPrompt =
        "You are a strict data extraction engine for Ultima Online gump UI assets. " +
        "Classify exactly one image using the provided JSON schema. " +
        "Images are PNG assets on a black background. Treat pure black as empty preview space unless black pixels are clearly part of the asset itself. " +
        "A gump is a 2D Ultima Online UI element: frames, buttons, containers, icons, status bars, dialogs, paperdoll overlays. " +
        "Use Unknown for any field you cannot determine reliably. " +
        "Mark is_paperdoll_related=true for anything tied to character equipment, body layout, worn slots, or wearable overlays. " +
        "Return ONLY valid JSON. No markdown. No commentary. No explanation.";

    private const string Pass2SystemPrompt =
        "You are a strict data extraction engine for Ultima Online paperdoll wearable assets. " +
        "Classify exactly one image using the provided JSON schema. " +
        "Images are PNG assets on a black background. Treat pure black as empty preview space. " +
        "A paperdoll wearable is clothing, armor, jewelry, robe, shield, weapon-in-hand, or empty slot overlay. " +
        "UO paperdoll slots: Back, Bracelet, Chest, Earrings, Feet, Gloves, Head, LeftHand, Legs, Neck, RightHand, Ring, Robe, Sash, Shirt, Skirt, Sleeves, Talisman, Waist. " +
        "Use Unknown conservatively. anchor offsets are candidate values only - use 0 and low anchor_confidence if uncertain. " +
        "Return ONLY valid JSON. No markdown. No commentary. No explanation.";

    public OllamaService(string model = "llava")
    {
        _model = model;
        _http  = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
    }

    public async Task<Pass1Result?> ClassifyPass1Async(string filePath, string fileId, CancellationToken ct = default)
    {
        var imageB64 = Convert.ToBase64String(await File.ReadAllBytesAsync(filePath, ct));
        var prompt   = $"Classify this Ultima Online gump asset.\nfile_id: {fileId}\nReturn only valid JSON matching the schema. Use Unknown when uncertain. Do not output markdown fences.";

        var payload = new JObject
        {
            ["model"]  = _model,
            ["stream"] = false,
            ["options"] = new JObject { ["temperature"] = 0.0 },
            ["messages"] = new JArray
            {
                new JObject { ["role"] = "system",    ["content"] = Pass1SystemPrompt },
                new JObject
                {
                    ["role"]    = "user",
                    ["content"] = new JArray
                    {
                        new JObject { ["type"] = "text",  ["text"] = prompt },
                        new JObject { ["type"] = "image_url", ["image_url"] = new JObject { ["url"] = $"data:image/png;base64,{imageB64}" } }
                    }
                }
            }
        };

        var json = await PostAsync(payload, ct);
        if (json is null) return null;
        var result = JsonConvert.DeserializeObject<Pass1Result>(json);
        if (result is not null) result.FileId = fileId;
        return result;
    }

    public async Task<PaperdollResult?> ClassifyPass2Async(string filePath, string fileId, CancellationToken ct = default)
    {
        var imageB64 = Convert.ToBase64String(await File.ReadAllBytesAsync(filePath, ct));
        var prompt   = $"Classify this Ultima Online paperdoll wearable asset.\nfile_id: {fileId}\nReturn only valid JSON matching the schema. Use Unknown when uncertain. Do not output markdown fences.";

        var payload = new JObject
        {
            ["model"]  = _model,
            ["stream"] = false,
            ["options"] = new JObject { ["temperature"] = 0.0 },
            ["messages"] = new JArray
            {
                new JObject { ["role"] = "system",    ["content"] = Pass2SystemPrompt },
                new JObject
                {
                    ["role"]    = "user",
                    ["content"] = new JArray
                    {
                        new JObject { ["type"] = "text",  ["text"] = prompt },
                        new JObject { ["type"] = "image_url", ["image_url"] = new JObject { ["url"] = $"data:image/png;base64,{imageB64}" } }
                    }
                }
            }
        };

        var json = await PostAsync(payload, ct);
        if (json is null) return null;
        var result = JsonConvert.DeserializeObject<PaperdollResult>(json);
        if (result is not null) result.FileId = fileId;
        return result;
    }

    private async Task<string?> PostAsync(JObject payload, CancellationToken ct)
    {
        try
        {
            var body = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"{BaseUrl}/api/chat", body, ct);
            resp.EnsureSuccessStatusCode();
            var raw     = await resp.Content.ReadAsStringAsync(ct);
            var wrapper = JObject.Parse(raw);
            var content = wrapper["message"]?["content"]?.ToString();
            if (string.IsNullOrWhiteSpace(content)) return null;
            // Strip markdown fences if model misbehaves
            content = content.Trim();
            if (content.StartsWith("```")) content = string.Join('\n', content.Split('\n').Skip(1).SkipLast(1));
            return content;
        }
        catch { return null; }
    }

    public async Task<bool> IsReachableAsync()
    {
        try
        {
            var resp = await _http.GetAsync($"{BaseUrl}/api/tags");
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
