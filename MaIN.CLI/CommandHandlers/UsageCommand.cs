namespace MaIN.CLI.CommandHandlers;

internal static partial class CommandHandlers
{
    internal static void ShowUsage()
    {
        Console.WriteLine(@"
MaIN CLI (mcli) - Command Line Interface for MaIN

Usage:
    mcli <command> [options]

Commands:
    start-demo       Start all services (API, image generation, and download models)
    api              Start only the API service
    image-gen        Start only the image generation service
    model            Download and manage models
    config           Configure environment variables
    infer            Run the minimal Infer chat interface
    help             Show this help message
    uninstall        Uninstall mcli

Examples:
    mcli start-demo
    mcli api --hard
    mcli model download gemma2-2b
    mcli config set
    mcli infer chat --model llama3.2:3b");
    }
}