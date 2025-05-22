
Console.WriteLine("Installing MaIN.CLI...");

var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var cliPath = Path.Combine(localAppDataPath, "MaIN", "CLI");

Directory.CreateDirectory(cliPath);

string[] directoriesToCopy = ["ImageGen", "infer", "server"];

string[] filesToCopy =
[
    "docker-compose.yml", 
    "mcli.deps.json", 
    "mcli.dll",
    "mcli.exe",
    "mcli.pdb",
    "mcli.runtimeconfig.json",
    "models_map.txt"
];

foreach (string dirName in directoriesToCopy)
{
    if (Directory.Exists(dirName))
    {
        Console.WriteLine($"Copying directory: {dirName}");
        CopyDirectory(dirName, Path.Combine(cliPath, dirName));
    }
    else
    {
        Console.WriteLine($"Directory not found: {dirName}");
    }
    
    foreach (string fileName in filesToCopy)
    {
        if (File.Exists(fileName))
        {
            Console.WriteLine($"Copying file: {fileName}");
            File.Copy(fileName, Path.Combine(cliPath, fileName), true);
        }
        else
        {
            Console.WriteLine($"File not found: {fileName}");
        }
    }
            
    Console.WriteLine("Installation completed! You can now use CLI by calling 'mcli' in terminal.");
}

static void CopyDirectory(string sourceDir, string destinationDir)
{
    Directory.CreateDirectory(destinationDir);
        
    // Copy all files
    foreach (string file in Directory.GetFiles(sourceDir))
    {
        string fileName = Path.GetFileName(file);
        File.Copy(file, Path.Combine(destinationDir, fileName), true);
    }
        
    // Copy all subdirectories
    foreach (string subDir in Directory.GetDirectories(sourceDir))
    {
        string dirName = Path.GetFileName(subDir);
        CopyDirectory(subDir, Path.Combine(destinationDir, dirName));
    }
}
