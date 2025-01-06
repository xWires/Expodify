using System.CommandLine;

namespace Expodify.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
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
            IProgress<string> progress = new Progress<string>(Console.WriteLine);
            var extractor = new Extractor(progress);

            extractor.SourceFolder = source;
            extractor.OutputFolder = destination;
            
            await Task.Run(()=>extractor.Extract()).ContinueWith(t=>
            {
                if (t.IsFaulted)
                {
                    progress.Report(t.Exception.Message);
                    if (t.Exception.StackTrace != null) progress.Report(t.Exception.StackTrace);
                }
            });
        }, sourceArgument, destinationArgument);
        
        return await rootCommand.InvokeAsync(args);
    }
}