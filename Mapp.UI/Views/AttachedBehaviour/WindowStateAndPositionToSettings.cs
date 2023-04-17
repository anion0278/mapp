using System;
using System.CodeDom;
using System.Drawing;
using System.Windows;
using Shmap.CommonServices;
using Shmap.UI.Settings;

namespace Shmap.UI.Views.AttachedBehaviour;

/// <summary>
/// Takes care of storing window position, size, state in settings and restoring them
/// </summary>
public class WindowFullStateToSettingsBehavior : Microsoft.Xaml.Behaviors.Behavior<WindowWithSettings>
{
    private ISettingsWrapper _settingsWrapper;

    public WindowFullStateToSettingsBehavior()
    {
        
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        _settingsWrapper = AssociatedObject.SettingsWrapper;
        AssociatedObject.StateChanged += AssociatedObject_StateChanged;
        AssociatedObject.SizeChanged += AssociatedObject_SizeChanged;
        AssociatedObject.LocationChanged += AssociatedObjectOnLocationChanged;

        AssociatedObject.Width = _settingsWrapper.MainWindowSize.Width;
        AssociatedObject.Height = _settingsWrapper.MainWindowSize.Height;

        AssociatedObject.Left = _settingsWrapper.MainWindowTopLeftCorner.X; // TODO add limits (0...Screen Size), because it may sometimes go out
        AssociatedObject.Top = _settingsWrapper.MainWindowTopLeftCorner.Y;

        if (_settingsWrapper.IsMainWindowMaximized) AssociatedObject.WindowState = WindowState.Maximized;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.StateChanged -= AssociatedObject_StateChanged;
        AssociatedObject.SizeChanged -= AssociatedObject_SizeChanged;
        AssociatedObject.LocationChanged -= AssociatedObjectOnLocationChanged;
    }

    private void AssociatedObjectOnLocationChanged(object sender, EventArgs e)
    {
        _settingsWrapper.MainWindowTopLeftCorner = new System.Drawing.Point((int)AssociatedObject.Left, (int)AssociatedObject.Top);
    }

    private void AssociatedObject_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _settingsWrapper.MainWindowSize = new System.Drawing.Size((int)AssociatedObject.ActualWidth, (int)AssociatedObject.ActualHeight);
    }

    private void AssociatedObject_StateChanged(object sender, EventArgs e)
    {
        _settingsWrapper.IsMainWindowMaximized = AssociatedObject.WindowState == WindowState.Maximized;
    }
}