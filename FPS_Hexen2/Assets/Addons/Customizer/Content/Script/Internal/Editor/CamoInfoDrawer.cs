using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CamoInfo))]
public class CamoInfoDrawe : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position.x += 25;
        position.width -= 30;
        GUI.Box(position, GUIContent.none);
        Rect headerRect = position;
        headerRect.height = EditorGUIUtility.singleLineHeight; ;
        if (property.isExpanded)
        {
            CamoInfo ci = bl_CustomizerData.Instance.Weapons[property.FindPropertyRelative("ofWeaponID").intValue].Camos[property.FindPropertyRelative("ID").intValue];
            if (ci != null && ci.Preview != null)
            {
                Rect pr = position;
                pr.y += EditorGUIUtility.singleLineHeight + 2;
                pr.height -= EditorGUIUtility.singleLineHeight + 2;
                pr.width = pr.height;
                GUI.DrawTexture(pr, ci.Preview, ScaleMode.StretchToFill);
                position.x += pr.width + 2 - 20;
                position.width -= pr.width + 2 - 20;
            }
        }
        position.height = EditorGUIUtility.singleLineHeight;
        if (GUI.Button(headerRect, label.text, EditorStyles.toolbarPopup))
        {
            property.isExpanded = !property.isExpanded;
        }
        if (property.isExpanded)
        {
            Rect defaultRect = position;
            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            SerializedProperty giProp = property.FindPropertyRelative("GlobalID");
            int globalID = giProp.intValue;
            defaultRect.y += EditorGUIUtility.singleLineHeight + 4;
            defaultRect.x += 20;
            defaultRect.width -= 25;
            defaultRect.height = EditorGUIUtility.singleLineHeight;
            globalID = EditorGUI.Popup(defaultRect, "Global ID", globalID, bl_CustomizerData.Instance.GetGlobalCamosStringArray(), EditorStyles.toolbarDropDown);
            giProp.intValue = globalID;

            GlobalCamo gc = null;
            SerializedProperty nameProp = property.FindPropertyRelative("Name");
            if (globalID <= bl_CustomizerData.Instance.GlobalCamos.Count - 1)
            {
                gc = bl_CustomizerData.Instance.GlobalCamos[globalID];
                nameProp.stringValue = gc.Name;
            }

            defaultRect.y += EditorGUIUtility.singleLineHeight + 4;
            EditorGUI.PropertyField(defaultRect, property.FindPropertyRelative("Camo"));
            defaultRect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(defaultRect, property.FindPropertyRelative("OverridePreview"));

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
        }

        EditorGUI.EndProperty();
        if (GUI.changed) { property.serializedObject.ApplyModifiedProperties(); }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight * 4 + 12;
        }
        else
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }
    }
}