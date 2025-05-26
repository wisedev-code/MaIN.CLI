using System.Diagnostics;
using System.Runtime.InteropServices;
using MaIN.CLI.Utils;

namespace MaIN.CLI.CommandHandlers;

internal partial class CommandHandlers
{
    private Process? _apiProcess;
    
    internal async Task StartImageGen(string[] args)
    {
        var manager = new PythonManager();

        var python = await manager.GetOrInstallPythonAsync();
        
        await manager.VerifyPythonInstallation(python);
        
        await manager.InstallDependencies(python);

        Console.WriteLine("Starting Image Generation API...");
        
        var mainPyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageGen", "main.py");

        _apiProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = python,
                Arguments = $"{mainPyPath}",
                UseShellExecute = false,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Verb = "runas"
            }
        };
        
        _apiProcess.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        _apiProcess.ErrorDataReceived += (sender, args) => Console.WriteLine($"ERROR: {args.Data}");

        _apiProcess.Start();
        _apiProcess.BeginOutputReadLine();
        _apiProcess.BeginErrorReadLine();
        //TODO: process is killed during starting - figure out why
        await _apiProcess.WaitForExitAsync();
    }
}