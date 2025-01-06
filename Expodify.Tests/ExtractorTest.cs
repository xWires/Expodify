using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace Expodify.Tests;

[TestSubject(typeof(Extractor))]
public class ExtractorTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ExtractorTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task Extract()
    {
        var outputFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(outputFolder);
        var testIPod = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles");
        var testFilesPath = Path.Combine(testIPod, "iPod_Control", "Music", "F00");

        var extractor = new Extractor(new Progress<string>(Console.WriteLine));
        extractor.SourceFolder = new DirectoryInfo(testIPod);
        extractor.OutputFolder = new DirectoryInfo(outputFolder);
        await Task.Run(()=>extractor.Extract()).ContinueWith(t=>
        {
            if (t.IsFaulted)
            {
                _testOutputHelper.WriteLine(t.Exception.Message);
                foreach (var e in t.Exception.InnerExceptions)
                {
                    _testOutputHelper.WriteLine(e.Message);
                    _testOutputHelper.WriteLine(e.StackTrace);
                }
            }
        });
        
        foreach (var sourceFile in new DirectoryInfo(testFilesPath).EnumerateFiles())
        {
            var copiedFile = Path.Combine(outputFolder, TagLib.File.Create(sourceFile.FullName).Tag.Title) + Path.GetExtension(sourceFile.FullName);
            
            var expectedHash = ComputeFileHash(sourceFile.FullName);
            var actualHash = ComputeFileHash(copiedFile);
            _testOutputHelper.WriteLine($"File: {sourceFile.FullName}");
            _testOutputHelper.WriteLine($"Copied File: {copiedFile}");
            _testOutputHelper.WriteLine($"Expected Hash: {expectedHash}");
            _testOutputHelper.WriteLine($"Actual Hash: {actualHash}");
            
            Assert.Equal(expectedHash, actualHash);
        }
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