using System;
using System.IO;
using System.Security.Cryptography;
using Expodify.Views;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace Expodify.Tests.Views;

[TestSubject(typeof(MainWindow))]
public class MainWindowTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MainWindowTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ExtractMp3()
    {
        var outputFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(outputFolder);
        var testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "TrackTribe - Walk Through the Park.mp3");
        var songName = TagLib.File.Create(testFilePath).Tag.Title;
        var copiedFilePath = Expodify.Views.MainWindow.CleanPath(outputFolder) + Path.DirectorySeparatorChar + Expodify.Views.MainWindow.ReplaceInvalidCharacters(songName) + Path.GetExtension(testFilePath);
        
        Expodify.Views.MainWindow._outputFolder = outputFolder;
        Expodify.Views.MainWindow.ExtractSong(testFilePath, new Progress<string>());

        var expectedHash = ComputeFileHash(testFilePath);
        var actualHash = ComputeFileHash(copiedFilePath);
        
        _testOutputHelper.WriteLine($"Expected Hash: {expectedHash}");
        _testOutputHelper.WriteLine($"Actual Hash: {actualHash}");
        Assert.Equal(expectedHash, actualHash);
    }
    
    public static string ComputeFileHash(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}