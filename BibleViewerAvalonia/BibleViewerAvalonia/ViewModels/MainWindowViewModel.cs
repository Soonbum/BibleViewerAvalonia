using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BibleViewerAvalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    // 검색 바 콤보박스를 위한 속성
    public ObservableCollection<string> SearchVersions { get; } = new ObservableCollection<string>
    {
        "킹제임스흠정역",
        "개역한글판",
    };
    [ObservableProperty]
    private string _selectedSearchVersion;

    // 모든 콤보박스가 공유할 역본 목록
    private static readonly List<string> SharedVersions = new List<string>
    {
        "킹제임스흠정역",
        "개역한글판",
    };

    public ObservableCollection<ComboBoxViewModel> BibleComboBoxes { get; }

    public MainWindowViewModel()
    {
        BibleComboBoxes = new ObservableCollection<ComboBoxViewModel>();
        // 초기 콤보박스에 공유 역본 목록을 전달
        AddComboBox();

        // 초기 선택 값 설정
        SelectedSearchVersion = SearchVersions.FirstOrDefault();
    }

    [RelayCommand(CanExecute = nameof(CanAddComboBox))]
    private void AddComboBox()
    {
        // 새 콤보박스 뷰모델 생성 시 공유 역본 목록을 전달
        BibleComboBoxes.Add(new ComboBoxViewModel(SharedVersions));
        AddComboBoxCommand.NotifyCanExecuteChanged();
        RemoveComboBoxCommand.NotifyCanExecuteChanged();
    }

    private bool CanAddComboBox()
    {
        return BibleComboBoxes.Count < 4;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveComboBox))]
    private void RemoveComboBox()
    {
        if (BibleComboBoxes.Any())
        {
            BibleComboBoxes.RemoveAt(BibleComboBoxes.Count - 1);
        }
        AddComboBoxCommand.NotifyCanExecuteChanged();
        RemoveComboBoxCommand.NotifyCanExecuteChanged();
    }

    private bool CanRemoveComboBox()
    {
        return BibleComboBoxes.Count > 1;
    }

    // 테마 변경
    [ObservableProperty]
    private string _themeButtonText = "어둡게";

    [RelayCommand]
    private void ToggleTheme()
    {
        if (Application.Current is null) return;

        var currentTheme = Application.Current.RequestedThemeVariant;
        Application.Current.RequestedThemeVariant =
            currentTheme == ThemeVariant.Light ? ThemeVariant.Dark : ThemeVariant.Light;

        if (currentTheme == ThemeVariant.Light)
            ThemeButtonText = "어둡게";
        else
            ThemeButtonText = "밝게";
    }
}