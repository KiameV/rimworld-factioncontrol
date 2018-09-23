using UnityEngine;
using Verse;

namespace FactionControl
{
    public class Controller_FactionOptions : Mod
    {
        public static Settings_FactionOptions Settings;

        public override string SettingsCategory() { return "RFC.FactionControlFactionOptions".Translate(); }

        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }

        public Controller_FactionOptions(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings_FactionOptions>();
        }
    }

    public class Settings_FactionOptions : ModSettings
    {
        public float factionCount = 5.5f;
        public float outlanderCivilMin = 1.5f;
        public float outlanderHostileMin = 1.5f;
        public float tribalCivilMin = 1.5f;
        public float tribalHostileMin = 1.5f;
        public float pirateMin = 1.5f;

        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = canvas.width
            };
            list.Begin(canvas);
            Text.Font = GameFont.Tiny;
            list.Label("RFC.factionNotes".Translate());
            Text.Font = GameFont.Small;
            list.Gap();
            list.Label("RFC.factionCount".Translate() + "  " + (int)factionCount);
            factionCount = list.Slider(factionCount, 0, 30.99f);
            list.Gap(24);
            list.Label("RFC.outlanderCivilMin".Translate() + "  " + (int)outlanderCivilMin);
            outlanderCivilMin = list.Slider(outlanderCivilMin, 0, 10.99f);
            list.Gap();
            list.Label("RFC.outlanderRoughMin".Translate() + "  " + (int)outlanderHostileMin);
            outlanderHostileMin = list.Slider(outlanderHostileMin, 0, 10.99f);
            list.Gap();
            list.Gap();
            list.Label("RFC.tribalCivilMin".Translate() + "  " + (int)tribalCivilMin);
            tribalCivilMin = list.Slider(tribalCivilMin, 0, 10.99f);
            list.Gap();
            list.Label("RFC.tribalRoughMin".Translate() + "  " + (int)tribalHostileMin);
            tribalHostileMin = list.Slider(tribalHostileMin, 0, 10.99f);
            list.Gap();
            list.Gap();
            list.Label("RFC.pirateMin".Translate() + "  " + (int)pirateMin);
            pirateMin = list.Slider(pirateMin, 0, 5.99f);
            list.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref factionCount, "factionCount", -1);
            Scribe_Values.Look(ref outlanderCivilMin, "outlanderCivilMin", 1.5f);
            Scribe_Values.Look(ref outlanderHostileMin, "outlanderHostileMin", 1.5f);
            Scribe_Values.Look(ref tribalCivilMin, "tribalCivilMin", 1.5f);
            Scribe_Values.Look(ref tribalHostileMin, "tribalHostileMin", 1.5f);
            Scribe_Values.Look(ref pirateMin, "pirateMin", 1.5f);
            SetIncidents.SetIncidentLevels();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (factionCount == -1 &&
                    Controller.Settings != null)
                {
                    factionCount = Controller.Settings.factionCount;
                    if (factionCount == -1)
                        factionCount = 5.5f;
                }
            }
        }
    }
}