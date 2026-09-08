using System.Text.Json;
using System.Text.Json.Serialization;
using DeskBox.Models;

namespace DeskBox.Services;

/// <summary>
/// Discovers versioned, data-only theme folders. Invalid or incompatible packs
/// are isolated and reported while the built-in Classic theme remains usable.
/// </summary>
public sealed class ThemePackService
{
    public const int CurrentSchemaVersion = 1;
    public const string ClassicThemeId = "deskbox.classic";
    public const string SmokeGlassThemeId = "deskbox.smoke-glass";
    public const string AlpineMistThemeId = "deskbox.alpine-mist";
    public const string ThemeFileName = "theme.json";
    public const long MaximumThemeFileBytes = 64 * 1024;
    public const long MaximumPreviewFileBytes = 2 * 1024 * 1024;

    private static readonly HashSet<string> SupportedPreviewExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp", ".svg" };

    private readonly string _builtInRoot;
    private readonly string _userRoot;
    private readonly Version _hostVersion;
    private ThemePackSnapshot _snapshot = new([CreateClassicTheme()], []);

    public ThemePackService()
        : this(
            Path.Combine(AppContext.BaseDirectory, "Themes"),
            Path.Combine(DeskBoxDataPathService.Current.RootPath, "Themes"),
            ResolveHostVersion())
    {
    }

    internal ThemePackService(string builtInRoot, string userRoot, Version hostVersion)
    {
        _builtInRoot = Path.GetFullPath(builtInRoot);
        _userRoot = Path.GetFullPath(userRoot);
        _hostVersion = hostVersion;
        Reload();
    }

    public IReadOnlyList<ThemePack> Themes => _snapshot.Themes;
    public IReadOnlyList<ThemePackDiagnostic> Diagnostics => _snapshot.Diagnostics;
    public string UserThemeFolder => _userRoot;

    public ThemePack ActiveOrClassic(string? themeId)
    {
        string normalizedThemeId = NormalizeSavedThemeId(themeId);
        return Themes.FirstOrDefault(theme =>
                   string.Equals(theme.Id, normalizedThemeId, StringComparison.OrdinalIgnoreCase)) ??
               Themes.First(theme => theme.Id == ClassicThemeId);
    }

    private static string NormalizeSavedThemeId(string? themeId)
    {
        string normalized = themeId?.Trim() ?? string.Empty;
        if (string.Equals(normalized, "SmokeGlass", StringComparison.OrdinalIgnoreCase))
        {
            return SmokeGlassThemeId;
        }

        if (string.Equals(normalized, "AlpineMist", StringComparison.OrdinalIgnoreCase))
        {
            return AlpineMistThemeId;
        }

        return normalized;
    }

    public ThemePackSnapshot Reload()
    {
        var themes = new List<ThemePack> { CreateClassicTheme() };
        var diagnostics = new List<ThemePackDiagnostic>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ClassicThemeId };

        LoadRoot(_builtInRoot, isBuiltIn: true, themes, ids, diagnostics);

        try
        {
            Directory.CreateDirectory(_userRoot);
        }
        catch (Exception ex)
        {
            diagnostics.Add(new(_userRoot, $"Cannot create the user theme folder: {ex.Message}"));
        }

        LoadRoot(_userRoot, isBuiltIn: false, themes, ids, diagnostics);
        _snapshot = new(themes.AsReadOnly(), diagnostics.AsReadOnly());
        return _snapshot;
    }

    private void LoadRoot(
        string root,
        bool isBuiltIn,
        List<ThemePack> themes,
        HashSet<string> ids,
        List<ThemePackDiagnostic> diagnostics)
    {
        if (!Directory.Exists(root))
        {
            return;
        }

        IEnumerable<string> folders;
        try
        {
            folders = Directory.EnumerateDirectories(root).Order(StringComparer.OrdinalIgnoreCase).Take(128).ToArray();
        }
        catch (Exception ex)
        {
            diagnostics.Add(new(root, $"Cannot enumerate theme folders: {ex.Message}"));
            return;
        }

        foreach (string folder in folders)
        {
            try
            {
                ThemePack pack = LoadTheme(folder, isBuiltIn);
                if (!ids.Add(pack.Id))
                {
                    throw new InvalidDataException($"Theme id '{pack.Id}' is already installed.");
                }

                themes.Add(pack);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
            {
                diagnostics.Add(new(folder, ex.Message));
            }
        }
    }

    private ThemePack LoadTheme(string folder, bool isBuiltIn)
    {
        var folderInfo = new DirectoryInfo(folder);
        if ((folderInfo.Attributes & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidDataException("Theme folders cannot be symbolic links or junctions.");
        }

        string themePath = Path.Combine(folder, ThemeFileName);
        var file = new FileInfo(themePath);
        if (!file.Exists)
        {
            throw new InvalidDataException($"Missing {ThemeFileName}.");
        }

        if ((file.Attributes & FileAttributes.ReparsePoint) != 0 || file.Length > MaximumThemeFileBytes)
        {
            throw new InvalidDataException($"{ThemeFileName} is unsafe or larger than {MaximumThemeFileBytes} bytes.");
        }

        byte[] json = File.ReadAllBytes(themePath);
        ThemePack pack = JsonSerializer.Deserialize(json, ThemePackJsonContext.Default.ThemePack) ??
            throw new InvalidDataException($"{ThemeFileName} is empty.");
        Validate(pack);

        if (!string.IsNullOrWhiteSpace(pack.Preview))
        {
            pack.PreviewPath = ResolvePreview(folder, pack.Preview);
        }

        pack.FolderPath = folder;
        pack.IsBuiltIn = isBuiltIn;
        return pack;
    }

    private void Validate(ThemePack pack)
    {
        if (pack.SchemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidDataException(
                $"Unsupported schemaVersion {pack.SchemaVersion}; DeskBox supports {CurrentSchemaVersion}.");
        }

        if (!IsValidId(pack.Id))
        {
            throw new InvalidDataException("Theme id must contain 3-64 lowercase letters, numbers, dots, or hyphens.");
        }

        if (string.IsNullOrWhiteSpace(pack.Name) || pack.Name.Length > 80 ||
            string.IsNullOrWhiteSpace(pack.Author) || pack.Author.Length > 80 ||
            string.IsNullOrWhiteSpace(pack.License) || pack.License.Length > 80)
        {
            throw new InvalidDataException("Theme name, author, and license are required and must be 80 characters or fewer.");
        }

        if (!TryParseVersion(pack.Version, out _))
        {
            throw new InvalidDataException("Theme version must use a numeric version such as 1.0.0.");
        }

        if (!TryParseVersion(pack.MinimumDeskBoxVersion, out Version minimumVersion))
        {
            throw new InvalidDataException("minimumDeskBoxVersion must use a numeric version such as 1.5.0.");
        }

        if (minimumVersion > _hostVersion)
        {
            throw new InvalidDataException(
                $"Theme requires DeskBox {minimumVersion} or later; this build is {_hostVersion}.");
        }

        pack.LocalizedNames ??= [];
        pack.Description ??= string.Empty;

        if (pack.Visuals is null || pack.Motion is null)
        {
            throw new InvalidDataException("Theme visuals and motion objects are required.");
        }

        ValidateVisuals(pack.Visuals);
        ValidateMotion(pack.Motion);
    }

    private static void ValidateVisuals(ThemeVisualTokens visuals)
    {
        string[] colors =
        [
            visuals.SurfaceColor, visuals.DeepSurfaceColor, visuals.TextPrimaryColor,
            visuals.TextSecondaryColor, visuals.AccentColor, visuals.EdgeStartColor,
            visuals.EdgeEndColor, visuals.SpecularColor, visuals.ShadowColor
        ];
        if (colors.Any(color => !IsHexColor(color)))
        {
            throw new InvalidDataException("Theme colors must use #RRGGBB or #AARRGGBB.");
        }

        if (visuals.SurfaceOpacity is < 0 or > 1 ||
            visuals.CornerRadius is < 0 or > 64 ||
            visuals.BorderThickness is < 0 or > 8 ||
            visuals.InnerBorderThickness is < 0 or > 8 ||
            visuals.Elevation is < 0 or > 64 ||
            visuals.SpecularOpacity is < 0 or > 1)
        {
            throw new InvalidDataException("Theme visual values are outside the supported range.");
        }

        visuals.DeepSurfaceWidgetKinds ??= [];
        if (visuals.DeepSurfaceWidgetKinds.Count > 32 ||
            visuals.DeepSurfaceWidgetKinds.Any(kind => string.IsNullOrWhiteSpace(kind) || kind.Length > 40))
        {
            throw new InvalidDataException("deepSurfaceWidgetKinds exceeds the supported range.");
        }

        if (visuals.Material is not ("Mica" or "MicaAlt" or "Acrylic" or "AcrylicBase" or "Solid"))
        {
            throw new InvalidDataException("Theme material is not supported.");
        }
    }

    private static void ValidateMotion(ThemeMotionTokens motion)
    {
        if (motion.OpenDurationMilliseconds is < 80 or > 1200 ||
            motion.CloseDurationMilliseconds is < 80 or > 1200 ||
            motion.HoverScale is < 1 or > 1.08 ||
            motion.PressScale is < 0.88 or > 1 ||
            motion.SpringDamping is < 0.3 or > 1)
        {
            throw new InvalidDataException("Theme motion values are outside the supported range.");
        }
    }

    private static string ResolvePreview(string folder, string relativeName)
    {
        if (Path.IsPathRooted(relativeName) ||
            relativeName.Contains('/') || relativeName.Contains('\\') ||
            relativeName is "." or "..")
        {
            throw new InvalidDataException("Preview must be a file in the theme folder.");
        }

        string extension = Path.GetExtension(relativeName);
        if (!SupportedPreviewExtensions.Contains(extension))
        {
            throw new InvalidDataException("Preview must be PNG, JPEG, WebP, or SVG.");
        }

        var preview = new FileInfo(Path.Combine(folder, relativeName));
        if (!preview.Exists || (preview.Attributes & FileAttributes.ReparsePoint) != 0 ||
            preview.Length > MaximumPreviewFileBytes)
        {
            throw new InvalidDataException("Preview is missing, unsafe, or larger than 2 MB.");
        }

        return preview.FullName;
    }

    private static bool IsValidId(string? id)
    {
        if (id is null || id.Length is < 3 or > 64 ||
            id[0] is < 'a' or > 'z')
        {
            return false;
        }

        return id.All(character =>
            character is >= 'a' and <= 'z' or >= '0' and <= '9' or '.' or '-');
    }

    private static bool IsHexColor(string? value)
    {
        if (value is null || value.Length is not (7 or 9) || value[0] != '#')
        {
            return false;
        }

        return value.AsSpan(1).ToString().All(Uri.IsHexDigit);
    }

    private static bool TryParseVersion(string? value, out Version version)
    {
        string normalized = value?.Split('-', 2)[0] ?? string.Empty;
        return Version.TryParse(normalized, out version!);
    }

    private static Version ResolveHostVersion() =>
        typeof(ThemePackService).Assembly.GetName().Version ?? new Version(1, 5, 0);

    private static ThemePack CreateClassicTheme() => new()
    {
        Id = ClassicThemeId,
        Name = "Classic",
        LocalizedNames = new() { ["zh-CN"] = "经典" },
        Version = "1.0.0",
        Author = "DeskBox",
        License = "GPL-3.0-only",
        Description = "The standard DeskBox appearance.",
        MinimumDeskBoxVersion = "1.5.0",
        IsBuiltIn = true,
        Visuals = new()
        {
            SurfaceColor = "#CC20242E",
            DeepSurfaceColor = "#E626303D",
            TextPrimaryColor = "#FFF7FAFF",
            TextSecondaryColor = "#BFD9E1EC",
            AccentColor = "#FF0078D4",
            EdgeStartColor = "#38FFFFFF",
            EdgeEndColor = "#18FFFFFF",
            SpecularColor = "#28FFFFFF",
            ShadowColor = "#78000000",
            SurfaceOpacity = 0.8,
            CornerRadius = 8,
            BorderThickness = 1,
            InnerBorderThickness = 0,
            Elevation = 8,
            SpecularOpacity = 0.16,
            Material = "Mica"
        },
        Motion = new()
        {
            OpenDurationMilliseconds = 240,
            CloseDurationMilliseconds = 180,
            HoverScale = 1.01,
            PressScale = 0.985,
            SpringDamping = 0.86
        }
    };
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    WriteIndented = true)]
[JsonSerializable(typeof(ThemePack), TypeInfoPropertyName = "ThemePack")]
internal sealed partial class ThemePackJsonContext : JsonSerializerContext;
