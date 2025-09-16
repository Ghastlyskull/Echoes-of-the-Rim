using AncientMarket_Libraray;
using EOTR;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using Site = RimWorld.Planet.Site;

namespace AncientUrbanRuinsCompat
{
    [StaticConstructorOnStartup]
    public static class Support
    {
        static Support()
        {
            Log.Message("Adding ancient urban ruins");
            EchoesOfTheRim_Mod.Settings.InitializeMod("Ancient urban ruins", [], ["AM_MALL_S_Site", "AM_MALL_L_Site", "AM_ReserveSite", "AM_StreetSite", "ACM_AncientRandomComplex"]);
            Helper.Handlers["Ancient urban ruins"] = AncientUrbanRuins;
        }

        public static Site AncientUrbanRuins(string def, PlanetTile tile)
        {
            Log.Message($"{def}: {tile}");
            if (def.Equals("AM_Mall_S_Site", StringComparison.OrdinalIgnoreCase) ||
                def.Equals("AM_StreetSite", StringComparison.OrdinalIgnoreCase) ||
                def.Equals("AM_ReserveSite", StringComparison.OrdinalIgnoreCase) ||
                def.Equals("AM_MALL_L_Site", StringComparison.OrdinalIgnoreCase))
            {
                Log.Message("WTF");
                Site site = GenerateCustomSite(
                    new[] { new SitePartDefWithParams(DefDatabase<SitePartDef>.GetNamed(def), new SitePartParams()
                    {
                        threatPoints = (Find.Storyteller.difficulty.allowViolentQuests ? StorytellerUtility.DefaultSiteThreatPointsNow() : 0f)
                    }) },
                    tile,
                    null,
                    true
                );
                Log.Message(site == null);
                return site;
            }
            if (def.Equals("ACM_AncientRandomComplex", StringComparison.OrdinalIgnoreCase))
            {
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
                    Site site = SiteMaker.MakeSite(Gen.YieldSingle(new SitePartDefWithParams(DefDatabase<SitePartDef>.GetNamed(def), parms)), tile, Faction.OfAncients);
                    TimedDetectionRaids component = site.GetComponent<TimedDetectionRaids>();
                    if (component != null)
                    {
                        component.alertRaidsArrivingIn = true;
                    }
                    return site;
                }
            }
            return null;
        }
        private static Site GenerateCustomSite(IEnumerable<SitePartDefWithParams> sitePartsParams, int tile, Faction faction, bool hiddenSitePartsPossible = false, RulePack singleSitePartRules = null)
        {
            SitePartParams parms = SitePartDefOf.PossibleUnknownThreatMarker.Worker.GenerateDefaultParams(0f, tile, faction);
            SitePartDefWithParams val = new SitePartDefWithParams(SitePartDefOf.PossibleUnknownThreatMarker, parms);
            sitePartsParams = sitePartsParams.Concat(Gen.YieldSingle(val));
            return MakeCustomSite(sitePartsParams, tile, faction);
        }

        private static Site MakeCustomSite(IEnumerable<SitePartDefWithParams> siteParts, int tile, Faction faction, bool ifHostileThenMustRemainHostile = true)
        {
            CustomSite customSite = (CustomSite)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("AM_CustomSite"));
            customSite.Tile = tile;
            customSite.SetFaction(faction);
            if (ifHostileThenMustRemainHostile && faction != null && faction.HostileTo(Faction.OfPlayer))
            {
                customSite.factionMustRemainHostile = true;
            }
            if (siteParts != null)
            {
                foreach (SitePartDefWithParams sitePart2 in siteParts)
                {
                    SitePart sitePart = new SitePart(customSite, sitePart2.def, sitePart2.parms);
                    customSite.AddPart(sitePart);
                    if (customSite.mapDef == null)
                    {
                        ModExtension_Map modExtension = sitePart.def.GetModExtension<ModExtension_Map>();
                        if (modExtension != null && modExtension.maps.Any())
                        {
                            customSite.mapDef = modExtension.maps.RandomElement();
                        }
                    }
                }
            }
            customSite.desiredThreatPoints = customSite.ActualThreatPoints;
            return customSite;
        }
    }
}
