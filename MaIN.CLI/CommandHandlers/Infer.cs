using System.Diagnostics;
using MaIN.CLI.Utils;

namespace MaIN.CLI.CommandHandlers;

internal partial class CommandHandlers
{
    internal async Task HandleInferCommand(string[] args)
    {
        if (args.Length == 0)
        {
            ShowCommandHelp("infer");
            return;
        }

        var subcommand = args[0];
        var inferArgs = args.Skip(1).ToArray();

        switch (subcommand.ToLower())
        {
            case "chat":
                await StartInferChat(inferArgs);
                break;
            default:
                Console.WriteLine("Invalid infer command. Available: chat");
                break;
        }
    }

    private static async Task StartInferChat(string[] args)
    {
        // Ensure .NET is available and adequate
        if (!DotnetVersionManager.EnsureDotNetAsync())
            return;

        var inferDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "infer");
        if (!Directory.Exists(inferDir))
        {
            Console.WriteLine($"Infer directory not found at {inferDir}");
            return;
        }

        // Change to the infer directory
        Directory.SetCurrentDirectory(inferDir);

        // Determine which DLL to use
        string? dll = null;

        if (File.Exists("MaIN.Infer.dll"))
            dll = "MaIN.Infer.dll";
        else if (File.Exists("MaIN.InferPage.dll"))
            dll = "MaIN.InferPage.dll";

        Process? inferProcess;

        if (dll != null)
        {
            // Start using the dll
            var argumentsList = new List<string> { dll };
            argumentsList.AddRange(args);

            Console.WriteLine(
                $"Starting Infer chat process using {dll} with arguments: {string.Join(' ', argumentsList)}");

            inferProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = string.Join(' ', argumentsList),
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };
        }
        else
        {
            // Start using dotnet run
            var argumentsList = new List<string> { "run", "--" };
            argumentsList.AddRange(args);

            Console.WriteLine(
                $"Starting Infer chat process using dotnet run with arguments: {string.Join(' ', argumentsList)}");

            inferProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = string.Join(' ', argumentsList),
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };
        }

        if (inferProcess == null)
        {
            Console.WriteLine("Failed to create the Infer process.");
            return;
        }

        // Start the process
        inferProcess.Start();
        Console.WriteLine($"Infer process started with PID {inferProcess.Id}");

        // Set up cancellation for Ctrl+C
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("Stopping Infer process...");

            try
            {
                if (!inferProcess.HasExited)
                {
                    inferProcess.Kill();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping process: {ex.Message}");
            }

            cts.Cancel();
        };

        // Wait a few seconds for the server to start
        await Task.Delay(2000, cts.Token);

        // Open browser
        const string portUrl = "http://localhost:5555";
        Console.WriteLine($"Opening browser at {portUrl}...");
        Process.Start(new ProcessStartInfo
        {
            FileName = portUrl,
            UseShellExecute = true
        });

        // Wait for the infer process to exit or cancellation
        try
        {
            await Task.Run(() => inferProcess.WaitForExit(), cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
    }
}