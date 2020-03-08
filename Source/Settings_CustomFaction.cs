using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FactionControl
{
    /*class Controller_CustomFactions : Mod
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
        private Vector2 pos = new Vector2(0, 0);
        public void DoWindowContents(Rect inRect)
        {
            const float HEIGHT = 70;
            float WIDTH = inRect.width - 16;

            float x = inRect.x;
            float y = inRect.y + 20;
            Widgets.BeginScrollView(new Rect(x, y, WIDTH + 16, 500), ref pos, new Rect(inRect.x, inRect.y, WIDTH, HEIGHT * Main.CustomFactions.Count * 10));

            foreach (CustomFaction f in Main.CustomFactions)
            {
                Widgets.Label(new Rect(x, y, WIDTH, 32), "RFC.factionBasic1".Translate() + f.FactionDef.label + "RFC.factionBasic2".Translate());
                y += 30;
                Widgets.Label(new Rect(x + 20, y, 40, 32), ((int)f.RequiredCount).ToString());
                f.RequiredCount = Widgets.HorizontalSlider(new Rect(x + 80, y, 200, 32), f.RequiredCount, 0, (f.MaxCountAtStart < 6) ? f.MaxCountAtStart : 6);
                if (Widgets.ButtonText(new Rect(x + 300, y - 2, 100, 28), "Reset".Translate()))
                {
                    f.RequiredCount = f.RequiredCountDefault;
                }
                y += 30;
                Widgets.DrawLineHorizontal(x, y, WIDTH);
                y += 10;
            }
            Widgets.EndScrollView();

            /*string label = (this.selected != null) ? this.selected.FactionDef.label : "RFC.SelectAFaction".Translate();
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
            }* /
        }

        internal static void VerifyCustomFactions()
        {
            Stack<CustomFaction> notDefined = new Stack<CustomFaction>();
            foreach (var cf in Main.CustomFactions)
            {
                if (cf.FactionDef == null)
                {
                    Log.Warning($"FactionDef [{cf.FactionDefName}] is not defined in any loaded mods.");
                    notDefined.Push(cf);
                }
            }

            while (notDefined.Count > 0)
            {
                Main.CustomFactions.Remove(notDefined.Pop());
            }
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
        public float RequiredCountDefault = -1;
        public float RequiredCount = -1;
        public float MaxCountAtStart = -1;
        //public bool UseHiddenDefault;
        //public bool UseHidden;
        private string factionDefName;

        public CustomFaction()
        {

        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref factionDefName, "faction");
            Scribe_Values.Look(ref RequiredCount, "requiredCount");
            //Scribe_Values.Look(ref UseHidden, "useHidden");
        }

        public FactionDef FactionDef
        {
            get
            {
                return DefDatabase<FactionDef>.GetNamed(this.factionDefName, false);
            }
            set
            {
                this.factionDefName = value.defName;
            }
        }

        public string FactionDefName => this.factionDefName;

        public override bool Equals(object obj)
        {
            return 
                this == obj || 
                (obj is CustomFaction cf && this.factionDefName == cf.factionDefName);
        }

        public override int GetHashCode()
        {
            return this.factionDefName.GetHashCode();
        }
    }*/
}