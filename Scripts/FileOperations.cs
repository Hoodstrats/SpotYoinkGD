/* https://twitter.com/HoodStrats || https://github.com/Hoodstrats */

using Godot;
using System;
using Directory = System.IO.Directory;
using File = System.IO.File;
using System.Collections.Generic;
using Environment = System.Environment;
using Path = System.IO.Path;

public static class FileOperations
{
  static RichTextLabel _fileOps;
  static LineEdit _selectedFolder;
  static TextureRect _imageRect;
  static Label _fileLabel;

  public static string SelectedDir { get => _selectedDir; set => _selectedDir = value; }
  public static bool CanConvert { get => _canConvert; set => _canConvert = value; }
  public static bool HasFiles { get => _hasFiles; set => _hasFiles = value; }
  public static List<string> FilesInFolder { get => _filesInFolder; set => _filesInFolder = value; }
  public static int ViewingIndex { get => _viewingIndex; set => _viewingIndex = value; }

  static bool _canConvert = false;
  static bool _hasFiles = false;
  static string _userProfPath;
  static string _spotLightLoc;
  static string _userPictures;
  //the directory the user selects, set this when using filedialog
  static string _selectedDir = string.Empty;
  static List<string> _filesInFolder;

  //keep track of where we our in the navigation menu
  static int _viewingIndex = 0;
  public static void InitNodes(RichTextLabel fileops, LineEdit selectedFolder, TextureRect imageRect, Label fileLabel)
  {
    _fileOps = fileops;
    _selectedFolder = selectedFolder;
    _imageRect = imageRect;
    _fileLabel = fileLabel;
  }
  public static void InitData()
  {
    //windows default location for the spotlight folder
    _userProfPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    _userPictures = $@"{_userProfPath}\Pictures\Spotlight";
    _spotLightLoc = $@"{_userProfPath}\Appdata\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets";

    if (System.IO.Directory.Exists(_spotLightLoc))
    {
      //letting the user know that the program found the spotlight and clearing out text field
      _fileOps.BbcodeText = "[color=green]Found Windows Spotlight directory. Ready to work...[/color]\n";

      if (System.IO.Directory.Exists(_userPictures))
      {
        _canConvert = true;
        _selectedDir = _userPictures;
        _selectedFolder.Text = _userPictures;
        _fileOps.BbcodeText += "[color=green]Looks like you've already been here before. Welcome back...[/color]\n";
        //if the folder exists check if we've already used the program today
        CheckIfCanConvert(_userPictures);
        //initialize the wallpaper 
        NavigatePics();
      }
      else
      {
        _canConvert = true;
        //inform the user we're gonna make a folder for them if they use the program
        _selectedDir = _userPictures;
        _selectedFolder.Text = $"Will create a folder at {_userPictures}.";
        _fileOps.BbcodeText += $"[color=yellow]Pressing Convert will create a folder at {_userPictures}...[/color]\n";
      }
    }
    else
    {
      _fileOps.BbcodeText = "[color=red]Looks like I can't locate the Windows Spotlight Directory...[/color]\n";
      _fileOps.BbcodeText += "[color=red]I was only designed to work with Windows 7 or later...[/color]\n";
    }
  }
  static void CheckIfCanConvert(string dest)
  {
    if (Directory.GetFiles(dest).Length != 0)
    {
      //populate current list of files and grab last index
      var lastFile = GetLastFileIndex(dest);

      //check if the last file's creation date is the same as current
      if (GetIfUsedToday(_filesInFolder[lastFile]))
      {
        //avoid any issues with user trying to convert twice in 1 day 
        _canConvert = false;
        _fileOps.BbcodeText += "[color=red]It seems like you've already used the app today...[/color]\n";
        _fileOps.BbcodeText += "[color=red]Try again tommorow. Spotlight only updates so many times...[/color]\n";
      }
    }
  }
  //returns the last index of JPG files in the folder
  static int GetLastFileIndex(string createdFolder)
  {
    _filesInFolder = new List<string>();
    foreach (string s in Directory.GetFiles(createdFolder, "*.jpg"))
    {
      _filesInFolder.Add(s);
    }

    //grab the last entry in the list and make the current index that
    return _filesInFolder.Count - 1;
  }

  //make a method that returns the creation date of the last file in the folder
  //if the date matches today's date then we've already used the program so stop operation/delete files grabbed 
  static bool GetIfUsedToday(string path)
  {
    //get file name raw so we can split at _
    var named = Path.GetFileNameWithoutExtension(path);
    var split = named.LastIndexOf("_") + 1;
    var sub = named.Substring(split);
    //substring split here 
    if (sub == DateTime.Now.ToString("MMM-d-yyyy"))
    {
      return true;
    }
    return false;
  }
  public static void ConvertSpotLight()
  {
    //make sure the spotlight folder actually exists
    Directory.CreateDirectory(_selectedDir);

    //make sure our selected folder text box is set correctly 
    _selectedFolder.Text = _selectedDir;

    _fileOps.BbcodeText += $"[color=yellow]Starting Yoink process...[/color]\n";
    //grab the files from the spotlight here 
    string[] files = Directory.GetFiles(_spotLightLoc);
    foreach (string s in files)
    {
      string fileName = Path.GetFileName(s);
      string destFileName = Path.Combine(_selectedDir, fileName + ".jpg");
      //looping through s is looping through the files themselves full adress so we have to copy the s
      File.Copy(s, destFileName, true);
      //the file, destination and whether or not to overwrite
      _fileOps.BbcodeText += $"[color=green]Copied {destFileName}...[/color]\n";
    }
    DeleteGarbage();
  }

  //can prolly seperate the logic from the scene and keep the operations in a static class 
  private static void DeleteGarbage()
  {
    _fileOps.BbcodeText += $"[color=red]Filtering out the NON 1080p wallpapers...[/color]\n";

    string[] files = Directory.GetFiles(_selectedDir);

    //need a new list because file won't be accesible within loop 
    List<string> filesToDel = new List<string>();

    foreach (string s in files)
    {
      using (System.Drawing.Image img = System.Drawing.Image.FromFile(s))
      {
        if (img.Height != 1080)
        {
          filesToDel.Add(s);
        }
      }
    }
    _fileOps.BbcodeText += $"[color=yellow]Deleting a total of: {filesToDel.Count} files...[/color]\n";
    foreach (string f in filesToDel)
    {
      File.Delete(f);
    }
    RenameFiles();
  }

  private static void RenameFiles()
  {
    _fileOps.BbcodeText += $"[color=yellow]Renaming files...[/color]\n";

    string[] files = Directory.GetFiles(_selectedDir);

    //store the actual new files made instead of entire folder to avoid throwing out entire list of files when complete
    List<String> createdFiles = new List<string>();

    int index = 0;
    foreach (string f in files)
    {
      //check original filename without renaming which should be series of random chars
      var named = Path.GetFileNameWithoutExtension(f);
      //avoid incrementing the index with the unnamed raw files before actually creating   
      if (named.Contains("Wallpaper"))
      {
        //think about making this equivalent to the number of files in the folder and set that as the current int before ++
        //also pass the name of that last file down to do a string comparison to see if we continue with the operation
        index++;
      }
    }
    foreach (string f in files)
    {
      var named = Path.GetFileNameWithoutExtension(f);
      var date = DateTime.Now.ToString("MMM-d-yyyy");
      string newName = $"Wallpaper_{index}_{date}.jpg";
      string newFile = Path.Combine(_selectedDir, newName);

      //if the file already has wallpaper in it that means it's already been renamed skip it
      if (!named.Contains("Wallpaper"))
      {
        File.Move(f, newFile);

        var createdFile = Path.GetFileName(newFile);

        createdFiles.Add(createdFile);

        index++;
      }
    }
    //completion confirmation and navigate pics
    ConfirmComplete(createdFiles);
  }

  private static void ConfirmComplete(List<string> createdFiles)
  {
    _filesInFolder = new List<string>();
    foreach (string f in Directory.GetFiles(_selectedDir))
    {
      //populate array so that we can navigate through
      _filesInFolder.Add(f);
    }

    foreach (string s in createdFiles)
    {
      _fileOps.BbcodeText += $"[color=green]{s}\n";
    }

    _fileOps.BbcodeText += $"[color=yellow]Done yoinking and converting. You have {createdFiles.Count} new Wallpapers. Enjoy!\n";
    //show the first image in the texture rect
    NavigatePics();
  }

  //left and right nav buttons call this and increase index
  //call when prog is done with conversion process atleast 1 time to set first pic 
  public static void NavigatePics()
  {
    //make sure we actually have files in the folder
    if (_filesInFolder.Count > 0)
    {
      _hasFiles = true;
    }
    else
    {
      return;
    }
    //reset index to avoid issues
    if (_viewingIndex >= _filesInFolder.Count)
    {
      _viewingIndex = 0;
    }
    else if (_viewingIndex < 0)
    {
      _viewingIndex = _filesInFolder.Count - 1;
    }
    //make sure the imagerect is empty first to avoid any conflicts 
    _imageRect.Texture = null;
    Godot.Image i = new Godot.Image();
    i.Load(_filesInFolder[_viewingIndex]);
    _fileLabel.Text = _filesInFolder[_viewingIndex].BaseName().GetFile();
    ImageTexture t = new ImageTexture();
    t.CreateFromImage(i);
    _imageRect.Texture = t;
  }
  //filedialogue button is currently disabled set to MOUSE FILTER: IGNORE POINTER
  //called when directory is selected in the file dialog pop up
  public static void CheckValidDir(string dir = "")
  {
    if (dir != string.Empty)
    {
      //make sure to get raw address because GODOT by default includes user:// before dir name
      string trimDir = dir.Remove(0, 7);
      _selectedFolder.Text = trimDir;
      _selectedDir = trimDir;
      _canConvert = true;
      _fileOps.BbcodeText += $"You've selected {_selectedDir}...\n";
      _fileOps.BbcodeText += $"Ready to Convert files to {_selectedDir}...\n";
    }
  }

}