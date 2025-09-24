using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.ViewModels;

public partial class InputDialogViewModel : ObservableObject
{
    // [ObservableProperty] 특성을 사용하면 코드가 간결해집니다.
    [ObservableProperty]
    private string _title = "입력"; // 창 제목

    [ObservableProperty]
    private string _message = ""; // 사용자에게 보여줄 메시지

    [ObservableProperty]
    private string? _inputText; // 사용자가 입력한 텍스트가 저장될 속성
}