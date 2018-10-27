using System;
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

        public string strFacCnt = "";
        public string strOutCiv = "";
        public string strOutHos = "";
        public string strTriCiv = "";
        public string strTriHos = "";
        public string strPir = "";

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

            this.DrawSlider(list, "RFC.factionCount", ref factionCount, ref strFacCnt, 0, 100);
            list.Gap(24);

            this.DrawSlider(list, "RFC.outlanderCivilMin", ref outlanderCivilMin, ref strOutCiv, 0, 20);
            list.Gap();

            this.DrawSlider(list, "RFC.outlanderRoughMin", ref outlanderHostileMin, ref strOutHos, 0, 20);
            list.Gap();
            list.Gap();

            this.DrawSlider(list, "RFC.tribalCivilMin", ref tribalCivilMin, ref strTriCiv, 0, 20);
            list.Gap();

            this.DrawSlider(list, "RFC.tribalRoughMin", ref tribalHostileMin, ref strTriHos, 0, 20);
            list.Gap();
            list.Gap();
            
            this.DrawSlider(list, "RFC.pirateMin", ref pirateMin, ref strPir, 0, 20);
            list.End();
        }

        private void DrawSlider(Listing_Standard list, string label, ref float value, ref string buffer, float min, float max)
        {
            float f;
            string s = buffer;
            buffer = list.ModTextEntryLabeled(label.Translate(), buffer);
            if (!s.Equals(buffer))
            {
                if (float.TryParse(buffer, out f))
                {
                    if (f > 0)
                        value = f;
                }
            }

            f = value;
            value = list.Slider(value, min, max);
            if (f != value)
            {
                buffer = ((int)value).ToString();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref factionCount, "factionCount", 5.5f);
            Scribe_Values.Look(ref outlanderCivilMin, "outlanderCivilMin", 1.5f);
            Scribe_Values.Look(ref outlanderHostileMin, "outlanderHostileMin", 1.5f);
            Scribe_Values.Look(ref tribalCivilMin, "tribalCivilMin", 1.5f);
            Scribe_Values.Look(ref tribalHostileMin, "tribalHostileMin", 1.5f);
            Scribe_Values.Look(ref pirateMin, "pirateMin", 1.5f);
            SetIncidents.SetIncidentLevels();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (factionCount < 0)
                    factionCount = 5.5f;
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                strFacCnt = ((int)factionCount).ToString();
                strOutCiv = ((int)outlanderCivilMin).ToString();
                strOutHos = ((int)outlanderHostileMin).ToString();
                strTriCiv = ((int)tribalCivilMin).ToString();
                strTriHos = ((int)tribalHostileMin).ToString();
                strPir = ((int)pirateMin).ToString();
            }
        }
    }

    static class LSUtil
    {
        public static string ModTextEntryLabeled(this Listing_Standard ls, string label, string buffer, int lineCount = 1)
        {
            Rect rect = ls.GetRect(Text.LineHeight * (float)lineCount);
            Widgets.Label(new Rect(rect.x, rect.y, rect.width - 75, rect.height), label);
            string result = Widgets.TextField(new Rect(rect.xMax - 65, rect.y, 65, rect.height), buffer);
            ls.Gap(ls.verticalSpacing);
            return result;
        }
    }
}