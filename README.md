# MikoCheat: A CS2 Overlay and Cheat Framework

MikoCheat is a proof-of-concept cheat framework for Counter-Strike 2 (CS2), featuring a customizable overlay built with ImGui.NET and ClickableTransparentOverlay. This project serves as an educational resource to understand game memory manipulation, overlay rendering, and cheat development concepts.

---

## Features

- **Overlay Rendering**: Displays ESP, aim-lock circles, and more directly on top of the game window.
- **Configurable UI**: Interactive settings panel using ImGui.NET.
- **Customizable Cheats**:
  - **AimBot**: Automatically aims at enemies.
  - **Bone ESP**: Visualizes player skeletons and health bars.
  - **Anti-Flash**: Negates flashbang effects.
  - **Auto Bunny Hop**: Automates bunny-hopping by holding space.
  - **AimLock Circle**: Displays an aim-assist radius on screen.
- **Color Configuration**: Modify ESP colors for teams, enemies, and bones.
- **Panic**: Includes a panic button to terminate the application instantly.

---

## Installation and Usage

### Prerequisites

Before running the project, ensure you have the following:

1. **.NET 8 SDK**: This project targets `.NET 8.0`. You can download the SDK from the [official .NET website](https://dotnet.microsoft.com/download).
2. **Windows 10 Build 26100 or later**: The project targets `net8.0-windows10.0.26100.0`.
3. **x64 Architecture**: The project is configured for `x64` and `AnyCPU`.
4. **Dependencies**: The following NuGet packages are required:
   - [**ClickableTransparentOverlay** (v9.1.0)](https://www.nuget.org/packages/ClickableTransparentOverlay): Used for overlay rendering.
   - [**ImGui.NET** (v1.91.0.1)](https://www.nuget.org/packages/ImGui.NET): ImGui bindings for .NET.
   - [**swed64** (v1.0.5)](https://www.nuget.org/packages/swed64): Memory manipulation library.

### Running the Cheat
1. Clone this repository and build the solution using Visual Studio or the .NET CLI.
2. Start `cs2.exe` and run the compiled program.
3. Use the interactive UI to enable/disable features and customize settings.

---

### Updating Offsets

Counter-Strike 2 updates often change memory addresses (offsets) used by this project. If the cheats stop working after an update, you'll need to update the offsets manually.

#### Steps to Update Offsets
1. **Locate the Changed Offsets:**
   - Use tools like [IDA Pro](https://hex-rays.com/ida-pro/), [Cheat Engine](https://cheatengine.org/), or similar to analyze the game's memory after an update.
   - Look for patterns or references to the old offsets in the updated binaries.

2. **Update the Code:**
   - Open the `Offsets.cs` file in the `MikoCheat` namespace.
   - Replace outdated offsets with the new values you've identified.
   - Example:  
     ```csharp
     public static int dwViewAngles = 0x1A5E650; // Update with the new value
     ```

3. **Rebuild the Project:**
   - Save the updated `Offsets.cs` file.
   - Rebuild the solution in Visual Studio by pressing `Ctrl+Shift+B`.

4. **Test the Updated Cheats:**
   - Launch the game and test to ensure everything works as expected.

---

## Technical Overview

- **Memory Reading/Writing**:
  - Utilizes `Swed64` to interact with game memory.
  - Reads entity data, player states, and game properties.
  - Modifies flashbang duration and jump states for anti-flash and bunny-hopping.

- **Overlay Integration**:
  - Built with `ClickableTransparentOverlay` for rendering on top of the game window.
  - Uses ImGui.NET to create a responsive UI with tabs and collapsible panels.

- **Concurrency**:
  - Implements `ConcurrentQueue` for thread-safe entity data sharing.
  - Runs overlay rendering and memory scanning on separate threads for smooth performance.

---

## Controls and Customization

### UI Settings
- **Settings Tab**:
  - Enable/disable cheats like AimBot, Auto Bunny Hop, and Anti-Flash.
  - Adjust parameters such as aim-lock radius, bone thickness, and ESP head size.
- **Colors Tab**:
  - Customize colors for team and enemy ESP, aim-lock circle, and bones.

### Panic Option
- **Panic Button**: Instantly terminate the program from the settings tab.

---

## Disclaimer

**This project is for educational purposes only.** Using cheats in multiplayer games is against the terms of service and can result in bans or other penalties. The author is not responsible for any misuse of this software.

---

## Contributions

Feel free to fork and modify this project. Pull requests are welcome for improvements or new features.

---

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.