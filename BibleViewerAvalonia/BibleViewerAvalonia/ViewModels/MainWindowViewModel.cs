using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using BibleViewerAvalonia.Models;
using BibleViewerAvalonia.Service;
using BibleViewerAvalonia.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly BibleService _bibleService = new();
    private readonly BookmarkService _bookmarkService = new();
    private readonly SettingsService _settingsService = new();
    private readonly ReadingStatsService _readingStatsService = new();
    private readonly IWindowService _windowService = new WindowService();

    // 성경 전체 구조와 순서가 있는 책 목록을 저장할 필드
    private readonly Dictionary<string, int> _bookStructure;
    private readonly List<string> _bookNames;

    [ObservableProperty]
    private string _themeButtonText = "어둡게";
    [ObservableProperty]
    private double _currentFontSize = 14; // double 타입으로 변경
    [ObservableProperty]
    private string _currentThemeVariant = "Light"; // "Light" 또는 "Dark"

    // 검색 바 콤보박스를 위한 속성
    public ObservableCollection<string> SearchVersions { get; } = [];
    [ObservableProperty]
    private string _selectedSearchVersion;

    // 모든 콤보박스가 공유할 역본 목록
    private static List<string> SharedVersions { get; } = [];

    // 역본 대조 보기를 선택하기 위한 콤보박스 컬렉션
    public ObservableCollection<VersionComboBoxViewModel> BibleComboBoxes { get; }

    // 생성자 ==================================================
    public MainWindowViewModel()
    {
        // 생성자에서 성경 구조와 책 목록을 미리 로드합니다.
        _bookStructure = _bibleService.GetBookStructure();
        _bookNames = [.. _bookStructure.Keys];

        var versions = _bibleService.GetAvailableVersions(); // 버전 목록 채우기

        foreach (var version in versions)
        {
            SearchVersions.Add(version);
            SharedVersions.Add(version);
        }

        SelectedSearchVersion = SearchVersions.FirstOrDefault(); // 초기 선택 값 설정

        BibleComboBoxes = []; // BibleComboBoxes를 빈 상태로 초기화만 합니다.

        LoadBookmarks(); // 책갈피 불러오기

        LoadSettings(); // 환경 설정 불러오기

        LoadReadingStats(); // 생성자 마지막에 통계 로드 호출
    }

    // 프로그램 종료 시 호출될 메서드
    public void OnShutdown()
    {
        SaveSettings();
        SaveBookmarks();
        SaveReadingStats();
    }

    // 환경 설정 ==================================================
    private void LoadSettings()
    {
        var settings = _settingsService.LoadSettings();

        // 이전에 있던 콤보박스들을 모두 제거
        foreach (var vm in BibleComboBoxes)
        {
            vm.PropertyChanged -= ComboBoxViewModel_PropertyChanged;
        }
        BibleComboBoxes.Clear();

        // 저장된 개수만큼 콤보박스 뷰모델을 다시 생성
        for (int i = 0; i < settings.VisibleComboBoxCount; i++)
        {
            // 공유 역본 목록을 전달하여 뷰모델 생성
            var vm = new VersionComboBoxViewModel(SharedVersions)
            {
                // 저장된 역본으로 선택 값 설정
                SelectedVersion = settings.SelectedVersions[i]
            };
            // 뷰모델의 속성 변경 이벤트를 구독 (역본 변경 감지용)
            vm.PropertyChanged += ComboBoxViewModel_PropertyChanged;
            BibleComboBoxes.Add(vm);
        }

        // Add/Remove 버튼의 활성화 상태 갱신
        AddComboBoxCommand.NotifyCanExecuteChanged();
        RemoveComboBoxCommand.NotifyCanExecuteChanged();

        CurrentThemeVariant = settings.ThemeVariant;
        ThemeButtonText = (CurrentThemeVariant == "Light") ? "어둡게" : "밝게";
        CurrentFontSize = settings.CurrentFontSize;
        CurrentBook = settings.CurrentBook;
        CurrentChapter = settings.CurrentChapter;
    }

    private void SaveSettings()
    {
        var settings = new AppSettings
        {
            VisibleComboBoxCount = BibleComboBoxes.Count,
            // 현재 콤보박스들의 선택된 역본 값을 가져오고, 나머지는 빈 값으로 채워 항상 4개를 유지
            SelectedVersions = [.. BibleComboBoxes.Select(vm => vm.SelectedVersion)
                                              .Concat(Enumerable.Repeat("", 4))
                                              .Take(4)],
            ThemeVariant = this.CurrentThemeVariant,
            CurrentFontSize = this.CurrentFontSize,
            CurrentBook = this.CurrentBook,
            CurrentChapter = this.CurrentChapter
        };
        _settingsService.SaveSettings(settings);
    }

    // 역본 선택 콤보박스 ==================================================
    // 콤보박스 뷰모델의 속성이 변경될 때마다 호출되는 이벤트 핸들러
    private void ComboBoxViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 변경된 속성이 'SelectedVersion'일 경우에만 저장
        if (e.PropertyName == nameof(VersionComboBoxViewModel.SelectedVersion))
        {
            SaveSettings();
        }
    }

    // 역본 선택 콤보박스
    [RelayCommand(CanExecute = nameof(CanAddComboBox))]
    private void AddComboBox()
    {
        var vm = new VersionComboBoxViewModel(SharedVersions);
        vm.PropertyChanged += ComboBoxViewModel_PropertyChanged; // 이벤트 구독
        BibleComboBoxes.Add(vm);

        AddComboBoxCommand.NotifyCanExecuteChanged();
        RemoveComboBoxCommand.NotifyCanExecuteChanged();
        SaveSettings(); // 변경 후 저장
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
            var vm = BibleComboBoxes.Last();
            vm.PropertyChanged -= ComboBoxViewModel_PropertyChanged; // 이벤트 구독 취소 (메모리 누수 방지)
            BibleComboBoxes.Remove(vm);
        }
        AddComboBoxCommand.NotifyCanExecuteChanged();
        RemoveComboBoxCommand.NotifyCanExecuteChanged();
        SaveSettings(); // 변경 후 저장
    }

    private bool CanRemoveComboBox()
    {
        return BibleComboBoxes.Count > 1;
    }

    // 테마 변경 ==================================================
    [RelayCommand]
    private void ToggleTheme()
    {
        if (CurrentThemeVariant == "Light")
        {
            ThemeButtonText = "밝게";
            CurrentThemeVariant = "Dark";
        }
        else
        {
            ThemeButtonText = "어둡게";
            CurrentThemeVariant = "Light";
        }

        SaveSettings();
    }

    // 글자 크기 변경 ==================================================
    [RelayCommand]
    private void IncreaseFontSize()
    {
        if (CurrentFontSize < 30)
        {
            CurrentFontSize += 2;
            SaveSettings();
        }
    }

    [RelayCommand]
    private void DecreaseFontSize()
    {
        if (CurrentFontSize > 10)
        {
            CurrentFontSize -= 2;
            SaveSettings();
        }
    }

    // 책, 장 선택 및 이동 ==================================================
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

    // 책갈피 관련 기능 ==================================================
    public ObservableCollection<BookmarkButtonViewModel> Bookmarks { get; } = [];    // 책갈피 모음

    [ObservableProperty]
    private BookmarkButtonViewModel? _selectedBookmark = null;     // 선택한 책갈피

    // 책갈피 추가
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
            // Model 객체를 먼저 생성
            var newBookmarkModel = new Bookmark
            {
                BookName = CurrentBook,
                ChapterName = CurrentChapter,
                Note = result
            };

            // Model로 ViewModel을 만들어 컬렉션에 추가
            Bookmarks.Add(new BookmarkButtonViewModel(newBookmarkModel));
            SaveBookmarks(); // 변경 후 저장
        }
    }

    // 책갈피 선택
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

    // 책갈피 로드
    private void LoadBookmarks()
    {
        var models = _bookmarkService.LoadBookmarks();
        // 불러온 Model 목록으로 ViewModel을 만들어 컬렉션에 추가
        foreach (var model in models)
        {
            Bookmarks.Add(new BookmarkButtonViewModel(model));
        }
    }

    // 책갈피 저장
    private void SaveBookmarks()
    {
        // 현재 ViewModel 목록에서 Model 데이터만 추출하여 서비스에 전달
        var models = Bookmarks.Select(vm => vm.Model);
        _bookmarkService.SaveBookmarks(models);
    }

    // 책갈피 삭제
    [RelayCommand]
    private void RemoveBookmark()
    {
        if (SelectedBookmark is not null)
        {
            Bookmarks.Remove(SelectedBookmark);
            SelectedBookmark = null;
            SaveBookmarks(); // 변경 후 저장
        }
    }

    // CurrentBook 속성이 변경된 후 자동으로 호출되는 메서드
    partial void OnCurrentBookChanged(string value)
    {
        DeselectCurrentBookmark();
        SaveSettings();
        UpdateReadStatusDisplay(); // 책이 바뀌면 '읽음' 상태 업데이트
    }

    // CurrentChapter 속성이 변경된 후 자동으로 호출되는 메서드
    partial void OnCurrentChapterChanged(string value)
    {
        DeselectCurrentBookmark();
        SaveSettings();
        UpdateReadStatusDisplay(); // 장이 바뀌면 '읽음' 상태 업데이트
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

    // 읽기 통계 ==================================================
    // 불러온 전체 통계 데이터를 저장할 필드
    private ReadingStatistics _readingStats = new();

    // UI의 '읽음' 토글 버튼과 바인딩될 속성
    [ObservableProperty]
    private bool _isCurrentChapterRead;

    // 통계 로드
    private void LoadReadingStats()
    {
        _readingStats = _readingStatsService.LoadStats();
        UpdateReadStatusDisplay(); // 로드 후 현재 장의 '읽음' 상태를 UI에 반영
    }

    // 통계 저장
    private void SaveReadingStats()
    {
        _readingStatsService.SaveStats(_readingStats);
    }

    // 현재 책/장이 바뀔 때마다 호출되어 UI(토글 버튼)의 상태를 업데이트하는 메서드
    private void UpdateReadStatusDisplay()
    {
        // 데이터가 없으면 false로 초기화
        IsCurrentChapterRead = false;

        // 현재 책에 대한 통계가 있는지 확인
        if (_readingStats.Books.TryGetValue(CurrentBook, out var bookStats))
        {
            // 파싱 없이 CurrentChapter (문자열, 예: "1장")를 키로 사용해 장 통계를 찾습니다.
            if (bookStats.Chapters.TryGetValue(CurrentChapter, out var chapterStats))
            {
                // 찾은 '읽음' 상태를 UI에 바인딩된 속성에 할당
                IsCurrentChapterRead = chapterStats.IsRead;
            }
        }
    }

    // '읽음' 토글 버튼을 클릭했을 때 실행될 커맨드
    [RelayCommand]
    private void ToggleReadStatus()
    {
        // 책 데이터가 없으면 새로 생성
        if (!_readingStats.Books.ContainsKey(CurrentBook))
        {
            _readingStats.Books[CurrentBook] = new BookStats();
        }

        // 장 데이터가 없으면 새로 생성 (CurrentChapter 문자열을 키로 사용)
        if (!_readingStats.Books[CurrentBook].Chapters.ContainsKey(CurrentChapter))
        {
            _readingStats.Books[CurrentBook].Chapters[CurrentChapter] = new ChapterStats();
        }

        // 현재 '읽음' 상태를 데이터 모델에 저장 (CurrentChapter 문자열을 키로 사용)
        _readingStats.Books[CurrentBook].Chapters[CurrentChapter].IsRead = IsCurrentChapterRead;

        // 변경사항을 파일에 즉시 저장
        SaveReadingStats();
    }
}