using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.ViewModels;

public partial class BookButtonViewModel : ObservableObject
{
    // 책 이름 (예: "창세기")
    public string BookName { get; }

    // 버튼의 배경색 (모든 장 읽음 여부에 따라 변경)
    [ObservableProperty]
    private IBrush _backgroundColor = Brushes.LightGray; // 기본값 (변경해도 적용 안되고 있음)

    // 책 버튼을 눌렀을 때 실행될 커맨드 ( MainWindowViewModel로 전달 )
    public IRelayCommand<string> SelectBookCommand { get; }

    public BookButtonViewModel(string bookName, IRelayCommand<string> selectBookCommand)
    {
        BookName = bookName;
        SelectBookCommand = selectBookCommand;
    }
}