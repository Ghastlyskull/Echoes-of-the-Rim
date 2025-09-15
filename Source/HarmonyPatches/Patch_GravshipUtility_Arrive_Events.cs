using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace EOTR.HarmonyPatches
{
    [HarmonyPatch]
    public static class Patch_GravshipUtility_ArriveExistingMap_Event
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(GravshipUtility), nameof(GravshipUtility.ArriveExistingMap));
        }
        public static bool Prefix(Gravship gravship)
        {
            if (gravship.destinationTile != null)
            {
                if (Rand.Chance(EchoesOfTheRim_Mod.Settings.chanceForInterruptionPerTile))
                {
                    Helper.TrySetupGravshipEvent(gravship);
                }
            }
            return true;
        }
    }
    [HarmonyPatch]
    public static class Patch_GravshipUtility_ArriveNewMap_Event
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(GravshipUtility), nameof(GravshipUtility.ArriveNewMap));
        }
        public static bool Prefix(Gravship gravship)
        {
            if (gravship.destinationTile != null)
            {
                if (Rand.Chance(EchoesOfTheRim_Mod.Settings.chanceForInterruptionPerTile))
                {
                    Helper.TrySetupGravshipEvent(gravship);
                }
            }
            return true;
        }
    }
}
