using DeskBox.Services;

namespace DeskBox.Tests;

public sealed class ThemePackServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DeskBox.ThemeTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void Reload_DiscoversBuiltInAndUserThemesWithLocalizedNames()
    {
        string builtIn = Directory.CreateDirectory(Path.Combine(_root, "built-in")).FullName;
        string user = Directory.CreateDirectory(Path.Combine(_root, "user")).FullName;
        WriteTheme(builtIn, "deskbox.smoke-glass", "Smoke Glass", "烟熏玻璃");
        WriteTheme(user, "community.snow", "Snow", "雪");

        var service = new ThemePackService(builtIn, user, new Version(1, 5, 0));

        Assert.Equal(3, service.Themes.Count);
        Assert.Equal("烟熏玻璃", service.ActiveOrClassic("deskbox.smoke-glass").GetDisplayName("zh-CN"));
        Assert.False(service.ActiveOrClassic("community.snow").IsBuiltIn);
        Assert.Empty(service.Diagnostics);
    }

    [Fact]
    public void ActiveOrClassic_FallsBackWhenSavedThemeWasRemoved()
    {
        string builtIn = Directory.CreateDirectory(Path.Combine(_root, "built-in")).FullName;
        string user = Path.Combine(_root, "user");
        var service = new ThemePackService(builtIn, user, new Version(1, 5, 0));

        Assert.Equal(ThemePackService.ClassicThemeId, service.ActiveOrClassic("missing.theme").Id);
        Assert.True(Directory.Exists(user));
    }

    [Fact]
    public void Reload_QuarantinesIncompatibleAndTraversalPacks()
    {
        string builtIn = Directory.CreateDirectory(Path.Combine(_root, "built-in")).FullName;
        string user = Directory.CreateDirectory(Path.Combine(_root, "user")).FullName;
        WriteTheme(user, "future.theme", "Future", "未来", minimumVersion: "9.0.0");
        WriteTheme(user, "unsafe.preview", "Unsafe", "不安全", preview: "..\\outside.png");

        var service = new ThemePackService(builtIn, user, new Version(1, 5, 0));

        Assert.Single(service.Themes);
        Assert.Equal(2, service.Diagnostics.Count);
        Assert.Contains(service.Diagnostics, item => item.Message.Contains("requires DeskBox", StringComparison.Ordinal));
        Assert.Contains(service.Diagnostics, item => item.Message.Contains("Preview must be", StringComparison.Ordinal));
    }

    [Fact]
    public void Reload_DoesNotAllowUserThemeToReplaceBuiltInId()
    {
        string builtIn = Directory.CreateDirectory(Path.Combine(_root, "built-in")).FullName;
        string user = Directory.CreateDirectory(Path.Combine(_root, "user")).FullName;
        WriteTheme(builtIn, "deskbox.smoke-glass", "Trusted", "可信");
        WriteTheme(user, "deskbox.smoke-glass", "Replacement", "替换");

        var service = new ThemePackService(builtIn, user, new Version(1, 5, 0));

        Assert.Equal("Trusted", service.ActiveOrClassic("deskbox.smoke-glass").Name);
        Assert.Single(service.Diagnostics);
        Assert.Contains("already installed", service.Diagnostics[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Reload_QuarantinesThemeWithNullRequiredObjects()
    {
        string builtIn = Directory.CreateDirectory(Path.Combine(_root, "built-in")).FullName;
        string user = Directory.CreateDirectory(Path.Combine(_root, "user")).FullName;
        string folder = Directory.CreateDirectory(Path.Combine(user, "broken.theme")).FullName;
        File.WriteAllText(
            Path.Combine(folder, ThemePackService.ThemeFileName),
            """
            {
              "schemaVersion": 1,
              "id": "broken.theme",
              "name": "Broken",
              "version": "1.0.0",
              "author": "Test Author",
              "license": "MIT",
              "minimumDeskBoxVersion": "1.5.0",
              "visuals": null,
              "motion": null
            }
            """);

        var service = new ThemePackService(builtIn, user, new Version(1, 5, 0));

        Assert.Single(service.Themes);
        Assert.Single(service.Diagnostics);
        Assert.Contains("visuals and motion", service.Diagnostics[0].Message, StringComparison.Ordinal);
    }

    private static void WriteTheme(
        string root,
        string id,
        string name,
        string localizedName,
        string minimumVersion = "1.5.0",
        string preview = "preview.svg")
    {
        string folder = Directory.CreateDirectory(Path.Combine(root, id)).FullName;
        File.WriteAllText(Path.Combine(folder, "preview.svg"), "<svg xmlns=\"http://www.w3.org/2000/svg\"/>");
        File.WriteAllText(
            Path.Combine(folder, ThemePackService.ThemeFileName),
            $$"""
            {
              "schemaVersion": 1,
              "id": "{{id}}",
              "name": "{{name}}",
              "localizedNames": { "zh-CN": "{{localizedName}}" },
              "version": "1.0.0",
              "author": "Test Author",
              "license": "MIT",
              "minimumDeskBoxVersion": "{{minimumVersion}}",
              "preview": "{{preview.Replace("\\", "\\\\")}}"
            }
            """);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
