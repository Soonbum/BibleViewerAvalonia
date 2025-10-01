using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

    // 장 선택 창에 표시될 장 버튼 ViewModel 목록
    public ObservableCollection<ChapterButtonViewModel> ChapterButtons { get; } = [];

    // 책 선택 창에 표시될 책 버튼 ViewModel 목록
    public ObservableCollection<BookButtonViewModel> BookButtons { get; } = [];

    // 생성자 ==================================================
    public MainWindowViewModel()
    {
        // 생성자에서 성경 구조와 책 목록을 미리 로드합니다.
        _bookStructure = _bibleService.GetBookStructure();
        _bookNames = [.. _bookStructure.Keys];

        var versions = _bibleService.VersionInfo.Keys; // 버전 목록 채우기

        foreach (var version in versions)
        {
            SearchVersions.Add(version);
            SharedVersions.Add(version);
        }

        SelectedSearchVersion = SearchVersions.FirstOrDefault(); // 초기 선택 값 설정

        BibleComboBoxes = []; // BibleComboBoxes를 빈 상태로 초기화만 합니다.

        LoadBookmarks(); // 책갈피 불러오기

        LoadSettings(); // 환경 설정 불러오기

        _ = LoadAllBibleTextsAsync(); // 본문 로드

        LoadReadingStats(); // 생성자 마지막에 통계 로드 호출

        // 앱 시작 시 책과 장 버튼 초기화
        InitializeBookButtons();
        UpdateChapterButtons(); // 현재 책에 따라 장 버튼 업데이트
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
            _ = LoadAllBibleTextsAsync(); // 본문 로드
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
        _ = LoadAllBibleTextsAsync(); // 본문 로드
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

    // 절 범위 버튼
    [ObservableProperty]
    private string _verseRange = "1-31절"; // 절 범위 표시 텍스트

    // --- 책 선택 커맨드 ---
    [RelayCommand]
    private void SelectBook(string bookName)
    {
        CurrentBook = bookName; // CurrentBook 변경 시 OnCurrentBookChanged가 자동으로 호출됨
        CurrentChapter = "1장"; // 책이 바뀌면 장을 1장으로 초기화
    }

    // --- 장 선택 커맨드 ---
    [RelayCommand]
    private void SelectChapter(int chapterNumber)
    {
        CurrentChapter = chapterNumber.ToString() + "장"; // CurrentChapter 변경 시 OnCurrentChapterChanged가 자동으로 호출됨
    }

    // 책 버튼 클릭시
    [RelayCommand]
    private async Task ShowBookSelectionWIndow(Window owner)
    {
        //var bookSelectionViewModel = new BookSelectionWindowViewModel();

        // ViewModel 생성 시 this.BookButtons를 전달합니다.
        var bookSelectionViewModel = new BookSelectionWindowViewModel(this.BookButtons);

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
        var chapterSelectionViewModel = new ChapterSelectionWindowViewModel(this.ChapterButtons);
        var dialog = new ChapterSelectionWindow
        {
            DataContext = chapterSelectionViewModel
        };

        // 반환 타입을 string?에서 int?로 수정합니다.
        var selectedChapterNumber = await dialog.ShowDialog<int?>(owner);

        // 결과가 있으면 (null이 아니면) CurrentChapter를 업데이트합니다.
        if (selectedChapterNumber.HasValue)
        {
            CurrentChapter = $"{selectedChapterNumber.Value}장";
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
        Regex pattern = new(@"^(.+)\s(\d+)장:.*$");
        Match match = pattern.Match(bookmark.BookmarkName);

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
        UpdateChapterButtons(); // 현재 책이 바뀌면 장 버튼 목록 업데이트 및 색상 반영
        UpdateBookButtonColor(value); // 현재 책의 버튼 색상 업데이트
        _ = LoadAllBibleTextsAsync(); // 본문 로드
    }

    // CurrentChapter 속성이 변경된 후 자동으로 호출되는 메서드
    partial void OnCurrentChapterChanged(string value)
    {
        DeselectCurrentBookmark();
        SaveSettings();
        UpdateReadStatusDisplay(); // 장이 바뀌면 '읽음' 상태 업데이트
        UpdateChapterButtonColor(CurrentBook, value.ToString()); // 현재 장의 버튼 색상만 업데이트
        UpdateBookButtonColor(CurrentBook); // 현재 책의 버튼 색상도 업데이트 (장이 바뀌면 책도 바뀔 수 있으니)
        _ = LoadAllBibleTextsAsync(); // 본문 로드
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
        IsCurrentChapterRead = false;
        if (_readingStats.Books.TryGetValue(CurrentBook, out var bookStats))
        {
            // "1장" 형태의 문자열 키를 그대로 사용
            if (bookStats.Chapters.TryGetValue(CurrentChapter, out var chapterStats))
            {
                IsCurrentChapterRead = chapterStats.IsRead;
            }
        }
    }

    // '읽음' 토글 버튼을 클릭했을 때 실행될 커맨드
    [RelayCommand]
    private void ToggleReadStatus()
    {
        if (!_readingStats.Books.ContainsKey(CurrentBook))
        {
            _readingStats.Books[CurrentBook] = new BookStats();
        }
        if (!_readingStats.Books[CurrentBook].Chapters.ContainsKey(CurrentChapter))
        {
            _readingStats.Books[CurrentBook].Chapters[CurrentChapter] = new ChapterStats();
        }
        _readingStats.Books[CurrentBook].Chapters[CurrentChapter].IsRead = IsCurrentChapterRead;

        // --- 저장 및 UI 업데이트 ---
        SaveReadingStats();
        UpdateChapterButtonColor(CurrentBook, CurrentChapter); // 파싱 없이 CurrentChapter 전달
        UpdateBookButtonColor(CurrentBook);
    }

    // 읽기 통계에 따른 색상 표시 ==================================================
    // 앱 시작 시 또는 통계 로드 시 모든 책 버튼 ViewModel을 초기화하고 색상을 설정합니다.
    private void InitializeBookButtons()
    {
        BookButtons.Clear();
        foreach (var bookName in _bookNames) // _bookNames는 이미 모든 책 이름을 가지고 있습니다.
        {
            var bookButton = new BookButtonViewModel(bookName, SelectBookCommand); // SelectBookCommand는 아직 없습니다. 나중에 추가합니다.
            BookButtons.Add(bookButton);
            UpdateBookButtonColor(bookName); // 각 책의 초기 색상 설정
        }
    }

    // 현재 CurrentBook에 해당하는 장 버튼 ViewModel 목록을 생성하고 색상을 설정합니다.
    private void UpdateChapterButtons()
    {
        ChapterButtons.Clear();
        if (_bookStructure.TryGetValue(CurrentBook, out int totalChapters))
        {
            for (int i = 1; i <= totalChapters; i++)
            {
                var chapterButton = new ChapterButtonViewModel(i, SelectChapterCommand);
                ChapterButtons.Add(chapterButton);

                // i.ToString() 대신, 키 형식에 맞는 "{i}장" 형태로 전달합니다.
                UpdateChapterButtonColor(CurrentBook, $"{i}장");
            }
        }
    }

    // 특정 책 버튼의 색상을 업데이트합니다.
    private void UpdateBookButtonColor(string bookName)
    {
        var bookButton = BookButtons.FirstOrDefault(b => b.BookName == bookName);
        if (bookButton is null) return;

        // 현재 테마를 가져옵니다.
        var currentTheme = Application.Current?.RequestedThemeVariant;

        if (!_readingStats.Books.TryGetValue(bookName, out var bookStats))
        {
            bookButton.BackgroundColor = currentTheme == ThemeVariant.Dark ? Brushes.DarkSlateGray : Brushes.LightGray;
            return;
        }

        if (!_bookStructure.TryGetValue(bookName, out int totalChapters))
        {
            bookButton.BackgroundColor = currentTheme == ThemeVariant.Dark ? Brushes.DarkSlateGray : Brushes.LightGray;
            return;
        }

        int readChaptersCount = 0;
        for (int i = 1; i <= totalChapters; i++)
        {
            // 키를 "1장", "2장" 형태로 만들어서 조회
            string chapterKey = $"{i}장";
            if (bookStats.Chapters.TryGetValue(chapterKey, out var chapterStats) && chapterStats.IsRead)
            {
                readChaptersCount++;
            }
        }

        if (readChaptersCount == totalChapters && totalChapters > 0)
        {
            // 모든 장 읽음: 테마에 따라 다른 녹색 계열 설정
            bookButton.BackgroundColor = currentTheme == ThemeVariant.Dark ? Brushes.DarkGreen : Brushes.LightGreen;
        }
        else if (readChaptersCount > 0)
        {
            // 일부 장 읽음: 테마에 따라 다른 파란색 계열 설정
            bookButton.BackgroundColor = currentTheme == ThemeVariant.Dark ? Brushes.DarkSlateBlue : Brushes.LightBlue;
        }
        else
        {
            // 읽지 않음: 테마에 따라 기본 색상 설정
            bookButton.BackgroundColor = currentTheme == ThemeVariant.Dark ? Brushes.DarkSlateGray : Brushes.LightGray;
        }
    }

    // 특정 장 버튼의 색상을 업데이트합니다. (CurrentChapter는 '1장' 형태이므로 int로 변환 필요)
    private void UpdateChapterButtonColor(string bookName, string chapterString)
    {
        if (!int.TryParse(chapterString.Replace("장", ""), out int chapterNumber))
            return;

        var chapterButton = ChapterButtons.FirstOrDefault(c => c.ChapterNumber == chapterNumber);
        if (chapterButton is null) return;

        // 현재 테마를 가져옵니다.
        var currentTheme = Application.Current?.RequestedThemeVariant;

        // 키를 chapterString("1장") 그대로 사용
        if (_readingStats.Books.TryGetValue(bookName, out var bookStats) &&
            bookStats.Chapters.TryGetValue(chapterString, out var chapterStats) &&
            chapterStats.IsRead)
        {
            // 읽음 상태일 때 테마별 색상
            chapterButton.BackgroundColor = currentTheme == ThemeVariant.Dark ? Brushes.DarkGreen : Brushes.LightGreen;
        }
        else
        {
            // 읽지 않은 상태일 때 테마별 색상
            chapterButton.BackgroundColor = currentTheme == ThemeVariant.Dark ? Brushes.DarkSlateGray : Brushes.LightGray;
        }
    }

    // 설정 ==================================================
    // 설정 창 열기 커맨드
    [RelayCommand]
    private async Task ShowSettingsWindow(Window owner)
    {
        var settingsViewModel = new SettingsWindowViewModel();

        // 설정 창의 '지우기' 버튼이 눌리면 실행될 로직을 정의하고 이벤트에 연결
        settingsViewModel.OnClearReadStats += () =>
        {
            // 서비스의 초기화 메서드 호출
            _readingStatsService.ClearAllReadStatus(_readingStats);

            // 변경된 내용을 파일에 즉시 저장
            SaveReadingStats();

            // UI(버튼 색상 등)를 즉시 갱신
            RefreshUiAfterStatsChange();
        };

        var dialog = new SettingsWindow
        {
            DataContext = settingsViewModel
        };

        await dialog.ShowDialog(owner);
    }

    // 통계 변경 후 UI를 새로고침하는 헬퍼 메서드
    private void RefreshUiAfterStatsChange()
    {
        // 현재 장의 읽음 상태 토글 버튼 갱신
        UpdateReadStatusDisplay();

        // 책/장 선택 창의 모든 버튼 색상 갱신
        foreach (var bookName in _bookNames)
        {
            UpdateBookButtonColor(bookName);
        }
        UpdateChapterButtons(); // 현재 책의 장 버튼들 색상 갱신
    }

    // 본문 처리 ==================================================
    // 본문 로드
    private async Task LoadAllBibleTextsAsync()
    {
        if (!int.TryParse(CurrentChapter.Replace("장", ""), out int chapterNumber))
        {
            return;
        }

        int verseMaxNum = 0;

        foreach (var comboVm in BibleComboBoxes)
        {
            if (string.IsNullOrEmpty(comboVm.SelectedVersion))
            {
                comboVm.Verses.Clear(); // 내용 지우기
                continue;
            }

            // BibleService로부터 string[] 배열을 가져옵니다.
            string[] versesArray = await GetChapterText(comboVm.SelectedVersion, CurrentBook, chapterNumber);

            verseMaxNum = versesArray.Length;

            // 기존 내용을 지우고 새로 가져온 절들로 컬렉션을 채웁니다.
            comboVm.Verses.Clear();
            foreach (var verse in versesArray)
            {
                comboVm.Verses.Add(verse);
            }
        }

        // 절 버튼의 텍스트를 업데이트
        VerseRange = $"1-{verseMaxNum}절";
    }

    // 파일 읽기 함수
    private async Task<string[]> GetChapterText(string versionName, string bookName, int chapterNumber)
    {
        // versionInfo.versionName --> korHKJV 등
        // versionInfo.fileType --> lfb 또는 bdf

        /*
            텍스트 파일의 특징은 다음과 같습니다.
              1) lfb 파일 구조는 다음과 같음
                - lfb 파일의 패턴: "korHKJV1_1.lfb 부터 korHKJV66_22.lfb 까지"
                - 역본명66_2.lfb --> 66권 2장이라는 뜻
                - "66계 2:1 너는 에베소에 있는 교회의 사자에게 이렇게 써라. "오른손에 일곱 별을 붙잡고 일곱 금촛대 사이로 거니시는 분께서 이와 같이 말씀하신다." --> 라인 하나에 권 번호, 책 이름, 장:절 본문 구조로 되어 있음, 공백만 있는 라인은 무시할 것

              2) bdf 파일 구조는 다음과 같음
                - bdf 파일의 패턴: "korKKJV1.pdf 부터 korKKJV7.pdf 까지"
                - 장별로 구분되어 있지 않음
                - "01창 1:1 <세계의 시작> 태초에 하나님께서 하늘과 땅을 창조하셨습니다." --> lfa와 동일함, 간혹 권 번호만 있는 라인도 있음, 공백만 있는 라인은 무시할 것
         */

        if (!_bibleService.VersionInfo.TryGetValue(versionName, out var versionInfo))
        {
            return ["오류: 해당 역본을 찾을 수 없습니다."];
        }

        // 프로그램의 루트 경로
        string baseDirectory = AppContext.BaseDirectory;

        // 책 이름으로 책 번호(순서)를 알아냄
        int bookNumber = _bibleService.GetBookStructure().Keys.ToList().IndexOf(bookName) + 1;

        if (versionInfo.fileType == "lfb")
        {
            string fileName = $"{versionInfo.versionName}{bookNumber:D2}_{chapterNumber}.lfb";
            string filePath = Path.Combine(baseDirectory, "Text", versionInfo.versionName, fileName);

            if (File.Exists(filePath))
            {
                string[] allLines = await File.ReadAllLinesAsync(filePath);

                var chapterLines = allLines
                    // 비어 있거나 공백만 있는 줄을 제거하는 Where 필터를 추가합니다.
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    // 내용이 있는 줄에 대해서만 접두사를 제거하는 Select를 실행합니다.
                    .Select(line =>
                    {
                        // 첫 번째 공백 문자의 인덱스를 찾아 그 다음 문자부터 끝까지 잘라냅니다.
                        int firstSpaceIndex = line.IndexOf(' ');
                        if (firstSpaceIndex != -1)
                        {
                            return line.Substring(firstSpaceIndex + 1);
                        }
                        return line;
                    }).ToArray();

                return chapterLines;
            }
            return [$"오류: {fileName} 파일을 찾을 수 없습니다."];
        }
        else if (versionInfo.fileType == "bdf")
        {
            // 책 약어 (예: "창" 또는 "Gn")
            string bookAbbreviation = _bibleService.GetBookAbbreviation(versionInfo.versionName, bookName);

            for (int i = 1; i <= 7; i++)
            {
                string fileName = $"{versionInfo.versionName}{i}.bdf";
                string filePath = Path.Combine(baseDirectory, "Text", versionInfo.versionName, fileName);

                if (File.Exists(filePath))
                {
                    string[] allLines = await File.ReadAllLinesAsync(filePath);

                    // 패턴 예시: "05신 1:"
                    Regex pattern = new($@"^{bookNumber:D2}\w+\s+{chapterNumber}:");

                    // Regex의 IsMatch 메서드를 사용해 패턴과 일치하는 줄을 찾습니다.
                    allLines = [.. allLines.Where(line => pattern.IsMatch(line))];

                    var chapterLines = allLines
                    // 비어 있거나 공백만 있는 줄을 제거하는 Where 필터를 추가합니다.
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    // 내용이 있는 줄에 대해서만 접두사를 제거하는 Select를 실행합니다.
                    .Select(line =>
                    {
                        // 첫 번째 공백 문자의 인덱스를 찾아 그 다음 문자부터 끝까지 잘라냅니다.
                        int firstSpaceIndex = line.IndexOf(' ');
                        if (firstSpaceIndex != -1)
                        {
                            return line.Substring(firstSpaceIndex + 1);
                        }
                        return line;
                    }).ToArray();

                    if (chapterLines.Length > 0)
                        return chapterLines;
                }
            }
            return ["오류: BDF 파일에서 내용을 찾을 수 없습니다."];
        }

        return [$"오류: 내용을 찾을 수 없습니다."];
    }
}