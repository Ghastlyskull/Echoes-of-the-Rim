using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            SitePartDef def = GetRandomGroundSite();
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
        public static SitePartDef GetRandomGroundSite()
        {
            string potential = EchoesOfTheRim_Mod.Settings.structureDataActual.Keys.Where(x => EchoesOfTheRim_Mod.Settings.structureDataActual[x].enabled == true && !EchoesOfTheRim_Mod.Settings.structureDataActual[x].orbital).RandomElementByWeight(x => EchoesOfTheRim_Mod.Settings.structureDataActual[x].weight);
            Log.Message(potential);
            return DefDatabase<SitePartDef>.AllDefsListForReading.Where(x => x.defName == potential).First();
        }
        public static SitePartDef GetRandomOrbitSite()
        {
            string potential = EchoesOfTheRim_Mod.Settings.structureDataActual.Keys.Where(x => EchoesOfTheRim_Mod.Settings.structureDataActual[x].enabled == true && EchoesOfTheRim_Mod.Settings.structureDataActual[x].orbital).RandomElementByWeight(x => EchoesOfTheRim_Mod.Settings.structureDataActual[x].weight);
            Log.Message(potential);
            return DefDatabase<SitePartDef>.AllDefsListForReading.Where(x => x.defName == potential).First();
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
            PlanetTile eventTile;
            bool orbit = false;
            if (initialTile.LayerDef == travellingTransporters.destinationTile.LayerDef)
            {
                eventTile = GetEventTilePathed(initialTile, travellingTransporters.destinationTile);
                orbit = initialTile.LayerDef == PlanetLayerDefOf.Orbit ? true : false;
            }
            else
            {
                eventTile = GetEventTileRandom(initialTile, travellingTransporters.destinationTile, out orbit);
            }
            Log.Message(orbit);
            Log.Message(eventTile);
            Log.Message("Part 2");
            SitePartDef def;
            def = orbit ? GetRandomOrbitSite() : GetRandomGroundSite();
            Log.Message("Part 3");
            Site site = CreateSite(def, eventTile);
            Log.Message("Part 4");
            if (site == null)
            {
                Log.Error("Could not make a site.");
            }
            else
            {
                site.GetComponent<TimeoutComp>().StartTimeout(60000 * EchoesOfTheRim_Mod.Settings.despawnTimer);
                Find.WorldObjects.Add(site);
                CameraJumper.TryJump(site);
                SendLetterTransporter(p, site);
            }
        }
        public static Site CreateSite(SitePartDef def, PlanetTile tile)
        {
            EchoesOfTheRim_Mod.Settings.structureDataActual.TryGetValue(def.defName, out var data);
            if (data == null || !data.enabled)
            {
                Log.Error("Tried to create a site with a disabled or null def.");
                return null;
            }
            Site site;
            switch (data.source)
            {
                case "Odyssey":
                    site = OdysseySites(def, tile);
                    break;
                case "Ancient urban ruins":
                    site = AncientUrbanRuinsSites(def, tile);
                    break;
                default:
                    site = SiteMaker.TryMakeSite([def], tile);
                    break;
            }
            return site;
        }
        private static Site AncientUrbanRuinsSites(SitePartDef def, PlanetTile tile)
        {
            WorldObjectDef objectDef = DefDatabase<WorldObjectDef>.GetNamed("AM_CustomSite");
            switch (def.defName)
            {
                case "AM_Mall_S_Site":
                case "AM_StreetSite":
                case "AM_ReserveSite":
                case "AM_MALL_L_Site":
                    Site site = SiteMaker.MakeSite([new SitePartDefWithParams(def, new SitePartParams())], tile, null, true, objectDef);
                    site.customLabel = "Unknown Site";
                    return site;
                case "ACM_AncientRandomComplex":
                    SimpleCurve ComplexSizeOverPointsCurve = new(){
                                                                         new CurvePoint(0f, 30f),
                                                                         new CurvePoint(10000f, 50f)
                                                                       };

                    SimpleCurve ThreatPointsOverPointsCurve = new SimpleCurve{
                                                                                   new CurvePoint(35f, 38.5f),
                                                                                   new CurvePoint(400f, 165f),
                                                                                   new CurvePoint(10000f, 4125f)
                                                                             };

                    int num = (int)ComplexSizeOverPointsCurve.Evaluate(7000);
                    StructureGenParams psarms = new StructureGenParams
                    {
                        size = new IntVec2(num, num)
                    };
                    LayoutStructureSketch layoutStructureSketch = DefDatabase<LayoutDef>.GetNamed("ACM_AncientRandomComplex_Loot").Worker.GenerateStructureSketch(psarms);
                    layoutStructureSketch.spawned = false;
                    if (layoutStructureSketch != null)
                    {
                        layoutStructureSketch.spawned = false;
                        SitePartParams parms = new SitePartParams
                        {
                            threatPoints = (Find.Storyteller.difficulty.allowViolentQuests ? ThreatPointsOverPointsCurve.Evaluate(StorytellerUtility.DefaultSiteThreatPointsNow()) : 0f),
                            ancientLayoutStructureSketch = layoutStructureSketch,
                            ancientComplexRewardMaker = ThingSetMakerDefOf.MapGen_AncientComplexRoomLoot_Better
                        };
                        site = SiteMaker.MakeSite(Gen.YieldSingle(new SitePartDefWithParams(def, parms)), tile, Faction.OfAncients);
                        TimedDetectionRaids component = site.GetComponent<TimedDetectionRaids>();
                        if (component != null)
                        {
                            component.alertRaidsArrivingIn = true;
                        }
                        return site;
                    }
                    return null;
                default:
                    return null;

            }
        }
        private static Site OdysseySites(SitePartDef def, PlanetTile tile)
        {
            switch (def.defName)
            {
                case "Opportunity_AbandonedPlatform":
                case "Opportunity_MechanoidPlatform":
                case "Opportunity_OrbitalWreck":
                    Site site = SiteMaker.MakeSite([new SitePartDefWithParams(def, new SitePartParams())], tile, null, true, WorldObjectDefOf.ClaimableSpaceSite);
                    site.customLabel = "Unknown Site";
                    return site;
                case "OpportunitySite_AncientInfestedSettlement":
                    tile.Tile.AddMutator(DefDatabase<TileMutatorDef>.GetNamed("AncientInfestedSettlement"));
                    site = SiteMaker.MakeSite([new SitePartDefWithParams(def, new SitePartParams
                    {
                        threatPoints = (Find.Storyteller.difficulty.allowViolentQuests ? StorytellerUtility.DefaultSiteThreatPointsNow() : 0f)
                    })], tile, null, true, WorldObjectDefOf.ClaimableSite);
                    site.customLabel = "Unknown Site";
                    return site;
                case "OpportunitySite_AncientWarehouse":
                    tile.Tile.AddMutator(DefDatabase<TileMutatorDef>.GetNamed("AncientWarehouse"));
                    site = SiteMaker.MakeSite([new SitePartDefWithParams(def, new SitePartParams
                    {
                        threatPoints = (Find.Storyteller.difficulty.allowViolentQuests ? StorytellerUtility.DefaultSiteThreatPointsNow() : 0f)
                    })], tile, null, true, WorldObjectDefOf.ClaimableSite);
                    site.customLabel = "Unknown Site";
                    return site;
                case "OpportunitySite_AncientStructureLaunchSite":
                    tile.Tile.AddMutator(DefDatabase<TileMutatorDef>.GetNamed("AncientLaunchSite"));
                    site = SiteMaker.MakeSite([new SitePartDefWithParams(def, new SitePartParams
                    {
                        threatPoints = (Find.Storyteller.difficulty.allowViolentQuests ? StorytellerUtility.DefaultSiteThreatPointsNow() : 0f)
                    })], tile, null, true, WorldObjectDefOf.ClaimableSite);
                    site.customLabel = "Unknown Site";
                    return site;
                case "OpportunitySite_AncientGarrison":
                    tile.Tile.AddMutator(DefDatabase<TileMutatorDef>.GetNamed("AncientGarrison"));
                    site = SiteMaker.MakeSite([new SitePartDefWithParams(def, new SitePartParams
                    {
                        threatPoints = (Find.Storyteller.difficulty.allowViolentQuests ? StorytellerUtility.DefaultSiteThreatPointsNow() : 0f)
                    })], tile, null, true, WorldObjectDefOf.ClaimableSite);
                    site.customLabel = "Unknown Site";
                    return site;
                case "OpportunitySite_AncientChemfuelRefinery":
                    tile.Tile.AddMutator(DefDatabase<TileMutatorDef>.GetNamed("AncientChemfuelRefinery"));
                    site = SiteMaker.MakeSite([new SitePartDefWithParams(def, new SitePartParams
                    {
                        threatPoints = (Find.Storyteller.difficulty.allowViolentQuests ? StorytellerUtility.DefaultSiteThreatPointsNow() : 0f)
                    })], tile, null, true, WorldObjectDefOf.ClaimableSite);
                    site.customLabel = "Unknown Site";
                    return site;
                default:
                    return null;
            }
        }
        private static PlanetTile GetProperTile(List<PlanetTile> tiles)
        {
            List<PlanetTile> compatTiles = tiles.Distinct().Where(t => t.Valid && !Find.World.Impassable(t) && Find.WorldObjects.SiteAt(t) == null).ToList();
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
        private static PlanetTile GetEventTileRandom(PlanetTile initialTile, PlanetTile destTile, out bool orbit)
        {
            List<PlanetTile> pathTiles;
            if (Rand.Chance(0.5f))
            {
                PlanetTile tempTile = initialTile.Layer.FastTileFinder.Query(new(destTile, 0f, 0f)).First();
                pathTiles = GetTilesBetween2Tiles(initialTile, tempTile);
            }
            else
            {
                PlanetTile tempTile = destTile.Layer.FastTileFinder.Query(new(initialTile, 0f, 0f)).First();
                pathTiles = GetTilesBetween2Tiles(destTile, tempTile);
            }
            if (pathTiles.First().LayerDef == PlanetLayerDefOf.Orbit)
            {
                orbit = true;
            }
            else
            {
                orbit = false;
            }
            return GetProperTile(pathTiles);
        }

        private static void SendLetterTransporter(Pawn p, Site site)
        {
            DiaNode startNode = new DiaNode("EOTR_TransporterSite_LetterText".Translate(p.LabelCap, site.LabelCap));
            DiaOption markForLater = new DiaOption("EOTR_CaravanSite_LetterMarkForLater".Translate());
            markForLater.resolveTree = true;
            startNode.options.Add(markForLater);
            Find.WindowStack.Add(new Dialog_NodeTree(startNode));
        }

        public static PlanetTile GetEventTilePathed(PlanetTile initialTile, PlanetTile destTile)
        {
            List<PlanetTile> pathTiles = GetTilesBetween2Tiles(initialTile, destTile);
            return GetProperTile(pathTiles);
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
            while (tile != destTile || iterations < 30000)
            {
                List<PlanetTile> neighbors = [];
                tile.Layer.GetTileNeighbors(tile, neighbors);
                PlanetTile closestNeighbor = neighbors.First();
                float closestDist = grid.ApproxDistanceInTiles(GenMath.SphericalDistance(grid.GetTileCenter(closestNeighbor).normalized, normalized));
                for (int i = 1; i < neighbors.Count(); i++)
                {
                    PlanetTile planetTile = neighbors[i];
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
