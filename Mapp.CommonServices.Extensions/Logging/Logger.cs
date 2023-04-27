using Serilog;

namespace Shmap.Common.Logging;


public interface ILogger
{
    void Error(System.Exception exception);
}

public class Logger : ILogger
{
    private readonly Serilog.Core.Logger _logger;

    public Logger()
    {
        _logger = new LoggerConfiguration()
            .Enrich.WithDemystifiedStackTraces()
            .MinimumLevel.Debug()
            .WriteTo.File(
                @"logs\log.txt",
                rollingInterval: RollingInterval.Year,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 10_000_000,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    public void Error(System.Exception exception)
    {
        _logger.Error("Exception occurred: {Exception}", exception);
    }
}