namespace MaIN.CLI.CommandHandlers;

internal partial class CommandHandlers
{
    internal void ShowCommandHelp(string cmd)
    {
        switch (cmd.ToLower())
        {
            case "start-demo":
                Console.WriteLine(@"
mcli start-demo - Start all MaIN services

Usage:
    mcli start-demo [options]

Options:
    --hard           Perform hard cleanup of containers before starting
    --no-api         Skip starting the API
    --no-models      Skip model downloads
    --no-image-gen   Skip image generation
    --models=<list>  Specify comma-separated list of models to download

Examples:
    mcli start-demo
    mcli start-demo --no-image-gen
    mcli start-demo --models=gemma2-2b");
                break;
            case "api":
                Console.WriteLine(@"
mcli api - Start the MaIN API service

Usage:
    mcli api [options]

Options:
    --hard    Perform hard cleanup of containers before starting

Examples:
    mcli api
    mcli api --hard");
                break;
            case "image-gen":
                Console.WriteLine(@"
mcli image-gen - Start the image generation service

Usage:
    mcli image-gen

Examples:
    mcli image-gen");
                break;
            case "model":
                Console.WriteLine(@"
mcli model - Manage MaIN models

Usage:
    mcli model <subcommand>

Subcommands:
    download <name>   Download a specific model
    list              List available models
    present           List installed models

Examples:
    mcli model download llama3.2-3b
    mcli model list
    mcli model present");
                break;
            case "config":
                Console.WriteLine(@"
mcli config - Configure environment variables

Usage:
    mcli config set

This command will allow you to choose which environment variable to update:
    1) MaIN_ModelsPath
    2) OPENAI_API_KEY
    3) GEMINI_API_KEY

Examples:
    mcli config set");
                break;
            case "infer":
                Console.WriteLine(@"
mcli infer - Run the minimal Infer chat interface

Usage:
    mcli infer chat [--model <model>] [--path <modelsPath>] [--backend <backend>] [--urls=<url>]

Options:
    --model     Specify the model to use
    --path      Specify the model path (required if --model is provided) used to load custom models
    --backend   Specify the backend (e.g., 'openai')

Example:
    mcli infer chat --model gemma2:2b
    mcli infer chat --model o1-mini --backend openai");
                break;
            default:
                ShowUsage();
                break;
        }
    }
}