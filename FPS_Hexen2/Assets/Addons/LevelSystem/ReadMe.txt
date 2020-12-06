Thanks for purchase Level System for MFPS 2.0
Version 1.4

Require:
 
MFPS 2.0 version 1.4++
Unity 5.6++
ULogin Pro v1.4.5++

Get Started: --------------------------------------------------------------------------------------------------------------------

- Import the package in the MFPS 2.0 project with ULogin Pro already installed.
- in toolbar go to MFPS -> Addons -> LevelManager -> Enable
- also MFPS -> Addons -> LevelManager -> Integrate
- Now you have installed Level System.

Using: ---------------------------------------------------------------------------------------------------------------------------

The package comes with 15 example levels already set up, but you can more of course,
simply go to (folder) Addons -> LevelSystem -> Content -> Resources -> LevelManager
there you will see a list called 'Levels' in this list you can add, remove or replace levels.

for add a new level simply add a new field in the list and filled the information:

Name =          Level name.
ScoreNeeded =   The score that the player need reached to unlock this level, of course this need be mayor to the previous one.
Icon =          Level Icon.

To show level progress in game player can click on the level icon next to the player name (in lobby scene).

Runtime Level Notifier: ----------------------------------------------------------------------------------------------------------

In order to show when player level up in Match time, you have to integrate the notifier UI in the map scene (have to do for each map scene):

 1 - Open the map scene in the Editor 
 2 - Go to MFPS -> Addons -> LevelManager -> Integrate RLN
 3 - Save the scene.

Weapon Unlock: --------------------------------------------------------------------------------------------------------------------

With this add on you also unlock the ability to block weapons by level / unlock weapons until player reach a level,
but for use this you also need 'Class Customization' add on,

Now you will see a new variable in the GunInfo of GameData -> AllWeapons -> WeaponInfo -> LevelUnlock
in this you can set in which level player can select this weapon for use, so for the default weapons you need set this as 0.

Change Log:

Version 1.4
Improve: Rank icons.
Improve: Setup 78 levels with custom icons for each.
Improve: Lobby level progress window UI.
Improve: Now the level scroll list will automatically scroll to the current level of the player.

Version 1.3.5
Add: Runtime Level Notifier, now you can show when player level up in match.

Version 1.3 (6/6/2019)
Improve: Integration process.
Improve: Now players levels are update in mid game.

Version 1.2
Add: Level progress window in Lobby.