using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.ViewModels;

public partial class BookmarkButtonViewModel : ObservableObject
{
    // 이 버튼에 표시될 책갈피 이름
    [ObservableProperty]
    public string _bookmarkName;

    public BookmarkButtonViewModel(string text)
    {
        BookmarkName = text;
    }
}
