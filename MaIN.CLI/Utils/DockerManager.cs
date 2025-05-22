using System.Diagnostics;

namespace MaIN.CLI.Utils;

public class DockerManager
{
    private static readonly HttpClient httpClient = new();
    
    public async Task<bool> ExecuteSetupAsync(bool hardReset = false)
    {
        try
        {
            Console.WriteLine($"Starting setup from: {AppDomain.CurrentDomain.BaseDirectory}");
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // Check and install .NET if needed
            if (!DotnetVersionManager.EnsureDotNetAsync())
            {
                Console.WriteLine("Failed to ensure .NET SDK is available.");
                return false;
            }

            // Handle Docker operations
            await HandleDockerOperationsAsync(hardReset);

            // Start the server and containers
            await StartServicesAsync();

            Console.WriteLine("Listening on http://localhost:5001 - happy travels");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during setup: {ex.Message}");
            return false;
        }
    }

    private async Task HandleDockerOperationsAsync(bool hardReset)
    {
        if (hardReset)
        {
            Console.WriteLine("Stopping and removing Docker containers, networks, images, and volumes...");
            await RunCommandAsync("docker-compose", "down -v");
        }
        else
        {
            Console.WriteLine("Stopping and removing Docker containers, networks, and images (volumes retained)...");
            await RunCommandAsync("docker-compose", "down");
        }

        await Task.Delay(10000); // Wait 10 seconds
    }

    private async Task StartServicesAsync()
    {
        Console.WriteLine("Starting API & Containers in detached mode...");
        
        // Start the main server
        var serverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server", "MaIN.exe");
        var serverWorkingDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server");
        
        if (File.Exists(serverPath))
        {
            var serverProcessInfo = new ProcessStartInfo
            {
                FileName = serverPath,
                WorkingDirectory = serverWorkingDir,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            Process.Start(serverProcessInfo);
        }
        else
        {
            Console.WriteLine($"Warning: Server executable not found at {serverPath}");
        }

        await Task.Delay(10000); // Wait 10 seconds

        // Start Docker containers
        await RunCommandAsync("docker-compose", "up -d");
    }

    private static async Task RunCommandAsync(string fileName, string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(processInfo);
            await process.WaitForExitAsync();
                
            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                Console.WriteLine($"Command failed: {fileName} {arguments}");
                Console.WriteLine($"Error: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running command '{fileName} {arguments}': {ex.Message}");
        }
    }

    public void Dispose()
    {
        httpClient?.Dispose();
    }
}