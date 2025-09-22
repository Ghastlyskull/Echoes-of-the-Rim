using EOTR;
using HarmonyLib;
using RimWorld;
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
    public static class Patch_AerialVehicleInFlight_ArriveAtTile_Event
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(AerialVehicleInFlight), "ArriveAtTile");
        }
        public static bool Prefix(AerialVehicleInFlight __instance, PlanetTile tile)
        {
            ////Log.Message("RAAAH");
            ////Log.Message(___transporters.Count());
            if (__instance.IsPlayerControlled)
            {
                Pawn p = __instance.Vehicle.AllPawnsAboard.Where(p => p.IsColonist && p.HostFaction == null).RandomElement();
                if (p != null)
                {
                    if (Rand.Chance(EchoesOfTheRim_Mod.Settings.chanceForInterruptionPerTile))
                    {
                        VehicleHelper.TrySetupVehicleEvent(__instance, tile, p);
                    }
                }
            }
            return true;
        }
    }
}
