using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPSEditor
{
    public class SpritePreviewAttribute : PropertyAttribute
    {
        public float Height { get; set; } = 0;
        public SpritePreviewAttribute()
        {
        }

        public SpritePreviewAttribute(float height)
        {
            Height = height;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SpritePreviewAttribute))]
    public class SpritePreviewAttributeDrawer : PropertyDrawer
    {
        SpritePreviewAttribute script { get { return ((SpritePreviewAttribute)attribute); } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(position, property);
            }
            else
            {
                Sprite spr = property.objectReferenceValue as Sprite;
                Texture2D icon = null;
                if (spr == null)
                {
                    icon = property.objectReferenceValue as Texture2D;
                }
                else icon = spr.texture;
                float height = script.Height <= 0 ? EditorGUIUtility.singleLineHeight * 2 : script.Height;
                Rect imgp = position;
                imgp.height = height;
                imgp.width = imgp.height;
                imgp.x += 20;
                GUI.DrawTexture(imgp, icon, ScaleMode.ScaleAndCrop);
                position.x += imgp.height + 25;
                position.width -= imgp.height + 25;
                position.height = EditorGUIUtility.singleLineHeight;
                position.y += EditorGUIUtility.singleLineHeight * 0.5f;
                EditorGUI.PropertyField(position, property);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            else
            {
                if (script.Height <= 0)
                    return EditorGUIUtility.singleLineHeight * 2;
                else
                    return script.Height;
            }
        }
    }
#endif
}