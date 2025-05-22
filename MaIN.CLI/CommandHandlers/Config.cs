namespace MaIN.CLI.CommandHandlers;

internal static partial class CommandHandlers
{
    internal static async Task HandleConfigCommand(string[] args)
    {
        if (args.Length == 0 || args[0].ToLower() != "set")
        {
            ShowCommandHelp("config");
            return;
        }

        Console.WriteLine("Which environment variable do you want to set?");
        Console.WriteLine("1) MaIN_ModelsPath");
        Console.WriteLine("2) OPENAI_API_KEY");
        Console.WriteLine("3) GEMINI_API_KEY");
        Console.Write("Enter your choice (1 or 2 or 3): ");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Console.Write("Please provide the local path where models will be stored: ");
                var modelsPath = Console.ReadLine();
                if (!Directory.Exists(modelsPath))
                {
                    Console.WriteLine("The provided path does not exist. Creating the directory...");
                    Directory.CreateDirectory(modelsPath);
                }

                Environment.SetEnvironmentVariable("MaIN_ModelsPath", modelsPath, EnvironmentVariableTarget.User);
                var newModelsPath =
                    Environment.GetEnvironmentVariable("MaIN_ModelsPath", EnvironmentVariableTarget.User);
                Console.WriteLine($"MaIN_ModelsPath set to: {newModelsPath}");
                break;
            case "2":
                Console.Write("Please provide your OpenAI API key: ");
                var openaiKey = Console.ReadLine();
                Environment.SetEnvironmentVariable("OPENAI_API_KEY", openaiKey, EnvironmentVariableTarget.User);
                Console.WriteLine($"OPENAI_API_KEY set successfully");
                break;
            case "3":
                Console.Write("Please provide your Gemini API key: ");
                var geminiKey = Console.ReadLine();
                Environment.SetEnvironmentVariable("GEMINI_API_KEY", geminiKey, EnvironmentVariableTarget.User);
                Console.WriteLine("GEMINI_API_KEY set successfully");
                break;
            default:
                Console.WriteLine("Invalid choice. Exiting.");
                break;
        }
    }
}