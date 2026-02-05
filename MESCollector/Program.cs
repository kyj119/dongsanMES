using MESCollector.Models;
using MESCollector.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Serilog 설정
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("MES Collector 시작 중...");

    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            // 설정 바인딩
            services.Configure<CollectorSettings>(
                context.Configuration.GetSection("Collector"));

            // HttpClient 등록
            services.AddHttpClient<ApiService>();

            // 서비스 등록
            services.AddSingleton<FileParserService>();
            services.AddHostedService<FileMonitorService>();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Collector가 예상치 못한 오류로 종료되었습니다.");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
