using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.ViewModels;

public partial class ChapterButtonViewModel : ObservableObject
{
    // 장 번호 (예: 1, 2, ...)
    public int ChapterNumber { get; }

    // 장 버튼에 표시될 텍스트 (예: "1장")
    public string ChapterText => $"{ChapterNumber}장";

    // 버튼의 배경색 (읽음 여부에 따라 변경)
    [ObservableProperty]
    private IBrush _backgroundColor = Brushes.LightGray; // 기본값

    // 장 버튼을 눌렀을 때 실행될 커맨드 ( MainWindowViewModel로 전달 )
    public IRelayCommand<int> SelectChapterCommand { get; }

    public ChapterButtonViewModel(int chapterNumber, IRelayCommand<int> selectChapterCommand)
    {
        ChapterNumber = chapterNumber;
        SelectChapterCommand = selectChapterCommand;
    }
}