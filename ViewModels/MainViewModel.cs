using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UOGumpClassifier.Models;
using UOGumpClassifier.Services;

namespace UOGumpClassifier.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // ── Queue ────────────────────────────────────────────────────────────────
    public ObservableCollection<GumpAssetItem> Queue { get; } = new();

    [ObservableProperty] private GumpAssetItem? _selectedItem;
    [ObservableProperty] private BitmapImage?   _currentImage;

    // ── Progress ─────────────────────────────────────────────────────────────
    [ObservableProperty] private int    _progressDone;
    [ObservableProperty] private int    _progressTotal;
    [ObservableProperty] private double _progressPercent;
    [ObservableProperty] private string _progressLabel  = "0 / 0";
    [ObservableProperty] private string _currentFileName = "-";

    // ── Inspector ────────────────────────────────────────────────────────────
    [ObservableProperty] private string _phase              = "Idle";
    [ObservableProperty] private string _selectedClassification = "Pending";
    [ObservableProperty] private string _selectedStatus     = "";
    [ObservableProperty] private string _selectedConfidence = "Confidence: 0%";
    [ObservableProperty] private string _rawJson            = "";

    // ── Status Log ───────────────────────────────────────────────────────────
    [ObservableProperty] private string _statusLog = "";

    // ── Settings ─────────────────────────────────────────────────────────────
    [ObservableProperty] private string _inputFolder  = "";
    [ObservableProperty] private string _outputFolder = "";
    [ObservableProperty] private string _ollamaModel  = "llava";

    private CancellationTokenSource? _cts;
    private readonly ClassifierService _classifier = new();

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void LoadFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Select input folder" };
        if (dialog.ShowDialog() != true) return;

        InputFolder = dialog.FolderName;
        if (string.IsNullOrEmpty(OutputFolder))
            OutputFolder = Path.Combine(Path.GetDirectoryName(InputFolder)!, "uo-classifier-output");

        Queue.Clear();
        var files = Directory.GetFiles(InputFolder, "*.png", SearchOption.TopDirectoryOnly);
        ProgressTotal = files.Length;
        ProgressDone  = 0;
        UpdateProgress(0, files.Length);

        foreach (var f in files)
        {
            var fn   = Path.GetFileNameWithoutExtension(f);
            var item = new GumpAssetItem
            {
                FilePath = f,
                FileName = Path.GetFileName(f),
                FileId   = fn,
                Status   = AssetStatus.Queued,
                Thumbnail = ImageMetadataService.LoadThumbnail(f, 64)
            };
            Queue.Add(item);
        }

        Log($"Loaded {files.Length} assets from {InputFolder}");
        Phase = "Idle";
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task StartPass1()
    {
        if (!await CheckOllamaAsync()) return;
        _cts = new CancellationTokenSource();
        Phase = "Pass 1 — Gump Classification";

        var progress = new Progress<(GumpAssetItem item, int done, int total)>(r =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateProgress(r.done, r.total);
                CurrentFileName = r.item.FileName;
                CurrentImage    = ImageMetadataService.LoadFull(r.item.FilePath);
                SelectedItem    = r.item;
                RefreshInspector(r.item);
                Log($"[Pass1] {r.item.FileName} -> {r.item.Pass1Result?.GumpType ?? r.item.StatusLabel}");
            });
        });

        try
        {
            await _classifier.RunPass1Async(Queue, progress, _cts.Token);
            ManifestExporter.ExportPass1(Queue, OutputFolder);
            Log($"Pass 1 complete. Manifests saved to {OutputFolder}");
        }
        catch (OperationCanceledException) { Log("Pass 1 cancelled."); }
        finally { Phase = "Idle"; _cts = null; }
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task StartPass2()
    {
        if (!await CheckOllamaAsync()) return;
        _cts = new CancellationTokenSource();
        Phase = "Pass 2 — Paperdoll Classification";

        var paperdollItems = Queue.Where(i => i.Pass1Result?.IsPaperdollRelated == true || i.Pass1Result?.GumpType == "Paperdoll").ToList();
        Log($"Pass 2: {paperdollItems.Count} paperdoll candidates queued.");

        var progress = new Progress<(GumpAssetItem item, int done, int total)>(r =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateProgress(r.done, r.total);
                CurrentFileName = r.item.FileName;
                CurrentImage    = ImageMetadataService.LoadFull(r.item.FilePath);
                SelectedItem    = r.item;
                RefreshInspector(r.item);
                Log($"[Pass2] {r.item.FileName} -> {r.item.PaperdollResult?.BaseName ?? r.item.StatusLabel}");
            });
        });

        try
        {
            await _classifier.RunPass2Async(Queue, progress, _cts.Token);
            ManifestExporter.ExportPass2(Queue, OutputFolder);
            FileSorter.SortPass1(Queue, OutputFolder);
            FileSorter.SortPass2(Queue, OutputFolder);
            Log($"Pass 2 complete. Catalogs and sorted folders saved to {OutputFolder}");
        }
        catch (OperationCanceledException) { Log("Pass 2 cancelled."); }
        finally { Phase = "Idle"; _cts = null; }
    }

    [RelayCommand]
    private void Cancel() => _cts?.Cancel();

    private bool CanRun() => Queue.Count > 0 && _cts is null;

    // ── Helpers ───────────────────────────────────────────────────────────────

    partial void OnSelectedItemChanged(GumpAssetItem? value)
    {
        if (value is null) return;
        CurrentImage = ImageMetadataService.LoadFull(value.FilePath);
        RefreshInspector(value);
    }

    private void RefreshInspector(GumpAssetItem item)
    {
        SelectedStatus = item.StatusLabel;
        if (item.PaperdollResult is not null)
        {
            SelectedClassification = $"{item.PaperdollResult.BaseName} [{item.PaperdollResult.Slot}] {item.PaperdollResult.Material}";
            SelectedConfidence     = $"Confidence: {item.PaperdollResult.Confidence:P0}";
            RawJson = Newtonsoft.Json.JsonConvert.SerializeObject(item.PaperdollResult, Newtonsoft.Json.Formatting.Indented);
        }
        else if (item.Pass1Result is not null)
        {
            SelectedClassification = $"{item.Pass1Result.GumpType} [{item.Pass1Result.FunctionGuess}]";
            SelectedConfidence     = $"Confidence: {item.Pass1Result.Confidence:P0}";
            RawJson = Newtonsoft.Json.JsonConvert.SerializeObject(item.Pass1Result, Newtonsoft.Json.Formatting.Indented);
        }
        else
        {
            SelectedClassification = item.StatusLabel;
            SelectedConfidence     = "Confidence: 0%";
            RawJson = "";
        }
    }

    private void UpdateProgress(int done, int total)
    {
        ProgressDone    = done;
        ProgressTotal   = total;
        ProgressPercent = total > 0 ? (double)done / total * 100.0 : 0;
        ProgressLabel   = $"Done: {done} / {total}";
    }

    private void Log(string message)
    {
        StatusLog += $"{DateTime.Now:HH:mm:ss}  {message}\n";
    }

    private async Task<bool> CheckOllamaAsync()
    {
        var ok = await _classifier.IsModelReachableAsync();
        if (!ok) Log("ERROR: Cannot reach Ollama at localhost:11434. Start Ollama and pull llava.");
        return ok;
    }
}
