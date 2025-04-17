using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace GalaxyMatchGUI.Services
{
    public static class AsyncImageLoader
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<IImage?> LoadAsync(string source)
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
                    var bytes = await _httpClient.GetByteArrayAsync(source);
                    using var ms = new MemoryStream(bytes);
                    return new Bitmap(ms);
                }
                else if (source.StartsWith("data:image"))
                {
                    var base64Data = source.Substring(source.IndexOf(',') + 1);
                    var bytes = Convert.FromBase64String(base64Data);
                    using var ms = new MemoryStream(bytes);
                    return new Bitmap(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
            }

            return null;
        }
    }
}