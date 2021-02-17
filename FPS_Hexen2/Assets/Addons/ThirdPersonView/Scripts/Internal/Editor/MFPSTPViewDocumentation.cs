using System.Collections.Generic;
using UnityEngine;
using MFPSEditor;
using UnityEditor;

public class MFPSTPViewDocumentation : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/tpview/";
    private NetworkImages[] ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.png", Image = null},
        new NetworkImages{Name = "img-2.png", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "gif-tpv-1.gif" },
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Get Started", StepsLenght = 0, DrawFunctionName = nameof(GetStartedDoc) },
     new Steps { Name = "Change View", StepsLenght = 0, DrawFunctionName = nameof(ChangeViewDoc) },
     new Steps { Name = "View Position", StepsLenght = 0, DrawFunctionName = nameof(ViewPositionDoc) },
    };

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
            base.m_GUISkin = gs;
        }
    }
    //final required////////////////////////////////////////////////

    void GetStartedDoc()
    {
        DrawText("<b><size=20>REQUIRE:</size></b>\n\n■ MFPS 1.8++\n■ Unity 2018.4++\n\n\nThe integration is <b>2-clicks</b> only:\n\n▉ <b>Enable the addon:</b> Go to MFPS ➔ Addons ➔ Third Person ➔ <b>Enable</b>.\n▉ <b>Setup the players:</b> Go to MFPS ➔ Addons ➔ Third Person ➔ <b>Integrate</b>.");

        DrawHyperlinkText("The default key to change view in-game is <b>'P'</b>, you can change this in the <link=asset:Assets/Addons/ThirdPersonView/Resources/CameraViewSettings.asset>CameraViewSettings</link> ➔ <b>ChangeViewKey</b>.");
    }

    void ChangeViewDoc()
    {
        DrawHyperlinkText("With this addon, you can define if you want the players can switch the player view <i>(from the first person to the third person and vice versa)</i> in-game pressing a single key/button or you can set the third person as the unique player view of the game.\n\nYou can set that option in the <link=asset:Assets/Addons/ThirdPersonView/Resources/CameraViewSettings.asset>CamerViewSettings</link> ➔ <b>Game Player View</b>.\n");
        DrawServerImage(0);
    }

    void ViewPositionDoc()
    {
        DrawText("If you want to modify the default <b>Third Person View</b> camera position or the <b>Aim position</b>, you can do it as follow:\n\n1. Drag one of the player prefabs <i>(from the Resources folder)</i> in a scene <i>(any scene but preferably an empty scene)</i>.\n\n2. With the instanced player selected, go to the inspector window ➔ <b>bl_PlayerCameraSwitcher</b> ➔ there you will see two red buttons, the first one is for modify the default third-person view and the second button is for modifying the default Aim position (in the third person),\nso depending on which one you want to modify, click on the respective button.");
        DrawServerImage(1);
        DrawText("3. Once you click the button you will see on the <b>Game View window</b> how the player camera automatically is positioned in the current third-person position, what you have to do is really simple, with the player Main Camera selected, move it wherever you wanna it to be the default view position <i>(use the Game View window for preview the position)</i>, once you have it, simply click on the red button again and that is!");
        DrawAnimatedImage(0);
    }

    [MenuItem("MFPS/Tutorials/Third Person")]
    private static void Open()
    {
        EditorWindow.GetWindow(typeof(MFPSTPViewDocumentation));
    }

    [MenuItem("MFPS/Addons/Third Person/Documentation")]
    private static void Open2()
    {
        EditorWindow.GetWindow(typeof(MFPSTPViewDocumentation));
    }
}