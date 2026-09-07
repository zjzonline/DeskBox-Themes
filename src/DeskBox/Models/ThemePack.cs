using System.Text.Json.Serialization;

namespace DeskBox.Models;

/// <summary>
/// Versioned, data-only visual theme. Theme packs deliberately contain no
/// executable code or XAML so folders from the community can be inspected and
/// removed without changing the application installation.
/// </summary>
public sealed class ThemePack
{
    public int SchemaVersion { get; set; } = 1;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> LocalizedNames { get; set; } = [];
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MinimumDeskBoxVersion { get; set; } = "1.5.0";
    public string Preview { get; set; } = string.Empty;
    public ThemeVisualTokens Visuals { get; set; } = new();
    public ThemeMotionTokens Motion { get; set; } = new();

    [JsonIgnore]
    public string FolderPath { get; internal set; } = string.Empty;

    [JsonIgnore]
    public string PreviewPath { get; internal set; } = string.Empty;

    [JsonIgnore]
    public bool IsBuiltIn { get; internal set; }

    public string GetDisplayName(string? language)
    {
        if (!string.IsNullOrWhiteSpace(language) &&
            LocalizedNames.TryGetValue(language, out string? exact) &&
            !string.IsNullOrWhiteSpace(exact))
        {
            return exact;
        }

        string neutral = language?.Split('-', 2)[0] ?? string.Empty;
        if (neutral.Length > 0 &&
            LocalizedNames.TryGetValue(neutral, out string? translated) &&
            !string.IsNullOrWhiteSpace(translated))
        {
            return translated;
        }

        return string.IsNullOrWhiteSpace(Name) ? Id : Name;
    }
}

public sealed class ThemeVisualTokens
{
    public string SurfaceColor { get; set; } = "#CC20242E";
    public string DeepSurfaceColor { get; set; } = "#E626303D";
    public string TextPrimaryColor { get; set; } = "#FFF7FAFF";
    public string TextSecondaryColor { get; set; } = "#BFD9E1EC";
    public string AccentColor { get; set; } = "#FF58E8EE";
    public string EdgeStartColor { get; set; } = "#FF5EF5EF";
    public string EdgeEndColor { get; set; } = "#FFE65AE8";
    public string SpecularColor { get; set; } = "#8CFFFFFF";
    public string ShadowColor { get; set; } = "#B3000000";
    public double SurfaceOpacity { get; set; } = 0.82;
    public double CornerRadius { get; set; } = 18;
    public double BorderThickness { get; set; } = 1.5;
    public double InnerBorderThickness { get; set; } = 1;
    public double Elevation { get; set; } = 18;
    public double SpecularOpacity { get; set; } = 0.56;
    public string Material { get; set; } = "Acrylic";
    public List<string> DeepSurfaceWidgetKinds { get; set; } = [];
}

public sealed class ThemeMotionTokens
{
    public int OpenDurationMilliseconds { get; set; } = 360;
    public int CloseDurationMilliseconds { get; set; } = 240;
    public double HoverScale { get; set; } = 1.018;
    public double PressScale { get; set; } = 0.985;
    public double SpringDamping { get; set; } = 0.78;
}

public sealed record ThemePackDiagnostic(string FolderPath, string Message);

public sealed record ThemePackSnapshot(
    IReadOnlyList<ThemePack> Themes,
    IReadOnlyList<ThemePackDiagnostic> Diagnostics);
