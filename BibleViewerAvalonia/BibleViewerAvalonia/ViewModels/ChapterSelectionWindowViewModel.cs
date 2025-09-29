using BibleViewerAvalonia.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.ViewModels;

public partial class ChapterSelectionWindowViewModel : ObservableObject
{
    // 창을 닫아달라고 요청하는 이벤트
    public event Action? CloseRequested;

    [ObservableProperty]
    public string _selectedChapterName = "";

    // View에서 바인딩할 장 이름 목록 (1장, 2장, 3장, ...)
    public ObservableCollection<string> ChapterNames { get; }

    public ChapterSelectionWindowViewModel(BibleService bibleService, string bookName)
    {
        var bookStructure = bibleService.GetBookStructure();
        if (bookStructure.TryGetValue(bookName, out int chapterCount))
            ChapterNames = new ObservableCollection<string>(Enumerable.Range(1, chapterCount).Select(n => $"{n}장"));
        else
            ChapterNames = [];
    }

    // 디자인 타임 미리보기를 위한 기본 생성자
    public ChapterSelectionWindowViewModel() : this(new BibleService(), "창세기")
    {
    }

    // 각 장 버튼을 클릭했을 때 실행될 커맨드
    [RelayCommand]
    private void SelectChapter(string chapterName)
    {
        SelectedChapterName = chapterName;

        // 창 닫기
        CloseRequested?.Invoke();
    }
}
