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
using System.Text.RegularExpressions;

namespace BibleViewerAvalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly BibleService _bibleService = new();
    private readonly IWindowService _windowService = new WindowService();

    // 성경 전체 구조와 순서가 있는 책 목록을 저장할 필드
    private readonly Dictionary<string, int> _bookStructure;
    private readonly List<string> _bookNames;

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
        // 생성자에서 성경 구조와 책 목록을 미리 로드합니다.
        _bookStructure = _bibleService.GetBookStructure();
        _bookNames = [.. _bookStructure.Keys];

        // 버전 목록 채우기
        var versions = _bibleService.GetAvailableVersions();

        foreach (var version in versions)
        {
            SearchVersions.Add(version);
            SharedVersions.Add(version);
        }

        BibleComboBoxes = [];
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

    // 책 버튼
    [ObservableProperty]
    private string _currentBook = "창세기"; // 현재 선택된 책

    // 장 버튼
    [ObservableProperty]
    private string _currentChapter = "1장"; // 현재 선택된 장

    // 책 버튼 클릭시
    [RelayCommand]
    private async Task ShowBookSelectionWIndow(Window owner)
    {
        var bookSelectionViewModel = new BookSelectionWindowViewModel();

        var dialog = new BookSelectionWindow
        {
            DataContext = bookSelectionViewModel
        };

        // ShowDialog<T>를 사용하여 창을 열고 T 타입의 결과를 기다립니다.
        // BookSelectionWindow의 Close(결과) 호출 시 그 결과가 여기에 반환됩니다.
        var selectedBook = await dialog.ShowDialog<string?>(owner);

        // 결과가 있고 비어있지 않다면 CurrentBook을 업데이트합니다.
        if (!string.IsNullOrEmpty(selectedBook))
        {
            CurrentBook = selectedBook;
            CurrentChapter = "1장"; // 책이 바뀌면 장을 1장으로 초기화
        }
    }

    // 장 버튼 클릭시
    [RelayCommand]
    private async Task ShowChapterSelectionWindow(Window owner)
    {
        var chapterSelectionViewModel = new ChapterSelectionWindowViewModel(_bibleService, CurrentBook);

        var dialog = new ChapterSelectionWindow
        {
            DataContext = chapterSelectionViewModel
        };

        // ShowDialog<T>를 사용하여 창을 열고 T 타입의 결과를 기다립니다.
        // ChapterSelectionWindow의 Close(결과) 호출 시 그 결과가 여기에 반환됩니다.
        var selectedChapter = await dialog.ShowDialog<string?>(owner);

        // 결과가 있고 비어있지 않다면 CurrentChapter를 업데이트합니다.
        if (!string.IsNullOrEmpty(selectedChapter))
        {
            CurrentChapter = selectedChapter;
        }
    }

    // 이전 책 (Q) 버튼 클릭시
    [RelayCommand]
    private void GoToPreviousBook()
    {
        int currentIndex = _bookNames.IndexOf(CurrentBook);
        // 첫 번째 책이면 마지막 책으로 이동 (순환)
        int previousIndex = (currentIndex == 0) ? _bookNames.Count - 1 : currentIndex - 1;

        CurrentBook = _bookNames[previousIndex];
        CurrentChapter = 1 + "장"; // 책을 옮기면 1장으로 초기화
    }

    // 이전 장 (W) 버튼 클릭시
    [RelayCommand]
    private void GoToNextBook()
    {
        int currentIndex = _bookNames.IndexOf(CurrentBook);
        // 마지막 책이면 첫 번째 책으로 이동 (순환)
        int nextIndex = (currentIndex == _bookNames.Count - 1) ? 0 : currentIndex + 1;

        CurrentBook = _bookNames[nextIndex];
        CurrentChapter = 1 + "장"; // 책을 옮기면 1장으로 초기화
    }

    // 다음 장 (E) 버튼 클릭시
    [RelayCommand]
    private void GoToPreviousChapter()
    {
        int currentChapterNumber = int.Parse(CurrentChapter.Replace("장", ""));

        if (currentChapterNumber > 1)
        {
            currentChapterNumber--;
            CurrentChapter = currentChapterNumber + "장";
        }
        else // 1장이면 이전 책의 마지막 장으로 이동
        {
            int currentIndex = _bookNames.IndexOf(CurrentBook);
            int previousIndex = (currentIndex == 0) ? _bookNames.Count - 1 : currentIndex - 1;

            CurrentBook = _bookNames[previousIndex];
            // 이전 책의 마지막 장 번호를 가져와 설정
            CurrentChapter = _bookStructure[CurrentBook] + "장";
        }
    }

    // 다음 책 (R) 버튼 클릭시
    [RelayCommand]
    private void GoToNextChapter()
    {
        int currentChapterNumber = int.Parse(CurrentChapter.Replace("장", ""));
        int totalChapters = _bookStructure[CurrentBook];

        if (currentChapterNumber < totalChapters)
        {
            currentChapterNumber++;
            CurrentChapter = currentChapterNumber + "장";
        }
        else // 마지막 장이면 다음 책의 1장으로 이동
        {
            GoToNextBook(); // 이미 만들어 둔 다음 책 이동 로직 재사용
        }
    }

    // 책갈피 추가/삭제
    public ObservableCollection<BookmarkButtonViewModel> Bookmarks { get; } = [];    // 책갈피 모음

    [ObservableProperty]
    private BookmarkButtonViewModel? _selectedBookmark = null;     // 선택한 책갈피

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
        string result = await dialog.ShowDialog<string>(owner);

        if (!string.IsNullOrWhiteSpace(result))
        {
            string bookmarkFullName = CurrentBook + " " + CurrentChapter + ": " + result;
            Bookmarks.Add(new BookmarkButtonViewModel(bookmarkFullName));
        }
    }

    [RelayCommand]
    private void SelectBookmark(BookmarkButtonViewModel bookmark)
    {
        // 이전에 선택된 북마크가 있었다면 선택 해제
        if (SelectedBookmark is not null)
        {
            SelectedBookmark.IsSelected = false;
        }

        // 현재 책, 장을 이동함
        // 정규식 패턴: (책이름) (숫자)장: (나머지)
        // 예: "창세기 1장: 텍스트"
        var pattern = new Regex(@"^(.+)\s(\d+)장:.*$");
        var match = pattern.Match(bookmark.BookmarkName);

        // 정규식 매칭에 성공하면
        if (match.Success)
        {
            // 첫 번째 그룹 (책 이름)과 두 번째 그룹 (장 번호)의 값을 가져옵니다.
            string bookName = match.Groups[1].Value.Trim();
            string chapterString = match.Groups[2].Value;

            // 장 번호를 int로 변환
            if (int.TryParse(chapterString, out int chapterNumber))
            {
                // CurrentBook과 CurrentChapter를 업데이트합니다.
                CurrentBook = bookName;
                CurrentChapter = chapterNumber + "장";
            }
        }

        // 새로 선택된 북마크를 저장하고
        SelectedBookmark = bookmark;

        // '선택됨' 상태로 변경
        SelectedBookmark.IsSelected = true;
    }

    [RelayCommand]
    private void RemoveBookmark()
    {
        if (SelectedBookmark is not null)
        {
            Bookmarks.Remove(SelectedBookmark);
            SelectedBookmark = null; // 삭제 후 선택된 북마크도 null로 초기화
        }
    }

    // CurrentBook 속성이 변경된 후 자동으로 호출되는 메서드
    partial void OnCurrentBookChanged(string value)
    {
        DeselectCurrentBookmark();
    }

    // CurrentChapter 속성이 변경된 후 자동으로 호출되는 메서드
    partial void OnCurrentChapterChanged(string value)
    {
        DeselectCurrentBookmark();
    }

    // 선택된 책갈피를 선택 해제하는 헬퍼 메서드
    private void DeselectCurrentBookmark()
    {
        if (SelectedBookmark is not null)
        {
            SelectedBookmark.IsSelected = false;
            SelectedBookmark = null;
        }
    }
}