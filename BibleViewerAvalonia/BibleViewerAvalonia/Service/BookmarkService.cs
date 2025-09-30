using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibleViewerAvalonia.Models;
using System.IO;
using System.Text.Json;

namespace BibleViewerAvalonia.Service;

public class BookmarkService
{
    // 저장될 파일 경로 (실행 파일과 동일한 위치에 bookmarks.json으로 저장)
    private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "bookmarks.json");

    // 파일에서 책갈피 목록을 불러오는 메서드
    public List<Bookmark> LoadBookmarks()
    {
        if (!File.Exists(_filePath))
        {
            return []; // 파일이 없으면 빈 목록 반환
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Bookmark>>(json) ?? [];
        }
        catch (Exception)
        {
            // JSON 파일이 손상되었을 경우를 대비
            return [];
        }
    }

    // 책갈피 목록을 파일에 저장하는 메서드
    public void SaveBookmarks(IEnumerable<Bookmark> bookmarks)
    {
        var options = new JsonSerializerOptions { WriteIndented = true }; // JSON을 예쁘게 포맷팅
        string json = JsonSerializer.Serialize(bookmarks, options);
        File.WriteAllText(_filePath, json);
    }
}
