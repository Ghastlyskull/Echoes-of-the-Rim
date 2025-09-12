using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Verse;
using static System.Net.WebRequestMethods;

namespace EOTR
{
    public static class Helper
    {
        public static void TrySetupCaravanEvent(Caravan caravan)
        {
            if (Find.WorldObjects.SiteAt(caravan.Tile) != null)
            {
                return;
            }
            SitePartDef def = GetRandomSite(caravan.Tile.Layer);
            Site site = SiteMaker.TryMakeSite([def], caravan.Tile);
            site.GetComponent<TimeoutComp>().StartTimeout(60000 * EchoesOfTheRim_Mod.Settings.despawnTimer);
            if (site == null)
            {
                Log.Error("Could not find any valid faction for this site.");
            }
            else
            {
                Find.WorldObjects.Add(site);
                SendLetterCaravan(caravan, site);
            }
        }
        public static SitePartDef GetRandomSite(PlanetLayer layer)
        {
            string potential = EchoesOfTheRim_Mod.Settings.structureDataActual.Keys.Where(x => EchoesOfTheRim_Mod.Settings.structureDataActual[x].enabled == true).RandomElementByWeight(x => EchoesOfTheRim_Mod.Settings.structureDataActual[x].weight);
            Log.Message(potential);
            return DefDatabase<SitePartDef>.AllDefsListForReading.Where(x => x.defName == potential).First();
            /*            List<string> possible = EchoesOfTheRim_Mod.Settings.structureDataActual.Keys.Where(x => EchoesOfTheRim_Mod.Settings.structureDataActual[x].enabled == true).ToList();
                        List<SitePartDef> defs = DefDatabase<SitePartDef>.AllDefsListForReading.Where(x => possible.Contains(x.defName) && x.).ToList();
                        .RandomElementByWeight(x => EchoesOfTheRim_Mod.Settings.structureDataActual[x].weight)
                        return def;*/
        }
        public static void SendLetterCaravan(Caravan caravan, Site site)
        {
            DiaNode startNode = new DiaNode("EOTR_CaravanSite_LetterText".Translate(site.LabelCap));
            DiaOption enterNow = new DiaOption("EOTR_CaravanSite_LetterEnterNow".Translate());
            enterNow.resolveTree = true;
            enterNow.action = () =>
            {
                Pawn p = caravan.pawns[0];
                MapGenerator.GenerateMap(site.PreferredMapSize, site, MapGeneratorDefOf.Encounter, site.ExtraGenStepDefs);
                CaravanEnterMapUtility.Enter(caravan, site.Map, CaravanEnterMode.Edge);
                CameraJumper.TryJump(p);
            };
            DiaOption markForLater = new DiaOption("EOTR_CaravanSite_LetterMarkForLater".Translate());
            markForLater.resolveTree = true;
            startNode.options.Add(enterNow);
            startNode.options.Add(markForLater);
            Find.WindowStack.Add(new Dialog_NodeTree(startNode));
        }

        public static void TrySetupTransporterEvent(TravellingTransporters travellingTransporters, List<ActiveTransporterInfo> transporters, PlanetTile initialTile, Pawn p)
        {

            Log.Message("Part 1");
            PlanetTile eventTile = GetEventTile(initialTile, travellingTransporters.destinationTile);
            Log.Message("Part 2");

            SitePartDef def = GetRandomSite(eventTile.Layer);
            Site site = SiteMaker.TryMakeSite([def], eventTile);
            site.GetComponent<TimeoutComp>().StartTimeout(60000 * EchoesOfTheRim_Mod.Settings.despawnTimer);
            if (site == null)
            {
                Log.Error("Could not find any valid faction for this site.");
            }
            else
            {
                Find.WorldObjects.Add(site);
                CameraJumper.TryJump(site);
                SendLetterTransporter(p, site);
            }
        }
        private static void SendLetterTransporter(Pawn p, Site site)
        {
            DiaNode startNode = new DiaNode("EOTR_TransporterSite_LetterText".Translate(p.LabelCap, site.LabelCap));
            DiaOption markForLater = new DiaOption("EOTR_CaravanSite_LetterMarkForLater".Translate());
            markForLater.resolveTree = true;
            startNode.options.Add(markForLater);
            Find.WindowStack.Add(new Dialog_NodeTree(startNode));
        }

        public static PlanetTile GetEventTile(PlanetTile initialTile, PlanetTile destTile)
        {
            List<PlanetTile> pathTiles = GetTilesBetween2Tiles(initialTile, destTile);
            List<PlanetTile> compatTiles = pathTiles.Distinct().Where(t => t.Valid && !Find.World.Impassable(t) && Find.WorldObjects.SiteAt(t) == null).ToList();
            Log.Message(compatTiles.Count());
            List<PlanetTile> possibleChoices = [.. compatTiles];
            foreach (var t in compatTiles)
            {
                List<PlanetTile> neighbors = [];
                Find.WorldGrid.GetTileNeighbors(t, neighbors);
                possibleChoices.AddRange(neighbors.Where(t => t.Valid && !Find.World.Impassable(t) && Find.WorldObjects.SiteAt(t) == null && !possibleChoices.Contains(t)));
            }
            return possibleChoices.RandomElement();
        }
        public static List<PlanetTile> GetTilesBetween2Tiles(PlanetTile startTile, PlanetTile destTile)
        {
            if (!startTile.Valid)
            {
                Log.Error($"Tried to FindPath with invalid start tile {startTile}");
                return null;
            }
            if (!destTile.Valid)
            {
                Log.Error($"Tried to FindPath with invalid dest tile {destTile}");
                return null;
            }
            if (startTile.Layer != destTile.Layer)
            {
                Log.Error($"Tried to FindPath to a different layer {startTile} -> {destTile}");
                return null;
            }
            PlanetTile tile = startTile;
            World world = Find.World;
            WorldGrid grid = world.grid;
            Vector3 normalized = grid.GetTileCenter(destTile).normalized;
            float[] array = world.pathGrid.layerMovementDifficulty[startTile.Layer];
            List<PlanetTile> path = [tile];
            int iterations = 0;
            Log.Message("Starting loop");
            while (tile != destTile || iterations < 3000)
            {
                Log.Message(iterations);
                List<PlanetTile> neighbors = [];
                tile.Layer.GetTileNeighbors(tile, neighbors);
                PlanetTile closestNeighbor = neighbors.First();
                float closestDist = grid.ApproxDistanceInTiles(GenMath.SphericalDistance(grid.GetTileCenter(closestNeighbor).normalized, normalized));
                Log.Message(closestNeighbor);
                for (int i = 1; i < neighbors.Count(); i++)
                {
                    Log.Message("Neighbor:");
                    Log.Message(i);
                    PlanetTile planetTile = neighbors[i];
                    Log.Message(planetTile);
                    Vector3 tileCenter = grid.GetTileCenter(planetTile);
                    float potential = grid.ApproxDistanceInTiles(GenMath.SphericalDistance(tileCenter.normalized, normalized));
                    if (potential < closestDist)
                    {
                        closestNeighbor = planetTile;
                        closestDist = potential;
                    }
                }
                path.Add(closestNeighbor);
                tile = closestNeighbor;
                iterations++;
            }
            return path;
        }
    }
}
