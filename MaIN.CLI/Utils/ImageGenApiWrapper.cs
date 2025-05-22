using System.Diagnostics;

namespace MaIN.CLI.Utils;

public class ImageGenApiWrapper
{
    private Process? _apiProcess;

    public ImageGenApiWrapper(string pythonExe)
    {
        StartImageGenApi(pythonExe);
    }
    
    ~ImageGenApiWrapper()
    {
        StopApi();
    }

    private void StartImageGenApi(string pythonExe)
    {
        Console.WriteLine("Starting Image Generation API...");
        
        var mainPyPath = Path.Combine(Environment.CurrentDirectory, "ImageGen", "main.py");
        
        _apiProcess = Process.Start(new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = $"\"{mainPyPath}\"",
            UseShellExecute = false,
            WorkingDirectory = Environment.CurrentDirectory
        });
    }
    
    public void StopApi()
    {
        if (_apiProcess == null || _apiProcess.HasExited) 
            return;
        
        Console.WriteLine("Stopping Image Generation API...");
        _apiProcess.Kill();
        _apiProcess.Dispose();
    }
}