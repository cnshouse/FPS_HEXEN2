using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MFPSEditor;

public class ClanSystemInitializer
{
    private const string DEFINE_KEY = "CLANS";

#if !CLANS
    [MenuItem("MFPS/Addons/ClanSystem/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif

#if CLANS
    [MenuItem("MFPS/Addons/ClanSystem/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif
}