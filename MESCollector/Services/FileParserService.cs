using System.Text.RegularExpressions;

namespace MESCollector.Services;

public class FileParserService
{
    /// <summary>
    /// LOG 파일 파싱 (작업 시작 정보)
    /// 예: JobName=8색, Copies=9380, StartTime=2026-02-03 13:35:56, EndTime=2026-02-03 13:36:03
    /// </summary>
    public Dictionary<string, string>? ParseLogFile(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            var metadata = new Dictionary<string, string>();

            // JobName 추출
            var jobNameMatch = Regex.Match(content, @"JobName=([^,\r\n]+)");
            if (jobNameMatch.Success)
            {
                metadata["JobName"] = jobNameMatch.Groups[1].Value.Trim();
            }

            // Copies 추출
            var copiesMatch = Regex.Match(content, @"Copies=(\d+)");
            if (copiesMatch.Success)
            {
                metadata["Copies"] = copiesMatch.Groups[1].Value;
            }

            // StartTime 추출
            var startTimeMatch = Regex.Match(content, @"StartTime=([^,\r\n]+)");
            if (startTimeMatch.Success)
            {
                metadata["StartTime"] = startTimeMatch.Groups[1].Value.Trim();
            }

            // EndTime 추출
            var endTimeMatch = Regex.Match(content, @"EndTime=([^,\r\n]+)");
            if (endTimeMatch.Success)
            {
                metadata["EndTime"] = endTimeMatch.Groups[1].Value.Trim();
            }

            return metadata.Count > 0 ? metadata : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LOG 파일 파싱 실패: {filePath}, 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// JOB 파일 파싱 (작업 완료 정보)
    /// 예: PrintFile=Z:\테스트\8색.eps, DestSizeX=980.000000, DestSizeY=199.988510
    /// </summary>
    public Dictionary<string, string>? ParseJobFile(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            var metadata = new Dictionary<string, string>();

            // PrintFile 추출
            var printFileMatch = Regex.Match(content, @"PrintFile=([^\r\n]+)");
            if (printFileMatch.Success)
            {
                metadata["PrintFile"] = printFileMatch.Groups[1].Value.Trim();
            }

            // DestSizeX 추출
            var destSizeXMatch = Regex.Match(content, @"DestSizeX=([0-9.]+)");
            if (destSizeXMatch.Success)
            {
                metadata["DestSizeX"] = destSizeXMatch.Groups[1].Value;
            }

            // DestSizeY 추출
            var destSizeYMatch = Regex.Match(content, @"DestSizeY=([0-9.]+)");
            if (destSizeYMatch.Success)
            {
                metadata["DestSizeY"] = destSizeYMatch.Groups[1].Value;
            }

            // BeginDate/BeginTime 추출
            var beginDateMatch = Regex.Match(content, @"BeginDate=([^\r\n]+)");
            if (beginDateMatch.Success)
            {
                metadata["BeginDate"] = beginDateMatch.Groups[1].Value.Trim();
            }

            // EndDate/EndTime 추출
            var endDateMatch = Regex.Match(content, @"EndDate=([^\r\n]+)");
            if (endDateMatch.Success)
            {
                metadata["EndDate"] = endDateMatch.Groups[1].Value.Trim();
            }

            return metadata.Count > 0 ? metadata : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JOB 파일 파싱 실패: {filePath}, 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 파일명에서 카드번호 추출
    /// 예: 20260204-01-1.bmp.tsc → 20260204-01-1
    /// </summary>
    public string? ExtractCardNumber(string fileName)
    {
        try
        {
            // 확장자 제거
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            
            // .bmp.tsc 같은 이중 확장자 처리
            if (nameWithoutExt.Contains('.'))
            {
                nameWithoutExt = Path.GetFileNameWithoutExtension(nameWithoutExt);
            }

            // 카드번호 형식 검증: YYYYMMDD-XX-Y
            if (IsValidCardNumber(nameWithoutExt))
            {
                return nameWithoutExt;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 카드번호 유효성 검사
    /// 형식: YYYYMMDD-XX-Y (예: 20260204-01-1)
    /// </summary>
    public bool IsValidCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return false;

        // 형식: YYYYMMDD-XX-Y (최소 13자리)
        var pattern = @"^\d{8}-\d{2}-\d+$";
        return Regex.IsMatch(cardNumber, pattern);
    }
}
