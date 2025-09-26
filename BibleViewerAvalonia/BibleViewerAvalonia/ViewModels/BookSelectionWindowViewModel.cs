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

public partial class BookSelectionWindowViewModel : ObservableObject
{
    // 창을 닫아달라고 요청하는 이벤트
    public event Action? CloseRequested;

    [ObservableProperty]
    public string _selectedBookName = "";

    // View에서 바인딩할 책 이름 목록
    public ObservableCollection<string> BookNames { get; }

    public BookSelectionWindowViewModel(BibleService bibleService)
    {
        // BibleService로부터 책 이름 목록을 가져와 ObservableCollection에 저장합니다.
        var books = bibleService.GetBookNames();
        BookNames = new ObservableCollection<string>(books);
    }

    // 디자인 타임 미리보기를 위한 기본 생성자
    public BookSelectionWindowViewModel() : this(new BibleService())
    {
    }

    // 각 책 버튼을 클릭했을 때 실행될 커맨드
    [RelayCommand]
    private void SelectBook(string? bookName)
    {
        if (bookName is null) return;

        SelectedBookName = bookName;

        // 창 닫기
        CloseRequested?.Invoke();
    }
}
