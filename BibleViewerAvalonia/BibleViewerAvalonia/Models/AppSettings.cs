using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.Models;

public class AppSettings
{
    // 1~4번 콤보박스에서 선택된 역본 이름을 저장 (항상 4개)
    public List<string> SelectedVersions { get; set; } = [];

    // 화면에 보이는 콤보박스의 개수
    public int VisibleComboBoxCount { get; set; } = 1;
}
