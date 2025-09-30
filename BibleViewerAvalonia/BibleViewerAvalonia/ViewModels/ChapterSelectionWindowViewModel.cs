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
    // 창을 닫아달라고 요청하는 이벤트. 선택된 장 이름을 함께 전달합니다.
    public event Action<int>? CloseRequested;

    // View(XAML)가 바인딩할 장 버튼 목록 속성
    public ObservableCollection<ChapterButtonViewModel> ChapterButtons { get; }

    public ChapterSelectionWindowViewModel()
    {
        // 디자이너에서 오류가 나지 않도록 빈 컬렉션으로 초기화합니다.
        ChapterButtons = [];
    }

    // 생성자에서 부모로부터 장 버튼 목록을 전달받음
    public ChapterSelectionWindowViewModel(ObservableCollection<ChapterButtonViewModel> chapterButtons)
    {
        ChapterButtons = chapterButtons;
    }

    // 버튼 클릭 시 실행될 커맨드
    [RelayCommand]
    private void SelectChapter(ChapterButtonViewModel? chapter)
    {
        if (chapter is not null)
        {
            // CloseRequested 이벤트를 발생시켜 View에게 창을 닫고
            // 선택된 책 이름을 반환하라고 요청합니다.
            CloseRequested?.Invoke(chapter.ChapterNumber);
        }
    }
}
