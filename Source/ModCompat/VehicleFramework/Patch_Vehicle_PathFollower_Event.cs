using EOTR;
using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vehicles.World;
using Verse;

namespace VehicleFrameworkCompat
{
    [HarmonyPatch]
    public static class Patch_Vehicle_PathFollower_Event
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(VehicleCaravan_PathFollower), "TryEnterNextPathTile");
        }
        public static void Postfix(VehicleCaravan_PathFollower __instance, Caravan ___caravan)
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

