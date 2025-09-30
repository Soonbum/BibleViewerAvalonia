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
}
