using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor.Addons;
using MFPS.InputManager;

namespace MFPSEditor
{
    public class InputManagerIntegration : AddonIntegrationWizard
    {
        //REQUIRED
        private const string ADDON_NAME = "Input Manager";
        private const string ADDON_KEY = "INPUT_MANAGER";

        string[] integrateGuids = new string[] { "0adfc49c0731d4445bf47acb9e6e9515", "8ad8ea65df03f8c458bba6580d849223" };
        private List<IntegrationScene> sceneList;

        /// <summary>
        /// 
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();
            addonName = ADDON_NAME;
            addonKey = ADDON_KEY;
            allSteps = 2;

            MFPSAddonsInfo addonInfo = MFPSAddonsData.Instance.Addons.Find(x => x.KeyName == addonKey);
            Dictionary<string, string> info = new Dictionary<string, string>();
            if (addonInfo != null) { info = addonInfo.GetInfoInDictionary(); }
            Initializate(info);

#if LOCALIZATION
            currentStep = 2;//skip the activation step.
#endif
        }
        //REQUIRED END

        public override void DrawWindow(int stepID)
        {
            if (stepID == 1)
            {
                DrawText("First, you have to <b>Enable</b> this addon, for it simply click on the button below:");
#if LOCALIZATION
                DrawText("The addons is already enabled, continue with the next step.\n");
#else
      DrawAddonActivationButton();
#endif
            }
            else if (stepID == 2)
            {
                DrawText("Now you have to <b>integrate</b> the addon content in the required game scenes,\nClick on the button below to see the integration details of each scene:");
                GUILayout.Space(10);

                if (sceneList != null)
                    DrawMFPSMaps(sceneList.ToArray(), true);

                GUILayout.Space(10);
                using (new MFPSEditorStyles.CenteredScope())
                {
                    if (DrawButton("Refresh Scenes Status", GUILayout.Width(200), GUILayout.Height(20)))
                    {
                        BuildScenes();
                    }
                }
                GUILayout.Space(40);

                using (new MFPSEditorStyles.CenteredScope())
                {
                    if (MFPSEditorStyles.ButtonOutline("INTEGRATE", BLUE_COLOR, GUILayout.Width(200), GUILayout.Height(30)))
                    {
                        if (GetMFPSSceneType() == MFPSSceneType.Lobby)
                        {
                            IntegrateInLobby();
                        }
                        else if (GetMFPSSceneType() == MFPSSceneType.Map)
                        {
                            IntegrateInMap();
                        }
                        BuildScenes();
                    }
                }
                GUILayout.FlexibleSpace();
                DrawText("Once you finish with the integration you are all set!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void BuildScenes()
        {
            sceneList = new List<IntegrationScene>();

            var scene = new IntegrationScene(MAIN_MENU_SCENE_PATH, "Main Menu");
            sceneList.Add(scene);
            CheckIntegrationInScene(scene, integrateGuids);

            var alls = bl_GameData.Instance.AllScenes;
            foreach (var mfpscene in alls)
            {
                var nscene = new IntegrationScene(mfpscene.m_Scene, mfpscene.ShowName);
                sceneList.Add(nscene);
                CheckIntegrationInScene(nscene, integrateGuids);
            }
            Repaint();
        }

        /// <summary>
        /// 
        /// </summary>
        public void IntegrateInLobby()
        {
            OpenMainMenuScene();
            if (bl_LobbyUI.Instance == null) { Debug.Log("Couldn't open the Main Menu scene"); return; }

            var script = bl_LobbyUI.Instance.GetComponentInChildren<bl_InputUI>(true);
            if (script != null) { Debug.Log($"{ADDON_NAME} has been already integrated in the lobby!"); return; }

            var parentRef = bl_LobbyUI.Instance.AddonsButtons[1];
            if (parentRef == null) { return; }


            var instance = InstancePrefab("Assets/Addons/InputManager/Content/Prefabs/UI/Input UI [1.8].prefab");
            if (instance == null) return;

            instance.transform.SetParent(parentRef.transform, false);
            instance.transform.SetAsLastSibling();
            EditorUtility.SetDirty(instance);
            ShowSuccessIntegrationLog(instance);
            MarkSceneDirty();
            Repaint();
        }

        /// <summary>
        /// 
        /// </summary>
        private void IntegrateInMap()
        {
            if (bl_UIReferences.Instance == null) { Debug.Log("Can't integrate in this scene."); return; }

            var script = bl_UIReferences.Instance.GetComponentInChildren<bl_InputUI>(true);
            if (script != null) { Debug.Log($"{ADDON_NAME} has been already integrated in this scene!"); return; }

            var rs = bl_UIReferences.Instance.GetComponentInChildren<MFPS.Runtime.Settings.bl_RuntimeSettings>(true);
            if (rs == null) { Debug.Log($"Can't integrate {ADDON_NAME} automatically in this scene!"); return; }

            var instance = InstancePrefab("Assets/Addons/InputManager/Content/Prefabs/UI/Input UI [1.8].prefab");
            if (instance == null) return;

            instance.transform.SetParent(rs.windows[3].Window.transform, false);
            instance.transform.SetAsLastSibling();
            EditorUtility.SetDirty(instance);
            ShowSuccessIntegrationLog(instance);
            MarkSceneDirty();
            Repaint();
        }

        public MFPSSceneType GetMFPSSceneType()
        {
            if (bl_Lobby.Instance != null) return MFPSSceneType.Lobby;
            if (bl_GameManager.Instance != null) return MFPSSceneType.Map;
            return MFPSSceneType.Other;
        }

        public enum MFPSSceneType
        {
            Lobby,
            Map,
            Other,
        }

        [MenuItem("MFPS/Addons/InputManager/Integrate")]
        static void Open()
        {
            GetWindow<InputManagerIntegration>();
        }
    }
}