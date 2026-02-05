namespace MESCollector.Models;

public class CollectorSettings
{
    public string ServerUrl { get; set; } = "http://localhost:5000";
    public string CollectorId { get; set; } = "COLLECTOR-001";
    public WatchPathsSettings WatchPaths { get; set; } = new();
    public int RetryCount { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
}

public class WatchPathsSettings
{
    public string Preview { get; set; } = @"C:\TOPAZ_RIP\preview";
    public string PrintLog { get; set; } = @"C:\TOPAZ_RIP\printlog";
    public string Job { get; set; } = @"C:\TOPAZ_RIP\job";
}
