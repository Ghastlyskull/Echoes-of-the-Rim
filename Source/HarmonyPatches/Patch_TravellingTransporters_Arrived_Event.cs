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
    public static class Patch_TravellingTransporters_Arrived_Event
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(TravellingTransporters), "Arrived");
        }
        public static bool Prefix(TravellingTransporters __instance, List<ActiveTransporterInfo> ___transporters, PlanetTile ___initialTile)
        {
            Log.Message("RAAAH");
            Log.Message(___transporters.Count());
            if (__instance.PodsHaveAnyFreeColonist)
            {
                Pawn p = __instance.Pawns.Where(p => p.IsColonist && p.HostFaction == null).RandomElement();
                if (p != null)
                {
                    if (Rand.Chance(EchoesOfTheRim_Mod.Settings.chanceForInterruptionPerTile))
                    {
                        Helper.TrySetupTransporterEvent(__instance, ___transporters, ___initialTile, p);
                    }
                }
            }
            return true;
        }
    }
}
