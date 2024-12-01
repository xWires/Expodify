using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Expodify.Views;

public partial class MainWindow : Window
{
    private IStorageFolder? _iPodFolder;
    private IStorageFolder? _outputFolder;
    private IStorageFolder? _iPodControl;
    private IStorageFolder? _musicFolder;
    
    public MainWindow()
    {
        InitializeComponent();
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
            if (c is IStorageFolder storageFolder)
            {
                if (storageFolder.Name == "iPod_Control") { _iPodControl = storageFolder; break; }
            }
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
        Log($"Found iPod_Control at {_iPodControl.Path.AbsolutePath}");
        
        var controlContents = _iPodControl.GetItemsAsync();
        await foreach (var c in controlContents)
        {
            if (c is IStorageFolder storageFolder)
            {
                if (storageFolder.Name == "Music") { _musicFolder = storageFolder; break; }
            }
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
        Log($"Found Music at {_musicFolder.Path.AbsolutePath}");
    }

    private async void SelectOutputFolder(object source, RoutedEventArgs args)
    {
        var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select output folder",
            AllowMultiple = false
        });
        
        if (folder.Count < 1) return;
        
        _outputFolder = folder[0];
        
        Log($"Set output folder to {_outputFolder.Path.AbsolutePath}");
    }

    private async void Extract(object source, RoutedEventArgs args)
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
        
        if (_outputFolder == null)
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
            var destinationPath = _outputFolder.Path.AbsolutePath + "Expodify-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            var destinationFolder = Directory.CreateDirectory(destinationPath);
            Log($"Created output folder at {destinationPath}");
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
    }

    private void Reset()
    {
        _iPodFolder = null;
        _outputFolder = null;
        _iPodControl = null;
        _musicFolder = null;
        
        openIPodButton.IsEnabled = true;
        selectOutputFolderButton.IsEnabled = true;
    }

    private void Log(string message)
    {
        logBox.Text += message + Environment.NewLine;
    }
}