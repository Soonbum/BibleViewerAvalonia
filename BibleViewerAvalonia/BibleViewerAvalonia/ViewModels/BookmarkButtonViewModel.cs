using BibleViewerAvalonia.Models;
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
    // 순수 데이터 모델
    public Bookmark Model { get; }

    // 화면에 표시될 이름 (Model 데이터를 기반으로 생성)
    public string BookmarkName => $"{Model.BookName} {Model.ChapterName}: {Model.Note}";

    // 책갈피 버튼 선택 여부
    [ObservableProperty]
    private bool _isSelected;

    public BookmarkButtonViewModel(Bookmark model)
    {
        Model = model;
    }
}
