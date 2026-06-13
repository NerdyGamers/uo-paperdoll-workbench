using System.IO;
using UOGumpClassifier.Models;

namespace UOGumpClassifier.Services;

public class ClassifierService
{
    private readonly OllamaService _ollama;

    public ClassifierService(string model = "llava") => _ollama = new OllamaService(model);

    public async Task RunPass1Async(
        IEnumerable<GumpAssetItem> items,
        IProgress<(GumpAssetItem item, int done, int total)> progress,
        CancellationToken ct)
    {
        var list  = items.ToList();
        int total = list.Count;
        int done  = 0;

        foreach (var item in list)
        {
            ct.ThrowIfCancellationRequested();
            item.Status = AssetStatus.Processing;

            try
            {
                // Deterministic metadata
                item.Sha256 = ImageMetadataService.ComputeSha256(item.FilePath);
                var (w, h, a) = ImageMetadataService.GetImageInfo(item.FilePath);
                item.ImageWidth  = w;
                item.ImageHeight = h;
                item.HasAlpha    = a;

                // AI classification
                var result = await _ollama.ClassifyPass1Async(item.FilePath, item.FileId, ct);
                if (result is not null)
                {
                    result.FileName = item.FileName;
                    result.Sha256   = item.Sha256;
                    result.Width    = w;
                    result.Height   = h;
                    result.HasAlpha = a;
                    item.Pass1Result = result;
                    item.Status = AssetStatus.Done;
                }
                else
                {
                    item.ErrorMessage = "No result from model";
                    item.Status = AssetStatus.Error;
                }
            }
            catch (OperationCanceledException) { item.Status = AssetStatus.Skipped; throw; }
            catch (Exception ex)
            {
                item.ErrorMessage = ex.Message;
                item.Status = AssetStatus.Error;
            }

            done++;
            progress.Report((item, done, total));
        }
    }

    public async Task RunPass2Async(
        IEnumerable<GumpAssetItem> items,
        IProgress<(GumpAssetItem item, int done, int total)> progress,
        CancellationToken ct)
    {
        // Only process items flagged as paperdoll-related in Pass 1
        var list = items
            .Where(i => i.Pass1Result?.IsPaperdollRelated == true || i.Pass1Result?.GumpType == "Paperdoll")
            .ToList();

        int total = list.Count;
        int done  = 0;

        foreach (var item in list)
        {
            ct.ThrowIfCancellationRequested();
            item.Status = AssetStatus.Processing;

            try
            {
                var result = await _ollama.ClassifyPass2Async(item.FilePath, item.FileId, ct);
                if (result is not null)
                {
                    result.FileName = item.FileName;
                    result.Sha256   = item.Sha256;
                    result.Width    = item.ImageWidth;
                    result.Height   = item.ImageHeight;
                    result.Metadata.HasAlpha = item.HasAlpha;
                    item.PaperdollResult = result;
                    item.Status = AssetStatus.Done;
                }
                else
                {
                    item.ErrorMessage = "No Pass2 result from model";
                    item.Status = AssetStatus.Error;
                }
            }
            catch (OperationCanceledException) { item.Status = AssetStatus.Skipped; throw; }
            catch (Exception ex)
            {
                item.ErrorMessage = ex.Message;
                item.Status = AssetStatus.Error;
            }

            done++;
            progress.Report((item, done, total));
        }
    }

    public Task<bool> IsModelReachableAsync() => _ollama.IsReachableAsync();
}
