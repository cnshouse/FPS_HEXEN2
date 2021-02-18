using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using MFPSEditor;
using UnityEngine.UI;

[CustomEditor(typeof(bl_GunRace))]
public class bl_GunRaceEditor : Editor
{

    bl_GunRace script;
    ReorderableList list;
    string[] Weapons;

    private void OnEnable()
    {
        script = (bl_GunRace)target;
        list = new ReorderableList(serializedObject, serializedObject.FindProperty("GunOrderList"), true, false, true, true);
        Weapons = bl_GameData.Instance.AllWeaponStringList();

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            string title = (index == 0) ? "Initial" : string.Format("{0} Kills", index);
            if(index == list.serializedProperty.arraySize - 1)
            {
                title = "Last Kill";
            }
            element.intValue = EditorGUI.Popup(rect, title, element.intValue, Weapons, EditorStyles.toolbarDropDown);
        };

        list.drawHeaderCallback = (Rect r) => { EditorGUI.LabelField(r, "Gun List", EditorStyles.boldLabel); };
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("In this list add all weapons available for Gun Race in the order that players will get them.", MessageType.Info);
        GUILayout.Space(10);
        serializedObject.Update();
        list.DoLayoutList();
        this.serializedObject.ApplyModifiedProperties();

        EditorGUILayout.BeginVertical("box");
        script.Content = EditorGUILayout.ObjectField("Content", script.Content, typeof(GameObject), true) as GameObject;
        script.PointAudio = EditorGUILayout.ObjectField("Point Audio", script.PointAudio, typeof(AudioClip), true) as AudioClip;
        script.ParticleEffects = EditorGUILayout.ObjectField("Particle Effects", script.ParticleEffects, typeof(GameObject), true) as GameObject;
        script.ScoreText = EditorGUILayout.ObjectField("ScoreText", script.ScoreText, typeof(Text), true) as Text;
        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}