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
    public class StructureData: IExposable
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
    [StaticConstructorOnStartup]
    public class EchoesOfTheRim_Settings : ModSettings
    {
        public int despawnTimer = 30;
        private string despawnTimerBuffer;
        public float chanceForInterruptionPerTile = 0.02f;
        public Dictionary<string, StructureData> structureDataSaved = [];
        public Dictionary<string, StructureData> structureDataActual = [];
        private Vector2 _scrollPosition;
        private static bool initialized = false;
        public void Initialize()
        {
            int count = 0;
            foreach(var def in DefDatabase<SitePartDef>.AllDefsListForReading)
            {
                count++;
                Log.Message(count);
                if (!structureDataSaved.ContainsKey(def.defName))
                {
                    StructureData newData = new StructureData(def.LabelCap, true, 1);
                    structureDataSaved.Add(def.defName, newData);
                    structureDataActual.Add(def.defName, newData);
                }
                else
                {
                    structureDataActual.Add(def.defName, structureDataSaved[def.defName]);
                }
            }
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
            if(!initialized)
            {
                Initialize();
                initialized = true;
            }
            Rect rect2 = new Rect(inRect);
            rect2.height = structureDataActual.Count * 75f;
            Rect rect3 = rect2;
            Widgets.AdjustRectsForScrollView(inRect, ref rect2, ref rect3);
            Widgets.BeginScrollView(inRect, ref _scrollPosition, rect3, showScrollbars: true);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect3);
            listing_Standard.TextFieldNumericLabeled("Despawn Time: ",ref despawnTimer, ref despawnTimerBuffer, 1, 1000);
            listing_Standard.Label($"Chance for Event per tile: {chanceForInterruptionPerTile * 100}%");
            chanceForInterruptionPerTile = (float)Math.Round(listing_Standard.Slider(chanceForInterruptionPerTile, 0, 1), 2);
            listing_Standard.Gap();
            foreach (var data in structureDataActual)
            {
                listing_Standard.CheckboxLabeled($"{data.Value.labelCap}({data.Key})", ref data.Value.enabled);
                if (data.Value.enabled)
                {
                    listing_Standard.Label("Weight: " + data.Value.weight.ToString());
                    data.Value.weight = (int)listing_Standard.Slider(data.Value.weight, 1, 100);
                }
                structureDataSaved[data.Key] = data.Value;
            }
            listing_Standard.End();
            Widgets.EndScrollView();
        }
    }
}
