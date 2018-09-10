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
        public float outlanderMin = 1.5f;
        public float tribalMin = 1.5f;
        public float pirateMin = 1.5f;
        public bool allowMechanoids = true;
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
            list.Label("RFC.outlanderMin".Translate() + "  " + (int)outlanderMin);
            outlanderMin = list.Slider(outlanderMin, 0, 10.99f);
            list.Gap();
            list.Label("RFC.tribalMin".Translate() + "  " + (int)tribalMin);
            tribalMin = list.Slider(tribalMin, 0, 10.99f);
            list.Gap();
            list.Label("RFC.pirateMin".Translate() + "  " + (int)pirateMin);
            pirateMin = list.Slider(pirateMin, 0, 5.99f);
            list.Gap(24);
            list.CheckboxLabeled("RFC.AllowMechanoids".Translate(), ref allowMechanoids, "RFC.AllowMechanoidsTip".Translate());
            list.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref factionGrouping, "factionGrouping", 0.5f);
            Scribe_Values.Look(ref factionDensity, "factionDensity", 2.5f);
            Scribe_Values.Look(ref factionCount, "factionCount", 5.5f);
            Scribe_Values.Look(ref outlanderMin, "outlanderMin", 1.5f);
            Scribe_Values.Look(ref tribalMin, "tribalMin", 1.5f);
            Scribe_Values.Look(ref pirateMin, "pirateMin", 1.5f);
            Scribe_Values.Look(ref allowMechanoids, "allowMechanoids", true);
            SetIncidents.SetIncidentLevels();
        }
    }

    /*public class Controller_ModdedFactions1 : Mod
    {
        public static Settings_ModdedFactions Settings;
        public override string SettingsCategory()
        {
            Main.Setup();
            if (Main.factionsModdedNames.Count > 0)
            {
                return "RFC.FactionControl1".Translate();
            }
            else { return null; }
        }
        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas, 0, 5); }
        public Controller_ModdedFactions1(ModContentPack content) : base(content) { Settings = GetSettings<Settings_ModdedFactions>(); }
    }

    public class Controller_ModdedFactions2 : Mod
    {
        public static Settings_ModdedFactions Settings;
        public override string SettingsCategory()
        {
            Main.Setup();
            if (Main.factionsModdedNames.Count > 6)
            {
                return "RFC.FactionControl2".Translate();
            }
            else { return null; }
        }
        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas, 6, 11); }
        public Controller_ModdedFactions2(ModContentPack content) : base(content) { Settings = GetSettings<Settings_ModdedFactions>(); }
    }

    public class Controller_ModdedFactions3 : Mod
    {
        public static Settings_ModdedFactions Settings;
        public override string SettingsCategory()
        {
            Main.Setup();
            if (Main.factionsModdedNames.Count > 12)
            {
                return "RFC.FactionControl3".Translate();
            }
            else { return null; }
        }
        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas, 12, 17); }
        public Controller_ModdedFactions3(ModContentPack content) : base(content) { Settings = GetSettings<Settings_ModdedFactions>(); }
    }

    public class Controller_ModdedFactions4 : Mod
    {
        public static Settings_ModdedFactions Settings;
        public override string SettingsCategory()
        {
            Main.Setup();
            if (Main.factionsModdedNames.Count > 18)
            {
                return "RFC.FactionControl4".Translate();
            }
            else { return null; }
        }
        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas, 18, 23); }
        public Controller_ModdedFactions4(ModContentPack content) : base(content) { Settings = GetSettings<Settings_ModdedFactions>(); }
    }

    public class Controller_ModdedFactions5 : Mod
    {
        public static Settings_ModdedFactions Settings;
        public override string SettingsCategory()
        {
            Main.Setup();
            if (Main.factionsModdedNames.Count > 24)
            {
                return "RFC.FactionControl5".Translate();
            }
            else { return null; }
        }
        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas, 24, 29); }
        public Controller_ModdedFactions5(ModContentPack content) : base(content) { Settings = GetSettings<Settings_ModdedFactions>(); }
    }

    public class Controller_ModdedFactions6 : Mod
    {
        public static Settings_ModdedFactions Settings;
        public override string SettingsCategory()
        {
            Main.Setup();
            if (Main.factionsModdedNames.Count > 30)
            {
                return "RFC.FactionControl6".Translate();
            }
            else { return null; }
        }
        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas, 30, 35); }
        public Controller_ModdedFactions6(ModContentPack content) : base(content) { Settings = GetSettings<Settings_ModdedFactions>(); }
    }

    public class Controller_ModdedFactions7 : Mod
    {
        public static Settings_ModdedFactions Settings;
        public override string SettingsCategory()
        {
            Main.Setup();
            if (Main.factionsModdedNames.Count > 36)
            {
                return "RFC.FactionControl7".Translate();
            }
            else { return null; }
        }
        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas, 36, 41); }
        public Controller_ModdedFactions7(ModContentPack content) : base(content) { Settings = GetSettings<Settings_ModdedFactions>(); }
    }

    public class Settings_ModdedFactions : ModSettings
    {
        public void DoWindowContents(Rect canvas, int begin, int end)
        {
            Listing_Standard list = new Listing_Standard();
            list.ColumnWidth = canvas.width;
            list.Begin(canvas);
            for (int i = 0; i < Main.factionsModdedNames.Count; i++)
            {
                if (i < begin) { continue; }
                if (i > end) { break; }
                string factionLabel = Main.factionsModdedLabels[i].CapitalizeFirst();
                if (Main.factionsModdedFreq[i] > 55)
                {
                    list.Gap(24);
                    bool tempCheck = Main.factionsModdedUseHidden[i];
                    list.CheckboxLabeled("RFC.factionHidden3".Translate() + factionLabel + "RFC.factionHidden2".Translate(), ref tempCheck);
                    Main.factionsModdedUseHidden[i] = tempCheck;
                    tempCheck = Main.factionsModdedTreatAsPirate[i];
                    list.CheckboxLabeled("RFC.factionPirate1".Translate() + factionLabel + "RFC.factionPirate2".Translate(), ref tempCheck, "RFC.factionPirateTip".Translate());
                    Main.factionsModdedTreatAsPirate[i] = tempCheck;
                }
                else if (Main.factionsModdedFreq[i] > 45)
                {
                    list.Gap(24);
                    bool tempCheck = Main.factionsModdedUseHidden[i];
                    list.CheckboxLabeled("RFC.factionHidden1".Translate() + factionLabel + "RFC.factionHidden2".Translate(), ref tempCheck);
                    Main.factionsModdedUseHidden[i] = tempCheck;
                }
                else
                {
                    list.Gap(24);
                    list.Label("RFC.factionBasic1".Translate() + factionLabel + "RFC.factionBasic2".Translate() + "  " + (int)Main.factionsModdedFreq[i]);
                    Main.factionsModdedFreq[i] = list.Slider(Main.factionsModdedFreq[i], 0, 10.99f);
                    bool tempCheck = Main.factionsModdedTreatAsPirate[i];
                    list.CheckboxLabeled("RFC.factionPirate1".Translate() + factionLabel + "RFC.factionPirate2".Translate(), ref tempCheck, "RFC.factionPirateTip".Translate());
                    Main.factionsModdedTreatAsPirate[i] = tempCheck;
                }
            }
            list.End();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref Main.factionsModdedNames, "factionsModdedNames", LookMode.Value, new object[0]);
            Scribe_Collections.Look(ref Main.factionsModdedLabels, "factionsModdedLabels", LookMode.Value, new object[0]);
            Scribe_Collections.Look(ref Main.factionsModdedFreq, "factionsModdedFreq", LookMode.Value, new object[0]);
            Scribe_Collections.Look(ref Main.factionsModdedUseHidden, "factionsModdedUseHidden", LookMode.Value, new object[0]);
            Scribe_Collections.Look(ref Main.factionsModdedTreatAsPirate, "factionsModdedTreatAsPirate", LookMode.Value, new object[0]);
        }
    }*/
}