using MaIN.CLI.Utils;

namespace MaIN.CLI.CommandHandlers;

internal partial class CommandHandlers
{
    internal async Task HandleModelCommand(string[] args)
    {
        var modelsPath = Environment.GetEnvironmentVariable("MaIN_ModelsPath");

        if (modelsPath == null)
        {
            Console.WriteLine("MaIN models path variable not set");
            return;
        }

        if (args.Length == 0)
        {
            ShowCommandHelp("model");
            return;
        }

        var subcommand = args[0];
        var modelArgs = args.Skip(1).ToArray();

        switch (subcommand.ToLower())
        {
            case "download":
                if (modelArgs.Length == 0)
                {
                    Console.WriteLine("Error: Model name required");
                    return;
                }

                await new ModelDownloader().DownloadModel(modelArgs[0], modelsPath, CancellationToken.None);

                break;
            case "list":
                var modelsMap = GetModelsMap();
                if (modelsMap == null)
                    return;
                Console.WriteLine("Available models:");
                foreach (var model in modelsMap.Keys.OrderBy(k => k))
                {
                    Console.WriteLine($"- {model}");
                }

                break;
            case "present":
                Console.WriteLine("Downloaded models:");
                Console.WriteLine($"Models path: {modelsPath}");

                if (Directory.Exists(modelsPath))
                {
                    var downloadedModels = Directory.GetFiles(modelsPath, "*.gguf")
                        .Select(Path.GetFileNameWithoutExtension)
                        .ToList();

                    if (downloadedModels.Count == 0)
                    {
                        Console.WriteLine($"No models found in {modelsPath}");
                    }
                    else
                    {
                        foreach (var model in downloadedModels.OrderBy(m => m))
                        {
                            Console.WriteLine($"- {model}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Models directory {modelsPath} not found");
                }

                break;
            case "update":
                Console.WriteLine("[NOT IMPLEMENTED] Updating all installed models... ");
                //await RunScriptEquivalent("download-models.ps1", Array.Empty<string>());
                break;
            default:
                ShowCommandHelp("model");
                break;
        }
    }

    private static Dictionary<string, string> GetModelsMap()
    {
        var modelsMapFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models_map.txt");
        if (!File.Exists(modelsMapFile))
        {
            Console.WriteLine($"Models map file not found at {modelsMapFile}. Please provide a valid file.");
            return null;
        }

        var modelsMap = new Dictionary<string, string>();
        foreach (var line in File.ReadAllLines(modelsMapFile))
        {
            var split = line.Split('|');
            if (split.Length < 2)
                continue;

            var key = split[0].Trim();
            var value = split[1].Trim();
            modelsMap[key] = value;
        }

        return modelsMap;
    }
}