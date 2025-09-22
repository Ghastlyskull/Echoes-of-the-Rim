using EOTR;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles.World;
using Verse;

namespace VehicleFrameworkCompat
{
    public static class VehicleHelper
    {
        public static void TrySetupVehicleEvent(AerialVehicleInFlight vehicle, PlanetTile destinationTile, Pawn p)
        {
            PlanetTile eventTile;
            bool orbit = false;
            if (vehicle.Tile.LayerDef == destinationTile.LayerDef)
            {
                eventTile = Helper.GetEventTilePathed(vehicle.Tile, destinationTile);
                orbit = vehicle.Tile.LayerDef == PlanetLayerDefOf.Orbit ? true : false;
            }
            else
            {
                eventTile = Helper.GetEventTileRandom(vehicle.Tile, destinationTile, out orbit);
            }
            //Log.Message(orbit);
            //Log.Message(eventTile);
            //Log.Message("Part 2");
            string def;
            def = orbit ? Helper.GetRandomOrbitSite() : Helper.GetRandomGroundSite();
            if (def == "")
            {
                Log.Error("No appropriate site found");
                return;
            }
            //Log.Message("Part 3");
            Site site = Helper.CreateSite(def, eventTile);
            //Log.Message("Part 4");
            if (site == null)
            {
                Log.Error("Could not make a site.");
            }
            else
            {
                site.GetComponent<TimeoutComp>().StartTimeout(60000 * EchoesOfTheRim_Mod.Settings.despawnTimer);
                Find.WorldObjects.Add(site);
                CameraJumper.TryJump(site);
                Helper.SendLetter(p);
            }
        }
    }
}
