Thanks for purchase ULogin Pro
Version 1.8


Get Started: 

- Import the ULogin Pro package in you Unity project.
- Add the ULogin Scenes in Build Settings, scenes are located in Assets -> Addons -> ULoginSystemPro -> Content -> Scenes, 
  be sure of set Login scene as the first scene in the build settings.
- Add the Set up your host check the In-Editor tutorial in (Unity Editor Toolbar) MFPS -> Addons -> ULogin -> Tutorial.
- In 'LoginDataBasePro' located in ULogin System Pro -> Content -> Resources set the scene to load after login in the field "OnLoginLoadLevel".
- Ready. (not needed for MFPS)

For Documentation go to MFPS -> Tutorials ->  ULogin Pro

Contact:

If you have any question or problem don't hesitate on contact us:

email: lovattostudio@gmail.com	
forum: http://www.lovattostudio.com/forum/index.php

Change Log:

1.8
-Add: Remember Me behave option, now you can select to remember just the user name or the session in LoginDataBasePro -> RememberMeBehave,
      if RememberSession is selected, player will automatically sign-in (without write user-name and password) next time that enter in game.
-Improve: Loading window, now show more details of the current query executing.
-Improve: Now submit buttons respond to the 'Enter/Return' key without the needed of manually focused the button.

1.7.5
Fix: Coins purchases where not begin saved.
Improve: MFPS Lobby player profile window popup, now you can click over the player name in order to open the player profile.
Add: User meta data, with this class (bl_UserMetaData.cs) you can easily add new fields to store as plain text (in json format) in data base.
Improve: compatibility with Class Customization addon.
Improve: Integrated the ranking window in MFPS lobby menu.
Improve: Build-in documentation.

1.7 (03/03/2020)
Add: bl_ULoginWebRequest.cs which make easier handle UnityWebRequests Operations.
Improve: Added unhidden / hide button in the password input field to make the password text visible.
Fix: Empty GUI Layer components in Login and Raking scenes cause build to fail in Unity 2019++
Improve: Add new table to store coins purchases metadata.

1.6.3
Fix: Guest Button still showing after player success log in.

1.6.1 (11/7/2019)
Fix: server was not retrieving purchases.

1.6 (20/5/2019)
Add: Game Version checking, check the In-Editor tutorial for more info.
Fix: Coins was not saved successfully
Improve: Now can force login scene, so when start from main menu will redirect automatically to Login scene.
Improve: Integrated Shop System addon. (require update database structure).

1.5.3
Fix: not define index on servers using php version 7++
Fix: Change user role was not working on Admin Panel.
Fix: Integration was not marked as dirty causing scene not save changes.

1.5
Integrate: Clan System addon (require update database structure).
Add: In-Editor tutorial, Open in MFPS -> Addons -> ULogin -> Tutorial
Fix: CPU spike when Moderator or Admin Log in.
Improve: Now nick name can't be the same as the login name.
Improve: Add more security filters to login and nick name.
Improve: Now Email field not appear if Email verification is not required.
Fix: Automatically login after write the password.
Improved: Smooth fade out when load Admin Panel.

1.4.5
Update to MFPS 1.4++

1.4
Add:  Add a filter words feature for avoid certain words on nick names, users will not be able to register the account is one of these words is present in the nick name.

1.3.5
Add: Button for admin and moderator can access to admin panel after login.
Add: DataBase creator (Editor Only), make easy way more easy create the tables needed for integration.