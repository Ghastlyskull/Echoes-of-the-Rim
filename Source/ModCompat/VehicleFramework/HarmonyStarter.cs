using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VehicleFrameworkCompat
{
    [StaticConstructorOnStartup]
    public static class HarmonyStarter
    {
        static HarmonyStarter()
        {
            var harmony = new HarmonyLib.Harmony("echoesoftherim.vehicles");
            harmony.PatchAll();
        }
    }
}
