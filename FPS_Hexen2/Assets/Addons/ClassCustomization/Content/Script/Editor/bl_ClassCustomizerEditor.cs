using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace MFPS.ClassCustomization
{

    [CustomEditor(typeof(bl_ClassCustomize))]
    public class bl_ClassCustomizerEditor : Editor
    {
        bl_ClassCustomize script;

        private void OnEnable()
        {
            script = (bl_ClassCustomize)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

           // EditorGUILayout.BeginVertical("box");
            DrawDefaultInspector();
           // EditorGUILayout.EndVertical();
            if (GUILayout.Button("Update"))
            {
                script.RefreshLists();
            }
            if (GUILayout.Button("Delete local saved classes"))
            {
                bl_ClassManager.Instance.DeleteKeys();
            }

            if (EditorGUI.EndChangeCheck())
            {
                script.RefreshLists();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
}