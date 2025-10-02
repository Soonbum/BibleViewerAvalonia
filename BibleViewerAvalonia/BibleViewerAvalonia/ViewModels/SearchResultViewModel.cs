using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BibleViewerAvalonia.ViewModels;

// 결과 창에 보여줄 단일 검색 결과 항목을 의미함
public partial class SearchResultViewModel : ObservableObject
{
    // 참조 장절 (예: "창세기 1:1")
    [ObservableProperty]
    private string _reference;

    // 검색 키워드가 포함된 구절 텍스트
    [ObservableProperty]
    private string _verseText;

    public SearchResultViewModel(string reference, string verseText)
    {
        _reference = reference;
        _verseText = verseText;
    }
}