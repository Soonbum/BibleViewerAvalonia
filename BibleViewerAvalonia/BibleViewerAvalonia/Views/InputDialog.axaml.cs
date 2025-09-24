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

    // Ȯ�� ��ư Ŭ�� ��, ViewModel�� �Էµ� �ؽ�Ʈ�� ����� ��ȯ�ϸ� â�� ����
    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        var vm = DataContext as InputDialogViewModel;
        Close(vm?.InputText); // Close()�� ���޵� ���� ShowDialog�� ��ȯ���� ��
    }

    // ��� ��ư Ŭ�� ��, null�� ����� ��ȯ�ϸ� â�� ����
    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}