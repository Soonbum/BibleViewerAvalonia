using Avalonia.Controls;
using BibleViewerAvalonia.ViewModels;
using BibleViewerAvalonia.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.Service;

public class WindowService : IWindowService
{
    // 뷰모델 타입과 그에 해당하는 뷰(창) 타입을 저장하는 딕셔너리
    private readonly Dictionary<Type, Type> _mappings = new();

    public WindowService()
    {
        // BookSelectionWindowViewModel이 오면 BookSelectionWindow를 열도록 등록합니다.
        Register<BookSelectionWindowViewModel, BookSelectionWindow>();
    }

    // 뷰모델과 뷰를 짝지어 등록하는 메서드
    public void Register<TViewModel, TView>() where TView : Window
    {
        _mappings[typeof(TViewModel)] = typeof(TView);
    }

    // 인터페이스의 실제 구현
    public void ShowWindow(object viewModel)
    {
        var viewModelType = viewModel.GetType();

        // 등록된 뷰가 있는지 찾습니다.
        if (!_mappings.TryGetValue(viewModelType, out var windowType))
        {
            throw new ArgumentException($"View for ViewModel '{viewModelType.FullName}' not registered.");
        }

        // 찾은 타입으로 Window 인스턴스를 생성합니다.
        var window = (Window)Activator.CreateInstance(windowType)!;

        // 새 창의 DataContext에 전달받은 뷰모델을 설정합니다.
        window.DataContext = viewModel;

        // 창을 보여줍니다.
        window.Show();
    }
}