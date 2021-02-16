using System.Collections.Generic;
using UnityEngine;
using MFPSEditor;
using UnityEditor;

public class MFPSGeneralDoc : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/general/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-0.jpg", Image = null},
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.png", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.png", Image = null},
        new NetworkImages{Name = "img-9.png", Image = null},
        new NetworkImages{Name = "img-10.png", Image = null},
        new NetworkImages{Name = "img-11.png", Image = null},
        new NetworkImages{Name = "img-12.png", Image = null},
        new NetworkImages{Name = "http://lovattostudio.com/documentations/mfps2/assets/images/image_33.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "http://lovattostudio.com/documentations/mfps2/assets/images/image_6.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "http://lovattostudio.com/documentations/mfps2/assets/images/image_27.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "http://lovattostudio.com/documentations/mfps2/assets/images/image_14.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "http://lovattostudio.com/documentations/mfps2/assets/images/image_31.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "http://lovattostudio.com/documentations/mfps2/assets/images/image_22.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "http://lovattostudio.com/documentations/mfps2/assets/images/image_24.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "http://lovattostudio.com/documentations/mfps2/assets/images/image_19.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "http://lovattostudio.com/documentations/mfps2/assets/images/image_29.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://img.youtube.com/vi/i6qfVKk0TqY/0.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "http://lovattostudio.com/documentations/mfps2/assets/images/image_23.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "img-13.jpg", Image = null},
        new NetworkImages{Name = "img-14.jpg", Image = null},
        new NetworkImages{Name = "img-15.png", Image = null},
        new NetworkImages{Name = "img-16.png", Image = null},
        new NetworkImages{Name = "img-17.png", Image = null},
        new NetworkImages{Name = "img-18.png", Image = null},
        new NetworkImages{Name = "img-19.png", Image = null},
        new NetworkImages{Name = "img-20.png", Image = null},//31
        new NetworkImages{Name = "img-21.png", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "createwindowobj.gif" },
        new GifData{ Path = "addnewwindowfield.gif" },
        new GifData{ Path = "createwindowbutton.gif" },
        new GifData{ Path = "addonintegrateprevw.gif"},
        new GifData{ Path = "mfps-urpcsps.gif"},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Resume", StepsLenght = 0, DrawFunctionName = nameof(Resume) },
    new Steps { Name = "GameData", StepsLenght = 0, DrawFunctionName = nameof(GameDataDoc) },
    new Steps { Name = "Photon PUN", StepsLenght = 0, DrawFunctionName = nameof(DrawPhotonPunDoc) },
    new Steps { Name = "Offline", StepsLenght = 0, DrawFunctionName = nameof(OfflineDoc) },
    new Steps { Name = "Universal RP", StepsLenght = 4, DrawFunctionName = nameof(UniversalRPDoc) },
    new Steps { Name = "Kill Feed", StepsLenght = 2, DrawFunctionName = nameof(KillFeedDoc) },
    new Steps { Name = "Player Prefabs", StepsLenght = 0, DrawFunctionName = nameof(PlayerPrefabsDoc) },
    new Steps { Name = "Player Classes", StepsLenght = 0, DrawFunctionName = nameof(PlayerClassesDoc) },
    new Steps { Name = "Head Bob", StepsLenght = 0, DrawFunctionName = nameof(HeadBobDoc) },
    new Steps { Name = "AFK", StepsLenght = 0, DrawFunctionName = nameof(AfkDoc) },
    new Steps { Name = "Kick Votation", StepsLenght = 0, DrawFunctionName = nameof(KickVotationDoc) },
    new Steps { Name = "FP Arms Material", StepsLenght = 0, DrawFunctionName = nameof(FPArmsMaterial) },
    new Steps { Name = "Teams", StepsLenght = 0, DrawFunctionName = nameof(DrawTeamsDoc) },
    new Steps { Name = "Lobby Chat", StepsLenght = 0, DrawFunctionName = nameof(DrawLobbyChat) },
    new Steps { Name = "Player Animations", StepsLenght = 0, DrawFunctionName = nameof(DrawPlayerAnimationDoc) },
    new Steps { Name = "Bullets", StepsLenght = 0 , DrawFunctionName = nameof(DrawBullets)},
    new Steps { Name = "Kits", StepsLenght = 0, DrawFunctionName = nameof(DrawKitsSystem) },
    new Steps { Name = "Kill Zones", StepsLenght = 0, DrawFunctionName = nameof(DrawKillZones) },
    new Steps { Name = "Room Properties", StepsLenght = 0, DrawFunctionName = nameof(RoomPropertiesDoc) },
    new Steps { Name = "Game Settings", StepsLenght = 0, DrawFunctionName = nameof(DrawGameSettings) },
    new Steps { Name = "Object Pooling", StepsLenght = 0, DrawFunctionName = nameof(DrawObjectPooling) },
    new Steps { Name = "Add New Menu", StepsLenght = 0, DrawFunctionName = nameof(AddNewMenu) },
    new Steps { Name = "Crosshair", StepsLenght = 0, DrawFunctionName = nameof(CrosshairDoc) },
    new Steps { Name = "Game Texts", StepsLenght = 0, DrawFunctionName = nameof(DrawGameTexts) },
    new Steps { Name = "Mobile", StepsLenght = 0, DrawFunctionName = nameof(DrawMobileDoc) },
    new Steps { Name = "Post Processing", StepsLenght = 0, DrawFunctionName = nameof(PostProcessingDoc) },
    new Steps { Name = "Friend List", StepsLenght = 0, DrawFunctionName = nameof(DrawFriendListDoc) },
    new Steps { Name = "FootStep", StepsLenght = 0, DrawFunctionName = nameof(FootStepsDoc) },
    new Steps { Name = "Game Staff", StepsLenght = 0, DrawFunctionName = nameof(GameStaffDoc) },
    new Steps { Name = "Events", StepsLenght = 0, DrawFunctionName = nameof(MFPSEventsDoc) },
    new Steps { Name = "Addons", StepsLenght = 0, DrawFunctionName = nameof(DrawAddonsDoc) },
    new Steps { Name = "Update MFPS", StepsLenght = 0, DrawFunctionName = nameof(UpdateMFPSDoc) },
    new Steps { Name = "Common Q/A", StepsLenght = 0, DrawFunctionName = nameof(CommonQADoc) },
    };

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
            base.m_GUISkin = gs;
        }
        FetchWebTutorials("mfps2/tutorials/");
    }

    void Resume()
    {
        DrawTitleText("MFPS 2.0");
        GUILayout.Label("Version: " + MFPSEditor.AssetData.Version);
        DrawYoutubeCover("MFPS Get Started Video", GetServerImage(22), "https://www.youtube.com/watch?v=i6qfVKk0TqY");
        DownArrow();
        DrawTitleText("Hot Tutorials");
        if (GUILayout.Button("<color=#3987D6>Add Maps</color>", EditorStyles.label))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Add Map");
        }
        if (GUILayout.Button("<color=#3987D6>Add Weapon</color>", EditorStyles.label))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Add Weapon");
        }
        if (GUILayout.Button("<color=#3987D6>Add Players</color>", EditorStyles.label))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Add Player");
        }
        if (GUILayout.Button("<color=#3987D6>Change Bots</color>", EditorStyles.label))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Change Bots");
        }
        DrawTitleText("Custom Integration Tutorials");
        if(GUILayout.Button("<color=#3987D6>Integrate Loading Screen to MFPS</color>", EditorStyles.label))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/integrate-loading-screen-to-mfps-2-0/");
        }
        if (GUILayout.Button("<color=#3987D6>Integrate DestroyIt to MFPS</color>", EditorStyles.label))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/integrate-destroyit-to-mfps-2-0/");
        }
    }

    void GameDataDoc()
    {
        DrawTitleText("GameData");
        DrawText("With MFPS you will notice that <b>GameData</b> is mentioned a lot in the documentation, readMe.txt, and many other comments.\nIf you don't know what <b>GameData</b> is, how it works or where it's located, here is a brief explanation:\n\n<b>GameData</b> is a <i>ScriptableObject</i> that contain many front-end settings of MFPS that you can easily tweak to fit your needs and reskin the game, the options go from simple toggles to show or hide blood in the game up to the Weapon and Game Mode information.");
        DrawHyperlinkText("<link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> is located in the <b>Resources</b> folder of MFPS:");
        DrawServerImage(3);
    }

    void DrawPhotonPunDoc()
    {
        DrawText("<b>Photon Unity Networking</b> a.k.a <b>Photon PUN</b> is the network solution that MFPS use to handle all the network/server side stuff, it's one if it's not the most solid solution out there for Unity, it's fast, reliable, scalable as you can expect from a generic network solution, PUN offer multiple server locations worldwide which you can connect to in order to get least ping.\n\nIn Unity PUN comes as a third party plugin which you can download for free from the Unity Asset Store <i><b><size=8>(you probably already did by following the Get Started tutorial)</size></b></i>,\nNow there some common question about this network solution, first of course when we talk about server side things that of course have a cost, since PUN handle all the server stuff including the code, Hosting, operations and scaling services, server maintainement, etc... you don't have to worry about that things since Photon Team takes care of that, but due that there is a cost for this service, <b>PUN is a Pay service</b> but offer a Free Plan that you can use for the development process and upgrade when you are about to release your game.\n");

        DrawHyperlinkText("You can see all the available Plans in their website:\n<link=https://www.photonengine.com/en-US/PUN/pricing>Photon Pun Plans</link>\n");
        DrawText("Another common question that I get is:\n\n<i><b><size=16>And what about Authoritative Server?</size></b></i>\n\nIf you have previous experience with networks systems, you may already noticed that Photon use a Client <i>(named as Master Client)</i> instead of a server <i>(Master Server)</i> to authority the game, this open a gap for cheaters to easily modded the gameplay in their end and duplicated for others clients, since there's not an independent Master Server to compare the logic.\n");
        DrawHyperlinkText("Out of the box, Photon PUN doesn't offer a solid solution for fix this problem, instead they offer <link=https://www.photonengine.com/en-us/Server>Photon OnPremise</link> aka Photon Server with which you can host the server side code/sdk and make changes in the serve code and create your own fully authoritative server.\n");
        DrawText("Integrate Photon Server with MFPS instead of Photon PUN doesn't require code changes in MFPS but you have to setup the server SDK manually and that may require some experience or knowledge  in the area.\n\nHere you have the documentation of Photon Server if you are interested:\n");
        DrawLinkText("https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-intro", true);
    }

    void OfflineDoc()
    {
        DrawText("MFPS supports the Photon <b>Offline mode</b>, this allows you to test the map scene without the need to go to the lobby -> create a room -> load the map scene, instead you play the scene directly.\n\nThis feature is especially useful when, for example, you make changes to the player's prefab or a weapon and want to test them at runtime, save lot of time and improve the development work-flow.\n\nTo enable or disable this feature go to <b>GameData</b> -> Offline Mode\nAfter you enable it, simply open the map scene and Play.\n\nNOTE: The offline mode It <b>is not</b> designed to develop an offline game using MFPS, but rather to facilitate the development.");
    }

    void UniversalRPDoc()
    {
        if (subStep == 0)
        {
            DrawText("By default MFPS use the legacy build-in render pipeline, MFPS will be using URP <i>(Universal Render Pipeline)</i> as the default Render Pipeline in the future when it's more standardized, but for the moment in order to use MFPS with URP or HDRP you have to manually convert the project, in this doc I'll teach you how you can do it:\n\n<b><size=20>Convert MFPS project to URP:</size></b>\n\n*<i>This tutorial takes for granted that you have an MFPS project working with the build-in render pipeline in Unity 2018.4 or later</i>*\n\nFirst of all, you have to remove the Post-Processing package, for this simply go to (Unity Top Menu) <b>MFPS -> Tools -> Delete Post-Processing</b> -> Wait until script compilation finish.\n\n<i>Continue in the next step.</i>");
        }
        else if (subStep == 1)
        {
            DrawText("<b><size=22>Installing URP</size></b>\n\n1. In Unity, open your Project.\n2. In the top navigation bar, select Window > Package Manager to open the Package Manager window.\n3. Select the All tab. This tab displays the list of available packages for the version of Unity that you are currently running.\n4. Select Universal RP from the list of packages.\n5. In the bottom right corner of the Package Manager window, select Install. Unity installs URP directly into your Project.\n\n<b><size=22>Configuring URP</size></b>\n\nBefore you can start using URP, you need to configure it. To do this, you need to create a Scriptable Render Pipeline Asset and adjust your Graphics settings.\n\nCreating the Universal Render Pipeline Asset\nThe Universal Render Pipeline Asset controls the global rendering and quality settings of your Project, and creates the rendering pipeline instance. The rendering pipeline instance contains intermediate resources and the render pipeline implementation.\n\n<b><size=16>To create a Universal Render Pipeline Asset:</size></b>\n\n1. In the Editor, go to the Project window.\n2. Right-click in the Project window, and select Create > Rendering: Universal Render    Pipeline: Pipeline Asset. Alternatively, navigate to the menu bar at the top, and select Assets: Create: Rendering: Universal Render Pipeline: Pipeline Asset.\n\nYou can either leave the default name for the new Universal Render Pipeline Asset, or type a new one.");
            DrawText("<b><size=16>Adding the Asset to your Graphics settings</size></b>\n\nTo use URP, you need to add the newly created Universal Render Pipeline Asset to your Graphics settings in Unity. If you don't, Unity still tries to use the Built-in render pipeline.\n\nTo add the Universal Render Pipeline Asset to your Graphics settings:\n\nNavigate to<b> Edit > Project Settings... > Graphics.</b>\nIn the <b>Scriptable Render Pipeline Settings</b> field, add the Universal Render Pipeline Asset you created earlier. When you add the Universal Render Pipeline Asset, the available Graphics settings immediately change. Your Project is now using URP.\n\n<b>Now you will see some (a lot) pink objects</b>, this is because the shaders from the build-in RP doesn't work on URP or HDRP, you have to upgrade the material shaders, in the next step, I'll show you how to convert them.");
        }
        else if (subStep == 2)
        {
            DrawText("<b><size=22>Upgrading your Shaders</size></b>\n\nIf your Project uses shaders from the built-in render pipeline, and you want to switch your Project to use the Universal Render Pipeline instead, you must convert those Shaders to the URP Shaders. This is because built-in Lit shaders are not compatible with URP Shaders. For an overview of the mapping between built-in shaders and URP Shaders, see Shader mappings.\n\nTo upgrade built-in Shaders:\n\n1. Open your Project in Unity, and go to Edit > Render Pipeline > Universal Render Pipeline.\n2. Select <b>Upgrade Project Materials to URP Materials</b>\n\n\n<b>Note:</b> These changes cannot be undone. Backup your Project before you upgrade it.\n\n<b>Tip:</b> If the Preview thumbnails in Project View are incorrect after you've upgraded, try right-clicking anywhere in the Project View window and selecting Reimport All.");
            Space(10);
            DrawText("After this, you still may see some pink objects, those objects were using a custom shader, so in order to fix them simply select them and change their material shader to a Universal RP Shader.\n");
            DrawServerImage(31);
            DrawText("There's one last thing you have to do, see the next step.");
        }
        else if (subStep == 3)
        {
            DrawText("Finally, there's one last thing that you have to set up.\nin URP and HDRP Camera's work different than the build-in RP, in URP/HDRP there's a 'Base' camera and if you want to render any other camera at the same time, you have to set up it as an '<b>Overlay Camera</b>' and add it on the <b>'Stack'</b> camera list of the '<b>Base Camera</b>'\n\nMFPS players use 2 cameras, one that draws only the FP Weapons and the other that draws everything else, so you have to configure the one that draws the FP Weapons as an 'Overlay Camera', <b>you have to do the following for each player prefab that you are using:</b>");

            DrawText("Finally, there's one last thing that you have to set up.\nin URP and HDRP Camera's work different than the build-in RP, in URP/HDRP there's a 'Base' camera and if you want to render any other camera at the same time, you have to set up it as an '<b>Overlay Camera</b>' and add it on the <b>'Stack'</b> camera list of the '<b>Base Camera</b>'\n\nMFPS players use 2 cameras, one that draws only the FP Weapons and the other that draws everything else, so you have to configure the one that draws the FP Weapons as an 'Overlay Camera', <b>you have to do the following for each player prefab that you are using:</b>\n\n1. In the <b>Project Window</b>, select the player prefab <i>(the default MFPS player prefabs are located in the Resources folder of MFPS)</i> ➔ Click on <b>Open Prefab</b> button.");

            DrawNote("<color=#FFFC01FF>NOTE:</color> You may see a warning message when you select the player prefab, this is because there's a null component attached in the 'Weapon Camera' because the Post-Processing package was removed, to fix this simply remove the null component from the Weapon Camera.");
            DrawText("With the player prefab open do the following:");
            DrawAnimatedImage(4);
            DrawText("And that's, you can start using MFPS with URP, just remember that this last step has to be done in all the player prefabs that you are using.");
        }
    }

    void KillFeedDoc()
    {
        if (subStep == 0)
        {
            DrawTitleText("KILL FEED");
            DrawText("Kill Feed or Who Kill Who is the UI text notification panel where display events of the match like but not limited to Kills events, showing the player that kill and the player that get killed.   Normally this panel " +
                "is set in a corner of the screen to not interfere with the game play but begin accessible in any moment.\n \nIn this tutorial I'll show you a few options that you have to customize it without touch code and how " +
                "you can display your own events on it.");
            DownArrow();
            DrawText("• MFPS come with two modes to display kills events on the kill feed, in a kill event there is 3 slots of information to display: the killer name, the killed name and the weapon with which the kill was perpetrated," +
                "so the choose for you here is how you wanna display the weapon, you have to option:\n \n<b>Weapon Name:</b>");
            DrawServerImage(0);
            DrawText("<b>Weapon Icon:</b>");
            DrawServerImage(1);
            DrawText("By default <b>Weapon Icon</b> is the default option, you can change that in <b>GameData</b> -> KillFeedWeaponShowMode.\n \nAnother option to customize is the color to highlight the local player name when this " +
                "appear in the kill feed, for the context player names in kill feed are represented by the color of his Team but in order to the local player easily knows when an event that include him appear in the kill feed, his name " +
                "should be highlight with a different color, <b>to choose that color</b> go to GameData -> <b>HighLightColor.</b>\n \nOkay, that are the front-end customize options, if you want to customize the way that the UI looks" +
                " you have to do the in the UI prefab which is located in: <i>Assets -> MFPS -> Content -> Prefabs -> UI -> Instances -> <b>KillFeed</b></i>, drag this prefab inside the kill feed panel in canvas which is located by default in: ");
            DrawServerImage(2);
            DrawText("Right, these are all customize options that you have in front end, if you wanna create your own events to display, check the next step.");
        }
        else if (subStep == 1)
        {
            DrawTitleText("CREATE KILLFEED EVENTS");
            DrawText("The kill feed system various type of events to display, use the one that fits your event:\n \n<b>Kill Event:</b>\n \n• This is should use when of course a kill event happen, but a kill that include two actors " +
                "the killer and the killed, to show that you have to call this:");
            DrawCodeText("bl_KillFeed.Instance.SendKillMessageEvent(string killer, string killed, int gunID, Team killerTeam, bool byHeadshot);");
            DrawText("<b>Message:</b>\n \n• If you want to show a simple text of an event in concrete that doesn't include a player in specific, use:");
            DrawCodeText("bl_KillFeed.Instance.SendMessageEvent(string message);");
            DrawText("<b>Team Highlight:</b>\n \n• If you want to show a text of an event in concrete that as subject have a team in specific and you wanna highlight a part of the text with the tam color, use:");
            DrawCodeText("bl_KillFeed.Instance.SendTeamHighlightMessage(string teamHighlightMessage, string normalMessage, Team playerTeam);");
        }
    }

    void PlayerClassesDoc()
    {
        DrawText("MFPS use \"<b>Classes</b>\" system to diversify the weapon loadout, these classes are: <b>Recon, Support, Assault, and Engineer</b> each with distinct weapons.\n\nYou can set up the weapon loadout of each class per player prefab in the <b>bl_GunManager</b> script attached in the <b>WeaponsManager</b> object inside of the player prefab.\n\nEach class require 4 weapons <i>(Primary, Secondary, Perk and Letal)</i>, for set up the default weapons for each class you have two options: Create a new Present/ScritableObject or Edit the default one, basically you only have to create a new Scriptable if you want to keep a backup of the current class setup or if you want use a different setup in a player prefab, if you don't need that, simply edit the default instance.\n\nindependent if you want to create or just edit open the Player Prefabs or the specif player prefab that you want to edit the player class for, then go to the <b>WeaponsManager</b> object -> <b>bl_GunManager</b> inspector -> foldout the target Player Class -> set the weapons for each slot.\n\n(if you want to create a new present before edit, simply click in the <b>New</b> button");
        DrawServerImage(8);
        DrawText("You also can edit the default class loadouts from the Project Window in the folder: <i>Assets->MFPS->Content->Prefabs->Weapons->Loadouts</i>\n");
    }

    void HeadBobDoc()
    {
        DrawText("<b>Head Bob</b> is the camera movement that simulates the reaction of the player head when walk or run, in MFPS this movement is procedurally generated by code and you can adjust the value to obtain the desired result.\n\nIn order to obtain a more realistic result in MFPS we have sync the weapon bob and the head bob movement, so the settings will apply to both movements.\n\nYou can modify the values in the bl_WeaponBob.cs<i> (Attached in WeaponsManager object inside the players prefabs')</i>, you can edit in runtime to preview the movement as you edit it.\n");
        DrawServerImage(9);
        DownArrow();
        DrawText("If you want to use different movements per player or just want to have a backup of the current movement settings you can create a new \"Present\" of the settings and assing it in the script instead of the current one.\n\nFor create a new present simple select the folder where you wanna create it <i>(In Project View)</i> -> Right Click -> MFPS -> Weapons -> Bob -> Settings -> Drag the created profile in the bl_WeaponBob -> Settings -> Them make the changes that you want.\n");
    }

    void AfkDoc()
    {
        DrawTitleText("AFK");
        DrawText("AFK is an abbreviation for <i>away from keyboard</i>, players are called AFK when they have not interact with the game in a extended period of time.  in multi player games AFK players could be a problem," +
            "like for example in MFPS where players play in teams, an AFK player represent free points for the enemy team, or in different context AFK player are used to leveling up, so that is way some games count with a system " +
            "to detect these AFK players and kick out of the server/room after a certain period of time begin AFK.  MFPS include this system but <b>is disable by default</b>.\n \n" +
            "In order to enable AFK detection, go to GameData -> turn on <b>Detect AFK</b>, -> set the seconds before kick out players after detected as AFK in <b>AFK Time Limit</b>");
    }

    void KickVotationDoc()
    {
        DrawTitleText("KICK VOTATION");
        DrawText("In order to give an option to players to get rip of toxic, hackers, non-rules players in a democratic way where a player put the option on the table and the majority of the players in room " +
            "decide to kick out or cancel the petition, MFPS include a voting system.\n \nTo start a vote in game, players have to open the menu -> in the scoreboard click / touch over the player that they want to request the vote -> " +
            "in the PopUp menu that will appear -> Click on <b>Request Kick</b> button.\n \nBy default the keys to vote are F1 for Yes and F2 for No, you can change these keys in bl_KickVotation.cs which is attached in <b>GameManager</b> " +
            "in maps scenes.");
        DownArrow();
        DrawText("If you want to implement your own way to start a voting request, you can do it by calling:");
        DrawCodeText("bl_KickVotation.Instance.RequestKick(Photon.Realtime.Player playerToKick);");
    }

    void FPArmsMaterial()
    {
        DrawText("Normally you will use the same hand model for all of your weapons model, using a different material and textures for each team, so you may encounter with the inconvenient of change the hand texture for each weapon in the player prefabs is a little bit annoying, well MFPS handle this.\nYou don't have two manually change the arms, sleeves, gloves, etc.. materials, you only" +
            "have to create a prefab and list all the arms materials along with the different textures per team.");
        DownArrow();
        DrawText("Let's start by creating a new \"Arms Material\" asset, in the <b>Project Window</b> select a folder where save the asset and do <b>Right Mouse Click</b> -> MFPS -> Player -> <b>Arm Material</b>");
        DrawServerImage(4);
        DownArrow();
        DrawText("Then select the created material and in the Inspector window you'll see a List, in this list you have to add all the materials your Arms model <b> that change of texture depending of the player team</b>, for example the default MFPS arms model have 3 materials: Sleeve, Skin and Gloves, but only the Sleeves and Gloves material change of texture, the skin is the same, so only those two materials are include in the list.\nProbably you only have to add the Gloves Material, so add a new field on the list and assign the material and add the different Textures depending of the Team.\n \nWith that the materials will automatically change of textures in runtime depending on which team the player spawm.");
        DrawServerImage(5);
    }

    void RoomPropertiesDoc()
    {
        DrawHyperlinkText("There're some properties that are different per room/match besides the game mode, that you can tweak, e.g: the max players options, max rounds time limits, game goals, etc... these properties options can be different per game mode and you can modify them in the game mode info like this:\n\n► Go to <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ Game Modes ➔ <i>*Open a game mode*</i> ➔ there you will see the list and\noptions to modify these properties.\n\n\n■ <b>Max Players:</b> the maximum number of players options that can join in the room for this\ngame mode.\n\n■ <b>Game Goals Options:</b> the score/point/kills goals options of this game mode.\n\n■ <b>Time Limits:</b> the rounds time limits options for this game mode in seconds.");
    }

    void DrawRoomOptions()
    {

    }

    void DrawTeamsDoc()
    {
        DrawText("On MFPS there are various game modes that use Team systems like CTF (Capture the Flag) or TDM (Team Death Match), for default these teams are named as \"Delta\" and \"Recon\", you can modify these team names and representative color, for it go to <b>Game Data</b> in find the \"Team\" section:\n");
        DrawServerImage(13);
    }

    void DrawLobbyChat()
    {
        DrawText("MFPS 2.0 include a <b>lobby chat system</b>, for players by able to communicate between meanwhile search or wait for join to a match, this chat use Photon Chat plugin, for use it you need to have a Photon Chat AppID <i>(it's not the same that Photon PUN AppID)</i>, you can get this appid from your photon dashboard:\n\nGet your AppId from the Chat Dashboard:");
        if(Buttons.FlowButton("Chat Dashboard"))
        {
            Application.OpenURL("https://www.photonengine.com/en-US/Chat");
        }
        DrawText("when you have your Chat AppID, paste it on the PhotonServerSettings:");
        DrawServerImage(6);
    }

    void DrawPlayerAnimationDoc()
    {
        DrawText("MFPS 2.0 use Mecanim for handle the player animations so change animations clips is as simple as drag and drop the animation clip in the motion state in the Animator window, you only need a humanoid animation clip and the assign in the Animator controller:\n\n<size=14>1 - Locate the required motion state</size>\n\n- First to all you need find the animation that you want replace, for example if you want to change the \"Reload Rifle\" animation,\n\nyou need find it in the player <b>Animator Controller</b> tree view, go to (Project View) MFPS -> Content -> Art -> Animations -> Player -> \n");
        DrawServerImage(14);
        DrawText("With the Animator Controller select, open the Animator Window (Window -> Animator), on the Animator view you need figure out for what part of player body is this animations Bottom or Up body (Legs or Arms), example \"Rifle Reload\" is for Arms so is for Upper Body, go to \"Layers\" - and select \"Upper\" layer, there you will see various state machine with the name of the weapon to which it belongs, select the weapon:\n");
        DrawServerImage(15);
        DrawText("Once you open that state machine you will see others states that represent the weapon states, as this example we are looking for \"Reload\" state, select the Reload state and in the inspector view you will see the settings of this state specifically the \"Motion\" field, in this you to need drag / replace the animation clip with the new one:\n");
        DrawServerImage(16);
        DrawText("ready, you have replace the animation, for all other animations is the same steps, you also can play with other settings of states like speed, trasittion time, etc... you may need have basic knowledge of Mecanim for not break the tree system.\n");
    }

    void PlayerPrefabsDoc()
    {
        DrawHyperlinkText("We called <b>Player Prefabs</b> to the unity prefab that contain all the required scripts, objects, and structure that make up the player controller.\n\nThe MFPS player prefabs are located in a special Unity folder called <b>'Resources'</b> <i>(you can find it inside the MFPS folder)</i>, if you wanna change anything related to one of the players like, modify a weapon position, animation, a script property, etc... you have to apply the change to these prefabs.\n\n<b>By default, MFPS uses 2 player prefabs</b> which are assigned in the <b><link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link></b> ➔ <b>Player1</b> and <b>Player2</b>, Player1 is used for Team 1 and Player2 for Team 2, in case the game mode is not a team-based mode like the Free For All game mode, the Player1 is used.\n\nSince version 1.8 you can also override the Player1 and Player2 per scene, which means that you can use different player prefabs for each team in each map, to do that you simply have to attach the script <b>bl_OverridePlayerPrefab.cs</b> in any object of your map scene ➔ then in the inspector of this script you will see the fields to assign the player prefabs and that's all.");
        DownArrow();
        DrawHyperlinkText("In case you are looking for a more advanced solution for the player selection or want to add more player prefabs where the players can select in-game their character, you should take a look at the <link=https://www.lovattostudio.com/en/shop/addons/player-selector/>Player Selector</link> addon.\n");
    }

    void DrawBullets()
    {
        DrawText("in MFPS bullets are pooled and like all other pooled objects in MFPS they are listed in bl_ObjectPooling script, which is attached in the GameManager on each map scene.\nin bl_Gun you assign only the \"Pooled Name\".\n\n\n\nYou may want to add a new bullet for a spesific weapon, let's say you want add a different Trail Render, well for do it you can do this:\n\n- Duplicated a bullet prefab:\n\nSelect one of the existing prefab (MFPS -> Content -> Prefabs -> Weapon -> Projectiles -> *, right click -> Duplicated.\n\nThen make the changes that you want to this prefab and after this add a new field in bl_ObjectPooling (it is attached in GameManager object in room scenes), in the new field drag the bullet prefab and change the pooled name:\n\n");
        DrawServerImage(17);
        DrawText("then open a player prefab and select the <b>FPWeapon</b> that you want assign the bullet, in the bl_Gun script of that weapon, write the pooled name of the bullet in the field \"Bullet\".\n");
        DrawServerImage(18);
        DrawText("Apply changes to the player prefab and ready.\n");
    }

    void DrawKitsSystem()
    {
        DrawText("MFPS have a simple but functional 'Kit System' where players can throw and pick up Ammunition or Medic kits in the map during the game, for default player can throw these kits whith the '<b>H</b>' key, the type of kit <i>(ammo or medic)</i> depend of the player class.\n");
        DrawServerImage(7);
        DownArrow();

        DrawTitleText("Change the Key to throw kits");
        DrawText("- in the root of Player prefabs you have a script called <b>bl_ThrowKits</b>, in this one you have  the propertie called <b>Throw Key</b>, there you can set the Key code for throw the kits.\n");
        DownArrow();
        DrawTitleText("Change the model of the kits");
        DrawServerImage(19);
        DrawText("•  You can find the kits prefabs in: MFPS -> Content -> Prefabs -> Other -> *\n\n•  Select the kit that you want change the model (MedKit or AmmoKit) and drag to the scene hierarchy.\n\n•  Replace the mesh with you new model, apply the changes and save the prefab.\n\n");
        DownArrow();
        DrawTitleText("Change the model of Kit deploy indicator");
        DrawServerImage(20);
        DrawText("You can find the prefab in MFPS -> Content -> Prefabs -> Other -> KitDeployIndicator\n");
    }

    void DrawKillZones()
    {
        DrawText("may be the case that there are limits in your map that you want to the players don't go any further, a solution that MFPS have for these cases is the <b>Kill Zones</b> where if the player enter, a warning will appear with a count down timer, if the player not leave this zone before the timer reach 0, he will automatically killed by the game and returning to a spawnpoint.\n");
        DrawServerImage(21);
        DrawText("to add a kill zone simple add a object with a <b>Box Collider</b> <i>(the Box Collider represent the zone)</i>, then add the script bl_DeathZone.cs script, setup the time that the player have to leave this zone and the string message that will appear in screen while player is in kill zone.\n");
    }

    void DrawGameSettings()
    {
        DrawText("MFPS allows players to modify some game settings of the game in-runtime like graphics quality and control settings, you as the developer are in charge to set the default values for these settings, the value that the player will have the first time that they play the game.");

        DrawHyperlinkText("To set the default values go to <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ Default Settings ➔ Setting Values ➔ *");
        DrawServerImage(28);
        DrawText("In this list, you will have all the available values, simply unfold the setting that you want to modify and set the desired value.");
        DownArrow();
        DrawTitleText("Add a new setting");
        DrawText("Add a new setting is really simple, in the same list <i><b>(Setting Values)</b></i> add a new field, set a unique name to the to identify the setting ➔ set the type of setting <i>(float, integer, bool, or string)</i> then set the default value.\n\nNow to use this value in-game you can load it with:");
        DrawCodeText("var val = bl_MFPS.Settings.GetSettingOf('THE_SETTING_NAME');");
        DrawText("once the setting is added in the list, it will automatically be saved when the player applies the settings in-game <i>(click on the <b>Save</b> button)</i>, but if you want to save the setting with your own rules you can do it with:");
        DrawCodeText("bl_MFPS.Settings.SetSettingOf('THE_SETTING_NAME', THE_SETTING_VALUE);");
        DrawText("As a reference of how you can use in-game you can inspect the script <b><color=#00E9FFFF>bl_SingleSettingsBinding.cs</color></b>\n");
    }

    void DrawObjectPooling()
    {
        DrawText("<b><size=15>What is Object Pooling?</size></b>\n\n<b>Instantiate()</b> and <b>Destroy()</b> are useful and necessary methods during gameplay. Each generally requires minimal CPU time.\n\nHowever, for objects created during gameplay that have a short lifespan and get destroyed in vast numbers per second like Bullets per example, the CPU needs to allocate considerably more time.\n\nThere is when Object Pooling is enter, <b>Object pooling</b> is where you pre-instantiate all the objects you’ll need at any specific moment before gameplay, in MFPS bullets, decals and hit particles are pooled.\n\nThe bl_ObjectPooling.cs class is really easy to use, all what you need to do to add a new object to pooled is listed the prefab in the <b>'pooledPrefabs'</b> list of bl_ObjectPooling inspector which is attached in the <b>GameManager</b> object in the map scenes, once you add the prefab simply set a key name and how many instances of this prefab you think will be enough and that's.\n\nNow for instance this prefab from a script, before you normally will use something like:\n\n");
        DrawCodeText("GameObject ob = Instantiate(MyPrefab, position, rotation);");
        DrawText("with bl_ObjectPooling script you simply has to replace that with:");
        DrawCodeText("GameObject ob = bl_ObjectPooling.Instance.Instantiate(\"PrefabKey\", position, rotation);");
    }

    void AddNewMenu()
    {
        DrawText("If you want add a new menu/window in the Lobby UI, follow this:\n\n1 - Create the menu/window UI: make the design of UI with what you need, but make all the design as child of a parent under canvas, example \"<i>MyNewWindow</i>\" (this is a empty game object under canvas) put all the buttons, text, images, etc.. of your new menu/window under this object.\n\n");
        DrawAnimatedImage(0);
        DrawText("2 - Add a new field in the <b>Windows</b> list in Lobby -> Canvas [Default Menu] -> bl_LobbyUI -> Windows, in the new field in this list and add the \"<i>MyNewWindow</i>\" object in the field and assign an unique name.\n");
        DrawAnimatedImage(1);
        DrawText("3 - Create a menu button: add a new button that will open the new menu/window, all the other buttons are in: Lobby -> Canvas -> Lobby -> Content -> Top Menu -> Buttons -> *, so you can duplicate one of these buttons and change the title text.\n\nin this new button add as listener the function of bl_Lobby -> ChangeWindow(string) -> and set the name of new window in the list \"Windows\".That's.\n");
        DrawAnimatedImage(2);
    }

    void DrawAddonsDoc()
    {
        DrawText("<color=#FFE300FF>M</color>FPS 2.0 comes with tons of features on the main package, but still missing some important features that almost all FPS have like e.g: <i>Mini Map, Login System, Level System, Localization, Shop, etc...</i> if I tell you that <b>MFPS have all those features</b>? Yes, MFPS have <b>extensions/add-ons</b> with which you can integrate all these features.\n\n<b><size=22>So you may wonder why they are not added by default?</size></b>\n\nThe main reason is to keep a relatively low price for the main core package, if all the addons were added by default in the main core the price of the package would rise to at least $250.00 or more, so we decided to add the essential features to the core and let developers choose which extra extensions they want to integrate, take in mind that the main core is fully functional by itself and doesn't require any the add-ons, they all are optional.\n\nYou can purchase these addons and import the package in the MFPS 2.0 project, pretty much every addon comes with an Automatically integration which means that you doesn't have to made any  manual change in order to integrate them.\n\nIf you wanna check which addons are available you can open the <b>Addon Manager</b> window in MFPS -> Addons -> Addons Manager.\n");
        if(Buttons.FlowButton("Open Addons Manager"))
        {
            GetWindow<MFPSEditor.Addons.MFPSAddonsWindow>("Addons Manager");
        }
        DrawText("<b><size=22>How Integrate Addons?</size></b>\n\nAll addons comes with a <b>ReadMe.txt</b> on the root folder of the addon with the instructions, but pretty much <b>all Addons comes with an Automatically integration</b>, you only have to enable and click on the Integrate MenuItem:\n");
        DrawAnimatedImage(3);
    }

    void DrawGameTexts()
    {
        DrawText("If you want to change some text in the game or maybe just change the text grammar, the mayor part of text are directly set in the UI Text's components inside the canvas objects but also there are some text that are set by script, to make easy for you to find all these texts we have put all those in just 1 script: <b>bl_GameText.cs</b>\n\nin this script you will find all the text that are send by script, you can change of language or change grammar, this facilitates the work of for example adding your own location system.\n");
    }

    void DrawFriendListDoc()
    {
        DrawText("Photon have a <b>Friend List</b> feature which allow players to check the status of other users connected in the same server, you only have to send the UserIDs of the players that you wanna check the status, the system is pretty limited since you only can know when a player is \"Online\" or not, and only allows you join to the player room if this is in one.\n\nMFPS comes with this Photon feature added, in the Main Menu scene you can add friends and save it locally <i>(or in database if you are using ULogin Pro)</i>, once you added you'll be able to see when this player is online and also you will be able to join to the same room when the player is in one room.\n\nAdded a friend doesn't require confirmation of the other player, you simple set the exact player name and ready.\n\nIn order to add a friend simply click in the top right button (in the lobby) with the person icon:\n\n");
        DrawServerImage(10);
        DrawText("The friend list have a limit number of friends that can be added (by default is 25) this only make sense if you are saving the friends in a database (using ULogin Pro for example) since the more friends they add per player more will be the size of the player data in the database.\n\nYou can change this limit in GameData -> MaxFriendsAdded.\n");
    }

    void CrosshairDoc()
    {
        DrawText("The Crosshair or reticle is a basic feature in most shooter games, in MFPS you can easily change the shape, color, and size of the crosshair, also you can use different crosshair for each type of weapon.");
        DownArrow();
        DrawTitleText("Modify crosshair");
        DrawText("Open one of your map scenes ➔ go to <i>(in the Hierarchy window)</i> <b>UI ➔ PlayerUI ➔ Crosshair ➔ Crosshairs ➔ *</b>, there you will see all the available crosshairs setups, by default each one of them is used for different kind of weapons <i>(machineguns, shotguns, knife, etc...)</i>.\n\nSo what you have to do here is Open one of the crosshair setup/styles and apply any kind of modification that you want, you can remove or add any UI component that you want, just make sure everything is under the crosshair style root object.");
        DrawServerImage(32);
        DownArrow();
        DrawTitleText("Hit Marker");
        DrawText("The <b>hit marker</b> is a small cross that shows up when the local player hit an enemy.\n\nYou can customize the design of this on <b>UI ➔ PlayerUI ➔ Crosshair ➔ Crosshairs ➔ Hitmarker</b>.\n\nThe hit marker appears with simple scale-up animation, you can define the final size of the animation in <b>bl_UCrosshair ➔ Increase Amount</b>.");
    }

    void FootStepsDoc()
    {
        DrawText("The footstep sound in MFPS is driven by the surface Tag, there're some predefined tags like <i>Metal, Concrete, Dirt, Wood, and Water</i>, when a player moves a footstep sound will play depending on the surface where the player is over, of course, you can change these sounds or add more surface tags, also you can change the sounds per player prefab so you can have different sounds for different player models.");
        DrawTitleText("Change Sounds");
        DrawHyperlinkText("In order to change the default, MFPS footstep sounds you can simply replace the AudioClips in the default <link=asset:Assets/MFPS/Content/Prefabs/Presents/Audio/FootStepsLibrary.asset>FootStepLibrary</link>, unfold the <b>Groups</b> list ➔ unfold the tag group field ➔ replace the AudioClips in the list.");
        DrawServerImage(29);
        DownArrow();
        DrawTitleText("Add Surfaces");
        DrawHyperlinkText("If you wanna add a new surface <i>(identified by a different tag)</i> and you wanna play a specific footstep sound for it, you can do it by simply add a new field in the Groups list of the <link=asset:Assets/MFPS/Content/Prefabs/Presents/Audio/FootStepsLibrary.asset>FootStepLibrary</link>, in that new field, in the propertie <b>Tag</b> set the tag identifier for that surface.");
        DownArrow();
        DrawTitleText("Terrain Surfaces");
        DrawText("Since the footstep system is driven by <b>Tags</b> you can only set a tag per object/mesh, this comes with a problem for the <b>Unity Terrain system</b>, since the terrain is a single object/mesh, you can set only one tag for it, that is inconvenient because terrains usually have various layers with different textures that simulate different surfaces, so in order to make the footstep system work correctly with the Unity Terrain system <i>(play different sounds depending on the terrain layer)</i> you need to set up your terrain layers as follow.\n\n•  First, select the Terrain object in your scene hierarchy and add the script <color=#00E9FFFF><b>bl_TerrainSurfaces.cs</b></color>\n\n•  Once you add the script you will notice in the inspector of the script that the list <b>TerrainSurfaces</b> have various fields, each of these fields represent one of the layers <i>(textures)</i> in the <b>Terrain</b>, what you have to do is set the <b>Tag</b> name to each layer, if you fold out one of the fields you will see the texture of the layer, so depending on the texture you can set which <b>tag</b> should be assigned in the property <b>Tag</b>.");
        DrawServerImage(30);
    }

    void GameStaffDoc()
    {
        DrawText("You may want to highlight working game development members with a badge on their behalf for example<b> Lovatto <color=#FF0000FF>[Admin]</color></b> with a different color that normal players, so other users can see that is a staff member on the game, on MFPS 2.0 there are a simple way to do this and you can set up right on the inspector.\n\nGo to <b>Game Data</b> and find the \"Game Team\" section at the bottom of the inspector:\n");
        DrawServerImage(23);
        DrawText("in this list you can add much member as you want, with a simple settings to set up:\n");
        DrawHorizontalColumn("UserName", "The name that the staff member need write to access to this account.");
        DrawHorizontalColumn("Role", "The staff rank / role on the team.");
        DrawHorizontalColumn("Password", "When try to sing in with this account name a password window will appear to write the password (with other names will not appear), so normal player can't fake identity.");
        DrawHorizontalColumn("Color", "The color with the name text will appear in the game.");
    }

    void DrawMobileDoc()
    {
        DrawText("If you are build targeting to a mobile platforms e.g: Android or iOS, keep in mind that although MFPS does work on mobile platforms, there are some things that you have to do before to build, since by default MFPS 2.0 is setup with high quality graphics for high-end devices for demonstration purposes, so by default it's not optimized for mobile platforms, also the main core package doesn't include any mobile control/input.");

        DrawHyperlinkText("So first thing you need is a mobile input control, for this matter there is already an addon specifically for MFPS 2.0 which is: <link=https://www.lovattostudio.com/en/shop/addons/mfps-mobile-control/>Mobile Control</link>, which contains all the necessary buttons/inputs to work in mobile/touch devices, but alternatively you can integrate your prefer system if you want.\n");

        DrawText("Secondly you have to made some manual optimization work, the same that you would do in any other mobile project, remove the Post-Process effects <b><size=8><i>(you can do this by removing the Post Processing Stack package from the Unity Package Manager)</i></size></b>, Change the Standard Shaders to mobile friendly ones, reducing the textures quality, of course your new levels/maps have to be mobile friendly, etc... <b>You don't have to do any code change</b>, MFPS scripts are mobile ready and optimized for low-end platforms.\n");
        DrawText("So basically, to build for mobile what you have to do is some graphic optimization, bellow I will leave you some useful links to tutorials that will help you with graphic optimization and things to keep in consideration:\n");
        DrawLinkText("https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html", true);
        DrawLinkText("https://learn.unity.com/search?k=%5B%22tag%3A5816095d0909150016dc7b17%22%5D", true);
        DrawLinkText("https://cgcookie.com/articles/maximizing-your-unity-games-performance", true);
        DrawLinkText("http://www.theappguruz.com/blog/graphics-optimization-in-unity", true);
    }

    void PostProcessingDoc()
    {
        DrawText("By default MFPS use the Unity <b>Post-Processing stack v2</b>, which is a collection of effects and image filters that apply to the cameras to improve the visual of the game.\n\nAlthough the image effects really improve the visual aspect of the game, that also carries a cost in the performance of the game, that is why you should use this only for high-end platforms like PC or Consoles, you definely <b>should NOT use for Mobile Platforms</b>, if you are targeting for a mobile platform you should delete this package.\n\nThis system is automatically imported from the Unity Package Manager (UPM) when you import MFPS for the first time in the project, if you want to <b>DELETE</b> it, you can do it clicking on this context menu:");
        DrawServerImage(11);
        DrawText("You can find the Post-Processing Stack official documentation here:");
        DrawLinkText("https://docs.unity3d.com/Packages/com.unity.postprocessing@2.3/manual/index.html", true);
    }

    void MFPSEventsDoc()
    {
        DrawText("There're some special events that you can use if you wanna implement a custom feature or modification in your own scripts, e.g: when the local player spawn, when the local player dies, when receiving damage, etc...\n\nUse these events are really simple, all you have to do is subscribe a function from your script that will listen to the callback of these events when they are dispatched in runtime.\n\nYou subscribe to these events on <b>OnEnable()</b> and unsubscribe on <b>OnDisable()</b> functions:");
        DrawCodeText("void OnEnable()\n{\nbl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;\n}\n\nprivate void OnDisable()\n{\nbl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;\n}\n\nvoid OnLocalPlayerSpawn()\n{\n//execute your code\n}");
        DownArrow();
        DrawText("Below you will have the list of all the available events with a short description\n");
        DownArrow();

        DrawCodeText("bl_EventHandler.onLocalPlayerDeath");
        DrawText("Event called when the LOCAL player die in game");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalPlayerSpawn");
        DrawText("Event called when the LOCAL player spawn");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPickUpGun");
        DrawText("Event called when the local player pick up a weapon");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onChangeWeapon");
        DrawText("Event Called when the LOCAL player change of weapon");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalAimChanged");
        DrawText("Event Called when the local player change their Aim state");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onMatchStart");
        DrawText("Event Called when the room match start");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onFall");
        DrawText("Event Called when the LOCAL player fall/land in a surface");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPickUpHealth");
        DrawText("Event called when the LOCAL player pick up a health in game");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onAirKit");
        DrawText("Event called when the LOCAL player call an air drop");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onAmmoPickUp");
        DrawText("Event called when the LOCAL player pick up ammo in game");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalKill");
        DrawText("Event called when the Local player get a kill or get killed in game.");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.OnRoundEnd");
        DrawText("Event called when a game round finish");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPlayerLand");
        DrawText("Event called when the LOCAL player land a surface after falling");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onRemoteActorChange");
        DrawText("Event called when a player that is not the local player spawn or die");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGameSettingsChange");
        DrawText("Event called when the local player change an in-game setting/option");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onEffectChange");
        DrawText("Event called when the local player change one or more post-process effect option in game.");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGameSettingsChange");
        DrawText("Event called when the LOCAL player change their game settings from the settings in-game menu.");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGamePause");
        DrawText("Event called when the LOCAL player pause or resume the game.");
        DrawHorizontalSeparator();
    }

    void UpdateMFPSDoc()
    {
        DrawText("MFPS get mayor version update approximately every 3-4 months, these updates comes with new features, fixes, and improvements, if you were already using MFPS you most likely will want to apply all those improves and fixes to your started project, unfortunately, due to the nature of the asset you can't just import the new update package over your existing project with the older version because that will override all your changes, making you lost all the work and progress that you have done until that moment.\n\nThe work required to update an old MFPS version project to a new MFPS version depends on various factors, like how much changes have you done, the MFPS version of your project compared to the new version, the number of changes in the new update, etc...\n\nBut would give you some method to update your MFPS version in different scenarios:");
        Space(10);
        DrawNote("<b><color=#FF0002FF>IMPORTANT:</color> BEFORE TRY ANY OF THE BELOW METHODS OR TRY UPDATE YOUR PROJECT IN GENERAL MAKE SURE TO CREATE A BACKUP COPY OF YOUR PROJECT </b>");
        Space(10);
        DrawTitleText("Front-end changes");
        DrawText("- If you only have made frontend changes like, replace the <i>player models, tweak properties, add maps, add weapons, change UI, etc...</i> but not backend changes (code changes), then update is simpler, you can import the new MFPS package over your project and just <b>unselect</b> some assets mentioned below:\n\nFirst, import the MFPS update package from the Asset Store or your disk.\n\nIn the <b>Unity Package Import window</b>, you can select which assets import and which not, by default, all assets are selected, you simply have to <b>unselect</b> the following to not override your changes:\n\n•  The <b>Resources</b> folder <i>(with all the prefabs inside and GameData)</i>\n•  MainMenu scene <i>(in case you changed the UI)</i>\n•  Any prefab that you change.\n\nDo import the ExampleLevel scene, since it almost sure that it will contain changes, and you can use as reference for your other maps scenes.\n");
        DrawServerImage(12);
        DownArrow();
        DrawTitleText("Back-end changes");
        DrawText("If you have made <b>code changes</b> to the MFPS to core scripts or modifications to the main prefabs like <b>GameManager, GameModes, Lobby, etc...</b> the update process is more complicated.\n\nFor start, it is not possible to import the new version on your current project since this would revert all your changes losing all the work done by you, to apply the changes of the new version you must merge the changes manually, checking the scripts and prefabricated modifications of the new version and comparing them with those of your project.\n\nThis process may take some time to be done and there're high chances that a wrong merger cause errors in your game, so I will give you a method that will facilitate and reduce the work to do in this case:\n\n•  First, create a new clean Unity project, in it, import the new MFPS version package as you will normally do <i>(importing Photon PUN as well)</i>, you will use this project as reference to see the frontend changes in scenes, prefabs, properties, etc...\n\n•  Then, you will <b>simulate</b> import the new MFPS version package in your started project <i>(the project with the old version of MFPS that you have modified)</i>, note that I said <b>simulate</b> because <b>you won't import it</b>, you only need that the Unity Package Import Windows show up so you can check the changed files, this window shows all the assets that the package contains and will be imported but also if one of the files already exist in the project, it shows if the file from the package is different with respect at the file in the project, you can differentiate hthose modify files with a \"Refresh\" icon at the right side of the file name:\n");
        DrawServerImage(26);
        DrawText("Now what you have to do is take note of the modified files, write their name in a simple text file so you can find later by their name as explained below.\n\nOnce you have all the changed file names collected, is time merge those files, unfortunatelly there's not a automated way to do this, you have to manually do this, check the changes on scripts, prefabs, etc...  but don't worry you don't have to check line by line, you can use some tools to automatically detect the changes in two files.\n\nFor check changes on scripts you can use these tools:\n");
        DrawHyperlinkText("Standalone program: <link=https://sourcegear.com/diffmerge/>Diffmerge</link>\nOnline System: <link=https://www.diffchecker.com/>Diffchecker</link>\n");
        DrawText("Both systems are really straight forward to use, you have two boxes, in one you assign the script and or code of the old file/script and in the other you assing the new file, then the program will analyze and highlight you the changed lines:\n");
        DrawServerImage(27);
        DrawText("You will assign the original file <i>(the one from your project)</i> in the left side and the file from the new MFPS version <i>(from the project that you create before)</i> in the right side.\n\nNow with the changes highlighted in the program you simple have to merge the changes in your project file <i>(the file of the old MFPS version)</i>\n\nWith the standalone program <b>Diffmerge</b> you can compare the whole MFPS folder and analyze all the files, by simple selecting the option: File -> Open Folder Diff... -> Set the path to the old version MFPS folder in the first box and set the path to the new version MFPS folder in the second box.\n");
        DownArrow();
        DrawText("Now that method works efficiently for text files like scripts, but for check the differences between prefabs and scenes you will have to do it differently, for check the difference between prefabs and scenes you will have to compare both projects, Fortunately, Unity allows you to open more than one instance of the editor, so you can open the old version project and the new MFPS version project in different editor windows and inspect the differences between the prefabs and or scenes in both projects.\n");


    }

    void CommonQADoc()
    {
        DrawSpoilerBox("Bots walk trough walls and objects", "You have to bake the <b>Navmesh</b> in your map scenes in order to let the bots know where they can navigate in your map.\n\nIf you don't know what Navmesh is or how to bake it, check this: <link=https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html>https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html</link>");

        DrawSpoilerBox("Map objects randomly disappear in certain areas.", "This is caused you are using <b>Occlusion Culling</b> and you haven't Bake it in your map scene, for more info about Occlusion Culling, check this: <link=https://docs.unity3d.com/Manual/OcclusionCulling.html>https://docs.unity3d.com/Manual/OcclusionCulling.html</link>\n\nHow to bake it, check this:\n<link=https://docs.unity3d.com/Manual/occlusion-culling-getting-started.html>https://docs.unity3d.com/Manual/occlusion-culling-getting-started.html</link>");

        DrawSpoilerBox("Is there a max limit of players per room?", "There's not a fixed number of players that can join in the same room at the same time, but since each player add an extra stress to the server and consume resources for the local client device after certain amount of players the game will start to feel 'Laggy' both in refresh rate (FPS) as in the network latency (Ping).\n\nThe number of players before the game start experimenting this performance issue depend on various factors like the runtime platform, device specs, network connection, etc...\n\nWe have done our own tests with the default MFPS (1.5) to have some benchmarks, these are the result:\n\nFor <b>PC</b> with these specs:\n<i>Intel i7 3.70GHz\n16Gb Ram\nNvidia GTX 1070</i>\n\n18 players in the same room\nMedium graphic quality\nRun with a average of 60-80 FPS\n\nFor <b>Mobile</b>:\nusing a Samsung S8 Plus\n\n12 Players in the same room\nWith MFPS optimized for mobile <i>(not the default scenes)</i>\nRun with a average of 60-75 FPS\n\nYou can use these statistics as a reference but keep in mind that there are many factors which can influence the result, so it is recommended that you do your own tests.");

        DrawSpoilerBox("Can I can convert MFPS to TPS (Third Person)", "Since <b>MFPS</b> comes with full source code practically everything is possible to change/edit,\nbut no all is supported by default with a front-end option, this is the case for TPS.\n\nMFPS has been designed for First Person View only and there's not an option to use as a Third Person View game, in order to do that you have to manually change the required code and setup the player prefabs for this purpose.");

        DrawSpoilerBox("Alternative network solution than Photon?", "Since MFPS comes with full source code you can make any change that you want, that includes integrate other network solution.\n\nBut by default this is not possible from a front-end option <i>(like a toggle to switch between libraries)</i>,\nintegration other network library will require a lot of code changes to switch from the Photon syntax to your network sdk syntax, and there is the possibility that some methods or features of Photon do not exist or are done differently in your network library.\n");

        DrawSpoilerBox("Can I host my own dedicated server?", "Yes, Photon offers a solution for host your own server using their <b>Photon OnPremise</b> <i>(a.k.a Photon Server)</i> switch to this from the default Photon PUN (Cloud) doesn't require code changes, all that you have to do is setup the Photon Server SDK and set the IP in your PhotonServerSettings.\n\nInfo: <link=https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-in-5min>Photon Server Information</link>\n");

        DrawSpoilerBox("Rooms doesn't show up for other players?", "Sometimes when you testing your game with other players you may find out that the rooms that you or one of the testers created doesn't appear in the Room/Server list, the most common cause of this is that you and your tester <b>are not in the same server region</b>.\n\nIn the MainMenu/Lobby scene, in the bottom right corner you will see a drop-down with a region name, you can use it to select and change of server, so make sure you and your testers are connected to the same server region.");
    }

    [MenuItem("MFPS/Tutorials/Documentation", false, 111)]
    private static void ShowWindowMFPS()
    {
        EditorWindow.GetWindow(typeof(MFPSGeneralDoc));
    }
}