using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleViewerAvalonia.Service;

public partial class BibleService
{
    // 성경 버전 정보 (역본 정식 명칭, 역본 약어 명칭, 파일 형식)
    public readonly Dictionary<string, (string versionName, string fileType)> VersionInfo = new()
    {
        { "킹제임스흠정역", ("korHKJV", "lfb") },
        { "개역한글", ("korHRV", "lfb") },
        { "개역개정", ("korNKRV", "bdf") },
        { "바른성경", ("korKTV", "lfb") },
        { "쉬운성경", ("korEasy", "bdf") },
        { "한글킹제임스", ("korKKJV", "bdf") },
        { "현대어성경", ("korTKV", "bdf") },
        { "현대인의 성경", ("korKLB", "bdf") },
        { "King James Version", ("engKJV", "lfb") },
        { "English Standard Version", ("engESV", "bdf") },
        { "Good News Translation", ("engGNT", "bdf") },
        { "Holman Christian Standard Bible", ("engHCSB", "bdf") },
        { "International Standard Version", ("engISV", "bdf") },
        { "New American Standard Bible", ("engNASB", "bdf") },
        { "New International Version", ("engNIV", "bdf") },
        { "New Revised Standard Version", ("engNRSV", "bdf") },
        { "New Living Translation", ("engNLT", "bdf") },
    };

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

    // 성경 책 이름과 약어 매핑
    public string GetBookAbbreviation(string versionAbbrName, string bookName)
    {
        int bookIndex = GetBookStructure().Keys.ToList().IndexOf(bookName); // 0부터 시작

        string[] BookAbbreviationEnglish = ["Gn", "Ex", "Lv", "Nm", "Dt", "Jos", "Jdg", "Ru", "1Sm", "2Sm", "1Kg", "2Kg", "1Ch", "2Ch", "Ezr", "Neh", "Est", "Jb", "Ps", "Pr", "Ec", "Sg", "Is", "Jr", "Lm", "Ezk", "Dn", "Hs", "Jl", "Am", "Ob", "Jnh", "Mc", "Nah", "Hab", "Zph", "Hg", "Zch", "Mal",
                                            "Mt", "Mk", "Lk", "Jn", "Ac", "Rm", "1Co", "2Co", "Gl", "Eph", "Php", "Col", "1Th", "1Tm", "2Tm", "Ti", "Phm", "Heb", "Jms", "1Pt", "2Pt", "1Jn", "2Jn", "3Jn", "Jd", "Rv"];

        string[] BookAbbreviationKorean = ["창", "출", "레", "민", "신", "수", "삿", "룻", "삼상", "삼하", "왕상", "왕하", "대상", "대하", "라", "느", "더", "욥", "시", "잠", "전", "아", "사", "렘", "애", "겔", "단", "호", "욜", "암", "옵", "욘", "미", "나", "합", "습", "학", "슥", "말",
                                           "마", "막", "눅", "요", "행", "롬", "고전", "고후", "갈", "엡", "빌", "골", "살전", "살후", "딤전", "딤후", "딛", "몬", "히", "약", "벧전", "벧후", "요일", "요이", "요삼", "유", "계"];

        // 영어
        if (versionAbbrName.StartsWith("eng", StringComparison.CurrentCultureIgnoreCase))
        {
            return BookAbbreviationEnglish[bookIndex];
        }
        // 한글
        else
        {
            return BookAbbreviationKorean[bookIndex];
        }
    }
}