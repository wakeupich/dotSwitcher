🌐 Language / Язык: [Русский](README.md) | **English**

# dotSwitcher

*If you're paranoid enough not to use [Punto switcher](http://punto.yandex.ru "closed source software")*

A lightweight keyboard layout switcher for Windows with automatic layout detection based on the Windows spell checker and a built-in phonotactic character N-gram model. No telemetry; the only network access is the optional update check against GitHub releases.

This is a community fork of the original [dotSwitcher](https://github.com/kurumpa/dotSwitcher) (GPL v3) that adds automatic switching, a reliable clipboard-preserving selection converter, a standalone installer, auto-updates, and a bilingual UI.

## Features
* **Convert the last word** to the other layout with a single hotkey.
* **Automatic switching** (optional, toggleable in Settings and the tray menu): detects words typed in the wrong layout and fixes them on `Space` or a short pause.
* **Convert selected text** between layouts while preserving clipboard contents (supports text, HTML, RTF, images/screenshots, and file drops up to 32 MB, without polluting `Win+V` clipboard history).
* **Two-language UI**: English / Russian.
* **Run as administrator** via Task Scheduler, so hotkeys also work in elevated apps and games.
* **Layout cycle filter**: choose which keyboard layouts participate in switching.
* **Standalone installer & built-in updater**: `dotSwitcher-Setup.exe` automatically detects an existing installation (Registry, Task Scheduler, Startup folder, or running process) and upgrades it in place.
* **Unified portable settings**: stored in `%LOCALAPPDATA%\dotSwitcher\settings.xml` (decoupled from executable path and version), with configurable switch delay (`1–200 ms`) and smart selection behaviour.

## Default hotkeys
* `Pause` — convert the last typed word.
* `Shift + Pause` — convert the selected text.
* `Right Ctrl` — switch to the next keyboard layout.

All hotkeys are configurable in the settings window.

## Personal dictionary
If you use personal names, hostnames, or uncommon words that should always be treated as valid by the layout auto-detection, put them into a plain-text file:

    %LOCALAPPDATA%\dotSwitcher\dictionary.txt

* One word per line, UTF-8 encoding.
* Lines starting with `#` are comments.
* **The file is not created automatically** — create it yourself.
* It is separate from session exceptions: pressing `Backspace` or `Pause` right after an auto-switch reverts and ignores that word **for the current session only** (kept in memory, not written to disk).

## Usage
* Run `dotSwitcher` (or install via `dotSwitcher-Setup.exe`).
* Type normally anywhere; press `Pause` to convert the last word or `Shift + Pause` to convert selected text.
* Launching the program a second time shows the settings window, even in hidden mode (no tray icon).

## Building
Requires .NET Framework 4.6.1 and MSBuild (VS 2022 Build Tools or newer). Run:

    build_someday.cmd

It builds `dotSwitcher.sln` in `Release` and produces:
* `bin\Release\dotSwitcher.exe` — portable executable
* `Setup\bin\Release\dotSwitcher-Setup.exe` — installer

## Credits
* Original dotSwitcher by [kurumpa](https://github.com/kurumpa).
* GitHub icon by [Picons.me](https://picons.me/).

## License
GNU General Public License v3.0 — see [LICENSE.txt](LICENSE.txt).

## Known bugs
If you experience erasing the word and retyping it again in the same layout (e.g., in some legacy apps), try increasing the switch delay in the settings window (`50–200 ms` usually helps). See also [this StackOverflow question](http://stackoverflow.com/questions/27720728/cant-send-wm-inputlangchangerequest-to-some-controls).
