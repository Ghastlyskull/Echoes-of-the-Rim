using HarmonyLib;
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
    public static class Patch_Caravan_PathFollower_TryEnterNextPathTile_Event
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(Caravan_PathFollower), "TryEnterNextPathTile");
        }
        public static void Postfix(Caravan_PathFollower __instance, Caravan ___caravan)
        {
            if (__instance.Moving)
            {
                if (Rand.Chance(EchoesOfTheRim_Mod.Settings.chanceForInterruptionPerTile))
                {
                    Helper.TrySetupCaravanEvent(___caravan);
                }
            }
        }
    }
}
