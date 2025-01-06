using System;
using System.IO;
using System.Security.Cryptography;
using Expodify.ViewModels;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace Expodify.Tests.ViewModels;

[TestSubject(typeof(MainWindowViewModel))]
public class MainWindowViewModelTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MainWindowViewModelTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Theory]
    [InlineData("TrackTribe - Walk Through the Park.mp3")]
    [InlineData("TrackTribe - Walk Through the Park.mp4")]
    public void ExtractFile(string fileName)
    {
        var outputFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(outputFolder);
        var testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", fileName);
        var songName = TagLib.File.Create(testFilePath).Tag.Title;
        var copiedFilePath = MainWindowViewModel.CleanPath(outputFolder) + Path.DirectorySeparatorChar + MainWindowViewModel.ReplaceInvalidCharacters(songName) + Path.GetExtension(testFilePath);
        
        MainWindowViewModel.OutputFolder = outputFolder;
        MainWindowViewModel.ExtractSong(testFilePath, new Progress<string>());

        var expectedHash = ComputeFileHash(testFilePath);
        var actualHash = ComputeFileHash(copiedFilePath);
        
        _testOutputHelper.WriteLine($"Expected Hash: {expectedHash}");
        _testOutputHelper.WriteLine($"Actual Hash: {actualHash}");
        Assert.Equal(expectedHash, actualHash);
    }

    private static string ComputeFileHash(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}