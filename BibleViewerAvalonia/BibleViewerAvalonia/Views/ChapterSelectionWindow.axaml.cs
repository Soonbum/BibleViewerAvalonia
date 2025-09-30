using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using BibleViewerAvalonia.ViewModels;

namespace BibleViewerAvalonia.Views;

public partial class ChapterSelectionWindow : Window
{
    public ChapterSelectionWindow()
    {
        InitializeComponent();

        // DataContext가 설정된 후 이벤트를 구독합니다.
        this.DataContextChanged += (sender, e) =>
        {
            if (DataContext is ChapterSelectionWindowViewModel viewModel)
            {
                // 뷰모델의 CloseRequested 이벤트가 발생하면,
                // 전달받은 장 번호(chapterNumber)을 결과로 반환하며 이 창을 닫습니다.
                viewModel.CloseRequested += (chapterNumber) => this.Close(chapterNumber);
            }
        };

    }
}