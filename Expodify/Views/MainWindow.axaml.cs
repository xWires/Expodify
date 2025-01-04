using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Expodify.Views;

public partial class MainWindow : Window
{
    private IStorageFolder? _iPodFolder;
    private IStorageFolder? _baseOutputFolder;
    internal static string? _outputFolder;
    private IStorageFolder? _iPodControl;
    private IStorageFolder? _musicFolder;
    
    public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private async void OpenIPodFolder(object source, RoutedEventArgs args)
    {
        Reset();
        var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Open iPod folder",
            AllowMultiple = false
        });

        if (folder.Count < 1) return;
        
        _iPodFolder = folder[0];

        var contents = _iPodFolder.GetItemsAsync();
        await foreach (var c in contents)
        {
            if (c is not IStorageFolder storageFolder) continue;
            if (storageFolder.Name == "iPod_Control") { _iPodControl = storageFolder; break; }
        }

        if (_iPodControl == null)
        {
            Log("Could not find iPod_Control folder");
            await MessageBoxManager.GetMessageBoxStandard(
                    "Invalid Folder",
                    "The folder you selected does not contain an \"iPod_Control\" folder.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            return;
        }
        Log($"Found iPod_Control at {CleanPath(_iPodControl.Path.ToString())}");
        
        var controlContents = _iPodControl.GetItemsAsync();
        await foreach (var c in controlContents)
        {
            if (c is not IStorageFolder storageFolder) continue;
            if (storageFolder.Name == "Music") { _musicFolder = storageFolder; break; }
        }
        
        if (_musicFolder == null)
        {
            Log("Could not find Music folder");
            await MessageBoxManager.GetMessageBoxStandard(
                    "No music found",
                    "The folder you selected does not contain any music.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            return;
        }
        Log($"Found Music at {CleanPath(_musicFolder.Path.ToString())}");
    }

    private async void SelectOutputFolder(object source, RoutedEventArgs args)
    {
        var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select output folder",
            AllowMultiple = false
        });
        
        if (folder.Count < 1) return;
        
        _baseOutputFolder = folder[0];
        
        Log($"Set output folder to {CleanPath(_baseOutputFolder.Path.ToString())}");
    }

    internal async void Extract(object source, RoutedEventArgs args)
    {
        if (_iPodFolder == null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "iPod folder not selected",
                    "You must open a folder containing iPod data first.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            return;
        }
        
        if (_baseOutputFolder == null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Output folder not selected",
                    "You must choose an output folder first.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            return;
        }
        
        openIPodButton.IsEnabled = false;
        selectOutputFolderButton.IsEnabled = false;
        extractButton.IsEnabled = false;
        
        Log("Starting extraction");

        try
        {
            _outputFolder = _baseOutputFolder.Path.AbsolutePath + "Expodify-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            var destinationFolder = Directory.CreateDirectory(_outputFolder);
            Log($"Created output folder at {_outputFolder}");
        }
        catch (PathTooLongException)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Failed to create a folder",
                    "The path was too long.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            Reset();
            return;
        }
        catch (DirectoryNotFoundException)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Failed to create a folder",
                    "The folder you selected does not exist.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            Reset();
            return;
        }
        catch (IOException)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Failed to create a folder",
                    "An error occured whilst trying to create a folder.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            Reset();
            return;
        }
        catch (UnauthorizedAccessException)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Failed to create a folder",
                    "You do not have permission to create files/folders in the output folder.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            Reset();
            return;
        }
        catch (ArgumentNullException)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Failed to create a folder",
                    "The path is null.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            Reset();
            return;
        }
        catch (ArgumentException)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Failed to create a folder",
                    "The path contains illegal characters.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            Reset();
            return;
        }
        catch (NotSupportedException)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Failed to create a folder",
                    "The path contains illegal characters.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            Reset();
            return;
        }
        catch (Exception)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Failed to create a folder",
                    "An unknown error occured whilst trying to create a folder.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
            Reset();
            return;
        }

        // Loop through each of the "F" folders (e.g. F00, F01, F02, etc.)
        await foreach (var item in _musicFolder!.GetItemsAsync())
        {
            if (item is not IStorageFolder folder) continue;
            Log($"Looking for music at {folder.Path.AbsolutePath}");

            await foreach (var song in folder.GetItemsAsync())
            {
                if (song is not IStorageFile) continue;
                var progress = new Progress<string>(Log);
                await Task.Run(()=>ExtractSong(CleanPath(song.Path.ToString()), progress));
            }
        }
        
        Log("Finished extraction");
        Log("Saving log");
        var logPath = Path.Combine(_outputFolder, "Expodify.log");
        await Task.Run(()=>File.AppendAllLinesAsync(logPath, Logs));
        Log($"Saved log to {logPath}");
        Reset();
    }

    internal static void ExtractSong(string path, IProgress<string> progress)
    {
        TagLib.File file;
        try
        {
            file = TagLib.File.Create(path);
        }
        catch (Exception e)
        {
            progress.Report($"ERROR: Failed to open {path}");
            progress.Report(e.Message);
            if (e.StackTrace != null) progress.Report(e.StackTrace);
            return;
        }
        var songName = file.Tag.Title;
        progress.Report($"Extracting \"{songName}\"");

        if (songName == null)
        {
            progress.Report("WARNING: Could not determine song title, it will be given a random name instead");
            songName = "Unknown_" + Path.GetRandomFileName().Substring(0, 8);
        }

        var newPath = CleanPath(_outputFolder!) + Path.DirectorySeparatorChar + ReplaceInvalidCharacters(songName);
        if (File.Exists(newPath + Path.GetExtension(path)))
        {
            progress.Report($"WARNING: {newPath} already exists, it will have random letters added to the end of the file name.");
            newPath += "_" + Path.GetRandomFileName().Substring(0, 8);
        }

        newPath += Path.GetExtension(path);
        
        File.Copy(path, newPath, false);
        progress.Report($"Extracted \"{songName}\" to {newPath}");
    }

    internal static string CleanPath(string path)
    {
        var prefix = Environment.OSVersion.Platform == PlatformID.Win32NT ? "file:///" : "file://";
        return path.StartsWith(prefix) ? path.Substring(prefix.Length) : path;
    }

    internal static string ReplaceInvalidCharacters(string path)
    {
        return string.Join("_", path.Split(Path.GetInvalidFileNameChars()));
    }

    private void Reset()
    {
        _iPodFolder = null;
        _baseOutputFolder = null;
        _outputFolder = null;
        _iPodControl = null;
        _musicFolder = null;
        
        openIPodButton.IsEnabled = true;
        selectOutputFolderButton.IsEnabled = true;
        extractButton.IsEnabled = true;
    }

    private void Log(string message)
    {
        Logs.Add(message);
        logBox.ScrollIntoView(message);
    }
}