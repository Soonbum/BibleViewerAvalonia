using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.Service;

public partial class BibleService
{
    // 성경 버전 목록을 반환하는 메서드
    public IEnumerable<string> GetAvailableVersions()
    {
        // 나중에 이 부분은 파일이나 데이터베이스에서 데이터를 읽어오도록 수정될 수 있습니다.
        return
        [
            "킹제임스흠정역", // korhkjv (LFA 버전)
            "개역한글", // korhrv (LFA 버전)
            "개역개정", // kornkrv (BDF 버전)
            "바른성경", // korktv (LFA 버전)
            "쉬운성경", // koreasy (BDF 버전)
            "한글킹제임스", // korkkjv (BDF 버전)
            "현대어성경", // kortkv (BDF 버전)
            "현대인의 성경", // korKLB (BDF 버전)
            "새번역", // korNRSV (BDF 버전)
            "King James Version", // engkjv (LFA 버전)
            "English Standard Version", // engESV (BDF 버전)
            "Good News Translation", // engGNT (BDF 버전)
            "Holman Christian Standard Bible", // engHCSB (BDF 버전)
            "International Standard Version", // engISV (BDF 버전)
            "New American Standard Bible", // engNASB (BDF 버전)
            "New International Version", // engNIV (BDF 버전)
            "New Living Translation", // engNLT (BDF 버전)
        ];

        // 텍스트 파일의 특징은 다음과 같습니다.
        /*
            1) lfa -> zip으로 변경 후 압축을 풀면 다음과 같다.
              - 역본명66_2.lfb --> 66권 2장이라는 뜻
              - "66계 2:1 너는 에베소에 있는 교회의 사자에게 이렇게 써라. "오른손에 일곱 별을 붙잡고 일곱 금촛대 사이로 거니시는 분께서 이와 같이 말씀하신다." --> 라인 하나에 권 번호, 책 이름, 장:절 본문 구조로 되어 있음, 공백만 있는 라인은 무시할 것

            2) bdf 파일 구조는 다음과 같다.
              - 장별로 구분되어 있지 않음
              - "01창 1:1 <세계의 시작> 태초에 하나님께서 하늘과 땅을 창조하셨습니다." --> lfa와 동일함, 간혹 권 번호만 있는 라인도 있음, 공백만 있는 라인은 무시할 것
         */
    }

    // 성경 책별 장, 절 수를 반환하는 메서드
    public Dictionary<string, int> GetBookStructure()
    {
        return new Dictionary<string, int>
        {
            { "창세기", 50 },
            { "출애굽기", 40 },
            { "레위기", 27 },
            { "민수기", 36 },
            { "신명기", 34 },
            { "여호수아", 21 },
            { "사사기", 21 },
            { "룻기", 4 },
            { "사무엘상", 31 },
            { "사무엘하", 24 },
            { "열왕기상", 22 },
            { "열왕기하", 25 },
            { "역대상", 29 },
            { "역대하", 36 },
            { "에스라", 10 },
            { "느헤미야", 13 },
            { "에스더", 10 },
            { "욥기", 42 },
            { "시편", 150 },
            { "잠언", 31 },
            { "전도서", 12 },
            { "아가", 8 },
            { "이사야", 66 },
            { "예레미야", 52 },
            { "예레미야애가", 5 },
            { "에스겔", 48 },
            { "다니엘", 12 },
            { "호세아", 14 },
            { "요엘", 3 },
            { "아모스", 9 },
            { "오바댜", 1 },
            { "요나", 4 },
            { "미가", 7 },
            { "나훔", 3 },
            { "하박국", 3 },
            { "스바냐", 3 },
            { "학개", 2 },
            { "스가랴", 14 },
            { "말라기", 4 },

            { "마태복음", 28 },
            { "마가복음", 16 },
            { "누가복음", 24 },
            { "요한복음", 21 },
            { "사도행전", 28 },
            { "로마서", 16 },
            { "고린도전서", 16 },
            { "고린도후서", 13 },
            { "갈라디아서", 6 },
            { "에베소서", 6 },
            { "빌립보서", 4 },
            { "골로새서", 4 },
            { "데살로니가전서", 5 },
            { "데살로니가후서", 3 },
            { "디모데전서", 6 },
            { "디모데후서", 4 },
            { "디도서", 3 },
            { "빌레몬서", 1 },
            { "히브리서", 13 },
            { "야고보서", 5 },
            { "베드로전서", 5 },
            { "베드로후서", 3 },
            { "요한일서", 5 },
            { "요한이서", 1 },
            { "요한삼서", 1 },
            { "유다서", 1 },
            { "요한계시록", 22 },
        };
    }

    // 책 이름 목록만 간단히 가져오는 메서드
    public IEnumerable<string> GetBookNames()
    {
        return GetBookStructure().Keys;
    }
}