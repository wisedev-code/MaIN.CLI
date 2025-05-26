using MaIN.CLI.Utils;

namespace MaIN.CLI.CommandHandlers;

internal partial class CommandHandlers
{
    internal async Task StartApi(string[] args)
    {
        var manager = new DockerManager();
        
        if (args.Length != 0 && args[0] == "--hard")
        {
            await manager.ExecuteSetupAsync(hardReset: true);
        }
        else
        {
            await manager.ExecuteSetupAsync();
        }
    }
}