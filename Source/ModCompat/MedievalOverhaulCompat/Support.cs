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
            Log.Message("Adding MedievalOverhaulCompat sites");
            EchoesOfTheRim_Mod.Settings.InitializeMod("Medieval Overhaul", [], ["DankPyon_Small_Ruin", "DankPyon_Medium_Ruin", "DankPyon_Big_Snake_Ruin", "Cultists_1Tier", "Cultists_2Tier", "Cultists_3Tier"]);
            Helper.Handlers["Medieval Overhaul"] = MedievalOverhaul;
        }

        public static Site MedievalOverhaul(string def, PlanetTile tile)
        {
            Faction f = null;
            Log.Message($"{def}: {tile}");
            if (def.Equals("DankPyon_Small_Ruin", StringComparison.OrdinalIgnoreCase) ||
                def.Equals("DankPyon_Medium_Ruin", StringComparison.OrdinalIgnoreCase))
            {
                f = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("DankPyon_BrigandFaction"));
            }
            if(def.Equals("Cultists_1Tier", StringComparison.OrdinalIgnoreCase) ||
               def.Equals("Cultists_2Tier", StringComparison.OrdinalIgnoreCase) ||
               def.Equals("Cultists_3Tier", StringComparison.OrdinalIgnoreCase))
            {
                f = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("DankPyon_ShadowSect"));
            }
            if (def.Equals("DankPyon_Big_Snake_Ruin", StringComparison.OrdinalIgnoreCase))
            {
                f = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("DankPyon_SnakeCave_Faction"));
            }
            Site site = SiteMaker.MakeSite([new SitePartDefWithParams(DefDatabase < SitePartDef >.GetNamed(def), new SitePartParams
                    {
                        threatPoints = (Find.Storyteller.difficulty.allowViolentQuests ? StorytellerUtility.DefaultSiteThreatPointsNow() : 0f)
                    })], tile, f, true, WorldObjectDefOf.Site);
            return site;

        }
    }
}
