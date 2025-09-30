using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using BibleViewerAvalonia.ViewModels;
using BibleViewerAvalonia.Views;
using System.ComponentModel;
using System.Linq;

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
            
            var viewModel = new MainWindowViewModel();
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };

            // ViewModel�� �Ӽ� ���� �̺�Ʈ�� ����
            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // �� ���� �� ����� �ʱ� �� ����
            ApplyViewModelSettings(viewModel);

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

    // ViewModel�� �Ӽ��� ����� ������ ȣ��Ǵ� �޼���
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // sender�� MainWindowViewModel���� Ȯ���մϴ�.
        if (sender is MainWindowViewModel vm)
        {
            // UI �����忡�� ApplyViewModelSettings�� ȣ���Ͽ� ��� �ݿ��մϴ�.
            Dispatcher.UIThread.InvokeAsync(() => ApplyViewModelSettings(vm));
        }
    }

    // ViewModel�� ���¸� ���� Application�� �����ϴ� ���� �޼���
    private void ApplyViewModelSettings(MainWindowViewModel vm)
    {
        if (Current is null) return;

        // �׸� ����
        Current.RequestedThemeVariant = vm.CurrentThemeVariant == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

        // �۲� ũ�� ����
        Current.Resources["GlobalFontSize"] = vm.CurrentFontSize;
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