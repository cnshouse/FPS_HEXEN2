Thanks for purchase Customizer.

Version 1.8

For full documentation please unzip the "documentation" file out of unity project for avoid errors.
or see the online documentation:
http://lovattostudio.com/documentations/customizer/

MFPS:---------------------------------------------------------------------------------------------

For use with MFPS:

USE THE IN-EDITOR TUTORIALS, IN UNITY TOOLBAR -> MFPS -> Tutorials -> Customizer

After import the package go to (Toolbar)MFPS -> Addons -> Customizer -> Enable
Then add the 'Customizer' scene to the Build Settings, the scene is located in: Assets -> Addons -> Customizer -> Content -> Scene -> *
Use the example player prefab which have the rifle integrated with customizer for test the system, the prefab is located in the 'Resources' folder of the addon.

Add a new weapon:

- First you need create the weapon info in 'CustomizerData' which is located in the 'Resources' folder of Customizer addon, add a new field in the list "Weapons" 
and add all the attachments and camos that you want for that weapon.

- Then open the player prefab, and first, select the FPWeapon -> add the bl_CustomizerWeapon.cs, select the weapon name that you previously setup in the CustomizerData and
 click in the Button 'Refresh', after this the attachments list will automatically generated with the required attachments, so here you need assign the attachments models,
 (the models need be positioned inside of the weapon not from prefabs), in 'CamoRender' drag the Mesh of the weapon and assign the ID of the material (from the materials list of the mesh) that will change in case of change of camo.
  check 'isFirstPersonWeapon'.

- Now select the TPWeapon (the bl_NetworkGun in the player model) and add again the script bl_CustomizerWeapon.cs where bl_NetworkGun is, and again select the weapon name and click Refresh button,
   add the attachments models and the camo mesh, now make sure to UNCHECK 'isFirstPersonWeapon' and 'ApplyOnStart' toggles.

- if you have troubles, use the example player prefab that comes in the addon Resources folder, check the First Person and Third Person Rifle weapon for check how it's setup.

Make attachment affect the gun:
 - For example silencers, when a silencer is attached the weapon must have a different fire sound than the normal, for change the sound when a silencer is attached simply
   add the script bl_AttachmentGunModifier.cs to the Silencer object of the FPWeapon, in the script mark "Override Fire Sound" and add your Audio Clip,
   do the same for 'Sights' in order to change the Aim Position of the weapon and for Magazines in order to add more bullets.

   Change Log:

   1.8
   -Improve: Exit window UI.
   -Improve: Attachment Scroll view.
   -Improve: Rotation input on touch devices.

   1.7.4
   -Fix: Randomize feature used non-purchased camos.
   -Fix: Weapon list display over other UI weapon there's more than 10 weapons added.

   1.7.3
   -Improve: Attachment gun modifier, add: Extra Damage, Extra Spread and Extra Aim Zoom options.
   -Improve: Add documentation about the attachment gun properties modifier.

   1.7
   -Improve: Integrated with Shop addons, now you can set price for camos in order to be for sale in the In-Game shop (using Shop addon).
   -Improve: Added: Global Camos, now you only have create 1 Camo Info for all weapons instead of one per weapon, you can set a global preview of the camo or can override per weapon.
   -Improve: Integrate with Shop addon, now weapons will not be available to customize if the weapon is for sale and it's not purchased by the player yet.
   -Improve: Integrate with Level Manager addon, now weapons will not be available to customize if the weapon require a minimum player level and player doesn't meet it.
   -Improve: Now the weapon icon is displayed in the button of customizer weapon list instead of just the weapon name.
   -Fix: Missing scripts in example player prefab.
   -Improve: Now you don't have to set the unique ID for each camo of each weapon.
   -Fix: Can't assign bl_Gun reference in bl_AttachmentGunModifier inspector.

   1.6
   -Add: In-Editor Documentation
   -Improve: Easily way to transfer attachments in weapons models (customizer, tp and fp weapon)
   -Fix: Error if an attachment model is not assigned in the customizer weapon.
   -Fix: Weapons reset attachments after change of weapon.
   -Improve: Integrate the Shotgun, Sniper and Pistol example weapons of MFPS.
   -Improve: bl_AttachmentGunModifier.cs

   1.5.3
   - Add attachment gun modifier, allow change properties of the weapon like: Aim Position, Fire Sound and Bullets when the attachment is active.

Any problem or question please contact us.
http://www.lovattostudio.com/en/select-support/