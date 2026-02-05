using System.IO;
using System.Text.RegularExpressions;

namespace MESSystem.Services;

public class FileUploadService
{
    private readonly string _sharedFolderPath = @"\\192.168.0.122\Designs\";
    
    /// <summary>
    /// 파일 업로드 및 자동 리네임
    /// </summary>
    /// <param name="file">업로드된 파일</param>
    /// <param name="orderNumber">주문번호 (예: 20260204-01)</param>
    /// <param name="cardNumber">카드번호 (예: 20260204-01-1)</param>
    /// <returns>저장된 파일의 전체 경로</returns>
    public async Task<string> UploadFileAsync(IFormFile file, string orderNumber, string cardNumber)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("파일이 비어있습니다.");
        }

        // 원본 파일명에서 확장자 추출
        var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
        var extension = Path.GetExtension(file.FileName);

        // 새 파일명 생성: {카드번호}_{원본파일명}.{확장자}
        // 예: 20260204-01-1_태극기.ai
        var newFileName = $"{cardNumber}_{SanitizeFileName(originalFileName)}{extension}";

        // 폴더 경로 생성: \\192.168.0.122\Designs\2026\02\20260204-01\
        var yearMonth = GetYearMonthFromOrderNumber(orderNumber);
        var folderPath = Path.Combine(_sharedFolderPath, yearMonth.Year, yearMonth.Month, orderNumber);

        // 폴더 생성 (없으면)
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 전체 파일 경로
        var fullPath = Path.Combine(folderPath, newFileName);

        // 파일 저장
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fullPath;
    }

    /// <summary>
    /// 파일명에서 사용할 수 없는 문자 제거
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
    }

    /// <summary>
    /// 주문번호에서 연/월 추출
    /// </summary>
    /// <param name="orderNumber">주문번호 (예: 20260204-01)</param>
    /// <returns>(Year: "2026", Month: "02")</returns>
    private (string Year, string Month) GetYearMonthFromOrderNumber(string orderNumber)
    {
        // 주문번호 형식: YYYYMMDD-XX
        if (orderNumber.Length >= 8)
        {
            var year = orderNumber.Substring(0, 4);   // 2026
            var month = orderNumber.Substring(4, 2);  // 02
            return (year, month);
        }

        // 기본값 (현재 날짜)
        var now = DateTime.Now;
        return (now.Year.ToString(), now.Month.ToString("00"));
    }

    /// <summary>
    /// 파일 존재 여부 확인
    /// </summary>
    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// 파일 삭제
    /// </summary>
    public void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// 공유폴더 접근 가능 여부 확인
    /// </summary>
    public bool IsSharedFolderAccessible()
    {
        try
        {
            return Directory.Exists(_sharedFolderPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 공유폴더 경로 반환
    /// </summary>
    public string GetSharedFolderPath()
    {
        return _sharedFolderPath;
    }
}
