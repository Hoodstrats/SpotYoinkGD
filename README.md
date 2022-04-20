#### SpotYoink Godot Version
- Simple C# Godot GUI implementation of my CLI tool that lets you grab those amazing wallpapers Windows uses for its login screens.

**I used this as an excuse to learn Godot's Mono implementation and messing with UI/Signals was a really good way to do so**

---
##### How to use
- Simply open up the program and press the Convert button
- The login screen wallpapers don't update very often, so try to use the command maybe once a week *I've programmed in a daily useage just incase*

#### Extra Functionality in this version
- can browse the coverted wallpapers directly in the program by pressing the left and right buttons on the interface or the arrow keys on your keyboard
- Can set the windows wallpaper directly by right clicking on the picture and setting the current picture as a wallpaper
- Can also delete the file directly by right clicking but this time choosing delete

---
### How it works
- Windows actually stores the wallpapers it uses for the login screen temporarily in a folder located at:`Appdata\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets";
- The files don't have an extension and are a mixed batch of different sizes/aspect ratios
- What this tool does:
	-  it grab those files
	-  convert them to a jpeg format
	-  Filter out the non 1920 x 1080 files
	-  Copy them all to the new folder stated above

---
### Credits
- FutilePro pixel font used was made by: https://www.patreon.com/posts/futile-pro-21019271

---
### Screenshots
![Idle](/_screenshots/regular.png)
![Rigtclicked](/_screenshots/rightclicked.png)