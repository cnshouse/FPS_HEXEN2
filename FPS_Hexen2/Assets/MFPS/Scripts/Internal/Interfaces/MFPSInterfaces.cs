using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Internal.Interfaces
{
    public interface IMFPSPlayerReferences
    {
        Collider[] AllColliders { get; }

        void IgnoreColliders(Collider[] list, bool ignore);
    }

    public interface IMFPSResumeScreen
    {
        void CollectData();
        void Show();
    }
}