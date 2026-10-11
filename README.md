# 🌙 Sleep Wallpaper

**Version 1.0** · A tiny Windows app that fills your screen with a pure black "sleep mode" look, or any color, image, or looping video you choose. No windows, no taskbar clutter, just your wallpaper.

<!-- Add a screenshot: put it in a /screenshots folder and uncomment -->
<!-- ![Sleep Wallpaper](screenshots/preview.png) -->

## ✨ Features

- Opens **full screen in black** by default, like a sleeping monitor
- **Right-click menu** for everything, with nothing else visible on screen
- Set a **solid color** using the Windows color picker
- **Screen color eyedropper**: pick any color from any app on your screen (for example, the editor background in VS Code)
- Set an **image** (JPG, PNG, BMP) or a **video** (MP4, WMV, AVI, MOV, MKV, WEBM) that plays **in a loop**
- **Zoom in / out** so a picture or video that doesn't match your screen can fill it
- **Remembers your last choice** and zoom level and restores them on launch
- Mouse cursor hides automatically when idle

## ⌨️ Controls

| Action | Shortcut |
|---|---|
| Open menu | Right-click |
| New (File picker / Color picker / Screen color) | `Ctrl + N` |
| Zoom in | `Ctrl + +` |
| Zoom out | `Ctrl + -` |
| Reset zoom | `Ctrl + 0` |
| Close app | `Ctrl + W` (or `Alt + F4`) |

**Screen color eyedropper:** choose *New > Screen color (eyedropper)...*. The app steps aside, you click any pixel on screen, and that color becomes your wallpaper. Press `Esc` or right-click to cancel.

## 📥 Download

Go to the [**Releases**](../../releases) page and download `SleepWallpaper.exe`.
- Requires **Windows 10/11 (64-bit)**

## 🛠️ Build from source

Requirements: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) on Windows.

```bash
git clone https://github.com/pardhasaradhi96/SleepWallpaper.git
cd SleepWallpaper

# run
dotnet run

# build a single-file exe (needs .NET 8 Desktop Runtime on the target PC)
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true

# build a fully self-contained exe (no runtime needed, larger file)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The output is in `bin/Release/net8.0-windows/win-x64/publish/`.

## 📁 Project structure

```
SleepWallpaper/
├── App.xaml / App.xaml.cs          # App entry point
├── MainWindow.xaml / .xaml.cs      # Full-screen window, menu, shortcuts, zoom, media
├── ScreenPicker.cs                 # Screen color eyedropper overlay
├── SleepWallpaper.csproj           # Project file (.NET 8, WPF + WinForms)
├── SleepWallpaper.ico              # App icon
└── README.md
```

## 💾 Settings

Your last choice is saved to:

```
%AppData%\SleepWallpaper\settings.json
```

Delete this file to reset the app to the default black screen.

## 🧰 Built with

- C# · .NET 8 · WPF (plus WinForms for the color dialog and eyedropper)

## 📝 Notes

- Video playback depends on the codecs installed on your PC. MP4 (H.264) is the safest choice.
- Videos are muted by default.
- The app is for Windows only.
