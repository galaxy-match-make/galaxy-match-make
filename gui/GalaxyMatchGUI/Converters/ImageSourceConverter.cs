using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace GalaxyMatchGUI.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string source)
            {
                if (string.IsNullOrEmpty(source))
                    return null;

                try
                {
                    if (source.StartsWith("avares://"))
                    {
                        var uri = new Uri(source);
                        using var stream = AssetLoader.Open(uri);
                        return new Bitmap(stream);
                    }
                    else if (source.StartsWith("http://") || source.StartsWith("https://"))
                    {
                        LoadFromWeb(source);
                        return null;
                    }
                    else if (source.StartsWith("data:image"))
                    {
                        return LoadFromBase64(source);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image source: {ex.Message}");
                    return null;
                }
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private async void LoadFromWeb(string url)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(url);
                using var ms = new MemoryStream(bytes);
                var bitmap = new Bitmap(ms);
                
                Console.WriteLine($"Successfully loaded image from URL: {url}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load image from URL: {url}, Error: {ex.Message}");
            }
        }

        private Bitmap? LoadFromBase64(string base64)
        {
            try
            {
                var base64Data = base64.Substring(base64.IndexOf(',') + 1);
                var bytes = System.Convert.FromBase64String(base64Data);
                
                using var ms = new MemoryStream(bytes);
                return new Bitmap(ms);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load base64 image: {ex.Message}");
                return null;
            }
        }
    }
}