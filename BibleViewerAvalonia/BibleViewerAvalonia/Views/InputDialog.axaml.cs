using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BibleViewerAvalonia.ViewModels;

namespace BibleViewerAvalonia.Views;

public partial class InputDialog : Window
{
    public InputDialog()
    {
        InitializeComponent();
    }

    // 확인 버튼 클릭 시, ViewModel의 입력된 텍스트를 결과로 반환하며 창을 닫음
    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        var vm = DataContext as InputDialogViewModel;
        Close(vm?.InputText); // Close()에 전달된 값이 ShowDialog의 반환값이 됨
    }

    // 취소 버튼 클릭 시, null을 결과로 반환하며 창을 닫음
    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}