using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.ViewModels;

public partial class ComboBoxViewModel : ObservableObject
{
    // 이 콤보박스에 표시될 역본 목록
    public ObservableCollection<string> Versions { get; }

    [ObservableProperty]
    private string _selectedVersion;

    public ComboBoxViewModel(List<string> versions, string defaultSelection = null)
    {
        Versions = new ObservableCollection<string>(versions);

        // 전달받은 값이 있으면 그 값으로, 없으면 첫 번째 값으로 설정합니다.
        if (!string.IsNullOrEmpty(defaultSelection) && Versions.Contains(defaultSelection))
        {
            SelectedVersion = defaultSelection;
        }
        else
        {
            SelectedVersion = Versions.FirstOrDefault();
        }
    }
}