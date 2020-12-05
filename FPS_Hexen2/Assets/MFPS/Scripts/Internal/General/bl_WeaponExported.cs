﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPSEditor
{
    public class bl_WeaponExported : MonoBehaviour
    {
        public bl_GunInfo WeaponInfo;
        public bl_Gun FPWeapon;
        public bl_NetworkGun TPWeapon;

        public Vector3 FPWPosition;
        public Quaternion FPWRotation;

        public Vector3 TPWPosition;
        public Quaternion TPWRotation;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(bl_WeaponExported))]
    public class bl_WeaponExportedEditor : Editor
    {
        private Texture goodIcon;
        private Texture badIcon;

        private void OnEnable()
        {
            goodIcon = EditorGUIUtility.IconContent("Collab").image;
            badIcon = EditorGUIUtility.IconContent("CollabNew").image;
        }

        public override void OnInspectorGUI()
        {
            bl_WeaponExported script = (bl_WeaponExported)target;

            if(script.WeaponInfo != null)
            {
                GUILayout.BeginVertical(GUILayout.Height(200));
                GUILayout.FlexibleSpace();
                GUILayout.Label(script.WeaponInfo.Name.ToUpper(), EditorStyles.toolbarButton);
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                if (script.WeaponInfo.GunIcon != null)
                {
                    float aspect = 256f / (float)script.WeaponInfo.GunIcon.texture.width;
                    GUILayout.Label(script.WeaponInfo.GunIcon.texture, GUILayout.Width(256),GUILayout.Height(script.WeaponInfo.GunIcon.texture.height * aspect));
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();
                GUILayout.Label("FPWeapon: ");
                Texture icon = script.FPWeapon == null ? badIcon : goodIcon;
                GUILayout.Label(icon);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("TPWeapon: ");
                icon = script.TPWeapon == null ? badIcon : goodIcon;
                GUILayout.Label(icon);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Gun Info: ");
                icon = script.WeaponInfo == null ? badIcon : goodIcon;
                GUILayout.Label(icon);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
        }
    }
#endif
}