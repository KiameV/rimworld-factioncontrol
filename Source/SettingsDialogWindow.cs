using UnityEngine;
using Verse;

namespace FactionControl
{
    public class SettingsDialogWindow : Window
    {
        private static Vector2 scroll = Vector2.zero;
        private Rect viewRect = new Rect();

        public override void DoWindowContents(Rect canvas)
        {
            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = canvas.width - 20,
            };

            list.BeginScrollView(
                new Rect(canvas.x, canvas.y, canvas.xMax, canvas.yMax - 40),
                ref scroll,
                ref viewRect);

            list.Gap();
            Settings_FactionOptions.DrawSlider(list, "RFC.factionDensity".Translate() + " " + Settings.GetFactionDensityLabel(Controller.Settings.factionDensity), ref Controller.Settings.factionDensity, ref Controller.Settings.strFacDen, 0.01f, 8f);

            list.Gap(24);
            list.Label("RFC.factionGrouping".Translate() + " " + Settings.GetFactionGroupingLabel(Controller.Settings.factionGrouping));// + "(" + factionGrouping.ToString("n2") + ")");
            Controller.Settings.factionGrouping = list.Slider(Controller.Settings.factionGrouping, 0.5f, 5f);

            list.Gap(24);
            list.CheckboxLabeled("RFC.AllowMechanoids".Translate(), ref Controller.Settings.allowMechanoids, "RFC.AllowMechanoidsTip".Translate());
            list.Gap(24);
            list.CheckboxLabeled("RFC.EnableFactionRandomGoodwill".Translate(), ref Controller.Settings.randomGoodwill, "RFC.EnableFactionRandomGoodwillToolTip".Translate());
            list.Gap(24);
            list.CheckboxLabeled("RFC.EnableFactionDynamicColors".Translate(), ref Controller.Settings.dynamicColors, "RFC.EnableFactionDynamicColorsTip".Translate());
            list.Gap(24);
            list.CheckboxLabeled("RFC.SpreadPirates".Translate(), ref Controller.Settings.spreadPirates);
            list.Gap(24);
            list.CheckboxLabeled("RFC.RelationChangesOverTime".Translate(), ref Controller.Settings.relationsChangeOverTime);
            list.Gap(40);

            Settings_FactionOptions.DrawSlider(list, "RFC.factionCount".Translate(), ref Controller_FactionOptions.Settings.factionCount, ref Controller_FactionOptions.Settings.strFacCnt, 0, 100);
            list.Gap(24);

            Settings_FactionOptions.DrawSlider(list, "RFC.outlanderCivilMin".Translate(), ref Controller_FactionOptions.Settings.outlanderCivilMin, ref Controller_FactionOptions.Settings.strOutCiv, 0, 20);
            list.Gap();

            Settings_FactionOptions.DrawSlider(list, "RFC.outlanderRoughMin".Translate(), ref Controller_FactionOptions.Settings.outlanderHostileMin, ref Controller_FactionOptions.Settings.strOutHos, 0, 20);
            list.Gap();

            Settings_FactionOptions.DrawSlider(list, "RFC.tribalCivilMin".Translate(), ref Controller_FactionOptions.Settings.tribalCivilMin, ref Controller_FactionOptions.Settings.strTriCiv, 0, 20);
            list.Gap();

            Settings_FactionOptions.DrawSlider(list, "RFC.tribalRoughMin".Translate(), ref Controller_FactionOptions.Settings.tribalHostileMin, ref Controller_FactionOptions.Settings.strTriHos, 0, 20);
            list.Gap();

            Settings_FactionOptions.DrawSlider(list, "RFC.tribalSavageMin".Translate(), ref Controller_FactionOptions.Settings.tribalSavageMin, ref Controller_FactionOptions.Settings.strTriSav, 0, 20);
            list.Gap();

            Settings_FactionOptions.DrawSlider(list, "RFC.pirateMin".Translate(), ref Controller_FactionOptions.Settings.pirateMin, ref Controller_FactionOptions.Settings.strPir, 0, 20);

            if (ModsConfig.RoyaltyActive)
            {
                list.Gap();
                Settings_FactionOptions.DrawSlider(list, "RFC.empireMin".Translate(), ref Controller_FactionOptions.Settings.empireMin, ref Controller_FactionOptions.Settings.strEmp, 0, 20);
            }

            if (Main.CustomFactions?.Count > 0)
            {
                list.Gap(40);
                foreach (CustomFaction f in Main.CustomFactions)
                {
                    string b = f.RequiredCount.ToString();
                    Settings_FactionOptions.DrawSlider(list, f.FactionDefName, ref f.RequiredCount, ref b, 0, 20);
                    list.Gap();
                }
            }

            list.EndScrollView(ref viewRect);
            if (Widgets.ButtonText(new Rect(0, canvas.yMax - 36, 100, 30), "Close".Translate()))
                this.Close();
        }
    }
}
