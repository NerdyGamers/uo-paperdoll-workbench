using System.IO;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;

namespace UOGumpClassifier.Services;

public static class ImageMetadataService
{
    public static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    public static (int width, int height, bool hasAlpha) GetImageInfo(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
            var frame = decoder.Frames[0];
            return (frame.PixelWidth, frame.PixelHeight, frame.Format.ToString().Contains('A'));
        }
        catch { return (0, 0, false); }
    }

    public static BitmapImage LoadThumbnail(string filePath, int size = 80)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.UriSource       = new Uri(filePath, UriKind.Absolute);
        bmp.DecodePixelWidth = size;
        bmp.CacheOption     = BitmapCacheOption.OnLoad;
        bmp.CreateOptions   = BitmapCreateOptions.IgnoreColorProfile;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    public static BitmapImage LoadFull(string filePath)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.UriSource     = new Uri(filePath, UriKind.Absolute);
        bmp.CacheOption   = BitmapCacheOption.OnLoad;
        bmp.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }
}
