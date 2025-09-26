using Avalonia;
using Avalonia.Controls;
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
                // ����� CloseRequested �̺�Ʈ�� �߻��ϸ� �� â�� �ݽ��ϴ�.
                viewModel.CloseRequested += () => Close(viewModel.SelectedBookName);
            }
        };
    }
}