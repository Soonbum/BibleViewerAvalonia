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

            // ViewModel의 속성 변경 이벤트를 구독
            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // 앱 시작 시 저장된 초기 값 적용
            ApplyViewModelSettings(viewModel);

            // 종료 요청 이벤트를 구독합니다.
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

    // ViewModel의 속성이 변경될 때마다 호출되는 메서드
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // sender가 MainWindowViewModel인지 확인합니다.
        if (sender is MainWindowViewModel vm)
        {
            // UI 스레드에서 ApplyViewModelSettings를 호출하여 즉시 반영합니다.
            Dispatcher.UIThread.InvokeAsync(() => ApplyViewModelSettings(vm));
        }
    }

    // ViewModel의 상태를 실제 Application에 적용하는 헬퍼 메서드
    private void ApplyViewModelSettings(MainWindowViewModel vm)
    {
        if (Current is null) return;

        // 테마 적용
        Current.RequestedThemeVariant = vm.CurrentThemeVariant == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

        // 글꼴 크기 적용
        Current.Resources["GlobalFontSize"] = vm.CurrentFontSize;
    }

    // 프로그램 종료 직전에 호출될 이벤트 핸들러
    private void OnShutdown(object? sender, ShutdownRequestedEventArgs e)
    {
        // MainWindow와 그 ViewModel을 가져옵니다.
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow?.DataContext is MainWindowViewModel viewModel)
        {
            // ViewModel의 저장 메서드를 호출합니다.
            viewModel.OnShutdown();
        }
    }
}