namespace MaIN.CLI;

public class Program
{
    // Global variables
    //private static string MCLI_ROOT = AppDomain.CurrentDomain.BaseDirectory;
    //private static string ModelsPath = Environment.GetEnvironmentVariable("MaIN_ModelsPath");

    public static async Task Main(string[] args)
    {
        var commandHandler = new CommandHandlers.CommandHandlers();
        
        if (args.Length == 0)
        {
            commandHandler.ShowUsage();
            return;
        }

        var command = args[0];
        var arguments = args.Skip(1).ToArray();

        switch (command.ToLower())
        {
            case "start-demo":
                await commandHandler.StartDemo(arguments);
                break;
            case "api":
                await commandHandler.StartApi(arguments);
                break;
            case "image-gen":
                await commandHandler.StartImageGen(arguments);
                break;
            case "model":
                await commandHandler.HandleModelCommand(arguments);
                break;
            case "config":
                await commandHandler.HandleConfigCommand(arguments);
                break;
            case "infer":
                await commandHandler.HandleInferCommand(arguments);
                break;
            case "uninstall":
                await commandHandler.Uninstall();
                break;
            case "help":
                if (arguments.Length > 0)
                {
                    commandHandler.ShowCommandHelp(arguments[0]);
                }
                else
                {
                    commandHandler.ShowUsage();
                }

                break;
            default:
                commandHandler.ShowUsage();
                if (!string.IsNullOrEmpty(command))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nError: Unknown command '{command}'");
                    Console.ResetColor();
                }

                break;
        }
    }
}