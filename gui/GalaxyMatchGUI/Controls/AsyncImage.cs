using System;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.IO;
using GalaxyMatchGUI.Services;

namespace GalaxyMatchGUI.Controls
{
    public class AsyncImage : Image
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        
        public static readonly new StyledProperty<string> SourceProperty = 
            AvaloniaProperty.Register<AsyncImage, string>(nameof(Source));
            
        public new string Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
        
        static AsyncImage()
        {
            SourceProperty.Changed.AddClassHandler<AsyncImage>((x, e) => x.OnSourceChanged(e));
            AffectsRender<AsyncImage>(SourceProperty);
        }
        
        private async void OnSourceChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is string newSource && !string.IsNullOrWhiteSpace(newSource))
            {
                try
                {
                    Bitmap? bitmap = null;
                    
                    if (newSource.StartsWith("http://") || newSource.StartsWith("https://"))
                    {
                        var response = await _httpClient.GetAsync(newSource);
                        if (response.IsSuccessStatusCode)
                        {
                            var data = await response.Content.ReadAsByteArrayAsync();
                            using var ms = new MemoryStream(data);
                            bitmap = new Bitmap(ms);
                        }
                    }
                    else if (newSource.StartsWith("avares://"))
                    {
                        try 
                        {
                            var uri = new Uri(newSource);
                            using var stream = AssetLoader.Open(uri);
                            bitmap = new Bitmap(stream);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading asset: {ex.Message}");
                        }
                    }
                    
                    if (bitmap != null)
                    {
                        base.Source = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image from {newSource}: {ex.Message}");
                }
            }
            else
            {
                base.Source = null;
            }
        }
    }
}