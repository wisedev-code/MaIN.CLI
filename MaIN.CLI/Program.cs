using Commands = MaIN.CLI.CommandHandlers.CommandHandlers;

namespace MaIN.CLI;

public class Program
{
    // Global variables
    //private static string MCLI_ROOT = AppDomain.CurrentDomain.BaseDirectory;
    //private static string ModelsPath = Environment.GetEnvironmentVariable("MaIN_ModelsPath");

    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Commands.ShowUsage();
            return;
        }

        var command = args[0];
        var arguments = args.Skip(1).ToArray();

        switch (command.ToLower())
        {
            case "start-demo":
                await Commands.StartDemo(arguments);
                break;
            case "api":
                await Commands.StartApi(arguments);
                break;
            case "image-gen":
                await Commands.StartImageGen(arguments);
                break;
            case "model":
                await Commands.HandleModelCommand(arguments);
                break;
            case "config":
                await Commands.HandleConfigCommand(arguments);
                break;
            case "infer":
                await Commands.HandleInferCommand(arguments);
                break;
            case "uninstall":
                await Commands.Uninstall();
                break;
            case "help":
                if (arguments.Length > 0)
                {
                    Commands.ShowCommandHelp(arguments[0]);
                }
                else
                {
                    Commands.ShowUsage();
                }

                break;
            default:
                Commands.ShowUsage();
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