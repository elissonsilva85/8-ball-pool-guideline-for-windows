# 8 Ball Pool Guideline (for Windows)

[![Build Status](https://github.com/elissonsilva85/8-ball-pool-guideline-for-windows/actions/workflows/build.yml/badge.svg)](https://github.com/elissonsilva85/8-ball-pool-guideline-for-windows/actions/workflows/build.yml)
[![Latest Release](https://img.shields.io/github/v/release/elissonsilva85/8-ball-pool-guideline-for-windows?label=release)](https://github.com/elissonsilva85/8-ball-pool-guideline-for-windows/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/elissonsilva85/8-ball-pool-guideline-for-windows/total)](https://github.com/elissonsilva85/8-ball-pool-guideline-for-windows/releases)
[![.NET](https://img.shields.io/badge/.NET-11.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/11.0)
[![C#](https://img.shields.io/badge/C%23-15-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D6?logo=windows&logoColor=white)](#requirements)
[![Last Commit](https://img.shields.io/github/last-commit/elissonsilva85/8-ball-pool-guideline-for-windows)](https://github.com/elissonsilva85/8-ball-pool-guideline-for-windows/commits)
[![Open Issues](https://img.shields.io/github/issues/elissonsilva85/8-ball-pool-guideline-for-windows)](https://github.com/elissonsilva85/8-ball-pool-guideline-for-windows/issues)

A transparent, always-on-top overlay that draws guide lines from a reference ball to all six pockets, to help you aim in pool games.

- [8 Ball Pool Official Website](https://8ballpool.com/en/game)
- [8 Ball Pool Android App](https://play.google.com/store/apps/details?id=com.miniclip.eightballpool&hl=en)
- [8 Ball Pool iOS App](https://apps.apple.com/br/app/8-ball-pool/id543186831)

## Download

[Download the latest release](https://github.com/elissonsilva85/8-ball-pool-guideline-for-windows/releases/latest)

| File | For |
|---|---|
| `8BallPool-win-x64.exe` | Most PCs (Intel / AMD) |
| `8BallPool-win-arm64.exe` | ARM devices (e.g. Snapdragon laptops) |

It's a single self-contained executable: no installer and no .NET runtime needed.

## How to use it

The overlay has two modes, which you switch with **F8**:

| Mode | What it does |
|---|---|
| **Setup** (on start) | The overlay is tinted so you can see and grab it. Drag anywhere to **move** it, drag the edges to **resize** it, and drag the reference ball to aim. |
| **Play** | The overlay is fully **click-through**: the guide lines stay visible, but every click goes to the game. |

1. Open the program. It starts in **Setup** mode.
2. Move and resize the overlay so the blue corners line up with the table's corner pockets.
3. Press **F8** to switch to **Play** mode.
4. Hover the mouse over the ball you want to hit and press **F9**. The reference ball jumps there and the guide lines update. You don't need to leave Play mode.
5. Win the game! :+1:

### Shortcuts

| Key | Action | Works in |
|---|---|---|
| `F8` | Switch between Setup and Play mode | Anywhere (global hotkey) |
| `F9` | Move the reference ball to the mouse cursor | Anywhere (global hotkey) |
| `Esc` | Exit | Setup mode, overlay focused |

There is also a **tray icon** next to the clock. Double-click it to switch modes, or right-click it to exit. If another app already uses F8 or F9, the app shows a notification and you can still switch modes from the tray icon.

## Requirements

- Windows 10 or Windows 11 (x64 or ARM64)

## Building from source

Requires the [.NET 11 SDK](https://dotnet.microsoft.com/download/dotnet/11.0) (or Visual Studio 2026).

```shell
# Build
dotnet build 8BallPool.slnx -c Release

# Run
dotnet run --project 8BallPool

# Publish a self-contained single-file exe
dotnet publish 8BallPool/8BallPool.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

## Tech Stack

| Component | Technology |
|---|---|
| Language | C# 15 |
| Framework | .NET 11 |
| UI | Windows Forms (WinForms) |
| Rendering | GDI+ (System.Drawing) on a per-pixel-alpha layered window (`UpdateLayeredWindow`) |
| Interop | Source-generated P/Invoke (`[LibraryImport]`) |
| IDE | Visual Studio 2026 |
| CI/CD | GitHub Actions (build, publish, release) + Dependabot |
| Platform | Windows (x64 / ARM64) |

## Screenshots

![](http://elissonsilva.com.br/8ball/print1.jpg)

![](http://elissonsilva.com.br/8ball/print2.jpg)

![](http://elissonsilva.com.br/8ball/print3.jpg)
