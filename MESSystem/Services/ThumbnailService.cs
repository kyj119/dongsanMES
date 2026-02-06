using System.Diagnostics;

namespace MESSystem.Services;

public class ThumbnailService
{
    private readonly ILogger<ThumbnailService> _logger;
    private readonly string _ghostscriptPath;
    private readonly string _errorImagePath;
    
    public ThumbnailService(ILogger<ThumbnailService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        
        // Ghostscript 경로 찾기 (일반적인 설치 경로들)
        var possiblePaths = new[]
        {
            @"C:\Program Files\gs\gs10.03.0\bin\gswin64c.exe",
            @"C:\Program Files\gs\gs10.02.1\bin\gswin64c.exe",
            @"C:\Program Files\gs\gs10.02.0\bin\gswin64c.exe",
            @"C:\Program Files\gs\gs10.01.0\bin\gswin64c.exe",
            @"C:\Program Files\gs\gs10.00.0\bin\gswin64c.exe",
            @"C:\Program Files (x86)\gs\gs10.03.0\bin\gswin32c.exe",
            @"gswin64c.exe", // PATH에 있는 경우
            @"gswin32c.exe"
        };
        
        _ghostscriptPath = possiblePaths.FirstOrDefault(File.Exists) ?? "gswin64c.exe";
        
        // 에러 이미지 경로
        _errorImagePath = Path.Combine(env.WebRootPath, "images", "error-thumbnail.jpg");
        
        // 에러 이미지가 없으면 생성
        EnsureErrorImageExists();
    }
    
    /// <summary>
    /// EPS 파일을 JPG 썸네일로 변환
    /// </summary>
    /// <param name="epsFilePath">원본 EPS 파일 경로</param>
    /// <returns>생성된 썸네일 경로 (실패 시 null)</returns>
    public async Task<string?> GenerateThumbnailAsync(string epsFilePath)
    {
        try
        {
            if (string.IsNullOrEmpty(epsFilePath) || !File.Exists(epsFilePath))
            {
                _logger.LogWarning("EPS 파일을 찾을 수 없습니다: {FilePath}", epsFilePath);
                return CopyErrorImage(epsFilePath);
            }
            
            // 확장자 확인 (EPS만 처리)
            var extension = Path.GetExtension(epsFilePath).ToLower();
            if (extension != ".eps")
            {
                _logger.LogWarning("EPS 파일이 아닙니다: {FilePath}", epsFilePath);
                return null;
            }
            
            // 썸네일 파일 경로 생성
            var directory = Path.GetDirectoryName(epsFilePath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(epsFilePath);
            var thumbnailPath = Path.Combine(directory!, $"{fileNameWithoutExt}_thumb.jpg");
            
            // 이미 썸네일이 있으면 건너뛰기
            if (File.Exists(thumbnailPath))
            {
                _logger.LogInformation("썸네일이 이미 존재합니다: {ThumbnailPath}", thumbnailPath);
                return thumbnailPath;
            }
            
            // Ghostscript로 EPS → JPG 변환
            var result = await ConvertEpsToJpgAsync(epsFilePath, thumbnailPath);
            
            if (result)
            {
                _logger.LogInformation("썸네일 생성 성공: {ThumbnailPath}", thumbnailPath);
                return thumbnailPath;
            }
            else
            {
                _logger.LogError("썸네일 생성 실패: {EpsFilePath}", epsFilePath);
                return CopyErrorImage(epsFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "썸네일 생성 중 예외 발생: {EpsFilePath}", epsFilePath);
            return CopyErrorImage(epsFilePath);
        }
    }
    
    /// <summary>
    /// Ghostscript를 사용하여 EPS를 JPG로 변환
    /// </summary>
    private async Task<bool> ConvertEpsToJpgAsync(string epsPath, string jpgPath)
    {
        try
        {
            // Ghostscript 명령 인자
            // -dNOPAUSE -dBATCH -dSAFER : 프롬프트 없이 배치 실행
            // -sDEVICE=jpeg : JPEG 출력
            // -r300 : 300 DPI 해상도 (고품질)
            // -dJPEGQ=95 : JPEG 품질 95%
            // -g2400x2400 : 최대 크기 2400x2400 (비율 유지)
            // -dFIXEDMEDIA : 미디어 크기 고정
            var arguments = $"-dNOPAUSE -dBATCH -dSAFER -sDEVICE=jpeg -r300 -dJPEGQ=95 -g2400x2400 -dFIXEDMEDIA -sOutputFile=\"{jpgPath}\" \"{epsPath}\"";
            
            var processInfo = new ProcessStartInfo
            {
                FileName = _ghostscriptPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using var process = new Process { StartInfo = processInfo };
            process.Start();
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0 && File.Exists(jpgPath))
            {
                _logger.LogInformation("Ghostscript 변환 성공: {EpsPath} → {JpgPath}", epsPath, jpgPath);
                return true;
            }
            else
            {
                _logger.LogError("Ghostscript 변환 실패 (ExitCode: {ExitCode})\nOutput: {Output}\nError: {Error}", 
                    process.ExitCode, output, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ghostscript 실행 실패: {EpsPath}", epsPath);
            return false;
        }
    }
    
    /// <summary>
    /// 에러 이미지를 대상 경로에 복사
    /// </summary>
    private string? CopyErrorImage(string originalFilePath)
    {
        try
        {
            if (!File.Exists(_errorImagePath))
            {
                _logger.LogWarning("에러 이미지를 찾을 수 없습니다: {ErrorImagePath}", _errorImagePath);
                return null;
            }
            
            var directory = Path.GetDirectoryName(originalFilePath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFilePath);
            var errorThumbnailPath = Path.Combine(directory!, $"{fileNameWithoutExt}_thumb.jpg");
            
            File.Copy(_errorImagePath, errorThumbnailPath, overwrite: true);
            
            _logger.LogInformation("에러 이미지 복사 완료: {ErrorThumbnailPath}", errorThumbnailPath);
            return errorThumbnailPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "에러 이미지 복사 실패");
            return null;
        }
    }
    
    /// <summary>
    /// 에러 이미지가 없으면 생성
    /// </summary>
    private void EnsureErrorImageExists()
    {
        try
        {
            var directory = Path.GetDirectoryName(_errorImagePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
            
            // 에러 이미지가 이미 있으면 건너뛰기
            if (File.Exists(_errorImagePath))
            {
                return;
            }
            
            // 간단한 에러 이미지 생성 (1x1 빨간 픽셀 - 실제로는 더 나은 이미지 사용)
            // TODO: 실제 에러 이미지는 디자이너가 제공하거나 무료 이미지 사용
            _logger.LogInformation("에러 이미지 생성: {ErrorImagePath}", _errorImagePath);
            
            // Base64로 인코딩된 간단한 빨간색 JPG 이미지
            var base64Image = "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAIBAQIBAQICAgICAgICAwUDAwMDAwYEBAMFBwYHBwcGBwcICQsJCAgKCAcHCg0KCgsMDAwMBwkODw0MDgsMDAz/2wBDAQICAgMDAwYDAwYMCAcIDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAz/wAARCAABAAEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlbaWmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD9/KKKKAP/2Q==";
            var imageBytes = Convert.FromBase64String(base64Image);
            File.WriteAllBytes(_errorImagePath, imageBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "에러 이미지 생성 실패");
        }
    }
    
    /// <summary>
    /// Ghostscript 설치 여부 확인
    /// </summary>
    public bool IsGhostscriptInstalled()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = _ghostscriptPath,
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(processInfo);
            process?.WaitForExit();
            
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 썸네일 삭제
    /// </summary>
    public void DeleteThumbnail(string? thumbnailPath)
    {
        try
        {
            if (!string.IsNullOrEmpty(thumbnailPath) && File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
                _logger.LogInformation("썸네일 삭제 완료: {ThumbnailPath}", thumbnailPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "썸네일 삭제 실패: {ThumbnailPath}", thumbnailPath);
        }
    }
}
