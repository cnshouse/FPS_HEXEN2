using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.InputManager
{
    [Serializable]
    public class Mapped
    {
        [Reorderable]
        public List<ButtonData> ButtonMap = new List<ButtonData>();
    }
}