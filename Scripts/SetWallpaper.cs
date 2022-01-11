using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Win32;
//have to fully evaluate the using not just System.Drawing
using Image = System.Drawing.Image;
using Path = System.IO.Path;

namespace Hoodstrats.Core
{
  public static class SetWallpaper
  {
    //different wallpaper configuarions (windows 10 has a few more like fill)
    public enum WallStyle : int
    {
      Stretched, //wallstyle 2 in reg
      Centered, //wallstyle 1 tiled 0
      Tiled, //wallstyle 1   tiled 1
      Fit, //wall 6 tiled 0
      Fill, //wall 10 //tiled 0
      Span  //wall 22 //tiled 0
    }

    //typical dll reference
    [DllImport("user32.dll")]
    public static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uParam, string vParam, UInt32 winIni);

    public static readonly UInt32 SPI_SETDESKWALLPAPER = 0X14;
    public static readonly UInt32 SPIF_UPDATEINIFILE = 0X01;
    public static readonly UInt32 SPIF_SENDWININICHANGE = 0X02;

    //call this from rightclick menu 
    public static bool SetWallFromFile(string filePath, WallStyle style)
    {
      bool wasDone = false;
      try
      {
        Image img = Image.FromFile(filePath);
        Set(img, style);
        wasDone = true;
      }
      catch //(System.Exception)
      {
        //error log
      }

      return wasDone;
    }

    private static bool Set(Image img, WallStyle style)
    {
      bool wasDone = false;
      try
      {
        //get the user's temp default path from windows and make a file there to use as wall
        string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");

        img.Save(tempPath, ImageFormat.Bmp);

        //this is where we set the wallpaper it overrides default security that the default way uses
        RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

        //within that regkey we're currently in there are wallpaper options that can be set by using int values and key names 
        //wallpaperstyle 2 being stretch etc
        //windows 10 has a few more values that can be added 
        switch (style)
        {
          case WallStyle.Stretched:
            regKey.SetValue(@"WallpaperStyle", 2.ToString());
            regKey.SetValue(@"TileWallpaper", 0.ToString());
            break;
          case WallStyle.Centered:
            regKey.SetValue(@"WallpaperStyle", 1.ToString());
            regKey.SetValue(@"TileWallpaper", 0.ToString());
            break;
          case WallStyle.Tiled:
            regKey.SetValue(@"WallpaperStyle", 1.ToString());
            regKey.SetValue(@"TileWallpaper", 1.ToString());
            break;
          case WallStyle.Fit:
            regKey.SetValue(@"WallpaperStyle", 6.ToString());
            regKey.SetValue(@"TileWallpaper", 0.ToString());
            break;
          case WallStyle.Fill:
            regKey.SetValue(@"WallpaperStyle", 10.ToString());
            regKey.SetValue(@"TileWallpaper", 0.ToString());
            break;
          case WallStyle.Span:
            regKey.SetValue(@"WallpaperStyle", 22.ToString());
            regKey.SetValue(@"TileWallpaper", 0.ToString());
            break;
        }
        //set the params we imported from 32dll
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        wasDone = true;
      }
      catch
      {
        //error log here 
      }
      return wasDone;
    }
  }
}