using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.Models;

public class AppSettings
{
    public List<string> SelectedVersions { get; set; } = []; // 1~4번 콤보박스에서 선택된 역본 이름을 저장 (항상 4개)
    public int VisibleComboBoxCount { get; set; } = 1; // 화면에 보이는 콤보박스의 개수

    public string ThemeVariant { get; set; } = "Light";
    public double CurrentFontSize { get; set; } = 14; // 기본 글자 크기
    public string CurrentBook { get; set; } = "창세기"; // 마지막으로 본 책
    public string CurrentChapter { get; set; } = "1장"; // 마지막으로 본 장
}
