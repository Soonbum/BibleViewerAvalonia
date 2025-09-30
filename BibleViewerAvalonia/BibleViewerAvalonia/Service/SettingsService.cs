using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibleViewerAvalonia.Models;
using System.IO;
using System.Text.Json;

namespace BibleViewerAvalonia.Service;

public class SettingsService
{
    private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "settings.json");

    // 설정 불러오기
    public AppSettings LoadSettings()
    {
        if (!File.Exists(_filePath))
        {
            // 파일이 없으면 기본값으로 새 설정 객체를 반환
            return new AppSettings
            {
                SelectedVersions = ["킹제임스흠정역", "", "", ""],
                VisibleComboBoxCount = 1
            };
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch (Exception)
        {
            // 파일이 손상되었으면 기본값 반환
            return new AppSettings();
        }
    }

    // 설정 저장하기
    public void SaveSettings(AppSettings settings)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(_filePath, json);
    }
}
