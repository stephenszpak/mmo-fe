# MMOClient Unity Setup

This repository contains the scaffold for a Unity third-person MMO client. The
project is intended to connect to an Elixir backend (network code is stubbed for
now).

## Requirements
- Unity **6.1** (URP or standard pipeline, choose when creating the project)
- .NET 4.x equivalent scripting runtime (default)

## Folder Structure
```
MMOClient/          Unity project root
  Assets/
    Animations/
    Materials/
    Models/
    Prefabs/
    Resources/
    Scenes/
    Scripts/
```

## Getting Started
1. Open Unity Hub and create a new 3D project named `MMOClient` using Unity 6.1.
2. Close Unity and copy the `Assets/` folders from this repo into the newly
   created project directory (replace if necessary).
3. Reopen the project in Unity.
4. Ensure the `ThirdPersonCamera`, `PlayerController`, `NPCController` and other
   scripts are compiled without errors.

### Input Manager
The old Unity input system is used. Default axes `Horizontal`, `Vertical`,
`Mouse X`, `Mouse Y` and `Mouse ScrollWheel` are sufficient. Ensure these exist
in **Edit → Project Settings → Input Manager**.

### Creating Prefabs
#### Player
1. Create a capsule in the scene and add a `CharacterController` component.
2. Add the `PlayerController` script to the capsule.
3. Create an empty child object named `CameraPivot` positioned at the top of the
   capsule (the head). Assign it to the `cameraPivot` field on the
   `PlayerController`.
4. Optionally assign simple material/colors for clarity.
5. Drag the capsule to the `Prefabs/` folder to create the **Player** prefab.

#### NPC
1. Create another capsule and add the `NPCController` and `FloatingName`
   components.
2. Assign a material/color.
3. Drag it to `Prefabs/` to create the **NPC** prefab.

### Test Scene
1. Create a new scene named `WorldTest` inside `Assets/Scenes/`.
2. Add a large plane or terrain as the ground.
3. Place an instance of the **Player** prefab at the origin.
4. Optionally place a few **NPC** prefabs around the scene.
5. Create an empty GameObject with the `ThirdPersonCamera` script and reference
the player's `CameraPivot` as the target.
6. Save the scene.

### Running
Play the `WorldTest` scene. Use `WASD` to move relative to the camera, hold both
mouse buttons to move forward, use the right mouse button to freely rotate the
camera and the scroll wheel to zoom.

All network calls are stubbed (`NetworkedEntity` class). Integrate these hooks
with your Elixir backend to synchronize player and NPC states.

### Chat System
This repo now includes a lightweight chat client for talking to a Phoenix
backend over WebSockets. The `PhoenixChatClient` component handles the Phoenix
socket protocol while `ChatUIManager` drives a simple Unity UI.

1. Download the open source **websocket-sharp** library and place the DLL inside
   `Assets/Plugins/`. This plugin provides the WebSocket implementation used by
   `PhoenixChatClient` to talk to the backend.
   Make sure the **API Compatibility Level** is set to `.NET Framework 4.x`
   (or `.NET Standard 2.0` in Unity 2022+) under **Edit → Project Settings →
   Player** so the DLL loads correctly.
2. Add the `PhoenixChatClient` and `ChatUIManager` scripts from `Assets/Scripts`
   to your scene.
3. Create a UI panel anchored to the edge of the screen containing a scrollable
   `Text` element for history, an `InputField` and a **Send** button.
4. Assign the UI elements and the `PhoenixChatClient` reference on the
   `ChatUIManager` component.
5. Press Play and type messages. Use `/w <target> <message>` in the input field
   to send a whisper (topic `chat:whisper:<target>`).

Messages are sent in Phoenix JSON format and the client automatically joins the
`"chat:global"` channel on connect.
