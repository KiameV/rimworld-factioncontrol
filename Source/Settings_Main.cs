using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FactionControl
{

   /* public class Settings : ModSettings
    {
        public float factionDensity = 1.25f;
        public float factionGrouping = 0.5f;
        public bool allowMechanoids = true;
        public bool randomGoodwill = true;
        public bool dynamicColors = true;
        public bool spreadPirates = true;
        public bool relationsChangeOverTime = true;

        private string strFacDen = "";

        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = canvas.width
            };
            list.Begin(canvas);

            list.Gap();
            Settings_FactionOptions.DrawSlider(list, "RFC.factionDensity".Translate() + " " + GetFactionDensityLabel(factionDensity), ref factionDensity, ref strFacDen, 0.01f, 8f);

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
            list.Gap(24);
            list.CheckboxLabeled("RFC.RelationChangesOverTime".Translate(), ref relationsChangeOverTime);
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
            Scribe_Values.Look(ref relationsChangeOverTime, "relationsChangeOverTime", true);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                float v = 0;
                Scribe_Values.Look(ref v, "factionCount");
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.strFacDen = ((int)this.factionDensity).ToString();
            }

            SetIncidents.SetIncidentLevels();
        }

        private static string GetFactionDensityLabel(float factionDensity)
        {
            if (factionDensity < .45)
            {
                return "RFC.factionDensityVeryLow".Translate();
            }
            else if (factionDensity < .75)
            {
                return "RFC.factionDensityLow".Translate();
            }
            else if (factionDensity < 1.5)
            {
                return "RFC.factionDensityNormal".Translate();
            }
            else if (factionDensity < 4)
            {
                return "RFC.factionDensityHigh".Translate();
            }
            else if (factionDensity < 6)
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
    }*/
}