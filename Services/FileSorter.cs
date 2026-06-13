using System.IO;
using UOGumpClassifier.Models;

namespace UOGumpClassifier.Services;

public static class FileSorter
{
    public static void SortPass1(IEnumerable<GumpAssetItem> items, string outputDir)
    {
        foreach (var item in items.Where(i => i.Pass1Result is not null))
        {
            var gumpType  = Sanitize(item.Pass1Result!.GumpType);
            var targetDir = Path.Combine(outputDir, "sorted_gumps", gumpType);
            Directory.CreateDirectory(targetDir);
            var dest = Path.Combine(targetDir, item.FileName);
            if (!File.Exists(dest)) File.Copy(item.FilePath, dest);
        }
    }

    public static void SortPass2(IEnumerable<GumpAssetItem> items, string outputDir)
    {
        foreach (var item in items.Where(i => i.PaperdollResult is not null))
        {
            var slot      = Sanitize(item.PaperdollResult!.Slot);
            var material  = Sanitize(item.PaperdollResult.Material);
            var targetDir = Path.Combine(outputDir, "paperdoll_sorted", slot, material);
            Directory.CreateDirectory(targetDir);
            var dest = Path.Combine(targetDir, item.FileName);
            if (!File.Exists(dest)) File.Copy(item.FilePath, dest);
        }
    }

    private static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "Unknown" : name;
    }
}
