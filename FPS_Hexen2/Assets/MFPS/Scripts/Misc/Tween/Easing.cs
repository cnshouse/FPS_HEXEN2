using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MFPS.Tween
{
    public static class Easing
    {
        public static float Do(float t, EasingType type = EasingType.Quintic, EasingMode mode = EasingMode.InOut)
        {
            switch (type)
            {
                //-----------------------------------------------------------
                case EasingType.Exponential:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Exponential.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Exponential.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Exponential.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Exponential.OutIn(t);
                        default: return EasingFunctions.Exponential.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Quintic:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Quintic.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Quintic.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Quintic.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Quintic.OutIn(t);
                        default: return EasingFunctions.Quintic.InOut(t);
                    }
                //-----------------------------------------------------------
                default:
                case EasingType.Linear:
                    switch (mode)
                    {
                        case EasingMode.In:
                        case EasingMode.Out:
                        case EasingMode.InOut:
                        case EasingMode.OutIn:
                        default: return EasingFunctions.Linear.Identity(t);
                    }
            }
        }
    }

    internal static class EasingFunctions
    {
        public static class Exponential
        {
            public static float In(float t)
            {
                return Mathf.Pow(2, 10 * (t - 1));
            }

            public static float Out(float t)
            {
                return -Mathf.Pow(2, -10 * t) + 1;
            }

            public static float InOut(float t)
            {
                var x = 2 * t - 1;

                return (t < 0.5f)
                    ? 0.5f * Mathf.Pow(2, 10 * x)
                    : 0.5f * -Mathf.Pow(2, -10 * x) + 1;
            }

            public static float OutIn(float t)
            {
                return (t < 0.5f)
                    ? 0.5f * (-Mathf.Pow(2, -20 * t) + 1)
                    : 0.5f * (Mathf.Pow(2, 20 * (t - 1)) + 1);
            }
        }
        public static class Linear
        {
            public static float Identity(float t)
            {
                return t;
            }
        }
        public static class Quintic
        {
            public static float In(float t)
            {
                return t * t * t * t * t;
            }

            public static float Out(float t)
            {
                float x = t - 1;

                return x * x * x * x * x + 1;
            }

            public static float InOut(float t)
            {
                return (t < 0.5f)
                    ? 16 * t * t * t * t * t
                    : 16 * (t - 1) * (t - 1) * (t - 1) * (t - 1) * (t - 1) + 1;
            }

            public static float OutIn(float t)
            {
                float x = 2 * t - 1;

                return 0.5f * (x * x * x * x * x + 1);
            }
        }
    }

    [Serializable]
    public enum EasingType
    {
        Exponential,
        Linear,
        Quintic,
    }

    [Serializable]
    public enum EasingMode
    {
        In,
        Out,
        InOut,
        OutIn,
    }
}