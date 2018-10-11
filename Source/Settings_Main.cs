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
        public static double pirateSprawl = 0;

        public override string SettingsCategory() { return "RFC.FactionControl".Translate(); }

        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }

        public Controller(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
        }
    }

    public class Settings : ModSettings
    {
        // TODO remove this in 1.0 release
        public float factionCount = -1f;

        public float factionDensity = 2.5f;
        public float factionGrouping = 0.5f;
        public bool allowMechanoids = true;
        public bool randomGoodwill = true;
        public bool dynamicColors = true;
        public bool spreadPirates = true;

        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = canvas.width
            };
            list.Begin(canvas);

            list.Gap();
            list.Label("RFC.factionDensity".Translate() + " " + GetFactionDensityLabel(factionDensity));// + "(" + factionDensity.ToString("n2") + ")");
            factionDensity = list.Slider(factionDensity, 0.01f, 24f);

            list.Gap(24);
            list.Label("RFC.factionGrouping".Translate() + "  " + GetFactionGroupingLabel(factionGrouping));// + "(" + factionGrouping.ToString("n2") + ")");
            factionGrouping = list.Slider(factionGrouping, 0.5f, 5f);

            list.Gap(24);
            list.CheckboxLabeled("RFC.AllowMechanoids".Translate(), ref allowMechanoids, "RFC.AllowMechanoidsTip".Translate());
            list.Gap(24);
            list.CheckboxLabeled("RFC.EnableFactionRandomGoodwill".Translate(), ref randomGoodwill, "RFC.EnableFactionRandomGoodwillToolTip".Translate());
            list.Gap(24);
            list.CheckboxLabeled("RFC.EnableFactionDynamicColors".Translate(), ref dynamicColors, "RFC.EnableFactionDynamicColorsTip".Translate());
            list.Gap(24);
            list.CheckboxLabeled("RFC.SpreadPirates".Translate(), ref spreadPirates);
            list.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref factionGrouping, "factionGrouping", 0.5f);
            Scribe_Values.Look(ref factionDensity, "factionDensity", 2.5f);
            Scribe_Values.Look(ref allowMechanoids, "allowMechanoids", true);
            Scribe_Values.Look(ref randomGoodwill, "randomGoodwill", true);
            Scribe_Values.Look(ref dynamicColors, "dynamicColors", true);
            Scribe_Values.Look(ref spreadPirates, "spreadPirates", true);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Values.Look(ref factionCount, "factionCount", -1);
                if (factionCount != -1 && 
                    Controller_FactionOptions.Settings != null && 
                    Controller_FactionOptions.Settings.factionCount == -1)
                {
                    Controller_FactionOptions.Settings.factionCount = factionCount;
                }
            }

            SetIncidents.SetIncidentLevels();
        }

        private static string GetFactionDensityLabel(float factionDensity)
        {
            if (factionDensity < 2.5)
            {
                return "RFC.factionDensityVeryLow".Translate();
            }
            else if (factionDensity < 5)
            {
                return "RFC.factionDensityLow".Translate();
            }
            else if (factionDensity < 7.5)
            {
                return "RFC.factionDensityNormal".Translate();
            }
            else if (factionDensity < 11.5)
            {
                return "RFC.factionDensityHigh".Translate();
            }
            else if (factionDensity < 14)
            {
                return "RFC.factionDensityVeryHigh".Translate();
            }
            return "RFC.factionDensityInsane".Translate();
        }

        private static string GetFactionGroupingLabel(float factionGrouping)
        {
            if (factionGrouping < 1f)
            {
                return "RFC.factionGroupingNone".Translate();
            }
            else if (factionGrouping < 2f)
            {
                return "RFC.factionGroupingLoose".Translate();
            }
            else if (factionGrouping < 3.5f)
            {
                return "RFC.factionGroupingTight".Translate();
            }
            return "RFC.factionGroupingVeryTight".Translate();
        }
    }
}