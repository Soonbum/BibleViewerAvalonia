using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using BibleViewerAvalonia.Service;
using BibleViewerAvalonia.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    // 검색 바 콤보박스를 위한 속성
    public ObservableCollection<string> SearchVersions { get; } = [];
    [ObservableProperty]
    private string _selectedSearchVersion;

    // 모든 콤보박스가 공유할 역본 목록
    private static List<string> SharedVersions { get; } = [];

    // 역본 대조 보기를 선택하기 위한 콤보박스 컬렉션
    public ObservableCollection<VersionComboBoxViewModel> BibleComboBoxes { get; }

    public MainWindowViewModel()
    {
        // 버전 목록 채우기
        var versionService = new BibleVersionService();

        var versions = versionService.GetAvailableVersions();

        foreach (var version in versions)
        {
            SearchVersions.Add(version);
            SharedVersions.Add(version);
        }

        BibleComboBoxes = new ObservableCollection<VersionComboBoxViewModel>();
        // 초기 콤보박스에 공유 역본 목록을 전달
        AddComboBox();

        // 초기 선택 값 설정
        SelectedSearchVersion = SearchVersions.FirstOrDefault();
    }

    [RelayCommand(CanExecute = nameof(CanAddComboBox))]
    private void AddComboBox()
    {
        // 새 콤보박스 뷰모델 생성 시 공유 역본 목록을 전달
        BibleComboBoxes.Add(new VersionComboBoxViewModel(SharedVersions));
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
            ThemeButtonText = "밝게";
        else
            ThemeButtonText = "어둡게";
    }

    // 글자 크기 변경
    [RelayCommand]
    private void IncreaseFontSize()
    {
        if (Application.Current is null) return;

        // "GlobalFontSize" 키를 사용하여 리소스를 찾습니다.
        if (Application.Current.Resources.TryGetValue("GlobalFontSize", out var currentSizeObj) && currentSizeObj is double currentSize)
        {
            double newSize = currentSize + 2;

            // 최대 크기 제한
            if (newSize > 30)
                newSize = 30;

            // 리소스 사전에 새 값을 다시 설정합니다. 이 순간 UI가 업데이트됩니다.
            Application.Current.Resources["GlobalFontSize"] = newSize;
        }
    }

    [RelayCommand]
    private void DecreaseFontSize()
    {
        if (Application.Current is null) return;

        // "GlobalFontSize" 키를 사용하여 리소스를 찾습니다.
        if (Application.Current.Resources.TryGetValue("GlobalFontSize", out var currentSizeObj) && currentSizeObj is double currentSize)
        {
            double newSize = currentSize - 2;

            // 최소 크기 제한
            if (newSize < 10)
                newSize = 10;

            // 리소스 사전에 새 값을 다시 설정합니다. 이 순간 UI가 업데이트됩니다.
            Application.Current.Resources["GlobalFontSize"] = newSize;
        }
    }

    // 책갈피 추가/삭제
    public ObservableCollection<BookmarkButtonViewModel> Bookmarks { get; }

    [RelayCommand]
    private async Task AddBookmark(Window owner)
    {
        var dialog = new InputDialog
        {
            DataContext = new InputDialogViewModel
            {
                Title = "책갈피 입력",
                Message = "책갈피 이름을 입력하세요."
            }
        };

        // ShowDialog<string>은 창이 닫힐 때 string 타입의 결과를 반환합니다.
        var result = await dialog.ShowDialog<string>(owner);

        //if (!string.IsNullOrWhiteSpace(result))
        //{
        //    // 책갈피 이름을 입력한 경우
        //    Bookmarks.Add(new BookmarkButtonViewModel(result));
        //}
    }

    [RelayCommand]
    private void RemoveBookmark()
    {
        //
    }
}