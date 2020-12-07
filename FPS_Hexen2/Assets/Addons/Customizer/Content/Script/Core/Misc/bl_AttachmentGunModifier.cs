using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class bl_AttachmentGunModifier : MonoBehaviour
{
    [Header("Sight")]
    public bool OverrideAimPosition = false;
    public bool disableScope = false;
    public Vector3 AimPosition;
    public int extraZoom = 0;
    [Header("Barrel")]
    public bool OverrdieFireSound = false;
    public AudioClip FireSound;
    public int extraDamage = 0;
    public float extraSpread = 0;
    [Header("Magazine")]
    public bool AddBullets = false;
    public int ExtraBullets = 0;

    public bl_Gun m_Gun;

    void OnEnable()
    {
        if (m_Gun == null)
            return;

        if (OverrideAimPosition)
        {
            m_Gun.AimPosition = AimPosition;
            if(m_Gun.GetComponent<bl_SniperScope>() != null)
            {
                m_Gun.GetComponent<bl_SniperScope>().enabled = !disableScope;
            }
        }

        if (OverrdieFireSound)
            m_Gun.FireSound = FireSound;

        if (AddBullets)
            m_Gun.bulletsPerClip += ExtraBullets;

        if(extraZoom != 0) { m_Gun.aimZoom += extraZoom; }
         m_Gun.Damage = extraDamage;//modify only the extra damage, that means if you set 0 of extra damage, only the gun base/default damage is gonna be applied.
        m_Gun.spread += extraSpread;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(bl_AttachmentGunModifier))]
public class bl_AttachmentGunModifierEditor : Editor
{
    bl_AttachmentGunModifier script;

    private void OnEnable()
    {
        script = (bl_AttachmentGunModifier)target;
        if(script.m_Gun == null)
        {
            script.m_Gun = script.transform.GetComponentInParent<bl_Gun>();
            if (script.m_Gun != null) EditorUtility.SetDirty(target);
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.BeginVertical("box");
        script.m_Gun = EditorGUILayout.ObjectField("Gun", script.m_Gun, typeof(bl_Gun), true) as bl_Gun;
        GUILayout.EndVertical();

        if (script.m_Gun != null)
        {
            GUILayout.BeginVertical("box");
            script.OverrideAimPosition = EditorGUILayout.ToggleLeft("Override Aim Position", script.OverrideAimPosition, EditorStyles.toolbarButton);
            if (script.OverrideAimPosition)
            {
                GUILayout.Space(2);
                script.AimPosition = EditorGUILayout.Vector3Field("Aim Position", script.AimPosition);
                if(script.m_Gun != null && script.m_Gun.GetComponent<bl_SniperScope>() != null)
                {
                    script.disableScope = EditorGUILayout.ToggleLeft("Disable Sniper Scope", script.disableScope, EditorStyles.toolbarButton);
                }
            }
            script.extraZoom = EditorGUILayout.IntSlider("Extra Aim Zoom", script.extraZoom, -50, 50);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            script.OverrdieFireSound = EditorGUILayout.ToggleLeft("Override Fire Sound", script.OverrdieFireSound, EditorStyles.toolbarButton);
            if (script.OverrdieFireSound)
            {
                GUILayout.Space(2);
                script.FireSound = EditorGUILayout.ObjectField("Fire Sound", script.FireSound, typeof(AudioClip), false) as AudioClip;
            }
            script.extraDamage = EditorGUILayout.IntField("Extra Damage", script.extraDamage);
            script.extraSpread = EditorGUILayout.FloatField("Extra Spread", script.extraSpread);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            script.AddBullets = EditorGUILayout.ToggleLeft("Add Bullets", script.AddBullets, EditorStyles.toolbarButton);
            if (script.AddBullets)
            {
                GUILayout.Space(2);
                script.ExtraBullets = EditorGUILayout.IntField("Extra Bullets", script.ExtraBullets);
            }
            GUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif