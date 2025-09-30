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

        // DataContext�� ������ �� �̺�Ʈ�� �����մϴ�.
        this.DataContextChanged += (sender, e) =>
        {
            if (DataContext is ChapterSelectionWindowViewModel viewModel)
            {
                // ����� CloseRequested �̺�Ʈ�� �߻��ϸ�,
                // ���޹��� �� ��ȣ(chapterNumber)�� ����� ��ȯ�ϸ� �� â�� �ݽ��ϴ�.
                viewModel.CloseRequested += (chapterNumber) => this.Close(chapterNumber);
            }
        };

    }
}