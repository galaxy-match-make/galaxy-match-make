using System;
using System.IO;
using System.Text.Json;
namespace GalaxyMatchGUI.lib;

public static class AppSettings
{
    public static string BackendUrl { get; set; } = "http://13.246.221.76:8080";
    public static string CallbackUrl { get; set; } = "http://localhost:3500/google-signin";
    public static string GoogleClientId { get; set; } = "878686516753-f5u5rclhvs1l6956vviumh9qrtk2uhrk.apps.googleusercontent.com";
}
