# HDR Toggler

A lightweight Windows system tray utility that lets you toggle HDR on and off **per monitor**.

## The problem

Windows 10 and 11 only allow toggling HDR globally, if you enable it, it turns on for every display at once. This is frustrating in multi-monitor setups where you have one HDR-capable screen alongside one that is not, or where you simply want HDR active only for specific content on a specific display.

Existing tools like *Quick HDR* only support the primary monitor.

## What HDR Toggler does

- Sits quietly in the system tray
- Detects all HDR-capable monitors on your system
- Lets you toggle HDR independently on each display with a single click
- Left-click tray icon → opens the monitor panel
- Right-click tray icon → opens a compact quick-access menu

## Requirements

- Windows 10 (version 1803+) or Windows 11
- .NET 10 runtime
- At least one HDR-capable monitor

## Usage

Run `HDRToggler.exe`. The monitor panel opens immediately on launch.

| Action | Result |
|---|---|
| Double-click exe | Opens the monitor panel |
| Left-click tray icon | Opens the monitor panel |
| Click a monitor box | Toggles HDR on that display |
| Right-click tray icon | Opens the quick-access menu |
| Exit (from menu) | Closes the app |

## Stack

- C# / WPF / .NET 10
- Win32 `DisplayConfig` API (`DisplayConfigGetDeviceInfo` / `DisplayConfigSetDeviceInfo`)