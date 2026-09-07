using DeskBox.Helpers;
using DeskBox.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
using Windows.UI;

namespace DeskBox.Controls;

public sealed partial class WidgetShell
{
    private ThemePack? _activeVisualTheme;
    private Storyboard? _themeInteractionStoryboard;
    private bool _isThemePointerPressed;

    public void ApplyVisualTheme(ThemePack theme, bool useDeepSurface)
    {
        bool themeChanged = !string.Equals(
            _activeVisualTheme?.Id,
            theme.Id,
            StringComparison.OrdinalIgnoreCase);
        _activeVisualTheme = theme;
        ThemeVisualTokens visuals = theme.Visuals;
        double radius = Math.Clamp(visuals.CornerRadius, 0, 64);
        Color surface = AccentColorHelper.FromHex(
            useDeepSurface ? visuals.DeepSurfaceColor : visuals.SurfaceColor);
        bool acrylic = visuals.Material is "Acrylic" or "AcrylicBase";

        ThemeSurfaceWash.Background = new SolidColorBrush(WithScaledAlpha(
            surface,
            acrylic ? 0.22 : visuals.SurfaceOpacity));
        ThemeSurfaceWash.CornerRadius = new CornerRadius(radius);

        Color shadow = AccentColorHelper.FromHex(visuals.ShadowColor);
        ThemeDepthEdge.BorderBrush = new SolidColorBrush(WithScaledAlpha(shadow, 0.72));
        double depth = Math.Clamp(visuals.Elevation / 8, 1, 6);
        ThemeDepthEdge.BorderThickness = new Thickness(0, 0, depth * 0.82, depth);
        ThemeDepthEdge.CornerRadius = new CornerRadius(radius);

        var outerRim = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1)
        };
        outerRim.GradientStops.Add(new GradientStop
        {
            Color = AccentColorHelper.FromHex(visuals.EdgeStartColor),
            Offset = 0
        });
        outerRim.GradientStops.Add(new GradientStop
        {
            Color = WithScaledAlpha(AccentColorHelper.FromHex(visuals.SpecularColor), 0.6),
            Offset = 0.42
        });
        outerRim.GradientStops.Add(new GradientStop
        {
            Color = AccentColorHelper.FromHex(visuals.EdgeEndColor),
            Offset = 1
        });
        ThemeOuterRim.BorderBrush = outerRim;
        ThemeOuterRim.BorderThickness = new Thickness(visuals.BorderThickness);
        ThemeOuterRim.CornerRadius = new CornerRadius(radius);

        ThemeInnerRim.BorderBrush = new SolidColorBrush(WithScaledAlpha(
            AccentColorHelper.FromHex(visuals.SpecularColor),
            visuals.SpecularOpacity));
        ThemeInnerRim.BorderThickness = new Thickness(visuals.InnerBorderThickness);
        ThemeInnerRim.CornerRadius = new CornerRadius(Math.Max(0, radius - 2));

        var specular = new LinearGradientBrush
        {
            StartPoint = new Point(0.05, 0),
            EndPoint = new Point(0.72, 1)
        };
        Color specularColor = AccentColorHelper.FromHex(visuals.SpecularColor);
        specular.GradientStops.Add(new GradientStop
        {
            Color = WithScaledAlpha(specularColor, visuals.SpecularOpacity * 0.34),
            Offset = 0
        });
        specular.GradientStops.Add(new GradientStop
        {
            Color = WithScaledAlpha(specularColor, visuals.SpecularOpacity * 0.08),
            Offset = 0.36
        });
        specular.GradientStops.Add(new GradientStop
        {
            Color = Colors.Transparent,
            Offset = 0.68
        });
        ThemeSpecularLayer.Background = specular;
        ThemeSpecularLayer.CornerRadius = new CornerRadius(radius);

        BackgroundPlate.Background = new SolidColorBrush(Colors.Transparent);
        BackgroundPlate.BorderBrush = new SolidColorBrush(Colors.Transparent);
        BackgroundPlate.BorderThickness = new Thickness(0);
        BackgroundPlate.CornerRadius = new CornerRadius(radius);
        HeaderDivider.Background = new SolidColorBrush(WithScaledAlpha(
            AccentColorHelper.FromHex(visuals.TextSecondaryColor),
            0.24));

        if (themeChanged && !_isPointerOverShell && !_isThemePointerPressed)
        {
            StopThemeInteractionAnimation(resetScale: false);
            double restScale = GetThemeRestScale(theme);
            ShellRootScale.ScaleX = restScale;
            ShellRootScale.ScaleY = restScale;
        }

        SetThemeLayerVisibility(Visibility.Visible);
    }

    public void ClearVisualTheme()
    {
        _activeVisualTheme = null;
        _isThemePointerPressed = false;
        StopThemeInteractionAnimation(resetScale: true);
        SetThemeLayerVisibility(Visibility.Collapsed);
        ThemeSurfaceWash.Background = null;
        ThemeDepthEdge.BorderBrush = null;
        ThemeOuterRim.BorderBrush = null;
        ThemeInnerRim.BorderBrush = null;
        ThemeSpecularLayer.Background = null;
    }

    private void SetThemeLayerVisibility(Visibility visibility)
    {
        ThemeSurfaceWash.Visibility = visibility;
        ThemeDepthEdge.Visibility = visibility;
        ThemeOuterRim.Visibility = visibility;
        ThemeInnerRim.Visibility = visibility;
        ThemeSpecularLayer.Visibility = visibility;
    }

    private static Color WithScaledAlpha(Color color, double scale)
    {
        byte alpha = (byte)Math.Clamp(Math.Round(color.A * Math.Clamp(scale, 0, 1)), 0, 255);
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }

    private void ShellRoot_ThemePointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (_activeVisualTheme is null || _isCollapsed ||
            !e.GetCurrentPoint(ShellRoot).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _isThemePointerPressed = true;
        AnimateThemeInteractionScale(_activeVisualTheme.Motion.PressScale, 90);
    }

    private void ShellRoot_ThemePointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isThemePointerPressed = false;
        RestoreThemeInteractionScale();
    }

    private void ShellRoot_ThemePointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        _isThemePointerPressed = false;
        RestoreThemeInteractionScale();
    }

    private void RestoreThemeInteractionScale()
    {
        ThemePack? theme = _activeVisualTheme;
        double target = theme is not null && _isPointerOverShell && !_isCollapsed
            ? 1
            : theme is null ? 1 : GetThemeRestScale(theme);
        AnimateThemeInteractionScale(target, 150);
    }

    private void AnimateThemeInteractionScale(double target, int baseDurationMilliseconds)
    {
        if (_activeVisualTheme is null || _isCollapsed || !SystemAnimationsEnabled())
        {
            StopThemeInteractionAnimation(resetScale: true);
            return;
        }

        _themeInteractionStoryboard?.Stop();
        double damping = Math.Clamp(_activeVisualTheme.Motion.SpringDamping, 0.3, 1);
        int duration = (int)Math.Round(baseDurationMilliseconds * (1.18 - (damping * 0.18)));
        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
        var scaleX = new DoubleAnimation
        {
            To = target,
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingFunction = easing,
            EnableDependentAnimation = true
        };
        var scaleY = new DoubleAnimation
        {
            To = target,
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingFunction = easing,
            EnableDependentAnimation = true
        };
        Storyboard.SetTarget(scaleX, ShellRootScale);
        Storyboard.SetTargetProperty(scaleX, nameof(ScaleTransform.ScaleX));
        Storyboard.SetTarget(scaleY, ShellRootScale);
        Storyboard.SetTargetProperty(scaleY, nameof(ScaleTransform.ScaleY));

        _themeInteractionStoryboard = new Storyboard();
        _themeInteractionStoryboard.Children.Add(scaleX);
        _themeInteractionStoryboard.Children.Add(scaleY);
        _themeInteractionStoryboard.Begin();
    }

    private void StopThemeInteractionAnimation(bool resetScale)
    {
        _themeInteractionStoryboard?.Stop();
        _themeInteractionStoryboard = null;
        if (resetScale)
        {
            ShellRootScale.ScaleX = 1;
            ShellRootScale.ScaleY = 1;
        }
    }

    private static double GetThemeRestScale(ThemePack theme) =>
        Math.Clamp(1 - ((theme.Motion.HoverScale - 1) * 0.35), 0.97, 1);
}
