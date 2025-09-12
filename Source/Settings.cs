using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace EOTR
{
    public class StructureData : IExposable
    {
        public string labelCap;
        public bool enabled;
        public int weight;
        public StructureData() { }
        public StructureData(string labelCap, bool enabled, int weight)
        {
            this.labelCap = labelCap;
            this.enabled = enabled;
            this.weight = weight;
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref labelCap, "labelCap", "Unknown Structure");
            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref weight, "weight", 1);
        }
    }

    public class EchoesOfTheRim_Settings : ModSettings
    {
        [TweakValue("AAAADefaultHeight", 1f, 400f)]
        public static float defaultHeight = 250f;
        [TweakValue("AAAALabelHeight", 1f, 100f)]
        public static float labelHeight = 20f;
        [TweakValue("AAAAOptionHeight", 1f, 100f)]
        public static float optionHeight = 75f;
        public int despawnTimer = 30;
        private string despawnTimerBuffer;
        public float chanceForInterruptionPerTile = 0.02f;
        public Dictionary<string, StructureData> structureDataSaved = [];
        public Dictionary<string, StructureData> structureDataActual = [];
        private Vector2 _scrollPosition;
        public Dictionary<string, bool> modsEnabled = [];
        //Big and ugly but it just works
        private readonly string[] mods ={
            "Alpha Genes", "Alpha Books", "Vanilla Genetics Expanded", "Vanilla Quests Expanded - Generator",
            "Vanilla Quests Expanded - Deadlife", "Mechanitor Orbital Platform", "Ancient urban ruins",
            "Ancient mining industry", "Ancient hydroponic farm facilities", "Protocol Anomaly: Syndicate"
        };
        private readonly Dictionary<string, List<string>> siteDictionary = new Dictionary<string, List<string>>
{
    { "Alpha Genes", new List<string> { "AG_AbandonedBiotechLab" } },
    { "Alpha Books", new List<string> { "ABooks_RuinedLibrary" } },
    { "Vanilla Genetics Expanded", new List<string> { "GR_AbandonedLab" } },
    { "Vanilla Quests Expanded - Generator", new List<string> { "VQE_Quest1Site" } },
    { "Vanilla Quests Expanded - Deadlife", new List<string> { "VQE_AncientSilo" } },
    { "Mechanitor Orbital Platform", new List<string> { "Opportunity_AbandonedMechanitorPlatform" } },

    { "Ancient urban ruins", new List<string>
        { "AM_MALL_S_Site", "AM_MALL_L_Site", "AM_ReserveSite", "AM_StreetSite", "ACM_AncientRandomComplex" } },

    { "Ancient mining industry", new List<string>
        { "AbandonedMine_Site", "MineralScreeningStation_Site", "AbandonedPlasteelMineSite_Site",
          "AbandonedUraniumMiningSite_Site", "AbandonedSteelMineSite_Site",
          "AncientOpenPitMiningSite_Site", "AncientTunnelRuins_Site" } },

    { "Ancient hydroponic farm facilities", new List<string>
        { "damaged_ancient_cotton_farm_SitePart", "damaged_ancient_Devilstrand_farm_SitePart",
          "discarded_nutrient_cream_factory_SitePart", "discarded_nutrient_solution_factory_SitePart",
          "damaged_ancient_grain_farm_B_SitePart", "damaged_ancient_grain_farm_A_SitePart" } },

    { "Protocol Anomaly: Syndicate", new List<string> { "AncientSite" } },

    { "Odyssey", new List<string>
        { "Opportunity_OrbitalWreck", "Opportunity_MechanoidPlatform", "Opportunity_AbandonedPlatform",
          "AncientStockpile", "AbandonedSettlement", "OpportunitySite_AncientInfestedSettlement",
          "OpportunitySite_AncientWarehouse", "OpportunitySite_AncientChemfuelRefinery",
          "OpportunitySite_AncientGarrison", "OpportunitySite_AncientLaunchsite" } },

    { "Biotech", new List<string> { "AncientComplex_Mechanitor" } },
    { "Ideology", new List<string> { "AncientComplex" } },
            {"Core", new List<string>{"BanditCamp"} }
};


        public void Initialize()
        {
            structureDataActual = [];
            #region Core
            modsEnabled["Core"] = true;
            #endregion
            #region Ideology
            if (ModLister.IdeologyInstalled)
            {
                modsEnabled["Ideology"] = true;
            }
            #endregion
            #region Biotech
            if (ModLister.BiotechInstalled)
            {
                modsEnabled["Biotech"] = true;
            }
            #endregion
            #region The Rest
            foreach (string modName in mods)
            {
                if (ModLister.HasActiveModWithName(modName))
                {
                    modsEnabled[modName] = true;
                }
            }
            #endregion
            #region Loading settings
            foreach (string modName in modsEnabled.Keys)
            {
                if (modsEnabled[modName])
                {
                    Log.Message("Checking for " + modName);
                    foreach (string defName in siteDictionary[modName])
                    {
                        Log.Message(" - " + defName);
                        if (structureDataSaved.ContainsKey(defName))
                        {
                            structureDataActual[defName] = structureDataSaved[defName];
                        }
                        else
                        {
                            StructureData newData = new StructureData(defName, true, 10);
                            newData.labelCap = DefDatabase<SitePartDef>.AllDefsListForReading.Where(x => x.defName == defName).First().LabelCap;
                            structureDataActual[defName] = newData;
                        }
                    }
                }
            }
            #endregion
            structureDataSaved = structureDataActual;
        }
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref structureDataSaved, "structureDataSaved", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref despawnTimer, "despawnTimer", 30);
            Scribe_Values.Look(ref chanceForInterruptionPerTile, "chanceForInterruptionPerTile", 0.02f);
            base.ExposeData();
        }
        public void DoWindowContents(Rect inRect)
        {
            Rect rect2 = new Rect(inRect);
            rect2.height = defaultHeight;
            foreach (string modName in modsEnabled.Keys)
            {
                if (modsEnabled[modName])
                {
                    rect2.height += labelHeight;
                    rect2.height += siteDictionary[modName].Count * optionHeight;
                }
            }
            Rect rect3 = rect2;
            Widgets.AdjustRectsForScrollView(inRect, ref rect2, ref rect3);
            Widgets.BeginScrollView(rect2, ref _scrollPosition, rect3, showScrollbars: true);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect3);
            using (new TextBlock(GameFont.Medium))
            {
                listing_Standard.Label("EOTR_General".Translate());
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            listing_Standard.Label("EOTR_DespawnTime".Translate(despawnTimer.ToString()));
            listing_Standard.TextFieldNumeric(ref despawnTimer, ref despawnTimerBuffer, 1, 1000);
            Text.Anchor = TextAnchor.UpperLeft;
            listing_Standard.Label("EOTR_ChanceForEvent".Translate(chanceForInterruptionPerTile * 100));
            chanceForInterruptionPerTile = (float)Math.Round(listing_Standard.Slider(chanceForInterruptionPerTile, 0, 1), 2);
            listing_Standard.GapLine();
            using (new TextBlock(GameFont.Medium))
            {
                listing_Standard.Label("EOTR_Structures".Translate());
            }
            foreach (string modName in modsEnabled.Keys)
            {
                if (modsEnabled[modName])
                {
                    listing_Standard.Label(modName);
                    foreach (string siteName in siteDictionary[modName])
                    {
                        listing_Standard.CheckboxLabeled($"{structureDataActual[siteName].labelCap}({siteName})", ref structureDataActual[siteName].enabled);
                        if (structureDataActual[siteName].enabled)
                        {
                            listing_Standard.Label("EOTR_Weight".Translate(structureDataActual[siteName].weight.ToString()));
                            structureDataActual[siteName].weight = (int)listing_Standard.Slider(structureDataActual[siteName].weight, 1, 100);
                        }
                        structureDataSaved[siteName] = structureDataActual[siteName];
                    }
                }
            }
            listing_Standard.End();
            Widgets.EndScrollView();
        }
    }
}
