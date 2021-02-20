﻿using MFPS.ThirdPerson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

public class bl_CameraViewSettings : ScriptableObject
{
    public MFPSGamePlayerView gamePlayerView = MFPSGamePlayerView.FirstPersonDefault;
    public KeyCode SwitchViewKey = KeyCode.P;
    [LovattoToogle] public bool detectCameraCollision = true;
    public LayerMask collisionMaks;

    [Header("References")]
    public AnimationCurve transitionCurve;
    public AnimationCurve aimTransitionCurve;
#if UNITY_POST_PROCESSING_STACK_V2
    public PostProcessResources postProcessResources;
#endif
    public bl_TPViewData[] customViews;

    private MPlayerViewMode m_currentViewMode = MPlayerViewMode.None;
    public MPlayerViewMode CurrentViewMode
    {
        get
        {
            if(m_currentViewMode == MPlayerViewMode.None)
            {
                if (gamePlayerView == MFPSGamePlayerView.FirstPersonDefault || gamePlayerView == MFPSGamePlayerView.FirstPersonOnly)
                    return MPlayerViewMode.FirstPerson;
                else return MPlayerViewMode.ThirdPerson;
            }
            return m_currentViewMode;
        }
        set
        {
            m_currentViewMode = value;
        }
    }

    public static bool IsThirdPerson() => bl_CameraViewSettings.Instance.CurrentViewMode == MPlayerViewMode.ThirdPerson;

    private static bl_CameraViewSettings _instance;
    public static bl_CameraViewSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<bl_CameraViewSettings>("CameraViewSettings") as bl_CameraViewSettings;
            }
            return _instance;
        }
    }
}