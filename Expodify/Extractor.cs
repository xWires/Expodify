namespace Expodify;

public class Extractor
{
   public DirectoryInfo? OutputFolder { get; set; }
   public DirectoryInfo? SourceFolder { get; set; }

   private DirectoryInfo? _iPodControl;
   private DirectoryInfo? _musicFolder;
   
   private readonly IProgress<string>? _progress;

   public Extractor(IProgress<string>? progress)
   {
      _progress = progress;
   }
   
   public async Task Extract()
   {
      if (SourceFolder == null) throw new InvalidOperationException("SourceFolder is null");
      if (OutputFolder == null) throw new InvalidOperationException("OutputFolder is null");
      
      foreach (var folder in SourceFolder.EnumerateDirectories())
      {
         if (folder.Name == "iPod_Control")
         {
            _iPodControl = folder;
            Log($"Found iPod_Control at {folder.FullName}");
            break;
         }
      }
      if (_iPodControl == null) throw new DirectoryNotFoundException("SourceFolder does not contain iPod_Control");

      foreach (var folder in _iPodControl.EnumerateDirectories())
      {
         if (folder.Name == "Music")
         {
            _musicFolder = folder;
            Log($"Found Music at {folder.FullName}");
            break;
         }
      }
      if (_musicFolder == null) throw new DirectoryNotFoundException("iPod_Control does not contain Music");

      // Loop through each of the "F" folders (e.g. F00, F01, F02, etc.)
      foreach (var folder in _musicFolder.EnumerateDirectories())
      {
         foreach (var file in folder.EnumerateFiles())
         {
            await Task.Run(()=>ExtractSong(file.FullName));
         }
      }
   }

   internal void ExtractSong(string path)
   {
      TagLib.File file;
      try
      {
         file = TagLib.File.Create(path);
      }
      catch (Exception e)
      {
         Log($"ERROR: Failed to open {path}");
         Log(e.Message);
         if (e.StackTrace != null) Log(e.StackTrace);
         return;
      }
      var songName = file.Tag.Title;
      Log($"Extracting \"{songName}\"");

      if (songName == null)
      {
         Log("WARNING: Could not determine song title, it will be given a random name instead");
         songName = "Unknown_" + Path.GetRandomFileName().Substring(0, 8);
      }

      var newPath = CleanPath(OutputFolder!.FullName) + Path.DirectorySeparatorChar + ReplaceInvalidCharacters(songName);
      if (File.Exists(newPath + Path.GetExtension(path)))
      {
         Log($"WARNING: {newPath} already exists, it will have random letters added to the end of the file name.");
         newPath += "_" + Path.GetRandomFileName().Substring(0, 8);
      }

      newPath += Path.GetExtension(path);
        
      File.Copy(path, newPath, false);
      Log($"Extracted \"{songName}\" to {newPath}");
   }

   private static string CleanPath(string path)
   {
      var prefix = Environment.OSVersion.Platform == PlatformID.Win32NT ? "file:///" : "file://";
      return path.StartsWith(prefix) ? path.Substring(prefix.Length) : path;
   }

   private static string ReplaceInvalidCharacters(string path)
   {
      return string.Join("_", path.Split(Path.GetInvalidFileNameChars()));
   }
   
   private void Log(string message)
   {
      _progress?.Report(message);
   }
}