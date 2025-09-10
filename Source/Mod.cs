using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace EOTR
{
    [StaticConstructorOnStartup]
    public class EchoesOfTheRim_Mod : Mod
    {
        private static EchoesOfTheRim_Settings settings;
        public static EchoesOfTheRim_Mod instance;
        public static EchoesOfTheRim_Settings Settings => settings ??= instance.GetSettings<EchoesOfTheRim_Settings>();
        public EchoesOfTheRim_Mod(ModContentPack content) : base(content)
        {
            instance = this;
        }
        public override string SettingsCategory()
        {
            return "Echoes of the Rim";
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Log.Message(Settings.structureDataSaved.Count);
            Settings.DoWindowContents(inRect);
        }
    }
}
