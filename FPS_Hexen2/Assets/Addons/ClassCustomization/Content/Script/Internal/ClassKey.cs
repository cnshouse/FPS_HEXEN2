using UnityEngine;
using System;

namespace MFPS.ClassCustomization
{
    public static class ClassKey
    {
        public const string PrimaryAssault = "PrimaryIDAssault";
        public const string SecundaryAssault = "SecundaryIDAssault";
        public const string KnifeAssault = "KnifeIDAssault";
        public const string GrenadeAssault = "GrenadeIDAssault";

        public const string PrimaryEnginner = "PrimaryIDEnginner";
        public const string SecundaryEnginner = "SecundaryIDEnginner";
        public const string KnifeEnginner = "KnifeIDEnginner";
        public const string GrenadeEnginner = "GrenadeIDEnginner";

        public const string PrimarySupport = "PrimaryIDSupport ";
        public const string SecundarySupport = "SecundaryIDSupport ";
        public const string KnifeSupport = "KnifeIDSupport ";
        public const string GrenadeSupport = "GrenadeIDSupport ";

        public const string PrimaryRecon = "PrimaryIDRecon";
        public const string SecundaryRecon = "SecundaryIDRecon";
        public const string KnifeRecon = "KnifeIDRecon";
        public const string GrenadeRecon = "GrenadeIDRecon";

        public const string PrimaryAngel = "PrimaryIDAngel";
        public const string SecundaryAngel = "SecundaryIDAngel";
        public const string KnifeAngel = "KnifeIDAngel";
        public const string GrenadeAngel = "GrenadeIDAngel";

        public const string ClassType = "ClassID";
        public static readonly string ClassKit = "class.kit.id";
    }

    [Serializable]
    public class ClassAllowedWeaponsType
    {
        [LovattoToogle] public bool AllowMachineGuns = true;
        [LovattoToogle] public bool AllowPistols = true;
        [LovattoToogle] public bool AllowShotguns = true;
        [LovattoToogle] public bool AllowSnipers = true;
        [LovattoToogle] public bool AllowLaunchers = true;
        [LovattoToogle] public bool AllowKnifes = true;
        [LovattoToogle] public bool AllowGrenades = true;

    }
}