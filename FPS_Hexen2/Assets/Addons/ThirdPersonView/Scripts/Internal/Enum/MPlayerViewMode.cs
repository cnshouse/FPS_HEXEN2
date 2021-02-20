namespace MFPS.ThirdPerson
{
    using System;
    public enum MPlayerViewMode
    {
        FirstPerson,
        ThirdPerson,
        None,
    }

    [Serializable]
    public enum MFPSGamePlayerView
    {
        FirstPersonOnly,
        ThirdPersonOnly,
        FirstPersonDefault,
        ThirdPersonDefault,
    }

    public enum TPViewOverrideState
    {
        OverrideDefault,
        OverrideAll,
        OverrideSingle,
        None,
    }
}