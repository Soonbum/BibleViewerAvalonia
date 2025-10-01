using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.ViewModels;

public partial class VersionComboBoxViewModel : ObservableObject
{
    // 이 콤보박스에 표시될 역본 목록
    public ObservableCollection<string> Versions { get; }

    [ObservableProperty]
    private string _selectedVersion; // 선택한 역본명

    public ObservableCollection<string> Verses { get; } = []; // 성경 본문 텍스트

    public VersionComboBoxViewModel(List<string> sharedVersions)
    {
        Versions = new ObservableCollection<string>(sharedVersions);
        _selectedVersion = Versions.FirstOrDefault() ?? "";
    }
}