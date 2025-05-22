using System.Diagnostics;

namespace MaIN.CLI.Utils;

internal class DotnetVersionManager
{
    internal static bool EnsureDotNetAsync()
    {
        var dotnetVersion = GetDotNetVersion();
        
        if (dotnetVersion == null)
        {
            Console.WriteLine("No .NET SDK installation detected.");
            return false;
        }
        
        Console.WriteLine($"Detected .NET SDK version {dotnetVersion}");
        
        if (dotnetVersion.Major < 8)
        {
            Console.WriteLine($".NET SDK version {dotnetVersion} is below required version 8.0");
            return false;
        }
        
        Console.WriteLine($".NET SDK version {dotnetVersion} meets requirements.");
        return true;
    }
    
    internal static Version GetDotNetVersion()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--list-sdks",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    var versions = output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => 
                        {
                            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 0 && Version.TryParse(parts[0], out var version))
                            {
                                return version;
                            }
                            return null;
                        })
                        .Where(v => v != null)
                        .OrderByDescending(v => v)
                        .FirstOrDefault();

                    return versions;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking .NET version: {ex.Message}");
        }

        return null;
    }
    
    // TODO: Create installer that would work on both Unix and Windows - and think if we want to install .net for users in the first place. 
    // private async Task<bool> InstallDotNet8Async()
    // {
    //     Console.WriteLine("Installing .NET 8 SDK...");
    //     
    //     const string dotnetInstallerUrl = "https://download.visualstudio.microsoft.com/download/pr/89a5ff62-7f4f-4931-896d-2c3e0b3db248/7a97ec4977e245b29d42db9de48c9db1/dotnet-sdk-8.0.100-win-x64.exe";
    //     var installerPath = Path.Combine(Path.GetTempPath(), "dotnet-sdk-8.0.100-win-x64.exe");
    //
    //     try
    //     {
    //         // Download the .NET installer
    //         Console.WriteLine("Downloading .NET 8 SDK installer...");
    //         var response = await httpClient.GetAsync(dotnetInstallerUrl);
    //         response.EnsureSuccessStatusCode();
    //         
    //         await using (var fileStream = File.Create(installerPath))
    //         {
    //             await response.Content.CopyToAsync(fileStream);
    //         }
    //
    //         // Install .NET silently
    //         Console.WriteLine("Running installer...");
    //         var processInfo = new ProcessStartInfo
    //         {
    //             FileName = installerPath,
    //             Arguments = "/quiet /norestart",
    //             UseShellExecute = false,
    //             CreateNoWindow = true
    //         };
    //
    //         using (var process = Process.Start(processInfo))
    //         {
    //             await process.WaitForExitAsync();
    //         }
    //
    //         // Cleanup installer
    //         try
    //         {
    //             File.Delete(installerPath);
    //         }
    //         catch
    //         {
    //             // Ignore cleanup errors
    //         }
    //
    //         // Verify installation
    //         await Task.Delay(2000); // Give some time for installation to complete
    //         var newVersion = GetDotNetVersion();
    //         
    //         if (newVersion != null && newVersion.Major >= 8)
    //         {
    //             Console.WriteLine($"Successfully installed .NET SDK version {newVersion}");
    //             return true;
    //         }
    //         
    //         Console.WriteLine("Installation completed but version verification failed.");
    //         return false;
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error during installation: {ex.Message}");
    //         return false;
    //     }
    // }
}