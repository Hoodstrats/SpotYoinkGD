/* https://twitter.com/HoodStrats || https://github.com/Hoodstrats */

using Godot;
using File = System.IO.File;

namespace Hoodstrats.Core
{
  public class Main : Control
  {
    enum ButtonState
    {
      Entered,
      Exited
    }

    [Export]
    NodePath _grabbedImage = "MainPanel/ImageBorder/GrabbedImage";
    [Export]
    NodePath _exitButPath = "Panel/TopBar/ExitBut";
    [Export]
    NodePath _minButPath = "Panel/TopBar/MinBut";
    [Export]
    NodePath _topPanelPath = "TopPanel";
    [Export]
    NodePath _popupPath = "FileDialog";
    [Export]
    NodePath _fileButPath = "FolderPanel/FileDiagButton";
    [Export]
    NodePath _folderEntryPath = "FolderPanel/UserDir";
    [Export]
    NodePath _fileOpPath = "InfoPanel/FileOperations";
    [Export]
    NodePath _progIcoPath = "TopPanel/Icon";
    [Export]
    NodePath _rightClickPath = "MainPanel/RightClickMenu";
    [Export]
    NodePath _wallStyleMenuPath = "MainPanel/RightClickMenu";
    [Export]
    NodePath _convertButPath = "MainPanel/ConvertButton";
    [Export]
    NodePath _navRightPath = "MainPanel/NavigateRight";
    [Export]
    NodePath _navLeftPath = "MainPanel/NavigateLeft";
    [Export]
    NodePath _filenameLabel = "MainPanel/FilenameLabel";

    Label _fileLabel;
    RichTextLabel _fileOps;
    LineEdit _selectedFolder;
    Panel _topPanel;
    Panel _mainPanel;
    Popup _popup;
    PopupMenu _rightClickMenu;
    PopupMenu _wallStyleMenu;
    TextureRect _progIcon;
    TextureRect _imageRect;
    TextureButton _exitButton;
    TextureButton _minButton;
    TextureButton _fileButton;
    TextureButton _convertButton;
    TextureButton _navRightButton;
    TextureButton _navLeftButton;
    Tween _tween;

    Vector2 _winStartPos;

    bool _dragging = false;

    public override void _Ready()
    {
      /*enable window transparency

      -- Enable these settings in editor --
      Display → Window → Per Pixel Transparency → Allowed
      Display → Window → Per Pixel Transparency → Enabled
      Display → Window → Per Pixel Transparency → Splash (only if the splash screen requires it)

      -- Add this line to code make sure it runs -- 
      GetViewport().TransparentBg = true;

      */

      InitNodes();
      //setup and check files and folder
      FileOperations.InitData();
      //init buttons after checking if the directories exist already etc
      InitConvertButton();
      //make sure to connect signals AFTER we init nodes
      ConnectSignals();
    }

    private void InitNodes()
    {
      _imageRect = GetNode<TextureRect>(_grabbedImage);
      _exitButton = GetNode<TextureButton>(_exitButPath);
      _minButton = GetNode<TextureButton>(_minButPath);
      _topPanel = GetNode<Panel>(_topPanelPath);
      //can straight up name the node here since its directly attached to the this script's node
      _mainPanel = GetNode<Panel>("MainPanel");
      _popup = GetNode<FileDialog>(_popupPath);
      _fileButton = GetNode<TextureButton>(_fileButPath);
      _selectedFolder = GetNode<LineEdit>(_folderEntryPath);
      _fileOps = GetNode<RichTextLabel>(_fileOpPath);
      _rightClickMenu = GetNode<PopupMenu>(_rightClickPath);
      PopulateWallMenu();
      //make sure that the richtextlabel can take in BB COLOR CODES (works like UNITY)
      _fileOps.BbcodeEnabled = true;
      _progIcon = GetNode<TextureRect>(_progIcoPath);
      _fileLabel = GetNode<Label>(_filenameLabel);
      _tween = GetNode<Tween>("Tween");

      //make sure the static class that handles everything is init
      FileOperations.InitNodes(_fileOps, _selectedFolder, _imageRect, _fileLabel);
    }

    //make sure the button is grayed out until dir is selected. Modulate so that the font is grayed too
    private void InitConvertButton()
    {
      //these are childed to the root node so we can just straight up name them
      _navRightButton = GetNode<TextureButton>(_navRightPath);
      _navLeftButton = GetNode<TextureButton>(_navLeftPath);
      _convertButton = GetNode<TextureButton>(_convertButPath);
      if (FileOperations.CanConvert)
      {
        _convertButton.Visible = true;
      }
      else
      {
        _convertButton.Visible = false;
      }
    }

    private void PopulateWallMenu()
    {
      _wallStyleMenu = GetNode<PopupMenu>(_wallStyleMenuPath);
      _wallStyleMenu.AddItem("Stretched");
      _wallStyleMenu.AddItem("Centered");
      _wallStyleMenu.AddItem("Tiled");
      _wallStyleMenu.AddItem("Fit");
      _wallStyleMenu.AddItem("Fill");
      _wallStyleMenu.AddItem("Span");
    }

    private void ConnectSignals()
    {
      _exitButton.Connect("pressed", this, nameof(ExitProg));
      _fileButton.Connect("pressed", this, nameof(PopupFile));
      _exitButton.Connect("mouse_entered", this, nameof(MouseEnterExit));
      _exitButton.Connect("mouse_exited", this, nameof(MouseExitExit));
      _minButton.Connect("pressed", this, nameof(MinimizeProg));
      _minButton.Connect("mouse_entered", this, nameof(MouseEnterMin));
      _minButton.Connect("mouse_exited", this, nameof(MouseExitMin));
      _convertButton.Connect("mouse_entered", this, nameof(MouseEnterConv));
      _convertButton.Connect("mouse_exited", this, nameof(MouseExitConv));
      _convertButton.Connect("pressed", this, nameof(ConvertPressed));
      _navLeftButton.Connect("pressed", this, nameof(NavLeftPressed));
      _navLeftButton.Connect("mouse_entered", this, nameof(NavLeftEntered));
      _navLeftButton.Connect("mouse_exited", this, nameof(NavLeftExit));
      _navRightButton.Connect("pressed", this, nameof(NavRightPressed));
      _navRightButton.Connect("mouse_entered", this, nameof(NavRightEntered));
      _navRightButton.Connect("mouse_exited", this, nameof(NavRightExit));
      _topPanel.Connect("gui_input", this, nameof(MoveWindow));
      _popup.Connect("dir_selected", this, nameof(FileOperations.CheckValidDir));
      _progIcon.Connect("gui_input", this, nameof(OpenTwitter));
      _mainPanel.Connect("gui_input", this, nameof(RightClickMenu));
      _rightClickMenu.Connect("index_pressed", this, nameof(RightClickSelect));
      _wallStyleMenu.Connect("index_pressed", this, nameof(WallSelectOpen));
    }
    //make a pop up window come up confirming if they want to use the default pictures location program sets itself instead of creating/choosing folder
    //toggle can convert to true and toggle buttons 
    private void ConvertPressed()
    {
      if (FileOperations.SelectedDir != string.Empty)
      {
        //don't need to check if dir exists just check if _selected dir isn't null and can copy there because filedialog doesnt give another choice but to select a dir
        _fileOps.BbcodeText += $"[color=green]Will start conversion operation...[/color]\n";
        //do the copying here
        FileOperations.ConvertSpotLight();
      }
      else
      {
        _fileOps.BbcodeText += $"[color=red]Sorry something went wrong...[/color]\n";
      }
      //disable button to avoid any issues
      _convertButton.Visible = false;
    }

    #region SignalConnections
    private void PopupFile()
    {
      _popup.Popup_();
    }
    private void OpenTwitter(InputEventMouseButton @event)
    {
      if (!@event.IsPressed())
      {
        if (@event.ButtonIndex == 1 || @event.ButtonIndex == 2)
        {
          //using shellopen targets the default operating systems program
          OS.ShellOpen("https://twitter.com/hoodstrats");
        }
      }
    }

    private void MoveWindow(InputEventMouseButton @event)
    {
      //this is essentially a BUTTONDOWN so kind updates every frame so clicking once can result in multiple clicks
      if (@event.ButtonIndex == 1)
      {
        _dragging = !_dragging;
        _winStartPos = GetLocalMousePosition();
      }
    }

    //for popup menu on right clicking the picture panel
    private void RightClickMenu(InputEventMouseButton @event)
    {
      //this is essentially a BUTTONDOWN so kind updates every frame so clicking once can result in multiple clicks
      if (@event.ButtonIndex == 2)
      {
        //so we can set the popup menu start point
        _winStartPos = GetLocalMousePosition();
        _rightClickMenu.Popup_();
        _rightClickMenu.SetPosition(_winStartPos);
      }
    }
    private void RightClickSelect(int index)
    {
      //right click menu logic based on the index of the items 
      GD.Print($"Selected{index}");
      if (index == 0)
      {
        //make a wallstyle selection submenu
        _wallStyleMenu.Popup_();
        _wallStyleMenu.SetPosition(_rightClickMenu.RectPosition + new Vector2(100, 0));
      }
      else if (index == 1)
      {
        //if DELETE file is selected delete from system, remove from list, increase index and rerun nav pics
        _fileOps.BbcodeText += $"[color=yellow]Deleted {FileOperations.FilesInFolder[FileOperations.ViewingIndex].BaseName().GetFile()}...[/color]\n";
        File.Delete(FileOperations.FilesInFolder[FileOperations.ViewingIndex]);
        FileOperations.FilesInFolder.Remove(FileOperations.FilesInFolder[FileOperations.ViewingIndex]);
        FileOperations.ViewingIndex++;
        FileOperations.NavigatePics();
      }
    }
    //accessed from the submenu of setwallpaper
    private void WallSelectOpen(int index)
    {
      switch (index)
      {
        case 0:
          SetWallpaper.SetWallFromFile(FileOperations.FilesInFolder[FileOperations.ViewingIndex], SetWallpaper.WallStyle.Stretched);
          break;

        case 1:
          SetWallpaper.SetWallFromFile(FileOperations.FilesInFolder[FileOperations.ViewingIndex], SetWallpaper.WallStyle.Centered);
          break;

        case 2:
          SetWallpaper.SetWallFromFile(FileOperations.FilesInFolder[FileOperations.ViewingIndex], SetWallpaper.WallStyle.Tiled);
          break;

        case 3:
          SetWallpaper.SetWallFromFile(FileOperations.FilesInFolder[FileOperations.ViewingIndex], SetWallpaper.WallStyle.Fit);
          break;

        case 4:
          SetWallpaper.SetWallFromFile(FileOperations.FilesInFolder[FileOperations.ViewingIndex], SetWallpaper.WallStyle.Fill);
          break;
      }
    }
    private void MouseEnterMin()
    {
      _minButton.Modulate = Godot.Color.ColorN("Green");
      ButtonTween(ButtonState.Entered, _minButton);
    }
    private void MouseExitMin()
    {
      _minButton.Modulate = Godot.Color.ColorN("White");
      ButtonTween(ButtonState.Exited, _minButton);
    }
    private void MouseEnterExit()
    {
      _exitButton.Modulate = Godot.Color.ColorN("Green");
      ButtonTween(ButtonState.Entered, _exitButton);
    }
    private void MouseExitExit()
    {
      _exitButton.Modulate = Godot.Color.ColorN("White");
      ButtonTween(ButtonState.Exited, _exitButton);
    }
    private void MouseEnterConv()
    {
      if (!FileOperations.CanConvert)
        return;
      _convertButton.Modulate = Godot.Color.ColorN("Green");
      ButtonTween(ButtonState.Entered, _convertButton);
    }
    private void MouseExitConv()
    {
      if (!FileOperations.CanConvert)
        return;
      _convertButton.Modulate = Godot.Color.ColorN("White");
      ButtonTween(ButtonState.Exited, _convertButton);
    }

    private void NavRightPressed()
    {
      if (!FileOperations.HasFiles)
        return;
      //check if the dir is selected/or has images
      FileOperations.ViewingIndex++;
      FileOperations.NavigatePics();
    }

    private void NavLeftPressed()
    {
      if (!FileOperations.HasFiles)
        return;
      //check if the dir is selected/or has images
      FileOperations.ViewingIndex--;
      FileOperations.NavigatePics();
    }

    private void NavRightEntered()
    {
      if (!FileOperations.HasFiles)
        return;
      _navRightButton.Modulate = Godot.Color.ColorN("Green");
      ButtonTween(ButtonState.Entered, _navRightButton);
    }

    private void NavLeftEntered()
    {
      if (!FileOperations.HasFiles)
        return;
      _navLeftButton.Modulate = Godot.Color.ColorN("Green");
      ButtonTween(ButtonState.Entered, _navLeftButton);
    }

    private void NavLeftExit()
    {
      if (!FileOperations.HasFiles)
        return;
      _navLeftButton.Modulate = Godot.Color.ColorN("White");
      ButtonTween(ButtonState.Exited, _navLeftButton);
    }

    private void NavRightExit()
    {
      if (!FileOperations.HasFiles)
        return;
      _navRightButton.Modulate = Godot.Color.ColorN("White");
      ButtonTween(ButtonState.Exited, _navRightButton);
    }

    public void ExitProg()
    {
      GetTree().Quit();
    }
    public void MinimizeProg()
    {
      OS.WindowMinimized = true;
    }
    #endregion

    private void ButtonTween(ButtonState state, Godot.Object nodeObject)
    {
      if (state == ButtonState.Entered)
      {
        Vector2 rectNew = new Vector2(1.1f, 1.1f);
        _tween.InterpolateProperty(nodeObject, "rect_scale", _minButton.RectScale, rectNew, .05f, Tween.TransitionType.Bounce, Tween.EaseType.OutIn);
        _tween.Start();
      }
      else
      {
        Vector2 rectNew = new Vector2(1f, 1f);
        _tween.InterpolateProperty(nodeObject, "rect_scale", _minButton.RectScale, rectNew, .05f, Tween.TransitionType.Bounce, Tween.EaseType.OutIn);
        _tween.Start();
      }
    }
    public override void _Process(float delta)
    {
      if (_dragging)
      {
        //smoother movement by subtracting the moousepos from start pos like we do in unity with delta movement on interface
        OS.WindowPosition = (OS.WindowPosition + GetGlobalMousePosition() - _winStartPos);
      }

      if (!FileOperations.HasFiles)
        return;

      if (Input.IsActionJustPressed("ui_left"))
      {
        NavLeftPressed();
      }
      else if (Input.IsActionJustPressed("ui_right"))
      {
        NavRightPressed();
      }
    }
  }
}
