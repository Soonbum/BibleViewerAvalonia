using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using BibleViewerAvalonia.ViewModels;

namespace BibleViewerAvalonia.Views;

public partial class BookSelectionWindow : Window
{
    public BookSelectionWindow()
    {
        InitializeComponent();

        // DataContext�� ������ �� �̺�Ʈ�� �����մϴ�.
        this.DataContextChanged += (sender, e) =>
        {
            if (DataContext is BookSelectionWindowViewModel viewModel)
            {
                // ����� CloseRequested �̺�Ʈ�� �߻��ϸ�,
                // ���޹��� å �̸�(bookName)�� ����� ��ȯ�ϸ� �� â�� �ݽ��ϴ�.
                viewModel.CloseRequested += (bookName) => this.Close(bookName);
            }
        };
    }
}