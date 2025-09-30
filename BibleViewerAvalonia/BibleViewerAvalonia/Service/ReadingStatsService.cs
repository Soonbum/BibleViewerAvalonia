using BibleViewerAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace BibleViewerAvalonia.Service;

public class ReadingStatsService
{
    private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "reading_stats.json");

    // 통계 불러오기
    public ReadingStatistics LoadStats()
    {
        if (!File.Exists(_filePath))
        {
            return new ReadingStatistics(); // 파일이 없으면 빈 통계 객체 반환
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<ReadingStatistics>(json) ?? new ReadingStatistics();
        }
        catch (Exception)
        {
            return new ReadingStatistics(); // 파일이 손상되었으면 빈 객체 반환
        }
    }

    // 통계 저장하기
    public void SaveStats(ReadingStatistics stats)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(stats, options);
        File.WriteAllText(_filePath, json);
    }

    // 읽기 통계 초기화
    public void ClearAllReadStatus(ReadingStatistics stats)
    {
        // 모든 책의 모든 장을 순회하며 IsRead를 false로 설정
        foreach (var book in stats.Books.Values)
        {
            foreach (var chapter in book.Chapters.Values)
            {
                chapter.IsRead = false;
            }
        }
    }
}
