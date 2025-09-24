using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.Models;

public partial class BibleVersionService
{
    // 성경 버전 목록을 반환하는 메서드
    public IEnumerable<string> GetAvailableVersions()
    {
        // 나중에 이 부분은 파일이나 데이터베이스에서 데이터를 읽어오도록 수정될 수 있습니다.
        return new List<string>
        {
            "킹제임스흠정역",
            "개역한글판",
        };
    }
}