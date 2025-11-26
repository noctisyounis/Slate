
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
- [Doc](#doc)
  + [Inputs](#inputs)
  + [Window Creator](#window-creator)
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



Doc
---

### Inputs
Inputs are based on the new Input System. 
They can be accessed through the `InputsHandler` class. You can get inputs as events or float/bool depending on your needs.
If you need to access extra inputs, you can : 
- open the CustomInputActions object and add new Actions to it. Don't forget to save it.
- use `Input.Keyboard.current` and access what you need locally (dirty way as you don't make it public for the rest of the crew but it's useful when testing out stuff).

Anyway, in case of doubt, check the [Unity doc](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0//manual/QuickStartGuide.html) 

### Window Creator
Tool that allows to create, modify and organise data at runtime. Each window is a database and each change is then saved onto a Json.
This window is based on the RPG Maker window used notably for the Game Designers of the team.

To use it, you :
- create a new window.
- create new records.
- fill all the date you need (you can choose between a variety of types : text, numbers, colours, vectors, databases, custom Enums).
- Save your data.

There are various locks in place to assure your data is valid (min/max, null cleaning, unique GUIDs, auto-repair,...)

### Minimap


### Localisation Tool
This Tool is based on the [talk of Anna Kipnis](https://www.youtube.com/watch?v=ZAW_9Eygid4).

Its purpose is to store groups of dictionaries that vary depending on the language chosen.
You start by creating a new group. Select it and add some new data. It is then saved onto a Json that will make the data persistent.
All the data can be seen under the Localisation Debug Tool.

### Grid Shader
The grid's shader is called `Grid_Slate` and was made using Amplify Shader. It is procedural and editable using the material. You don't need Amplify for it to work. If you need to change it, you will have to take the HLSL version and edit it manually. Note that doing so will break the link with Amplify.
If I have the time, I will try to make a Shader Graph version of it.



What's Next ?
---

### Known Bugs
- [Slate Window] Drag & Drop window is painful when done vertically.
- [Localisation Tool] Window doesn't start at the right size.
- [Localisation Tool] Extracting data from our Json hasn't been properly tested. While it may work fine, there might be couple of pain points on the way.
- [Localisation Debug Tool] Current language displayed is incorrect. It always takes the last element of the list.
- [WindowCreator] There's no image preview for filepath.

### To Do
- [Whole Project] Naming Conventions Doc (script, project hierarchy)
- [Slate Window] Focus button : Takes all the existing Imgui windows and put them all inside user's view. Think about the way it's displayed to the User.
- [Localisation Tool] Add a button to remove an entry
- [Localisation Tool] Open
- Start working on a ImGui Custom Console ?
- Start working on an [Animation Tool](https://gdcvault.com/play/1024894/A-Fun-Time-in-Which) ?
- [WindowCreator] Records Templates
- [WindowCreator] Drag & Drop categories / fields
- [WindowCreator] Search / Filter in Records
- [WindowCreator] Export onto Scriptable Objects
- [WindowCreator] Inheriting System for records
- [WindowCreator] Read-only mode



Credits
---

- "Aluth" : *"It is better to place oneself in a complicated situation to learn something useful, than to be in a useless situation to learn something complicated"*
- Ambre
- "Lex"
- "Ryospi" : *Voici la base du projet slate, en espérant que cela pourra vous aider dans vos futurs projets.*
- "Voyager_001" : *"Ce slate est un petit pas, mais un pas de géant pour nos futures productions.*"
- Zachary Lefèbvre : *Hey! Thanks for taking the lead (forced or not), I can't wait to see what you'll come up with :)*
