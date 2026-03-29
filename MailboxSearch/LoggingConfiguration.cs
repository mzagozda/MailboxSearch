using Serilog;
using Serilog.Events;

namespace MailboxSearch;

internal static class LoggingConfiguration
{
    public static ILogger CreateLogger()
    {
        var logDirectoryPath = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectoryPath);

        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.WithMachineName()
            .WriteTo.File(
                Path.Combine(logDirectoryPath, "mailboxsearch-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{MachineName}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .CreateLogger();
    }
}