using System;
using UnityEngine;
using Verse;

namespace FactionControl
{
    /*public class Controller_FactionOptions : Mod
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
        public float factionCount;
        public float outlanderCivilMin;
        public float outlanderHostileMin;
        public float tribalCivilMin;
        public float tribalHostileMin;
        public float tribalSavageMin;
        public float pirateMin;
        public float empireMin;

        public string strFacCnt;
        public string strOutCiv;
        public string strOutHos;
        public string strTriCiv;
        public string strTriHos;
        public string strTriSav;
        public string strPir;
        public string strEmp;

        private Vector2 scroll = new Vector2(0, 0);
        private Rect viewRect = new Rect();

        public Settings_FactionOptions()
        {
            this.Reset();
        }

        public int MinFactionCount => (int)(outlanderCivilMin + outlanderHostileMin + tribalCivilMin + tribalHostileMin + tribalSavageMin + ((ModsConfig.RoyaltyActive) ? empireMin : 0f) + pirateMin);

        private void Reset()
        {
            if (ModsConfig.RoyaltyActive)
                factionCount = 6f;
            else
                factionCount = 5f;
            outlanderCivilMin = 1f;
            outlanderHostileMin = 1f;
            tribalCivilMin = 1f;
            tribalHostileMin = 1f;
            tribalSavageMin = 1f;
            pirateMin = 1f;
            empireMin = 1f;

            strFacCnt = "6";
            strOutCiv = "1";
            strOutHos = "1";
            strTriCiv = "1";
            strTriHos = "1";
            strTriSav = "1";
            strPir = "1";
            strEmp = "1";
        }

        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = canvas.width - 20,
            };
            list.BeginScrollView(
                new Rect(canvas.x, canvas.y, canvas.width, canvas.height - 40), 
                ref scroll,
                ref viewRect);
            Text.Font = GameFont.Tiny;
            list.Label("RFC.factionNotes".Translate());
            Text.Font = GameFont.Small;
            list.Gap();

            DrawSlider(list, "RFC.factionCount".Translate(), ref factionCount, ref strFacCnt, 0, 100);
            list.Gap(24);

            DrawSlider(list, "RFC.outlanderCivilMin".Translate(), ref outlanderCivilMin, ref strOutCiv, 0, 20);
            list.Gap();

            DrawSlider(list, "RFC.outlanderRoughMin".Translate(), ref outlanderHostileMin, ref strOutHos, 0, 20);
            list.Gap();
            list.Gap();

            DrawSlider(list, "RFC.tribalCivilMin".Translate(), ref tribalCivilMin, ref strTriCiv, 0, 20);
            list.Gap();

            DrawSlider(list, "RFC.tribalRoughMin".Translate(), ref tribalHostileMin, ref strTriHos, 0, 20);
            list.Gap();

            DrawSlider(list, "RFC.tribalSavageMin".Translate(), ref tribalSavageMin, ref strTriSav, 0, 20);
            list.Gap();
            list.Gap();
            
            DrawSlider(list, "RFC.pirateMin".Translate(), ref pirateMin, ref strPir, 0, 20);

            if (ModsConfig.RoyaltyActive)
            {
                list.Gap();
                list.Gap();

                DrawSlider(list, "RFC.empireMin".Translate(), ref empireMin, ref strEmp, 0, 20);
            }
            list.EndScrollView(ref viewRect);
            
            if (Widgets.ButtonText(new Rect(canvas.x, canvas.yMax - 30, 100, 30), "Default Values"))
            {
                Find.WindowStack.Add(new Dialog_MessageBox(
                    "Reset all values to default?", 
                    "Yes", () => {
                        this.Reset();
                    }, 
                    "No", () => {}));
            }
        }

        public static void DrawSlider(Listing_Standard list, string label, ref float value, ref string buffer, float min, float max)
        {
            float f;
            string s = buffer;
            buffer = list.ModTextEntryLabeled(label, buffer);
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
            Scribe_Values.Look(ref factionCount, "factionCount", 6f);
            Scribe_Values.Look(ref outlanderCivilMin, "outlanderCivilMin", 1f);
            Scribe_Values.Look(ref outlanderHostileMin, "outlanderHostileMin", 1f);
            Scribe_Values.Look(ref tribalCivilMin, "tribalCivilMin", 1f);
            Scribe_Values.Look(ref tribalHostileMin, "tribalHostileMin", 1f);
            Scribe_Values.Look(ref tribalSavageMin, "tribalSavageMin", 1f);
            Scribe_Values.Look(ref pirateMin, "pirateMin", 1f);
            Scribe_Values.Look(ref empireMin, "empireMin", 1f);
            SetIncidents.SetIncidentLevels();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (factionCount < 0)
                {
                    if (ModsConfig.RoyaltyActive)
                        factionCount = 6f;
                    else
                        factionCount = 5f;
                }
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                strFacCnt = ((int)factionCount).ToString();
                strOutCiv = ((int)outlanderCivilMin).ToString();
                strOutHos = ((int)outlanderHostileMin).ToString();
                strTriCiv = ((int)tribalCivilMin).ToString();
                strTriHos = ((int)tribalHostileMin).ToString();
                strTriSav = ((int)tribalSavageMin).ToString();
                strPir = ((int)pirateMin).ToString();
                strEmp = ((int)empireMin).ToString();
            }
        }
    }

    public static class LSUtil
    {
        public static string ModTextEntryLabeled(this Listing_Standard ls, string label, string buffer, int lineCount = 1)
        {
            Rect rect = ls.GetRect(Text.LineHeight * (float)lineCount);
            Widgets.Label(new Rect(rect.x, rect.y, rect.width - 75, rect.height), label);
            string result = Widgets.TextField(new Rect(rect.xMax - 65, rect.y, 65, rect.height), buffer);
            ls.Gap(ls.verticalSpacing);
            return result;
        }
    }*/
}