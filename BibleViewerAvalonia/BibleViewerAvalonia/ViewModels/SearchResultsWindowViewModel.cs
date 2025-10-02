using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BibleViewerAvalonia.ViewModels;

// SearchResultsWindow의 뷰모델이며 검색 키워드와 검색 결과 컬렉션을 가지고 있음
public partial class SearchResultsWindowViewModel : ObservableObject
{
    // 검색 키워드, Window 타이틀에 표시함  The search keyword, used for the window title.
    [ObservableProperty]
    private string _searchKeyword = "";

    // 표시할 검색 결과들을 담은 컬렉션
    public ObservableCollection<SearchResultViewModel> SearchResults { get; }

    public SearchResultsWindowViewModel(string keyword, ObservableCollection<SearchResultViewModel> results)
    {
        SearchKeyword = keyword;
        SearchResults = results;
    }

    public SearchResultsWindowViewModel()
    {
        SearchKeyword = "검색 결과";
        SearchResults = [];
    }
}