using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Expodify.GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private IStorageFolder? _iPodFolder;
    private IStorageFolder? _baseOutputFolder;
    private static string? _outputFolder;

    [ObservableProperty] private bool _isOpenIPodButtonEnabled;
    [ObservableProperty] private bool _isSelectOutputFolderButtonEnabled;
    [ObservableProperty] private bool _isExtractButtonEnabled;
    
    public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

    private readonly Window _mainWindow;

    public MainWindowViewModel(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }
    
    [RelayCommand]
    private async Task OpenIPodFolder()
    {
        Reset();
        var folder = await _mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Open iPod folder",
            AllowMultiple = false
        });

        if (folder.Count < 1) return;
        
        _iPodFolder = folder[0];
        
        Log($"Set iPod folder to {CleanPath(_iPodFolder.Path.ToString())}");
    }

    [RelayCommand]
    private async Task SelectOutputFolder()
    {
        var folder = await _mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select output folder",
            AllowMultiple = false
        });
        
        if (folder.Count < 1) return;
        
        _baseOutputFolder = folder[0];
        
        Log($"Set output folder to {CleanPath(_baseOutputFolder.Path.ToString())}");
    }

    [RelayCommand]
    private async Task Extract()
    {
        if (_iPodFolder == null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "iPod folder not selected",
                    "You must open a folder containing iPod data first.",
                    ButtonEnum.Ok,
                    Icon.Error)
                .ShowAsync();
            return;
        }
        
        if (_baseOutputFolder == null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Output folder not selected",
                    "You must choose an output folder first.",
                    ButtonEnum.Ok,
                    Icon.Error)
                .ShowAsync();
            return;
        }
        
        IsOpenIPodButtonEnabled = false;
        IsSelectOutputFolderButtonEnabled = false;
        IsExtractButtonEnabled = false;
        
        Log("Starting extraction");

        try
        {
            _outputFolder = _baseOutputFolder.Path.AbsolutePath + "Expodify-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            Directory.CreateDirectory(_outputFolder);
            Log($"Created output folder at {_outputFolder}");
        }
        catch (PathTooLongException)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Failed to create a folder",
                    "The path was too long.",
                    ButtonEnum.Ok,
                    Icon.Error)
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
                    Icon.Error)
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
                    Icon.Error)
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
                    Icon.Error)
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
                    Icon.Error)
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
                    Icon.Error)
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
                    Icon.Error)
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
                    Icon.Error)
                .ShowAsync();
            Reset();
            return;
        }

        IProgress<string> progress = new Progress<string>(Log);
        var extractor = new Extractor(progress);

        extractor.OutputFolder = new DirectoryInfo(_outputFolder);
        extractor.SourceFolder = new DirectoryInfo(_iPodFolder.Path.AbsolutePath);
        await Task.Run(()=>extractor.Extract()).ContinueWith(t=>
        {
            if (t.IsFaulted)
            {
                progress.Report(t.Exception.Message);
                if (t.Exception.StackTrace != null) progress.Report(t.Exception.StackTrace);
            }
        });
        
        Log("Finished extraction");
        Log("Saving log");
        var logPath = Path.Combine(_outputFolder, "Expodify.log");
        await Task.Run(()=>File.AppendAllLinesAsync(logPath, Logs));
        Log($"Saved log to {logPath}");
        Reset();
    }

    private void Reset()
    {
        _iPodFolder = null;
        _baseOutputFolder = null;
        _outputFolder = null;
        
        IsOpenIPodButtonEnabled = true;
        IsSelectOutputFolderButtonEnabled = true;
        IsExtractButtonEnabled = true;
    }
    
    private static string CleanPath(string path)
    {
        var prefix = Environment.OSVersion.Platform == PlatformID.Win32NT ? "file:///" : "file://";
        return path.StartsWith(prefix) ? path.Substring(prefix.Length) : path;
    }

    private void Log(string message)
    {
        Logs.Add(message);
    }
}