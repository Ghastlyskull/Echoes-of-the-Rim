using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

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
                SendLetter(caravan, site);
            }
 
            Log.Message("HIII");
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
        public static void SendLetter(Caravan caravan, Site site)
        {
            DiaNode startNode = new DiaNode("EOTR_CaravanSite_LetterText".Translate(site.def.LabelCap));
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
    }
}
