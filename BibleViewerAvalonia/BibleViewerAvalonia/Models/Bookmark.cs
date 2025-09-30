using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.Models;

public class Bookmark
{
    // 저장될 데이터: 책 이름, 장, 사용자 메모
    public string BookName { get; set; } = string.Empty;
    public string ChapterName { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}