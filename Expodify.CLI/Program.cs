using System.CommandLine;

namespace Expodify.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        int returnCode = 0;
        
        var rootCommand = new RootCommand("A tool to extract music from iPods.");

        var sourceArgument = new Argument<DirectoryInfo>(
            name: "source",
            description: "The iPod to extract music from.");
        rootCommand.AddArgument(sourceArgument);
        
        var destinationArgument = new Argument<DirectoryInfo>(
            name: "destination",
            description: "The folder to put the music in.");
        rootCommand.AddArgument(destinationArgument);
        
        rootCommand.SetHandler(async (source, destination) =>
        {
            if (!source.Exists)
            {
                Console.WriteLine("Source directory does not exist.");
                returnCode = 1;
                return;
            }

            if (!destination.Exists)
            {
                Console.WriteLine("Destination directory does not exist.");
                returnCode = 1;
                return;
            }
            
            IProgress<string> progress = new Progress<string>(Console.WriteLine);
            var extractor = new Extractor(progress);

            extractor.SourceFolder = source;
            extractor.OutputFolder = destination;
            
            await Task.Run(()=>extractor.Extract()).ContinueWith(t=>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine(t.Exception.Message);
                    if (t.Exception.StackTrace != null) Console.WriteLine(t.Exception.StackTrace);
                    returnCode = 1;
                }
            });
        }, sourceArgument, destinationArgument);
        
        await rootCommand.InvokeAsync(args);
        
        return returnCode;
    }
}