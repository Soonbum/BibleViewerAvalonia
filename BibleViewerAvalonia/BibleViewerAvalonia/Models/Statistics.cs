using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.Models;

/// <summary>
/// 사용자의 전체 성경 읽기 통계 데이터를 관리하는 최상위 클래스입니다.
/// </summary>
public class ReadingStatistics
{
    /// <summary>
    /// 책 이름(예: "창세기")을 키로 사용하는 책별 통계 데이터입니다.
    /// </summary>
    public Dictionary<string, BookStats> Books { get; set; } = [];
}

/// <summary>
/// 특정 성경 책 한 권에 대한 통계 데이터를 관리합니다.
/// </summary>
public class BookStats
{
    /// <summary>
    /// 장 번호를 키로 사용하는 장별 통계 데이터입니다.
    /// </summary>
    public Dictionary<string, ChapterStats> Chapters { get; set; } = [];
}

/// <summary>
/// 특정 장에 대한 통계 데이터를 관리합니다.
/// </summary>
public class ChapterStats
{
    /// <summary>
    /// 이 장을 읽었는지 여부를 나타냅니다.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// 절 번호를 키로 사용하는 절별 통계 데이터입니다.
    /// </summary>
    public Dictionary<int, VerseStats> Verses { get; set; } = [];
}

/// <summary>
/// 특정 절에 대한 통계 데이터를 관리합니다.
/// </summary>
public class VerseStats
{
    /// <summary>
    /// 이 절에 밑줄을 그었는지 여부를 나타냅니다.
    /// </summary>
    public bool IsHighlighted { get; set; }

    /// <summary>
    /// 이 절에 남긴 메모입니다. 메모가 없으면 null 또는 빈 문자열입니다.
    /// </summary>
    public string? Memo { get; set; }
}