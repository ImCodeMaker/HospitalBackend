using HospitalApp.Core.Application.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HospitalApp.Infrastructure.Shared.Services;

public class LocalFileStorageService(IWebHostEnvironment env, IConfiguration config) : IFileStorageService
{
    private readonly string _baseUrl = config["FileStorage:BaseUrl"] ?? "/files";
    private readonly string _uploadPath = Path.Combine(env.ContentRootPath, "uploads");

    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    public async Task<string> SaveAsync(Stream fileStream, string fileName, string folder, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName);
        var safeFileName = $"{Guid.NewGuid():N}{ext}";
        var dir = Path.Combine(_uploadPath, folder);
        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, safeFileName);

        if (ImageExtensions.Contains(ext))
        {
            await ProcessAndSaveImageAsync(fileStream, filePath, folder, safeFileName, ext, ct);
        }
        else
        {
            await using var dest = File.Create(filePath);
            await fileStream.CopyToAsync(dest, ct);
        }

        return Path.Combine(folder, safeFileName).Replace('\\', '/');
    }

    private static async Task ProcessAndSaveImageAsync(
        Stream fileStream,
        string filePath,
        string folder,
        string safeFileName,
        string ext,
        CancellationToken ct)
    {
        using var image = await Image.LoadAsync(fileStream, ct);

        // Strip ALL EXIF / metadata
        image.Metadata.ExifProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.XmpProfile = null;
        image.Metadata.IccProfile = null;

        // Watermark text
        var watermarkText = $"Lova Salud | {folder} | {DateTime.UtcNow:yyyy-MM-dd}";
        ApplyWatermark(image, watermarkText);

        await using var dest = File.Create(filePath);

        var lowerExt = ext.ToLowerInvariant();
        if (lowerExt is ".jpg" or ".jpeg")
            await image.SaveAsJpegAsync(dest, new JpegEncoder { Quality = 90 }, ct);
        else if (lowerExt == ".png")
            await image.SaveAsPngAsync(dest, new PngEncoder(), ct);
        else if (lowerExt == ".webp")
            await image.SaveAsWebpAsync(dest, cancellationToken: ct);
        else if (lowerExt == ".gif")
            await image.SaveAsGifAsync(dest, cancellationToken: ct);
        else
            await image.SaveAsJpegAsync(dest, cancellationToken: ct);
    }

    private static void ApplyWatermark(Image image, string text)
    {
        const int fontSize = 12;
        const int paddingX = 6;
        const int paddingY = 3;

        FontFamily fontFamily;

        // Try to find a system font; fall back gracefully if none available
        if (!SystemFonts.TryGet("DejaVu Sans", out fontFamily) &&
            !SystemFonts.TryGet("Arial", out fontFamily) &&
            !SystemFonts.TryGet("Liberation Sans", out fontFamily) &&
            !SystemFonts.TryGet("Helvetica", out fontFamily))
        {
            var families = SystemFonts.Families.ToList();
            if (families.Count == 0)
                return; // No system fonts available — skip watermark gracefully
            fontFamily = families[0];
        }

        var font = fontFamily.CreateFont(fontSize, FontStyle.Regular);
        var textOptions = new TextOptions(font);
        var measured = TextMeasurer.MeasureSize(text, textOptions);

        int boxWidth  = (int)measured.Width  + paddingX * 2;
        int boxHeight = (int)measured.Height + paddingY * 2;

        int boxX = image.Width  - boxWidth  - 4;
        int boxY = image.Height - boxHeight - 4;

        if (boxX < 0) boxX = 0;
        if (boxY < 0) boxY = 0;

        image.Mutate(ctx =>
        {
            // Dark semi-transparent background rectangle
            ctx.Fill(
                new DrawingOptions(),
                Color.FromRgba(0, 0, 0, 180),
                new SixLabors.ImageSharp.Drawing.RectangularPolygon(boxX, boxY, boxWidth, boxHeight));

            // White text on top
            ctx.DrawText(
                text,
                font,
                Color.White,
                new PointF(boxX + paddingX, boxY + paddingY));
        });
    }

    public Task DeleteAsync(string filePath, CancellationToken ct = default)
    {
        var full = Path.Combine(_uploadPath, filePath);
        if (File.Exists(full))
            File.Delete(full);
        return Task.CompletedTask;
    }

    public string GetUrl(string filePath) => $"{_baseUrl}/{filePath}";
}
