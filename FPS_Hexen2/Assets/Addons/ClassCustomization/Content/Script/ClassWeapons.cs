using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MFPSEditor;
#endif

namespace MFPS.ClassCustomization
{
    [Serializable]
    public class ClassWeapons
    {
        public List<WeaponItemData> AllWeapons = new List<WeaponItemData>();

        public void UpdateList(bl_ClassCustomize target)
        {
            List<bl_GunInfo> all = bl_GameData.Instance.AllWeapons;
            for(int i = 0; i < all.Count; i++)
            {
                int index = AllWeapons.FindIndex(x => x.Name == all[i].Name);
                if(index < 0)
                {
                    WeaponItemData wid = new WeaponItemData()
                    {
                        Name = all[i].Name,
                        GunID = i,
                    };
                    AllWeapons.Add(wid);
#if UNITY_EDITOR
                    EditorUtility.SetDirty(target);
#endif
                }
                else
                {
                    if (AllWeapons[index].GunID != i)
                    {
                        AllWeapons[index].GunID = i;
#if UNITY_EDITOR
                        EditorUtility.SetDirty(target);
#endif
                    }
                }
            }
            //clean non existing fields
            for (int i = 0; i < AllWeapons.Count; i++)
            {
                int index = all.FindIndex(x => x.Name == AllWeapons[i].Name);
                if (index < 0)
                {
                    AllWeapons.RemoveAt(i);
#if UNITY_EDITOR
                    EditorUtility.SetDirty(target);
#endif
                }
            }
        }
    }

    [Serializable]
    public class WeaponItemData
    {
        public string Name;
        public int GunID;
        public bool isEnabled = true;

        public bl_GunInfo Info => bl_GameData.Instance.GetWeapon(GunID);
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ClassWeapons))]
    public class ClassWeaponsDrawer : PropertyDrawer
    {
        static readonly Color disableColor = new Color(0, 0, 0, 0.5f);
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            MFPSEditorStyles.DrawBackground(position, new Color(0,0,0,0.2f));
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);

            if (!property.isExpanded)
                return;
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

          
            var list = property.FindPropertyRelative("AllWeapons");
            Rect r = position;
            for (int i = 0; i < list.arraySize; i++)
            {
                var isEnabled = list.GetArrayElementAtIndex(i).FindPropertyRelative("isEnabled");
                GUI.enabled = isEnabled.boolValue;
                r = position;
                GUI.Box(r, GUIContent.none);
                r.x += 4;
                GUI.Label(r, list.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue);
                if (!isEnabled.boolValue)
                {
                    MFPSEditorStyles.DrawBackground(position, disableColor);
                }
                GUI.enabled = true;
                r.x = r.width - 25;
                r.width = 20;
                if (!isEnabled.boolValue)
                {
                    if (GUI.Button(r, GUIContent.none, "OL Plus"))
                    {
                        isEnabled.boolValue = true;
                    }
                }
                else
                {
                    if (GUI.Button(r, GUIContent.none, "OL Minus"))
                    {
                        isEnabled.boolValue = false;
                    }
                }
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            GUI.enabled = true;
            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                int size = property.FindPropertyRelative("AllWeapons").arraySize;
                if (size <= 0) { size = 1; } else { size++; }
                float space = EditorGUIUtility.standardVerticalSpacing * size;
                float h = EditorGUIUtility.singleLineHeight * size;
                return h + space;
            }
            else return EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}