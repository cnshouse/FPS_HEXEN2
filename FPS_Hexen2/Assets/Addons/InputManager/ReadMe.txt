Thanks for purchase Input Manager addons for MFPS 2.0 from Lovatto Studio!
Version 1.7

Required:
MFPS 2.0 v1.8++
Unity 2018.4++


Integration:

- Import the package into your MFPS 2.0 project.
- For integrate simple go to (Top editor menu) MFPS -> Addons -> InputManager -> Integrate
- Save the scene.
- That's

- Usage:
After integrate you should be able to see a new button in the 'Options' menu of lobby called "Controls", if you click this the input control panel will open,
there you can change the input for each action listed in the Input Manager.

- Add New keys:
The asset comes with a wizard editor window to easily add, remove or edit keys, for open it go to: (Toolbar) MFPS -> Addons -> InputManager -> Add Keys.

- GamePad / Console setup:
 if you want use the game pad input you need made some extra set ups:

 1 - Edit the Unity Input Manager, the addon package comes with a InputManager.asset with all the gamepad input set up, you only need put this on the 'ProjectSettings' folder
  extract the InputManager.asset of the addon located in: Addons -> InputManager -> Content -> UnityInputSettings -> UnityInputSettings, copy and paste it in the ProjectSettings folder.

 2 - Go to the MainMenu scene, if you have integrate the addon already, you'll have a InputManager prefab in the hierarchy, select this and in the variable 'Input Type' select 'Xbox' option
 on the InputMapped variable drag the GamePad InputMapped located in: Addons -> InputManager -> Content -> Prefabs -> Mappeds -> GamePad.

 Ready.

 Contact:
 If you have problems with the addons feel free to contact us if you purchase it.
 Forum: http://www.lovattostudio.com/forum/index.php

 Change Log:

 1.7
 -ADD: Now can change input device (keyboard or control) in runtime.
 -IMPROVE: Input mapped UI.
 -IMPROVE: Compatibility with MFPS 1.8

 1.6 (06/10/2020)
 -ADD: GamePad pointer, this simulate the mouse pointer but controlled by the GamePad in order to navigate in the UI with control.
 -IMPROVE: Completely rewrite the system to be more scalable and easier to use.
 -ADD: Alternative key support.
 -IMPROVE: Custom KeyCode inspector drawer to make easier find the keys in the key list.
 -IMPROVE: The controller mapped.
 -ADD: Option to run with button or by joystick in controller.
 -IMPROVE: the key binding menu UI.
 -ADD: Build-In documentation.

 1.5.6 (11/20/2019)
 -FIX: Single fire with controller not working (detected as automatic fire)
 -FIX: Single fire with keyboard not working (detected as automatic fire)
 -IMPROVE: integration

 1.5.3
 -FIX: scene was not mark as dirty after integrate the addon, making the changes not saved and causing errors.
 -FIX: Integration was not properly applied if ULogin Pro is integrated.
 -FIX: Replace a key info always change the first key info with the Key Editor window.