# SPlugin Remote Console for Unity
[한국어](Documentation%7E%2FREADME_KR.md)
- - -
## Table of contents
* [Introduction](#introduction)
* [Features](#features)
* [Getting started](#getting-started)
* [Disable SRemoteConsole](#disable-sremoteconsole)

- - -
## Introduction
[RemoteConsole is a powerful tool for debugging with remotely sent logs and custom commands to facilitate development.](https://www.youtube.com/watch?v=Da6OSc6FiX8)
* Supports all features of the Unity **native console**.
* Connects **remotely** to the built app to execute **customized commands**.
* Connects **remotely** to built apps to show their **logs**.
* Remotely connected app can be executed frame by frame by using the **Pause, Step** feature.
* You can easily show the time, frame count, and object at the time of log generation.
* Provides a convenient log filter feature.
* System information and simple profile information (FPS, UsedHeap) of connected apps can be viewed.

- - -
## Features
### Custom commands
Custom command values can be change during play and are useful for testing and debugging.
1. Use the SPlugin.SCommand.Register function in your script to register the custom command.
2. You can find registered commands in CommandView of SConsoleEditor.
3. Change the value of the custom command.

<img src="Documentation~/Images%2Fregister_commands_code.png" width="600" height="250">
<img src="Documentation~/Images%2Fapply_commands_to_remote_app.gif">

- - - 

### Log
* Using UnityEngine.Debug
    1. Use the UnityEngine.Debug.Log function in your script to write a log.
    2. Enable `ShowUnityDebLog` in SConsoleEditor's preferences.
    3. You can view the logs in SConsoleEditor's LogView.
* Using SPlugin.SDebug
    1. Use the SPlugin.SDebug.Log function in your script to write a log.
    2. You can view the logs in SConsoleEditor's LogView.
       
<img src="Documentation~/Images%2Flog_code.png" width="600" height="300">
<img src="Documentation~/Images%2Fshow_log_from_remote_app.gif">

- - -

### Convenient filter features
* Find logs using SearchField.
* Exclude logs using search ExcludeField.
* Find logs using QuickSearch feature.
  
![quick_search.gif](Documentation~/Images%2Fquick_search.gif)

- - -

### Pause, Step
Frame-by-frame play and pause are possible in remotely connected apps, which helps with debugging.
![remote_app_pause_and_step.gif](Documentation~/Images%2Fremote_app_pause_and_step.gif)
- - -

### Remote Connections
* Find and connect to an accessible app on your local network
    1. In the ApplicationView of the SConsoleEditor, look for apps that can be connected.
    2. If an app is available, it will be registered in the list.
    3. Select an app.
* Connect the app using a private IP or public IP on the local network
    1. Enter the **public IP** or **private IP of the local network** in ApplicationView of the SConsoleEditor.
    2. Click the Connect button to connect.

![connect_to_remote_app.gif](Documentation~/Images%2Fconnect_to_remote_app.gif)
- - -

### SystemInfo
1. Look at the app list in ApplicationView of SConsoleEditor..
2. Click the ShowInfo button to view system and profile information..

![systeminfo.png](Documentation~/Images%2Fsysteminfo.png)
- - -

## Getting started
1. Install the SRemoteConsole package
    1. Select `Window/Package` Manager in the menu bar.
       
       ![select_package_manager.png](Documentation%7E%2FImages%2Finstall%2Fselect_package_manager.png)
       
    2. Click `+` button at the top left of the window, and select `Add package from git URL...`
       
       ![select_add_menu_with_git.png](Documentation%7E%2FImages%2Finstall%2Fselect_add_menu_with_git.png)
       
    3. Enter the string below to the input field.
       
       `https://github.com/hope1026/unity_s_remote_console_package.git`
       
    4. Click `Add` button, and will start install the package.
       
       ![add_git_url.png](Documentation%7E%2FImages%2Finstall%2Fadd_git_url.png)
       
2. Run SRemoteConsole
   1. Select `Window/SPlugin/SConsole` in the menu bar.
      
      ![select_remote_console.png](Documentation%7E%2FImages%2Finstall%2Fselect_remote_console.png)

   2. Write logs to the script.
       - SPlugin.SDebug.Log("log");
   3. Find the log in SConsoleEditor.

- - -
## Disable SRemoteConsole
* You can disable it by adding **DISABLE_SREMOTE_CONSOLE** to Project Settings -> Player -> Scripting Define Symbols.
  * SRemoteConsole runs in the editor, but does not run in runtime, so it can be useful when distributing a release.

  ![disable_sremote_console_define.png](Documentation%7E%2FImages%2Fdisable_sremote_console_define.png)