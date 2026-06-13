using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;

namespace UOGumpClassifier.Models;

public enum AssetStatus { Queued, Processing, Done, Error, Skipped }

public partial class GumpAssetItem : ObservableObject
{
    [ObservableProperty] private string _filePath = string.Empty;
    [ObservableProperty] private string _fileName = string.Empty;
    [ObservableProperty] private string _fileId   = string.Empty;
    [ObservableProperty] private AssetStatus _status = AssetStatus.Queued;
    [ObservableProperty] private BitmapImage? _thumbnail;
    [ObservableProperty] private Pass1Result?     _pass1Result;
    [ObservableProperty] private PaperdollResult? _paperdollResult;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _sha256 = string.Empty;
    [ObservableProperty] private int    _imageWidth;
    [ObservableProperty] private int    _imageHeight;
    [ObservableProperty] private bool   _hasAlpha;

    public string StatusLabel => Status.ToString();

    public string StatusColor => Status switch
    {
        AssetStatus.Done       => "#4CAF82",
        AssetStatus.Error      => "#E85858",
        AssetStatus.Processing => "#7B68EE",
        AssetStatus.Skipped    => "#505068",
        _                      => "#505068"
    };

    public string DisplayResult
    {
        get
        {
            if (PaperdollResult is not null)
                return $"{PaperdollResult.BaseName}  [{PaperdollResult.Slot}]  {PaperdollResult.Material}  conf:{PaperdollResult.Confidence:P0}";
            if (Pass1Result is not null)
                return $"{Pass1Result.GumpType}  [{Pass1Result.FunctionGuess}]  conf:{Pass1Result.Confidence:P0}";
            return Status == AssetStatus.Error ? ErrorMessage : "Pending";
        }
    }
}
