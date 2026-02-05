using System.Net.Http.Json;
using System.Text.Json;
using MESCollector.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MESCollector.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly CollectorSettings _settings;
    private readonly ILogger<ApiService> _logger;

    public ApiService(
        HttpClient httpClient,
        IOptions<CollectorSettings> settings,
        ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// 이벤트를 서버로 전송
    /// </summary>
    public async Task<bool> SendEventAsync(EventDto eventDto)
    {
        var retryCount = 0;
        var maxRetries = _settings.RetryCount;

        while (retryCount <= maxRetries)
        {
            try
            {
                var url = $"{_settings.ServerUrl}/api/events";
                var response = await _httpClient.PostAsJsonAsync(url, eventDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<EventResponse>();
                    _logger.LogInformation(
                        "이벤트 전송 성공: {EventType} - {CardNumber}, 서버 응답: {Message}",
                        eventDto.EventType,
                        eventDto.CardNumber,
                        result?.Message ?? "OK");
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // 400 오류는 재시도하지 않음 (카드 번호 오류 등)
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "이벤트 전송 실패 (재시도 안 함): {CardNumber}, 상태: {StatusCode}, 오류: {Error}",
                        eventDto.CardNumber,
                        response.StatusCode,
                        errorContent);
                    return false;
                }
                else
                {
                    _logger.LogWarning(
                        "이벤트 전송 실패 (재시도 {Retry}/{MaxRetries}): {CardNumber}, 상태: {StatusCode}",
                        retryCount + 1,
                        maxRetries,
                        eventDto.CardNumber,
                        response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "네트워크 오류 (재시도 {Retry}/{MaxRetries}): {CardNumber}",
                    retryCount + 1,
                    maxRetries,
                    eventDto.CardNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "예상치 못한 오류 (재시도 {Retry}/{MaxRetries}): {CardNumber}",
                    retryCount + 1,
                    maxRetries,
                    eventDto.CardNumber);
            }

            retryCount++;
            if (retryCount <= maxRetries)
            {
                var delaySeconds = _settings.RetryDelaySeconds * retryCount; // 점진적 지연
                _logger.LogInformation("재시도 대기 중... {Delay}초", delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }

        _logger.LogError("이벤트 전송 최종 실패: {CardNumber}", eventDto.CardNumber);
        return false;
    }

    /// <summary>
    /// 서버 연결 상태 확인
    /// </summary>
    public async Task<bool> CheckServerHealthAsync()
    {
        try
        {
            var url = $"{_settings.ServerUrl}/";
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
