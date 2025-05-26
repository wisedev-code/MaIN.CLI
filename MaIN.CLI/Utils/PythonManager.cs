using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MaIN.CLI.Utils;

internal class PythonManager
{
    private readonly string _pythonVersion = "3.9.13";
    private Process _apiProcess;
    
    internal async Task<string> GetOrInstallPythonAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return await GetOrInstallPythonWindowsAsync();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return await GetOrInstallPythonLinuxAsync();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return await GetOrInstallPythonMacOSAsync();
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported operating system");
        }
    }
    
    private async Task<string> GetOrInstallPythonWindowsAsync()
    {
        var pythonInstallDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs", "Python", "Python39");
        var pythonExe = Path.Combine(pythonInstallDir, "python.exe");

        if (!File.Exists(pythonExe))
        {
            await InstallPythonWindowsAsync();
            UpdateEnvironmentPathWindows(pythonInstallDir);
        }
        else
        {
            Console.WriteLine($"Python {_pythonVersion} is already installed at {pythonInstallDir}.");
        }

        return pythonExe;
    }
    
    private async Task<string> GetOrInstallPythonLinuxAsync()
    {
        // First check if python3 is available
        var pythonExe = await FindPythonExecutableAsync("python3");

        if (pythonExe != null && await IsPythonVersionCompatible(pythonExe))
        {
            Console.WriteLine($"Compatible Python found at {pythonExe}");
            return pythonExe;
        }

        // Install Python using package manager
        await InstallPythonLinuxAsync();
        
        // Try to find Python again after installation
        pythonExe = await FindPythonExecutableAsync("python3");
        
        if (pythonExe == null)
        {
            throw new InvalidOperationException("Python installation failed or Python executable not found in PATH");
        }

        return pythonExe;
    }

    private async Task<string> GetOrInstallPythonMacOSAsync()
    {
        // First check if python3 is available
        var pythonExe = await FindPythonExecutableAsync("python3");

        if (pythonExe != null && await IsPythonVersionCompatible(pythonExe))
        {
            Console.WriteLine($"Compatible Python found at {pythonExe}");
            return pythonExe;
        }

        // Check if Homebrew is available
        var brewPath = await FindExecutableAsync("brew");
        if (brewPath != null)
        {
            await InstallPythonMacOSBrewAsync();
        }
        else
        {
            throw new InvalidOperationException("Python 3.9+ not found and Homebrew is not installed. Please install Python manually or install Homebrew first.");
        }

        // Try to find Python again after installation
        pythonExe = await FindPythonExecutableAsync("python3");
        
        if (pythonExe == null)
        {
            throw new InvalidOperationException("Python installation failed or Python executable not found in PATH");
        }

        return pythonExe;
    }
    
    private async Task<string> FindPythonExecutableAsync(string pythonCommand)
    {
        try
        {
            var whichCommand = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which";
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = whichCommand,
                Arguments = pythonCommand,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                return output.Trim().Split('\n')[0]; // Return first match
            }
        }
        catch
        {
            // Ignore exceptions
        }

        return null;
    }

    private async Task<string> FindExecutableAsync(string command)
    {
        return await FindPythonExecutableAsync(command);
    }

    private async Task<bool> IsPythonVersionCompatible(string pythonExe)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var version = output.Replace("Python ", "").Trim();
                var versionParts = version.Split('.');
                
                if (versionParts.Length >= 2 && 
                    int.TryParse(versionParts[0], out var major) && 
                    int.TryParse(versionParts[1], out var minor))
                {
                    return major == 3 && minor >= 9; // Python 3.9+
                }
            }
        }
        catch
        {
            // Ignore exceptions
        }

        return false;
    }

    private async Task InstallPythonWindowsAsync()
    {
        var installerUrl = $"https://www.python.org/ftp/python/{_pythonVersion}/python-{_pythonVersion}-amd64.exe";
        var installerPath = Path.Combine(Path.GetTempPath(), $"python-{_pythonVersion}-installer.exe");

        Console.WriteLine($"Downloading Python {_pythonVersion}...");
        
        using var client = new HttpClient();
        var response = await client.GetAsync(installerUrl);
        response.EnsureSuccessStatusCode();
        
        await using var fileStream = File.Create(installerPath);
        await response.Content.CopyToAsync(fileStream);
        fileStream.Close();

        Console.WriteLine($"Installing Python {_pythonVersion}...");
        
        var installProcess = Process.Start(new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = "/quiet InstallAllUsers=0 PrependPath=1 Include_pip=1",
            UseShellExecute = false
        });

        await installProcess.WaitForExitAsync();
        File.Delete(installerPath);

        if (installProcess.ExitCode != 0)
        {
            throw new InvalidOperationException($"Python installation failed with exit code {installProcess.ExitCode}");
        }

        await Task.Delay(10000); // Wait 10 seconds for installation completion
    }

    private async Task InstallPythonLinuxAsync()
    {
        Console.WriteLine("Installing Python 3.9+ using package manager...");

        // Try different package managers
        var packageManagers = new[]
        {
            ("apt-get", "update && apt-get install -y python3 python3-pip python3-venv"),
            ("yum", "install -y python3 python3-pip"),
            ("dnf", "install -y python3 python3-pip"),
            ("pacman", "-S --noconfirm python python-pip"),
            ("zypper", "install -y python3 python3-pip")
        };

        foreach (var (manager, installCmd) in packageManagers)
        {
            var managerPath = await FindExecutableAsync(manager);
            if (managerPath != null)
            {
                await RunCommandAsync("sudo", $"{manager} {installCmd}");
                return;
            }
        }

        throw new InvalidOperationException("No supported package manager found. Please install Python 3.9+ manually.");
    }

    private async Task InstallPythonMacOSBrewAsync()
    {
        Console.WriteLine("Installing Python using Homebrew...");
        await RunCommandAsync("brew", "install python@3.9");
    }

    private void UpdateEnvironmentPathWindows(string pythonInstallDir)
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH");
        Environment.SetEnvironmentVariable("PATH", $"{pythonInstallDir};{currentPath}");
    }

    public async Task VerifyPythonInstallation(string pythonExe)
    {
        Console.WriteLine("Verifying Python installation...");
        
        await RunPythonCommand(pythonExe, "--version");
        await RunPythonCommand(pythonExe, "-m pip --version");
    }

    internal async Task InstallDependencies(string pythonExe)
    {
        Console.WriteLine("Installing dependencies from requirements.txt...");
        
        var requirementsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageGen", "requirements.txt");

        var requirements = (await File.ReadAllTextAsync(requirementsPath)).Replace(Environment.NewLine, " ");

        await RunPythonCommand(pythonExe, $"-m pip install {requirements}");
    }


    private async Task RunPythonCommand(string pythonExe, string arguments)
    {
        await RunCommandAsync(pythonExe, arguments);
    }

    private async Task RunCommandAsync(string fileName, string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };
        
        process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        process.ErrorDataReceived += (sender, args) => Console.WriteLine($"ERROR: {args.Data}");

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        Console.WriteLine($"Exit code: {process.ExitCode}");
        
        // var process = Process.Start(new ProcessStartInfo
        // {
        //     FileName = fileName,
        //     Arguments = arguments,
        //     UseShellExecute = false,
        //     RedirectStandardOutput = true,
        //     RedirectStandardError = true,
        // });
        //
        // await process.WaitForExitAsync();
        //
        // if (process.ExitCode != 0)
        // {
        //     var error = await process.StandardError.ReadToEndAsync();
        //     throw new InvalidOperationException($"Command '{fileName} {arguments}' failed: {error}");
        // }
        //
        // var output = await process.StandardOutput.ReadToEndAsync();
        // if (!string.IsNullOrWhiteSpace(output))
        // {
        //     Console.WriteLine(output);
        // }
    }
}