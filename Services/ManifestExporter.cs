using System.IO;
using System.Text;
using Newtonsoft.Json;
using UOGumpClassifier.Models;

namespace UOGumpClassifier.Services;

public static class ManifestExporter
{
    public static void ExportPass1(IEnumerable<GumpAssetItem> items, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var results = items
            .Where(i => i.Pass1Result is not null)
            .Select(i => i.Pass1Result!)
            .ToList();

        // JSON
        var json = JsonConvert.SerializeObject(results, Formatting.Indented);
        File.WriteAllText(Path.Combine(outputDir, "all_gumps_manifest.json"), json, Encoding.UTF8);

        // CSV
        var csv = new StringBuilder();
        csv.AppendLine("file_id,file_name,gump_type,shape,function_guess,has_text,is_paperdoll_related,confidence,width,height,has_alpha,sha256,short_description");
        foreach (var r in results)
        {
            csv.AppendLine($"\"{r.FileId}\",\"{r.FileName}\",\"{r.GumpType}\",\"{r.Shape}\",\"{r.FunctionGuess}\",{r.HasText},{r.IsPaperdollRelated},{r.Confidence:F3},{r.Width},{r.Height},{r.HasAlpha},\"{r.Sha256}\",\"{EscapeCsv(r.ShortDescription)}\"");
        }
        File.WriteAllText(Path.Combine(outputDir, "all_gumps_manifest.csv"), csv.ToString(), Encoding.UTF8);
    }

    public static void ExportPass2(IEnumerable<GumpAssetItem> items, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var results = items
            .Where(i => i.PaperdollResult is not null)
            .Select(i => i.PaperdollResult!)
            .ToList();

        // JSON
        var json = JsonConvert.SerializeObject(results, Formatting.Indented);
        File.WriteAllText(Path.Combine(outputDir, "paperdoll_catalog.json"), json, Encoding.UTF8);

        // CSV
        var csv = new StringBuilder();
        csv.AppendLine("file_id,file_name,base_name,slot,material,gender,body_type,state,is_hueable,is_partial_piece,theme,confidence,width,height,sha256,short_description");
        foreach (var r in results)
        {
            csv.AppendLine($"\"{r.FileId}\",\"{r.FileName}\",\"{r.BaseName}\",\"{r.Slot}\",\"{r.Material}\",\"{r.Gender}\",\"{r.BodyType}\",\"{r.State}\",{r.IsHueable},{r.IsPartialPiece},\"{r.Theme}\",{r.Confidence:F3},{r.Width},{r.Height},\"{r.Sha256}\",\"{EscapeCsv(r.ShortDescription)}\"");
        }
        File.WriteAllText(Path.Combine(outputDir, "paperdoll_catalog.csv"), csv.ToString(), Encoding.UTF8);
    }

    private static string EscapeCsv(string s) => s.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
}
