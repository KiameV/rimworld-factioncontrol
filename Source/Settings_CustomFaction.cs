using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FactionControl
{
    class Controller_CustomFactions : Mod
    {
        public static Settings_ModdedFactions Settings;

        public override string SettingsCategory()
        {
            if (Main.CustomFactions.Count > 0)
                return "RFC.CustomFactionControl".Translate();
            return null;
        }

        public override void DoSettingsWindowContents(Rect inRect) { Settings.DoWindowContents(inRect); }

        public Controller_CustomFactions(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings_ModdedFactions>();
        }
    }

    class Settings_ModdedFactions : ModSettings
    {
        private CustomFaction selected = null;

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = inRect.width
            };
            list.Begin(inRect);

            string label = (this.selected != null) ? this.selected.FactionDef.label : "RFC.SelectAFaction".Translate();
            if (list.ButtonText(label))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (CustomFaction f in Main.CustomFactions)
                {
                    options.Add(new FloatMenuOption(f.FactionDef.label, delegate
                    {
                        this.selected = f;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            if (this.selected != null)
            {
                list.Gap(24);
                list.Label("RFC.factionBasic1".Translate() + this.selected.FactionDef.label + "RFC.factionBasic2".Translate() + "  " + (int)this.selected.Frequency);
                this.selected.Frequency = list.Slider(this.selected.Frequency, 0, 10.99f);
                list.CheckboxLabeled("RFC.factionPirate1".Translate() + this.selected.FactionDef.label + "RFC.factionPirate2".Translate(), ref this.selected.TreatAsPirate, "RFC.factionPirateTip".Translate());
                if (this.selected.Frequency > 55)
                {
                    list.Gap(24);
                    list.CheckboxLabeled("RFC.factionHidden3".Translate() + this.selected.FactionDef.label + "RFC.factionHidden2".Translate(), ref this.selected.UseHidden);
                }
                else if (this.selected.Frequency > 45)
                {
                    list.Gap(24);
                    list.CheckboxLabeled("RFC.factionHidden3".Translate() + this.selected.FactionDef.label + "RFC.factionHidden2".Translate(), ref this.selected.UseHidden);
                }
            }
            list.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref Main.CustomFactions, "customFactions", LookMode.Deep, new object[0]);

            if (Main.CustomFactions == null)
                Main.CustomFactions = new List<CustomFaction>();
        }
    }

    class CustomFaction : IExposable
    {
        public float Frequency;
        public bool TreatAsPirate;
        public bool UseHidden;
        private string factionDef;

        public CustomFaction()
        {

        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref factionDef, "faction");
            Scribe_Values.Look(ref Frequency, "frequency");
            Scribe_Values.Look(ref TreatAsPirate, "treatAsPirate");
            Scribe_Values.Look(ref UseHidden, "useHidden");
        }

        public FactionDef FactionDef
        {
            get
            {
                return DefDatabase<FactionDef>.GetNamed(this.factionDef);
            }
            set
            {
                this.factionDef = value.defName;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                if (obj == this)
                    return true;
                if (obj is CustomFaction)
                    return string.Equals(this.factionDef, ((CustomFaction)obj).factionDef);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.factionDef.GetHashCode();
        }
    }
}