using System.Diagnostics;
using System.Reflection;

namespace MaIN.CLI.Utils;

public class ModelDownloader
{
    private readonly HttpClient _httpClient;

    public ModelDownloader()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromHours(2);
    }

    public async Task DownloadModel(string modelName, string modelsPath, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(modelName))
            {
                Console.WriteLine("Please enter a valid model name");
                return;
            }

            var modelsMap = await LoadModelsMap();

            modelName = modelName.Trim();
            var modelFileName = $"{modelName.Trim()}.gguf";
            var modelFilePath = Path.Combine(modelsPath, modelFileName);
        
            if (File.Exists(modelFilePath))
            {
                Console.WriteLine($"Model '{modelName}' already exists at {modelsPath}. Skipping download...");
                return;
            }

            if (!modelsMap.TryGetValue(modelName, out var modelUrl))
            {
                Console.WriteLine($"Error: Model '{modelName}' not found in models map. Skipping...");
                return;
            }

            Console.WriteLine($"Downloading model: {modelName} from {modelUrl}");
        
            var stopwatch = Stopwatch.StartNew();
            await DownloadFileWithProgressAsync(modelUrl, modelFilePath, stopwatch, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error downloading model '{modelName}': {ex.Message}");
            Console.ResetColor();
        }
    }

    private static async Task<Dictionary<string, string>> LoadModelsMap()
    {
        var modelsMap = new Dictionary<string, string>();
        var scriptDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var modelsMapFile = Path.Combine(scriptDirectory!, "models_map.txt");
        
        if (!File.Exists(modelsMapFile))
        {
            throw new FileNotFoundException($"Models map file not found at {modelsMapFile}. Please provide a valid file.");
        }
        
        var lines = await File.ReadAllLinesAsync(modelsMapFile);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var split = line.Split('|');
            
            if (split.Length < 2) 
                continue;
            
            var key = split[0].Trim();
            var value = split[1].Trim();
            modelsMap[key] = value;
        }

        return modelsMap;
    }
    
    private async Task DownloadFileWithProgressAsync(string url, string filePath, Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? 0;
        var downloadedBytes = 0L;
        const int progressBarWidth = 20;

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            downloadedBytes += bytesRead;

            if (totalBytes <= 0) 
                continue;
            
            var progressPercentage = Math.Round((downloadedBytes / (double)totalBytes) * 100, 0);
            var downloadedMB = Math.Round(downloadedBytes / (1024.0 * 1024.0), 2);
            var totalMB = Math.Round(totalBytes / (1024.0 * 1024.0), 2);

            // Calculate download speed
            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            var speedMBps = elapsedSeconds > 0 ? Math.Round(downloadedBytes / (1024.0 * 1024.0) / elapsedSeconds, 2) : 0;

            // Calculate ETA
            var remainingBytes = totalBytes - downloadedBytes;
            var etaSeconds = speedMBps > 0 ? Math.Round(remainingBytes / (1024.0 * 1024.0) / speedMBps) : 0;
            var etaFormatted = TimeSpan.FromSeconds(etaSeconds).ToString(@"hh\:mm\:ss");

            // Build progress bar
            var progressChars = Math.Min(Math.Max((int)Math.Round(progressPercentage * progressBarWidth / 100), 0), progressBarWidth);
            var progressBar = "[" + new string('#', progressChars) + new string(' ', progressBarWidth - progressChars) + "]";

            // Clear current line and write progress
            Console.Write($"\r{progressBar} {progressPercentage}% ({downloadedMB} MB / {totalMB} MB) {speedMBps} MB/s ETA: {etaFormatted}     ");
        }
    }
}