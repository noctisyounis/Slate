
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
  + [Slate Windows](#slate-windows)
  + [Minimap](#minimap)
  + [Localisation](#localisation)
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



What's Next ?
---

### Known Bugs
- Drag & Drop window is painful when done vertically

### To Do
- Naming Conventions Doc (script, project hierarchy)
- Focus button : Takes all the existing Imgui windows and put them all inside user's view. Think about the way it's displayed to the User.
- [Animation Flag Tool](https://gdcvault.com/play/1024894/A-Fun-Time-in-Which)
- ImGui custom console
- How are Json organised ?



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

### Slate Windows


### Minimap


### Localisation


### Grid Shader
The grid's shader is called `Grid_Slate` and was made using Amplify Shader. It is procedural and editable using the material. You don't need Amplify for it to work. If you need to change it, you will have to take the HLSL version and edit it manually. Note that doing so will break the link with Amplify.
If I have the time, I will try to make a Shader Graph version of it.



Credits
---

- "Aluth" : *"It is better to place oneself in a complicated situation to learn something useful, than to be in a useless situation to learn something complicated"*
- Ambre
- "Lex"
- "Ryospi" : *Voici la base du projet slate, en espérant que cela pourra vous aider dans vos futurs projets.*
- "Voyager_001" : *"Ce slate est un petit pas, mais un pas de géant pour nos futures productions.*"
- Zachary Lefèbvre : *Hey! Thanks for taking the lead (forced or not), I can't wait to see what you'll come up with :)*



