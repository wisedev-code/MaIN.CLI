using System.Diagnostics;
using MaIN.CLI.Utils;

namespace MaIN.CLI.CommandHandlers;

internal partial class CommandHandlers
{
    internal async Task StartImageGen(string[] args)
    {
        var manager = new PythonManager();

        var python = await manager.GetOrInstallPythonAsync();
        
        await manager.VerifyPythonInstallation(python);
        
        // TODO: This process for some reason hangs indefinitely. Fix it. 
        //await manager.InstallDependencies(python);

        // TODO: Some issue with process - api doesn't seem starting
        //var imageGenApi = new ImageGenApiWrapper(python);
        Console.WriteLine("[NOT WORKING] Image Generation API is running.");
    }
}