using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(bl_FirstPersonController))]
public class bl_FirstPersonControllerEditor : Editor
{
    bl_FirstPersonController script;
    public Dictionary<string, AnimBool> animatedBools = new Dictionary<string, AnimBool>()
    {
        {"move", null },  {"jump", null }, {"fall", null }, {"mouse", null }, {"bob", null }, {"sound", null }, {"misc", null }
    };

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        script = (bl_FirstPersonController)target;

        animatedBools["move"] = new AnimBool(script._movementExpand, Repaint);
        animatedBools["jump"] = new AnimBool(script._jumpExpand, Repaint);
        animatedBools["fall"] = new AnimBool(script._fallExpand, Repaint);
        animatedBools["mouse"] = new AnimBool(script._mouseExpand, Repaint);
        animatedBools["bob"] = new AnimBool(script._mouseExpand, Repaint);
        animatedBools["sound"] = new AnimBool(script._soundExpand, Repaint);
        animatedBools["misc"] = new AnimBool(script._miscExpand, Repaint);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        MovementSpeeds();
        JumpBox();
        FallBox();
        MouseLookBox();
        HeadBobBox();
        MiscBox();
        SoundBox();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void MovementSpeeds()
    {
        script._movementExpand = animatedBools["move"].target = MFPSEditorStyles.ContainerHeaderFoldout("Speed", script._movementExpand);
        EditorGUILayout.BeginFadeGroup(animatedBools["move"].faded);
        {
            if (animatedBools["move"].value)
            {
                EditorGUILayout.BeginVertical("box");
                script.WalkSpeed = EditorGUILayout.Slider("Walk Speed", script.WalkSpeed, 2, 12);
                script.runSpeed = EditorGUILayout.Slider("Run Speed", script.runSpeed, script.WalkSpeed, 16);
                script.crouchSpeed = EditorGUILayout.Slider("Crouch Speed", script.crouchSpeed, 1, 8);
                script.crouchTransitionSpeed = EditorGUILayout.Slider("Crouch Transition Speed", script.crouchTransitionSpeed, 0.01f, 0.5f);
                script.slideSpeed = EditorGUILayout.Slider("Slide Speed", script.slideSpeed, 10, 20);
                script.slideTime = EditorGUILayout.Slider("Slide Time", script.slideTime, 0.2f, 1.5f);
                script.slideFriction = EditorGUILayout.Slider("Slide Friction", script.slideFriction, 1, 12);
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void JumpBox()
    {
        script._jumpExpand = animatedBools["jump"].target = MFPSEditorStyles.ContainerHeaderFoldout("Jump", script._jumpExpand);
        EditorGUILayout.BeginFadeGroup(animatedBools["jump"].faded);
        {
            if (animatedBools["jump"].value)
            {
                EditorGUILayout.BeginVertical("box");
                script.jumpSpeed = EditorGUILayout.Slider("Jump Force", script.jumpSpeed, 4, 15);
                script.JumpMinRate = EditorGUILayout.Slider("Jump Rate", script.JumpMinRate, 0.2f, 1.5f);
                script.m_GravityMultiplier = EditorGUILayout.Slider("Gravity Multiplier", script.m_GravityMultiplier, 0.1f, 5);
                script.m_StickToGroundForce = EditorGUILayout.Slider("Stick To Ground Force", script.m_StickToGroundForce, 4, 12);
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void FallBox()
    {
        script._fallExpand = animatedBools["fall"].target = MFPSEditorStyles.ContainerHeaderFoldout("Fall", script._fallExpand);
        EditorGUILayout.BeginFadeGroup(animatedBools["fall"].faded);
        {
            if (animatedBools["fall"].value)
            {
                EditorGUILayout.BeginVertical("box");
                Rect r =  GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none,GUILayout.Height(EditorGUIUtility.singleLineHeight));
                script.FallDamage = MFPSEditorStyles.FeatureToogle(r, script.FallDamage, "Fall Damage");
                script.SafeFallDistance = EditorGUILayout.Slider("Safe Distance", script.SafeFallDistance, 0.1f, 7f);
                script.DeathFallDistance = EditorGUILayout.Slider("Deathly Distance", script.DeathFallDistance, script.SafeFallDistance, 25);
                script.AirControlMultiplier = EditorGUILayout.Slider("Air Control Multiplier", script.AirControlMultiplier, 0, 2);
                GUILayout.Space(10);
                GUILayout.Label("Dropping",EditorStyles.boldLabel);
                script.dropControlSpeed = EditorGUILayout.Slider("Drop Control Speed", script.dropControlSpeed, 15, 40);
                EditorGUILayout.MinMaxSlider("Drop Angle Speed Range", ref script.dropTiltSpeedRange.x, ref script.dropTiltSpeedRange.y, 10, 75);
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void MouseLookBox()
    {
        script._mouseExpand = animatedBools["mouse"].target = MFPSEditorStyles.ContainerHeaderFoldout("Mouse Look", script._mouseExpand);
        EditorGUILayout.BeginFadeGroup(animatedBools["mouse"].faded);
        {
            if (animatedBools["mouse"].value)
            {
                EditorGUILayout.BeginVertical("box");
                if (script.mouseLook == null) script.mouseLook = new MFPS.PlayerController.MouseLook();
                Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                script.mouseLook.clampVerticalRotation = MFPSEditorStyles.FeatureToogle(r, script.mouseLook.clampVerticalRotation, "Clamp Vertical Rotation");
                if (script.mouseLook.clampVerticalRotation)
                {
                    EditorGUILayout.LabelField($"Vertical Rotation Clamp ({script.mouseLook.MinimumX.ToString("0.0")},{script.mouseLook.MaximumX.ToString("0.0")})");
                    EditorGUILayout.MinMaxSlider(ref script.mouseLook.MinimumX, ref script.mouseLook.MaximumX, -180, 180);
                }
                r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                script.mouseLook.useSmoothing = MFPSEditorStyles.FeatureToogle(r, script.mouseLook.useSmoothing, "Use Smoothing");
                if (script.mouseLook.useSmoothing)
                {
                    script.mouseLook.framesOfSmoothing = EditorGUILayout.Slider("Smoothing Frames", script.mouseLook.framesOfSmoothing, 2, 12);
                }
                r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                script.mouseLook.lerpMovement = MFPSEditorStyles.FeatureToogle(r, script.mouseLook.lerpMovement, "Lerp Movement");
                if (script.mouseLook.lerpMovement)
                {
                    script.mouseLook.smoothTime = EditorGUILayout.Slider("Smoothness", script.mouseLook.smoothTime, 2, 12);
                }
                GUILayout.Label("You can modify the default sensitivity settings in GameData -> Default Settings.", EditorStyles.helpBox);
                var prop = serializedObject.FindProperty("headRoot");
                EditorGUI.indentLevel++;
                prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, "References");
                if (prop.isExpanded)
                {
                    EditorGUILayout.PropertyField(prop);
                    script.CameraRoot = EditorGUILayout.ObjectField("Camera Root", script.CameraRoot, typeof(Transform), true) as Transform;
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void HeadBobBox()
    {
        script._bobExpand = animatedBools["bob"].target = MFPSEditorStyles.ContainerHeaderFoldout("Head Bob", script._bobExpand);
        EditorGUILayout.BeginFadeGroup(animatedBools["bob"].faded);
        {
            if (animatedBools["bob"].value)
            {
                EditorGUILayout.BeginVertical("box");
                script.headBobMagnitude = EditorGUILayout.Slider("Head Bob Magnitude", script.headBobMagnitude, 0, 1.2f);
                if (script.m_JumpBob == null) script.m_JumpBob = new MFPS.PlayerController.LerpControlledBob();
                script.m_JumpBob.BobAmount = EditorGUILayout.Slider("Jump Bob Magnitude", script.m_JumpBob.BobAmount, 0.1f, 1);
                script.m_JumpBob.BobDuration = EditorGUILayout.Slider("Jump Bob Duration", script.m_JumpBob.BobDuration, 0.1f, 1);

                GUILayout.Label("You can modify the Head Bob properties in bl_WeaponBob.", EditorStyles.helpBox);
                if(GUILayout.Button("Ping bl_WeaponBob.cs", EditorStyles.toolbarButton))
                {
                    var wb = script.transform.GetComponentInChildren<bl_WeaponBob>(true);
                    if(wb != null)
                    {
                        Selection.activeObject = wb.gameObject;
                        EditorGUIUtility.PingObject(wb.gameObject);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void SoundBox()
    {
        script._soundExpand = animatedBools["sound"].target = MFPSEditorStyles.ContainerHeaderFoldout("Sounds", script._soundExpand);
        EditorGUILayout.BeginFadeGroup(animatedBools["sound"].faded);
        {
            if (animatedBools["sound"].value)
            {
                EditorGUILayout.BeginVertical("box");
                script.footstep = EditorGUILayout.ObjectField("FootStep Controller", script.footstep, typeof(bl_Footstep), true) as bl_Footstep;
                script.jumpSound = EditorGUILayout.ObjectField("Jump Sound", script.jumpSound, typeof(AudioClip), true) as AudioClip;
                script.landSound = EditorGUILayout.ObjectField("Land Sound", script.landSound, typeof(AudioClip), true) as AudioClip;
                script.slideSound = EditorGUILayout.ObjectField("Slide Sound", script.slideSound, typeof(AudioClip), true) as AudioClip;
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void MiscBox()
    {
        script._miscExpand = animatedBools["misc"].target = MFPSEditorStyles.ContainerHeaderFoldout("Misc", script._miscExpand);
        EditorGUILayout.BeginFadeGroup(animatedBools["misc"].faded);
        {
            if (animatedBools["misc"].value)
            {
                EditorGUILayout.BeginVertical("box");
                Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                script.KeepToCrouch = MFPSEditorStyles.FeatureToogle(r, script.KeepToCrouch, "Toggle Crouch");
                r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                script.RunFovEffect = MFPSEditorStyles.FeatureToogle(r, script.RunFovEffect, "Sprint FoV Effect");
                if (script.RunFovEffect)
                {
                    script.runFOVAmount = EditorGUILayout.Slider("Run FOV Amount", script.runFOVAmount, 0, 12);
                }
                script.StandIcon = EditorGUILayout.ObjectField("Stand Icon", script.StandIcon, typeof(Sprite), false) as Sprite;
                script.CrouchIcon = EditorGUILayout.ObjectField("Crouch Icon", script.CrouchIcon, typeof(Sprite), false) as Sprite;
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndFadeGroup();
    }
}