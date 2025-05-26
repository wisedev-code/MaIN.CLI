using System.Diagnostics;

namespace MaIN.CLI.CommandHandlers;

internal partial class CommandHandlers
{
    internal async Task Uninstall()
    {
        try
        {
            // Get the current executable path and directory
            var currentExePath = Process.GetCurrentProcess().MainModule.FileName;
            var appDirectory = Path.GetDirectoryName(currentExePath);

            Console.WriteLine($"Current executable: {currentExePath}");
            Console.WriteLine($"App directory: {appDirectory}");

            // Confirm with user
            Console.WriteLine("This will permanently delete the application and all its files.");
            Console.Write("Are you sure you want to continue? (y/N): ");
            string response = Console.ReadLine();

            if (response?.ToLower() != "y" && response?.ToLower() != "yes")
            {
                Console.WriteLine("Uninstall cancelled.");
                return;
            }

            // Create a batch file to delete the directory after the process exits
            string batchPath = Path.Combine(Path.GetTempPath(), "uninstall_app.bat");
            string batchContent = $@"@echo off
timeout /t 2 /nobreak >nul
echo Removing application directory...
rd /s /q ""{appDirectory}""
if exist ""{appDirectory}"" (
    echo Failed to remove directory. It may be in use.
    echo Directory: {appDirectory}
    pause
) else (
    echo Application successfully uninstalled.
)
del ""{batchPath}""
";

            File.WriteAllText(batchPath, batchContent);

            // Start the batch file and exit immediately
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = batchPath,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Console.WriteLine("Uninstalling application...");
            Process.Start(startInfo);

            // Exit the application immediately
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during uninstall: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}