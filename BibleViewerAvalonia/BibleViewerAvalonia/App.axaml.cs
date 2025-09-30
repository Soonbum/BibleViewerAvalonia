using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using BibleViewerAvalonia.ViewModels;
using BibleViewerAvalonia.Views;

namespace BibleViewerAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };

            // ���� ��û �̺�Ʈ�� �����մϴ�.
            desktop.ShutdownRequested += OnShutdown;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    // ���α׷� ���� ������ ȣ��� �̺�Ʈ �ڵ鷯
    private void OnShutdown(object? sender, ShutdownRequestedEventArgs e)
    {
        // MainWindow�� �� ViewModel�� �����ɴϴ�.
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow?.DataContext is MainWindowViewModel viewModel)
        {
            // ViewModel�� ���� �޼��带 ȣ���մϴ�.
            viewModel.OnShutdown();
        }
    }
}