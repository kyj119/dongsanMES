using MESCollector.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MESCollector.Services;

public class FileMonitorService : BackgroundService
{
    private readonly CollectorSettings _settings;
    private readonly FileParserService _parser;
    private readonly ApiService _apiService;
    private readonly ILogger<FileMonitorService> _logger;
    private readonly List<FileSystemWatcher> _watchers = new();

    public FileMonitorService(
        IOptions<CollectorSettings> settings,
        FileParserService parser,
        ApiService apiService,
        ILogger<FileMonitorService> logger)
    {
        _settings = settings.Value;
        _parser = parser;
        _apiService = apiService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("=== MES Collector 시작 ===");
        _logger.LogInformation("Collector ID: {CollectorId}", _settings.CollectorId);
        _logger.LogInformation("서버 URL: {ServerUrl}", _settings.ServerUrl);

        // 서버 연결 확인
        var isServerAlive = await _apiService.CheckServerHealthAsync();
        if (!isServerAlive)
        {
            _logger.LogWarning("서버 연결 실패! 서버가 실행 중인지 확인하세요: {ServerUrl}", _settings.ServerUrl);
        }
        else
        {
            _logger.LogInformation("서버 연결 성공!");
        }

        // 폴더 존재 확인 및 생성
        EnsureDirectoriesExist();

        // FileSystemWatcher 설정
        SetupFileWatchers();

        _logger.LogInformation("=== 파일 모니터링 시작 ===");
        _logger.LogInformation("Preview 폴더: {Path}", _settings.WatchPaths.Preview);
        _logger.LogInformation("PrintLog 폴더: {Path}", _settings.WatchPaths.PrintLog);
        _logger.LogInformation("Job 폴더: {Path}", _settings.WatchPaths.Job);

        // 무한 대기 (Ctrl+C로 종료될 때까지)
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("종료 신호 받음. Collector를 종료합니다...");
        }
    }

    private void EnsureDirectoriesExist()
    {
        var paths = new[]
        {
            _settings.WatchPaths.Preview,
            _settings.WatchPaths.PrintLog,
            _settings.WatchPaths.Job
        };

        foreach (var path in paths)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogWarning("폴더가 존재하지 않습니다. 생성 시도: {Path}", path);
                try
                {
                    Directory.CreateDirectory(path);
                    _logger.LogInformation("폴더 생성 완료: {Path}", path);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "폴더 생성 실패: {Path}", path);
                }
            }
        }
    }

    private void SetupFileWatchers()
    {
        // 1. Preview 폴더 모니터링 (.bmp, .tsc)
        var previewWatcher = new FileSystemWatcher(_settings.WatchPaths.Preview)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
            EnableRaisingEvents = true
        };
        previewWatcher.Created += OnPreviewFileCreated;
        _watchers.Add(previewWatcher);

        // 2. PrintLog 폴더 모니터링 (.log)
        var printLogWatcher = new FileSystemWatcher(_settings.WatchPaths.PrintLog)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.log",
            EnableRaisingEvents = true
        };
        printLogWatcher.Created += OnPrintLogFileCreated;
        _watchers.Add(printLogWatcher);

        // 3. Job 폴더 모니터링 (.job)
        var jobWatcher = new FileSystemWatcher(_settings.WatchPaths.Job)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.job",
            EnableRaisingEvents = true
        };
        jobWatcher.Created += OnJobFileCreated;
        _watchers.Add(jobWatcher);
    }

    /// <summary>
    /// Preview 파일 생성 시 (작업 대기)
    /// </summary>
    private async void OnPreviewFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("[Preview] 파일 감지: {FileName}", e.Name);

        // 카드번호 추출
        var cardNumber = _parser.ExtractCardNumber(e.Name ?? "");
        if (string.IsNullOrEmpty(cardNumber))
        {
            _logger.LogWarning("[Preview] 유효하지 않은 카드번호: {FileName}", e.Name);
            return;
        }

        // 이벤트 생성
        var eventDto = new EventDto
        {
            EventType = "작업대기",
            CardNumber = cardNumber,
            CollectorId = _settings.CollectorId,
            Timestamp = DateTime.Now,
            Metadata = new Dictionary<string, string>
            {
                { "FileName", e.Name ?? "" },
                { "FilePath", e.FullPath }
            }
        };

        // 서버로 전송
        await _apiService.SendEventAsync(eventDto);
    }

    /// <summary>
    /// PrintLog 파일 생성 시 (작업 시작)
    /// </summary>
    private async void OnPrintLogFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("[PrintLog] 파일 감지: {FileName}", e.Name);

        // 파일이 완전히 기록될 때까지 대기
        await Task.Delay(500);

        // 카드번호 추출
        var cardNumber = _parser.ExtractCardNumber(e.Name ?? "");
        if (string.IsNullOrEmpty(cardNumber))
        {
            _logger.LogWarning("[PrintLog] 유효하지 않은 카드번호: {FileName}", e.Name);
            return;
        }

        // LOG 파일 파싱
        var metadata = _parser.ParseLogFile(e.FullPath);
        if (metadata == null)
        {
            metadata = new Dictionary<string, string>();
        }
        metadata["FileName"] = e.Name ?? "";
        metadata["FilePath"] = e.FullPath;

        // 이벤트 생성
        var eventDto = new EventDto
        {
            EventType = "작업시작",
            CardNumber = cardNumber,
            CollectorId = _settings.CollectorId,
            Timestamp = DateTime.Now,
            Metadata = metadata
        };

        // 서버로 전송
        await _apiService.SendEventAsync(eventDto);
    }

    /// <summary>
    /// Job 파일 생성 시 (작업 완료)
    /// </summary>
    private async void OnJobFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("[Job] 파일 감지: {FileName}", e.Name);

        // 파일이 완전히 기록될 때까지 대기
        await Task.Delay(500);

        // 카드번호 추출
        var cardNumber = _parser.ExtractCardNumber(e.Name ?? "");
        if (string.IsNullOrEmpty(cardNumber))
        {
            _logger.LogWarning("[Job] 유효하지 않은 카드번호: {FileName}", e.Name);
            return;
        }

        // JOB 파일 파싱
        var metadata = _parser.ParseJobFile(e.FullPath);
        if (metadata == null)
        {
            metadata = new Dictionary<string, string>();
        }
        metadata["FileName"] = e.Name ?? "";
        metadata["FilePath"] = e.FullPath;

        // 이벤트 생성
        var eventDto = new EventDto
        {
            EventType = "작업완료",
            CardNumber = cardNumber,
            CollectorId = _settings.CollectorId,
            Timestamp = DateTime.Now,
            Metadata = metadata
        };

        // 서버로 전송
        await _apiService.SendEventAsync(eventDto);
    }

    public override void Dispose()
    {
        foreach (var watcher in _watchers)
        {
            watcher.Dispose();
        }
        base.Dispose();
    }
}
