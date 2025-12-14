# SoundHop

**Switch your audio devices with a single click. Modern. Fast. Beautiful.**

# ⚠️⚠️⚠️ WARNING THIS APP IS FULLY VIBE CODED! ⚠️⚠️⚠️
Aside from my guiding hand in UX/UI, this app is fully vibe coded, with most of the features being done in what amounts to about a week of work. I have zero Windows app dev experience (though I do web dev for my day job) and cannot definitively say that this app is production ready. Use at your own risk.

---

## Overview

SoundHop is a modern audio device switcher for Windows, built with WinUI 3.

I "created" SoundHop because the existing solutions just didn't feel right. While they worked, they often came with dated interfaces, quirky behaviors, or were missing that polished, native Windows 11 experience. I wanted something that felt like it belonged on a modern Windows desktop, something fast, intuitive, and beautiful. And obviously, Windows 11's native device switcher was out of the question...no hotkey support, no favourites, no custom icons, and to open it required 2 clicks (unless you count the hotkey to open it). If you're someone who switches audio devices a lot, that's a lot of clicks.

A huge shoutout to [xenolightning](https://github.com/xenolightning) and the original [AudioSwitcher](https://github.com/xenolightning/AudioSwitcher_v1/) project. I had been using AudioSwitcher for years and loved it (though it had its problems). SoundHop is heavily inspired by it, and many of its features are directly based on that project. Thank you for building something so useful!

## Features

- 🎧 **Quick Device Switching** – Click the tray icon to instantly switch between audio devices
- ⌨️ **Global Hotkeys** – Assign keyboard shortcuts to switch devices without touching your mouse
- ⭐ **Favorites** – Mark your most-used devices for quick access
- 🎤 **Input & Output Support** – Manage both playback and recording devices
- 🔄 **Quick Switch Mode** – Cycle through favorite devices with middle-click
- 🎨 **Custom Icons** – Personalize device icons to easily identify them
- 📍 **Taskbar Integration** – Native system tray flyout that feels like Windows 11
- 🌙 **Dark Mode** – Full dark theme support with Mica backdrop
- ⚡ **Launch on Startup** – Start with Windows so it's always ready
- 🔗 **Sync Communication Device** – Optionally keep your communication device in sync with your default device

## System Requirements

- **Windows 10** version 1903 (19H1) or later
- **Windows 11** recommended
- **.NET 8.0** Runtime

## Installation

### Option 1: Download Release

<!-- TODO: Add download link when releases are published -->
Download the latest release from the [Releases](https://github.com/yourusername/SoundHop/releases) page.

## Contributing

Contributions are welcome! Whether it's bug reports, feature requests, or pull requests, all input is appreciated. Be sure to check out the [Contribution Guidelines]() file for more information.

If you're someone with a lot of experience with Windows app development, and don't mind taking a look at the godforsaken mess of AI code, I'd love for you to join me as a core maintainer in making this app better.

<!-- TODO: Add contribution guidelines (maybe a separate file?) -->

## Credits

SoundHop wouldn't exist without the work of these amazing projects and developers:

### [AudioSwitcher](https://github.com/xenolightning/AudioSwitcher_v1/) by xenolightning
The original inspiration for this project. I used AudioSwitcher for years and based many of SoundHop's features directly on it. A sincere thank you for building such a useful tool.

### [SystemTrayWinUI3](https://github.com/MEHDIMYADI/SystemTrayWinUI3) by MEHDIMYADI
Was struggling with system tray integration for WinUI 3 for a while until I found this library, and it worked great.

### [EarTrumpet](https://github.com/File-New-Project/EarTrumpet)
The flyout positioning logic and animation code were adapted from EarTrumpet.

---

<p align="center">
  Made with ❤️ for Windows
</p>
