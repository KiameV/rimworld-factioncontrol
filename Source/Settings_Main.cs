using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FactionControl
{
    public class Controller : Mod
    {
        public static Dictionary<Faction, int> factionCenters = new Dictionary<Faction, int>();
        public static Dictionary<Faction, int> failureCount = new Dictionary<Faction, int>();
        public static Settings Settings;
        public static double minFactionSeparation = 0;
        public static double maxFactionSprawl = 0;

        public override string SettingsCategory() { return "RFC.FactionControl".Translate(); }

        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }

        public Controller(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
        }
    }

    public class Settings : ModSettings
    {
        public float factionGrouping = 0.5f;
        public float factionDensity = 2.5f;
        public float factionCount = 5.5f;
        public bool allowMechanoids = true;
        public bool randomGoodwill = true;
        public bool dynamicColors = true;

        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard list = new Listing_Standard();
            list.ColumnWidth = canvas.width;
            list.Begin(canvas);
            list.Gap();
            list.Label("RFC.factionCount".Translate() + "  " + (int)factionCount);
            factionCount = list.Slider(factionCount, 0, 30.99f);
            Text.Font = GameFont.Tiny;
            list.Label("RFC.factionNotes".Translate());
            Text.Font = GameFont.Small;
            list.Gap(24);
            if (factionDensity < 1)
            {
                list.Label("RFC.factionDensity".Translate() + "  " + "RFC.factionDensityVeryLow".Translate());
            }
            else if (factionDensity < 2)
            {
                list.Label("RFC.factionDensity".Translate() + "  " + "RFC.factionDensityLow".Translate());
            }
            else if (factionDensity < 3)
            {
                list.Label("RFC.factionDensity".Translate() + "  " + "RFC.factionDensityNormal".Translate());
            }
            else if (factionDensity < 4)
            {
                list.Label("RFC.factionDensity".Translate() + "  " + "RFC.factionDensityHigh".Translate());
            }
            else if (factionDensity < 5)
            {
                list.Label("RFC.factionDensity".Translate() + "  " + "RFC.factionDensityVeryHigh".Translate());
            }
            else
            {
                list.Label("RFC.factionDensity".Translate() + "  " + "RFC.factionDensityInsane".Translate());
            }
            factionDensity = list.Slider(factionDensity, 0, 5.99f);
            list.Gap(24);
            if (factionGrouping < 1)
            {
                list.Label("RFC.factionGrouping".Translate() + "  " + "RFC.factionGroupingNone".Translate());
            }
            else if (factionGrouping < 2)
            {
                list.Label("RFC.factionGrouping".Translate() + "  " + "RFC.factionGroupingLoose".Translate());
            }
            else if (factionGrouping < 3)
            {
                list.Label("RFC.factionGrouping".Translate() + "  " + "RFC.factionGroupingTight".Translate());
            }
            else
            {
                list.Label("RFC.factionGrouping".Translate() + "  " + "RFC.factionGroupingVeryTight".Translate());
            }
            factionGrouping = list.Slider(factionGrouping, 0, 3.99f);
            list.Gap(24);
            list.CheckboxLabeled("RFC.AllowMechanoids".Translate(), ref allowMechanoids, "RFC.AllowMechanoidsTip".Translate());
            list.Gap(24);
            list.CheckboxLabeled("RFC.EnableFactionRandomGoodwill".Translate(), ref randomGoodwill, "RFC.EnableFactionRandomGoodwillToolTip".Translate());
            if (randomGoodwill)
            {
                list.Gap(24);
                list.CheckboxLabeled("RFC.EnableFactionDynamicColors".Translate(), ref dynamicColors, "RFC.EnableFactionDynamicColorsTip".Translate());
            }
            list.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref factionGrouping, "factionGrouping", 0.5f);
            Scribe_Values.Look(ref factionDensity, "factionDensity", 2.5f);
            Scribe_Values.Look(ref factionCount, "factionCount", 5.5f);
            Scribe_Values.Look(ref allowMechanoids, "allowMechanoids", true);
            Scribe_Values.Look(ref randomGoodwill, "randomGoodwill", true);
            Scribe_Values.Look(ref dynamicColors, "dynamicColors", true);
            SetIncidents.SetIncidentLevels();
        }
    }
}