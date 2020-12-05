﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Bob Settings", menuName = "MFPS/Weapons/Bob/Settings")]
public class bl_WeaponBobSettings : ScriptableObject
{
    [Range(0.1f, 2)] public float WalkSpeedMultiplier = 1f;
    [Range(0.1f, 2)] public float RunSpeedMultiplier = 1f;
    [Range(0, 15)] public float EulerZAmount = 5;
    [Range(0, 15)] public float RunEulerZAmount = 5;
    [Range(0, 15)] public float EulerXAmount = 5;
    [Range(0, 15)] public float RunEulerXAmount = 5;

    public float idleBobbingSpeed = 0.1f;
    [Range(0, 0.2f)] public float WalkOscillationAmount = 0.04f;
    [Range(0, 0.2f)] public float RunOscillationAmount = 0.1f;

    public float WalkLerpSpeed = 2;
    public float RunLerpSpeed = 4;
}