using DeskBox.Helpers;
using DeskBox.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DeskBox.Views.SettingsSections;

public sealed partial class AppearanceSettingsSection : UserControl
{
    public AppearanceSettingsSection()
    {
        InitializeComponent();
    }

    public event EventHandler<SettingsSectionNavigationRequestedEventArgs>? NavigationRequested;

    private void NestedSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string sectionTag })
        {
            NavigationRequested?.Invoke(this, new SettingsSectionNavigationRequestedEventArgs(sectionTag));
        }
    }

    private void AccentPresetButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel viewModel ||
            sender is not Button { Tag: string hex } ||
            !AccentColorHelper.TryParseHex(hex, out var color))
        {
            return;
        }

        viewModel.SetCustomAccentColor(color);
    }

    private async void OpenVisualThemeFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel viewModel)
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(viewModel.VisualThemeFolderPath);
            var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(
                viewModel.VisualThemeFolderPath);
            await Windows.System.Launcher.LaunchFolderAsync(folder);
        }
        catch (Exception ex)
        {
            App.Log($"[Themes] Failed to open user theme folder: {ex.Message}");
        }
    }

    private void ReloadVisualThemesButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel viewModel)
        {
            viewModel.ReloadVisualThemes();
        }
    }
}
