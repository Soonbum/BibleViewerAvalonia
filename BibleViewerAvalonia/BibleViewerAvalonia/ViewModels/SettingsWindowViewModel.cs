using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace BibleViewerAvalonia.ViewModels;

public partial class SettingsWindowViewModel : ObservableObject
{
    // MainWindowViewModel로부터 데이터 초기화 액션을 전달받기 위한 이벤트
    public event Action? OnClearReadStats;

    [RelayCommand]
    private void ClearReadStats()
    {
        // 이벤트 발생시켜 MainWindowViewModel에게 실제 로직 실행을 요청
        OnClearReadStats?.Invoke();
    }

    // TODO: 밑줄, 메모 지우기 기능 구현 시 아래 커맨드들 사용
    // [RelayCommand]
    // private void ClearHighlights() { }
    
    // [RelayCommand]
    // private void ClearMemos() { }
}
