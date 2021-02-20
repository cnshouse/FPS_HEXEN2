using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using System.IO;
using System;

public class InputManagerDocumentation : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/input-manager/";
    private NetworkImages[] ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-0.png", Image = null},
        new NetworkImages{Name = "img-1.png", Image = null},
        new NetworkImages{Name = "img-2.png", Image = null},
        new NetworkImages{Name = "img-3.png", Image = null},
        new NetworkImages{Name = "img-4.png", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "createwindowobj.gif" },
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Integration", StepsLenght = 0, DrawFunctionName = nameof(IntegrationDoc) },
     new Steps { Name = "Default Mapped", StepsLenght = 0, DrawFunctionName = nameof(DefaultMappedDoc) },
     new Steps { Name = "Add Input", StepsLenght = 0, DrawFunctionName = nameof(AddInputDoc) },
     new Steps { Name = "GamePad", StepsLenght = 0, DrawFunctionName = nameof(GamePadDoc) },
     new Steps { Name = "Create Mapped", StepsLenght = 0, DrawFunctionName = nameof(CreateMappedDoc) },
    };

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(ServerImages, AllSteps, ImagesFolder, AnimatedImages);
    }

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }
    //final required////////////////////////////////////////////////

    void IntegrationDoc()
    {
        DrawText("1 - Enable the addon in <i>(Top Menu)</i>MFPS -> Addons -> InputManager -> Enable.\n\n2 - Open the MainMenu scene and run the Lobby Integration on <b>MFPS -> Addons -> InputManager -> Lobby Integration.</b>\n\nThat's.\n\nFor open the key binding menu in game click on the Settings button -> select the Control tab -> there you go.\n");
        DrawServerImage(0);
    }

    void DefaultMappedDoc()
    {
        DrawText("If you want change the default input mapped of the keyboard or gamepad you simple have to modify the mapped object.\n\nSelected the input mapped scriptable object, if you are using one of the default ones, they are located at: <i>Assets->Addons->InputManager->Content->Prefabs->Mappeds->*</i>, select it and in the inspector you will have the list of all the setup inputs, fold out the input that you want to modify and edit the info <i>(keycode, axis name, description, etc..)</i>\n");
        DrawServerImage(1);
        DownArrow();
        DrawText("Also you can reorder the input order, the same order in this list is the order that will be displayed in game.\n");
    }

    void AddInputDoc()
    {
        DrawText("Add a new input is really simple, basically you only have to add a new field in the input mapped and set the keycode of the input.\n\n1 - Select the Input Mapped in which you want to add the input, by default there're only 2: Keyboard and Xbox Controller, so select the one that you want edit, they are located at: <i>Assets->Addons->InputManager->Content->Prefabs->Mappeds->*</i>.\n\n2 - In the inspector of the mapped you will see the list called<b> Button Map</b> with all the current inputs, add a new field in this list and fill the info of it:\n");

        DrawPropertieInfo("KeyName", "string", "The custom key name of this input");
        DrawPropertieInfo("PrimaryKey", "KeyCode", "The Unity Keycode of this input");
        DrawPropertieInfo("PrimaryAxis", "string", "if this input is not a key but a axis, set the name here.");

        DownArrow();

        DrawText("Once you have configured the key, now you can use it in your code, the usage is pretty similar to the default Unity Input, instead of:\n");
        DrawCodeText("Input.GetKeyDown('keyName'){...}");
        DrawText("You have to use:");        
        DrawCodeText("bl_Input.isButtonDown('keyName'){...}");
        DrawText("or");
        DrawCodeText("bl_Input.isButton('keyName'){...}\nbl_Input.isButtonUp('keyName'){...}");
        DrawText("Where the <i>'keyName'</i> value is the <b>KeyName</b> of your setup input in the Input Mapped.\n");
    }

    void GamePadDoc()
    {
        DrawText("In order to use a GamePad/Controler with MFPS and Input Manager you have to do some extra steps.\n\n- First you have to modify the Unity Input Settings to add the required control axis, Input Manager comes with a prepared Input Settings.asset with all this already setup, so you only have to override it in your project by unzip the asset from: <i>Assets->Addons->InputManager->Content->UnityInputSettings->UnityInputSettings</i>, that will give you a <b>InputManager.asset</b> which you have to move the ProjectSettings folder of your Unity project to override the current one.\n\nbut this can be done automatically by just clicking the button below.\n");

        if (!File.Exists("ProjectSettings/InputManager-backup.asset"))
        {
            if (Buttons.FlowButton("Setup Unity Input Manager"))
            {
                string sourcePath = "Assets/Addons/InputManager/Content/UnityInputSettings/InputManager.txt";
                if (!File.Exists(sourcePath))
                {
                    Debug.LogWarning("The MFPS InputSettings data couldn't be found.");
                    return;
                }
                string imFile = "ProjectSettings/InputManager.asset";
                if (!File.Exists(imFile))
                {
                    Debug.LogWarning("The InputManager data couldn't be found.");
                    return;
                }
                File.Move(imFile, imFile.Replace("InputManager", "InputManager-backup"));
                File.Copy(sourcePath, "ProjectSettings/InputManager.asset");
                AssetDatabase.Refresh();
            }
        }
        else
        {
            GUILayout.Label("MFPS input settings has been integrated already.");
        }

        DownArrow();

        DrawHyperlinkText("Now in the <link=asset:Assets/Addons/InputManager/Resources/InputManager.asset>InputManager</link> in the field <b>Mapped</b> set the input mapped for your controller, by default the addon comes with the <link=asset:Assets/Addons/InputManager/Content/Prefabs/Mappeds/GamePad [xbox].asset>mapped for xbox controller</link>, drag this or your created one in the <b>Mapped</b> field");
        DrawServerImage(2);
    }

    void CreateMappedDoc()
    {
        DrawText("To create a new input Mapped simply go to the folder where you want to create it <i>(in the Project View window)</i> -> Right Mouse Click -> Create -> MFPS -> Input -> Input Mapped, now you will see the new created object, select it and setup all the inputs of your controller keyboard.\n");
        DrawServerImage(3);
        DrawText("Optionally you can just duplicate one of default mappeds and edit the inputs.\n");
    }

    [MenuItem("MFPS/Tutorials/Input Manager")]
    private static void Open()
    {
        EditorWindow.GetWindow(typeof(InputManagerDocumentation));
    }

    [MenuItem("MFPS/Addons/InputManager/Documentation")]
    private static void Open2()
    {
        EditorWindow.GetWindow(typeof(InputManagerDocumentation));
    }
}