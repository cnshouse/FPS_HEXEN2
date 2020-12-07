using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;

public class CustomizerInitializer : MonoBehaviour
{
    private const string DEFINE_KEY = "CUSTOMIZER";

#if !CUSTOMIZER
    [MenuItem("MFPS/Addons/Customizer/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif

#if CUSTOMIZER
    [MenuItem("MFPS/Addons/Customizer/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif
}