Thanks for purchase Class Customization from Lovatto Studio Online Store!.
Version 1.6

Require:

MFPS 1.7++
Unity 2018.4++

Get Started:-------------------------------------------------------------

First enable the addon, go to MFPS -> Addons -> ClassCustomizer -> Enable
Then for integrate go to MFPS -> Addons -> ClassCustomizer -> Integrate

Add New Weapons:------------------------------------------------------------

When you add a new weapon in the game, is required that add it in the Class Customizer too.

- Open 'ClassCustomizer' Scene.
- in the gameObject called 'ClassManager' in the script bl_ClassCustomizer you have 4 list (AssaultWeapons, EngineerWeapons, SupportWeapons and ReconWeapons)
  one for each player class, in order to define which weapons will be available for each class simple click on the "Plus" or "Minus" button at the right side of each 
  weapon field, darker highlighted weapons means that they are not available, which means that this weapon wouldn't appear in the list for this player class.

  If you don't see some weapon in the lists, Click on the button "Update" (on the inspector of bl_ClassCustomize)

Edit the default weapons loadouts:------------------------------------------

- In ClassManager located in Assets -> ClassCustomization -> Resources -> *
  you have the loadout profiles for each class, select them and then you will see the default weapon for each slot.

Contact:--------------------------------------------------------------------
contact.lovattostudio@gmail.com

Change Log:

1.6
-FIX: Grenade Launchers and Burst type rifles never show up in the list of the Class Customizer menu.
-IMPROVE: Integrated with ULogin Pro, now when ULogin is used the weapons loadouts will be saved in database instead of locally.

1.5.2
-FIX: Error when not start from the class customization scene.
-FIX: Purchased weapons doesn't get unlocked.

1.5 (03/13/2020)
-IMPROVE: Now all weapons from GameData will be automatically added in the Class Customization lists in order to you select which ones are
          available for each class, before you had to manually add new weapons.
-IMPROVE: Now is easier to define which weapons are or not available for each class from the bl_ClassCustomize inspector.
-IMPROVE: Remove deprecated components.
-IMPROVE: Now players can't equipped the same weapon multiple times in the same player class.
-FIX: When change of class in the menu also change of weapon slot list
-CHANGE: Update to Post-Process Stack V2.

1.4 (06/13/2019)
-ADD: Kit slot, for change the kit that the player will use in game (Health or Ammo)
-ADD: Slots Rules, now you can define in inspector which weapons type are allowed in each slot (primary,secondary,etc...)
-IMPROVE: Add fade UI effect when change of slot list.
-IMPROVE: Integrated with Shop addon, if you have shop addon, weapons that are not purchase and are not free gonna be unavailable to select.

1.3
-IMPROVE: Anchored left panel to be responsive independent of the screen resolution.
-IMPROVE: Make ClassManager a scrip table object instead of a instance prefab.
-IMPROVE: Auto integration process.
-ADD: Molotov to the available weapons.
-IMPROVE: Option to close list when select weapon.