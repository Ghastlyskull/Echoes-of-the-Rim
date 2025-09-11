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
        public static void TrySetupCaravanEvent(PlanetTile curTile)
        {
            // To be implemented
            Log.Message("HIII");
        }
        public static void SendLetter()
        {
            DiaNode startNode = new DiaNode("EOTR_CaravanSite_LetterText".Translate());
            DiaOption enterNow = new DiaOption("EOTR_CaravanSite_LetterExplore".Translate());
            enterNow.action = () =>
            {

            };
            DiaOption markForLater = new DiaOption("EOTR_CaravanSite_LetterMarkForLater".Translate());
            markForLater.action = () =>
            {

            };

            startNode.options.Add(enterNow);
            startNode.options.Add(markForLater);
        }
    }
}
