using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MFPS.Addon.Customizer
{
    [CustomEditor(typeof(bl_Customizer))]
    public class bl_CustomizerEditor : Editor
    {

        bl_Customizer script;
        SerializedProperty attac;
        SerializedProperty camor;
        SerializedProperty positions;
        private string weaponName = "";
        private bool editOpen = false;
        bl_CustomizerManager customizerManager;

        private void OnEnable()
        {
            attac = serializedObject.FindProperty("Attachments");
            positions = serializedObject.FindProperty("Positions");
            camor = serializedObject.FindProperty("CamoRender");
            customizerManager = FindObjectOfType<bl_CustomizerManager>();
        }

        public override void OnInspectorGUI()
        {
            script = (bl_Customizer)target;
            weaponName = script.WeaponName;

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal("box");
            script.WeaponID = EditorGUILayout.Popup("Customizer ID", script.WeaponID, bl_CustomizerData.Instance.GetWeaponStringArray(), EditorStyles.toolbarDropDown);
            GUILayout.Space(5);
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                script.RefreshAttachments();
            }
            GUILayout.EndHorizontal();
            if (customizerManager != null && !customizerManager.AllCustom.Exists(x => x.WeaponID == script.WeaponID))
            {
                if (GUILayout.Button("Listed Customizer Weapon"))
                {
                    customizerManager.AllCustom.Add(script);
                    EditorUtility.SetDirty(customizerManager);
                }
            }
            if (GUI.changed)
            {
                script.WeaponName = bl_CustomizerData.Instance.Weapons[script.WeaponID].WeaponName;
                if (script.WeaponName != weaponName)
                {
                    script.BuildAttachments();
                    weaponName = script.WeaponName;
                }
            }

            serializedObject.Update();
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(camor, true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(attac, true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(positions, true);
            GUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
            GUILayout.EndVertical();
            script.SeparateCurve = EditorGUILayout.CurveField("Separate Curve", script.SeparateCurve);
            script.ChangeMovementPath = EditorGUILayout.CurveField("Change Movement Path", script.ChangeMovementPath);
            EditorGUI.EndChangeCheck();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void OnSceneGUI()
        {
            script = (bl_Customizer)target;

            if (editOpen)
            {
                script.Positions.BarrelPosition.position = Handles.PositionHandle(script.Positions.BarrelPosition.position, Quaternion.identity);
                script.Positions.OpticPosition.position = Handles.PositionHandle(script.Positions.OpticPosition.position, Quaternion.identity);
                script.Positions.FeederPosition.position = Handles.PositionHandle(script.Positions.FeederPosition.position, Quaternion.identity);
                script.Positions.CylinderPosition.position = Handles.PositionHandle(script.Positions.CylinderPosition.position, Quaternion.identity);

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);
                }
            }
            if (Handles.Button(script.transform.position, Quaternion.identity, 0.05f, 0.1f, Handles.RectangleHandleCap))
            {
                editOpen = !editOpen;
            }
        }
    }
}