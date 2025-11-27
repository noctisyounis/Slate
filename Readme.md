
 Slate
 ===

The core idea of Slate is to display a borderless window that will serve as a container for all our custom ImGui windows. Those different tools will then write onto Json files to keep persistant data for the developer.

The goal of Slate is to serve as a base and a tool allowing you to fasten your workflow. It's designed to be easily extensible and adjustable to your needs. For this reason, it's based on ImGui to have an adaptative UI. The concrete integration is made in Unity 6000.0.58f1 using URP.

Table of Contents
---
- [Getting Started](#getting-started)
  + [Installation](#installation)
  + [How To](#how-to-)
- [Support](#support)
  + [Main Links](#main-links)
  + [Mac and Laptops Support](#mac-and-laptops-support)
- [Doc](#doc)
  + [Inputs](#inputs)
  + [WindowBaseBehaviour](#windowbasebehaviour)
  + [Toolbar](#toolbar)
  + [WindowPosManager](#windowposmanager)
  + [Window Creator](#window-creator)
  + [Draw Graph](#draw-graph)
  + [Minimap](#minimap)
  + [Localisation Tool](#localisation-tool)
  + [Grid Shader](#grid-shader)
- [What's Next](#support)
  + [Known Bugs](#known-bugs)
  + [To Do](#to-do)
-  [Credits](#credits)


    
Getting Started
---

### Installation
Have Unity installed on your machine. Ideally Unity 6000.0+

### How To ?
Pan : RMB or MMB + Move Mouse
Move Window : Drag & Drop title bar (showing up when hovered)
Display title bar : Hover title bar
Display existing ImGui Windows : Tools > ...



Support
---

### Main Links
Please submit bug reports [here](https://github.com/noctisyounis/Slate/issues).  
Submit your questions and feature requests [here](https://github.com/noctisyounis/Slate/discussions).

### Mac and Laptops Support
Slate does support Mac devices. We have been developing on Mac and Windows in parallel.
Laptops are also supported. Inputs have been chosen to accomodate to the fact that you might not have, say, middle-click mouse or numpads for example.       



Doc
---

### Inputs
Inputs are based on the new Input System. 
They can be accessed through the `InputsHandler` class. You can get inputs as events or float/bool depending on your needs.
If you need to access extra inputs, you can : 
- open the CustomInputActions object and add new Actions to it. Don't forget to save it.
- use `Input.Keyboard.current` and access what you need locally (dirty way as you don't make it public for the rest of the crew but it's useful when testing out stuff).

Anyway, in case of doubt, check the [Unity doc](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0//manual/QuickStartGuide.html) 

### WindowBaseBehaviour
Abstract class that defines the basic behaviour of all UImGUI MonoBehaviour windows.
This way, we manage subscription and unsubscribe to/from UImGUI events.

It also registers the current window to the `WindowPosManager`, the class that handles movement and resize when panning or zooming in/out.
`WindowBaseBehaviour` also calls the needed methods from `WindowPosManager` in order to move/resize the windows and also draw or not said windows when they go outside of the camera view.

**Usage** 
Simply make your UImGUI MonoBehaviour inherit from `WindowBaseBehaviour`, then override WindowLayout with your specific window’s style and logic:

```
public class ExampleWindow : WindowBaseBehaviour
{
    protected override void WindowLayout()
    {
        // your UImGUI layout and logic here
    }
}
```

### WindowPosManager
This class handles window movements and sizes when panning and zooming in/out.
`CameraPan` calls the `MoveWindows()` method with a movement delta, we then iterate over the list of registered windows and add the movement delta to the old position offset of said window.
The actual repositioning is done in `SyncWindowPosition()` which handles if the window should move or not and if the window should be drawn.

A window is considered to be “outside” of the camera view if its size + its offset adds up to the max screen width or height, or if they go into the negatives. If they are technically outside of the camera view then we stop rendering them, this is handled by the `ShouldDraw()` method.

`ResizeWindows` is also called from the `CameraPan` class when we zoom in/out. 
It simply receives a scale factor from 1-2 and multiplies the window’s initial size with the scale factor, we then set the window size to the new calculated size. (We do not save the current size in order to avoid resizing the windows infinitely)

**Usage**
This class’ implementation is handled through the `WindowBaseBehaviour` class:
- It registers the current window through `WindowPosManager.RegisterWindow(_windowName)`. Take note that the `_windowName` variable should always be initialised so that the window can be registered to the `WindowPosManager`.

- Early return if `WindowPosManager.ShouldDraw(_windowName)` returns false or if the visibility flag is overridden to false.

- Inside `ImGui.Begin()` we call `WindowPosManager.UpdateWindowCache(_windowName)` if it’s the first time the window appears in order to initialise all its values.
    - then we call `WindowPosManager.UpdateWindowCache(_windowName)` each frame to move the window if we pan around our view.

In short, everything is handled through `WindowBaseBehaviour` so there’s no need to call `WindowPosManager` repeatedly.

### Toolbar
The Toolbar is the main entry point of the application. It is a retractable bar located at the top of the screen, centralizing access to all windows (tools, settings, features) as well as core system commands. 

1. Behaviour (Auto-Hide)
Managed by `Toolbar.cs`
- Hidden by default: The bar remains invisible until user interaction.
- Reveal zone: Hovering the top edge of the application window reveals the Toolbar with a smooth animation.
- Locking: As long as a menu is open or the mouse cursor stays over the Toolbar, it remains visible.

2. Dynamic Menu System
Managed by `ToolbarMenu.cs` and `WindowRegistry.cs`
The left side of the Toolbar displays tool categories. This system is fully automatic—no manual drag-and-drop in the Unity editor is required to register a new entry.
Window State Management (+ / x)
- Menus reflect the current state of each window:
- Add (+): If the window is closed, clicking its name (or the + icon) will spawn and display it.
- Close (x): If the window is already open, clicking its name (or the x icon) will immediately destroy it.

3. Integration
To make a window appear automatically in the Toolbar, simply add the `[SlateWindow]` attribute above your window class.
The class must inherit from `MonoBehaviour` or `WindowBaseBehaviour`.

```cs
using SharedData.Runtime;
using UnityEngine;

// 1. Define the category (dropdown menu name)
// 2. Define the entry name (label shown in the list)
[SlateWindow(categoryName = "Tools", entry = "My Custom Tool")]
public class MyCustomTool : MonoBehaviour
{
    // Your ImGui code here...
}
```
After compilation, the WindowRegistry detects this class via reflection, creates the Tools menu if it does not already exist, and registers the `My Custom Tool` entry automatically.

4. System Controls
Managed by `ToolbarButton.cs`
The right side of the Toolbar contains essential application controls, styled as Ghost Buttons (transparent by default, visible on hover):
- Minimize (_): Minimizes the application to the Windows taskbar.
- Windowed / Borderless Toggle (□): Switches between exclusive fullscreen and borderless windowed mode. Particularly useful for development and multi-monitor setups.
- Quit (X): Closes the application (or stops Play Mode in the Unity Editor).

Technical Insight
The architecture is built around a clear separation between data and view:
- `ToolbarSharedState` (ScriptableObject) : Stores global state such as Toolbar visibility and height. This allows other systems to know whether the mouse is interacting with the Toolbar without directly referencing it.
- `ToolbarView`: Responsible exclusively for ImGui rendering (tables, styles, colors).

---

ImGui Settings
This window allows configuration of the global appearance, ergonomics, and style of the user interface. It is divided into four main tabs.

1. Fonts (Typography)
Managed by `FontSettingsPanel.cs`
This tab controls text readability and scaling across the application.
Font Selection: Choose between built-in fonts:
- Default (UImGui Default)
- Noto Sans Symbols 2 (extended symbol support)
- OpenDyslexic (accessibility-oriented)
Font Scale: A slider adjusts global text size (from ×0.75 to ×2.00).
Live Preview: A preview area displays a pangram (“The quick brown fox…”) and symbols for immediate feedback.

2. Styles (Geometry & Layout)
Managed by `StyleSettingsPanel.cs`
This tab adjusts the physical dimensions and spacing of UI elements. Settings are organized into categories:
- Main (Spacing): Window padding, item spacing, and resize grip sizes.
- Borders: Border thickness for windows, popups, and frames.
- Rounding: Corner radius for windows, buttons, tabs, and scrollbars.
- Scrollbar & Tabs: Dedicated sizing and rounding for scrollbars and tabs.
- Display: Global safe margins for rendering.

Note: The Save sizes button persists these geometry settings.

3. Colors (Themes & Styling)
Managed by `ColorSettingsPanel.cs`
This is the full theme editor, allowing customization of every ImGui color.
Base Schemes: Apply default themes (Dark, Light, Classic) as a starting point.
Preset System:
- Create, name, load, and save custom color presets.
- Low-alpha preset option to quickly make backgrounds nearly transparent.
Granular Editing: Colors are grouped logically (Text, Windows, Widgets, Scrollbars, Tables, etc.).

Diagnostic Tools (?):
- Each color row includes a ? button.
- Hover: Shows a tooltip explaining the purpose of the color.
- Click: Highlights the corresponding UI element in real time to visually identify it.

4. Preset Manager (Import / Export)
Managed by `PresetManagerPanel.cs`
This tab enables sharing and long-term storage of configurations using external JSON files—ideal for version control or transferring styles between projects.
It is divided into three independent sections:
- Fonts: Export/Import font selection and scale (fonts.json).
- Style Sizes: Export/Import geometry settings (sizes.json).
- Colors: Export/Import the full color palette (colors.json).
By default, files are stored in the Data/themes/ directory at the project root (or next to the executable in builds).

Technical Insight
Settings are persisted in two complementary ways:
1. PlayerPrefs: Immediate local persistence between sessions (keys like imgui_style_colors_v1, imgui_style_sizes_v1, etc.).
2. JSON Files: Managed via the Preset Manager for long-term storage and use in builds.

### Window Creator
Tool that allows to create, modify and organise data at runtime. Each window is a database and each change is then saved onto a Json.
This window is based on the RPG Maker window used notably for the Game Designers of the team.

To use it, you can :
- Create a new window.
- Create new records.
- Fill all the date you need (you can choose between a variety of types : text, numbers, colours, vectors, databases, custom Enums).
- Save your data.

There are various locks in place to assure your data is valid (min/max, null cleaning, unique GUIDs, auto-repair,...)

### Draw Graph
*work in progress*


### Minimap
A mimimap is being displayed when moving in the Slate. Its purpose is to show you where you currently stand in the window compared to existing Imgui.

### Localisation Tool
This Tool is based on the [talk of Anna Kipnis](https://www.youtube.com/watch?v=ZAW_9Eygid4).

Its purpose is to store groups of dictionaries that vary depending on the language chosen.
You start by creating a new group. Select it and add some new data. It is then saved onto a Json that will make the data persistent.
All the data can be seen under the Localisation Debug Tool.

### Grid Shader
The grid's shader is called `Grid_Slate` and was made using Amplify Shader. It is procedural and editable using the material. You don't need Amplify for it to work. 
If you need to change it, you will have to take the HLSL version and edit it manually. Note that doing so will break the link with Amplify.
If I have the time, I will try to make a Shader Graph version of it.



What's Next ?
---

### Known Bugs
- [Slate Window] Drag & Drop window is painful when done vertically. To move it, we are detecting the mouse movement and applying it to the window when criterias are met. 
- [WindowCreator] There's no image preview for filepath.
- [Localisation Tool] Window doesn't start at the right size.
- [Localisation Tool] Extracting data from our Json hasn't been properly tested. While it may work fine, there might be couple of pain points on the way.
- [Localisation Debug Tool] Current language displayed is incorrect. It always takes the last element of the list.
- [Minimap] It doesn't take Imgui windows sizes into account when drawing them

### To Do
- [Whole Project] Naming Conventions Doc (script, project hierarchy)
- [Slate Window] Focus button : Takes all the existing Imgui windows and put them all inside user's view. Think about the way it's displayed to the User.
- [Any Imgui Windows] The ResizeWindows method resizes from the lower-right corner, to make this behaviour less noticeable we should move the windows according to the difference in sizes. The issue is making it so that the movement is not exaggerated since the window will stop being displayed if moved outside of the camera view.
- [WindowCreator] Records Templates
- [WindowCreator] Drag & Drop categories / fields
- [WindowCreator] Search / Filter in Records
- [WindowCreator] Export onto Scriptable Objects
- [WindowCreator] Inheriting System for records
- [WindowCreator] Read-only mode
- [Localisation Tool] Add a button to remove an entry
- [Localisation Tool] Add an option to see options from all languages at once
- [Minimap] Render the Minimap when hovering its position on the Slate Window (it's currently only rendered when panning)
- Start working on a ImGui Custom Console ?
- Start working on an [Animation Tool](https://gdcvault.com/play/1024894/A-Fun-Time-in-Which) ?



Credits
---

- "Aluth" : *"It is better to place oneself in a complicated situation to learn something useful, than to be in a useless situation to learn something complicated"*
- Ambre
- Nicolas Carlier : "I wish Unity.GraphView will be cool"
- "Lex"
- "Ryospi" : *Voici la base du projet slate, en espérant que cela pourra vous aider dans vos futurs projets.*
- "Voyager_001" : *"A small step with this Slate — a giant leap for our future productions."*
- Zachary Lefèbvre : *Hey! Thanks for taking the lead (forced or not), I can't wait to see what you'll come up with :)*







