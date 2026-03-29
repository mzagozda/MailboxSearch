using Serilog;

namespace MailboxSearch;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Log.Logger = LoggingConfiguration.CreateLogger();

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly.");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}