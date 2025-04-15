using System;
using System.IO;
using System.Text.Json;
namespace GalaxyMatchGUI;

public class AppSettings
{
    public string BackendUrl { get; set; }
    public string CallbackUrl { get; set; }

    public static AppSettings Load()
    {
        string projectDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
        string fullPath = Path.Combine(projectDir, "appsettings.json");

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"appsettings.json not found at: {fullPath}");

        string json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<AppSettings>(json);
    }
}
