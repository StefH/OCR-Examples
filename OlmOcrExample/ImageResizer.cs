using OlmOcrExample.Models;
using SkiaSharp;

namespace OlmOcrExample;

internal static class ImageResizer
{
    private static readonly double AspectRatioA4 = Math.Sqrt(2);

    internal static (byte[] ResizedImage, Dimensions PageDimensions, SKRect Image) Resize(byte[] imageBytes, int maxHeight = 1024)
    {
        using var inputImage = SKBitmap.Decode(imageBytes);

        int newHeight;
        if (inputImage.Width > inputImage.Height)
        {
            newHeight = Math.Min((int)(inputImage.Width * AspectRatioA4), maxHeight);
        }
        else
        {
            newHeight = Math.Min(inputImage.Height, maxHeight);
        }

        var newWidth = (int)(newHeight / AspectRatioA4);

        // Resize image while keeping aspect ratio
        var scale = Math.Min((float)newWidth / inputImage.Width, (float)newHeight / inputImage.Height);
        var resizedWidth = (int)(inputImage.Width * scale);
        var resizedHeight = (int)(inputImage.Height * scale);

        using var resizedImage = inputImage.Resize(new SKImageInfo(resizedWidth, resizedHeight), SKSamplingOptions.Default);

        // Create a new canvas with the target dimensions
        using var surface = SKSurface.Create(new SKImageInfo(newWidth, newHeight));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        
        // Center the resized image on the new canvas
        var offsetX = (newWidth - resizedWidth) / 2;
        var offsetY = (newHeight - resizedHeight) / 2;
        var rect = new SKRect(offsetX, offsetY, offsetX + resizedWidth, offsetY + resizedHeight);
        canvas.DrawBitmap(resizedImage, rect);

        // Add a 1-pixel, red border around the new image
        var borderPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = 1
        };
        canvas.DrawRect(0, 0, newWidth - 1, newHeight - 1, borderPaint);

        // Save the output image as Png
        using var image = surface.Snapshot();
        var data = image.Encode(SKEncodedImageFormat.Png, 100).ToArray();

#if DEBUG
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var folder = Path.Combine(userProfile, "OneDrive", "Training", "Generative AI Foundations", "Images");
        File.WriteAllBytes(Path.Combine(folder, "image_resized.png"), data);
#endif

        // Return the resized image, new dimensions, and the rectangle where the image was drawn
        return (data, new Dimensions(newWidth, newHeight), rect);
    }
}