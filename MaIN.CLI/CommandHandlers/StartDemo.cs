using System.Text.RegularExpressions;
using MaIN.CLI.Utils;

namespace MaIN.CLI.CommandHandlers;

internal partial class CommandHandlers
{
    private bool Hard { get; set; }
    private List<string> Models { get; set; } = [];
    private bool NoApi { get; set; }
    private bool ApiOnly { get; set; }
    private bool NoImageGen { get; set; }
    private bool NoModels { get; set; }
    
    internal async Task StartDemo(string[] args)
    {
        ParseArgs(args);

        // Run setup tasks unless --api-only is provided
        if (!ApiOnly)
        {
            // Handle model downloads unless --no-models is specified
            if (!NoModels)
            {
                Console.WriteLine("Starting model downloads...");

                foreach (var model in Models)
                {
                    await HandleModelCommand(["download", model]);
                }
            }

            // Handle Image Generation API unless --no-image-gen is specified
            if (!NoImageGen)
            {
                Console.WriteLine("Starting Image Generation API as a background job...");
                await StartImageGen([]);
            }
        }

        // Start API unless --no-api is specified
        if (!NoApi)
        {
            Console.WriteLine("Starting main API...");
            var manager = new DockerManager();
            
            if (Hard)
            {
                await manager.ExecuteSetupAsync(hardReset: true);
            }
            else
            {
                await manager.ExecuteSetupAsync();
            }
        }
        
        
    }

    private void ParseArgs(string[] args)
    {
        var arguments = args.ToList();
        if (arguments.Count == 1)
        {
            arguments = Regex.Split(arguments[0], @"\s+(?=--)").ToList();
        }

        foreach (var arg in arguments)
        {
            Console.WriteLine($"Processing argument: {arg}");

            switch (arg)
            {
                case "--hard":
                    Hard = true;
                    break;
                case "--no-api":
                    NoApi = true;
                    break;
                case "--api-only":
                    ApiOnly = true;
                    break;
                case "--no-image-gen":
                    NoImageGen = true;
                    break;
                case "--no-models":
                    NoModels = true;
                    break;
                default:
                    var modelsMatch = Regex.Match(arg, @"^--models=(.+)$");
                    if (modelsMatch.Success)
                    {
                        Models = modelsMatch.Groups[1].Value.Split(',').ToList();
                    }
                    else if (!string.IsNullOrWhiteSpace(arg))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Warning: Unknown argument '{arg}'");
                        Console.ResetColor();
                    }
                    break;
            }
        }
    }
}