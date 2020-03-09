using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FactionControl
{
    public class ControllerWindow : Window
    {
        public ControllerWindow()
        {

        }

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
        }

        public override void DoWindowContents(Rect r)
        {
            Controller.DoWindowContents(new Rect(r.x, r.y, r.xMax, r.yMax - 40), true);
            if (Widgets.ButtonText(new Rect(r.x, r.yMax - 36, 100, 32), "Close".Translate()))
                this.Close();
        }
    }

    public class Controller : Mod
    {
        internal const float MIN_DENSITY = 0.01f;
        internal const float MAX_DENSITY = 4f;
        internal const float MIN_SETTLEMENTS = 1f;
        internal const float MAX_SETTLEMENTS = 10f;

        public static Settings Settings;
        private static float innerY = 0;
        private static Vector2 scroll = Vector2.zero;

        public override string SettingsCategory() { return "RFC.FactionControl".Translate(); }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Controller.DoWindowContents(canvas, false);
        }

        public static void DoWindowContents(Rect canvas, bool isDialogWindow)
        {
            float yBuffer = Text.LineHeight + 8f;
            float doubleYBuffer = Text.LineHeight * 2 + 6f;

            Widgets.BeginScrollView(
                new Rect(canvas.x, canvas.y, canvas.xMax, canvas.yMax - 40f),
                ref scroll,
                new Rect(0, 0, canvas.xMax - 16f, innerY));
            innerY = 0;

            CheckboxLabeled(
                new Rect(0, innerY, 300, Text.LineHeight), 
                "RFC.AllowMechanoids".Translate(), 
                ref Settings.allowMechanoids,
                 "RFC.AllowMechanoidsTip".Translate());
            innerY += yBuffer;

            if (!isDialogWindow)
            {
                CheckboxLabeled(new Rect(0, innerY, 300, Text.LineHeight), "RFC.EnableFactionDynamicColors".Translate(), ref Settings.dynamicColors, "RFC.EnableFactionDynamicColorsTip".Translate());
                innerY += yBuffer;
            }

            Widgets.CheckboxLabeled(new Rect(0, innerY, 300, Text.LineHeight), "RFC.RelationChangesOverTime".Translate(), ref Settings.relationsChangeOverTime);
            innerY += yBuffer;

            float x = 0;
            Widgets.Label(new Rect(0, innerY, 250, Text.LineHeight), "RFC.FactionCountMinMax".Translate());
            x += 210;
            Settings.minFactionCountBuffer = Widgets.TextArea(new Rect(x, innerY, 100, Text.LineHeight), Settings.minFactionCountBuffer, false);
            x += 110;
            Settings.maxFactionCountBuffer = Widgets.TextArea(new Rect(x, innerY, 100, Text.LineHeight), Settings.maxFactionCountBuffer, false);
            innerY += doubleYBuffer;

            if (int.TryParse(Settings.minFactionCountBuffer, out int i))
                Settings.minFactionCount = i;
            if (int.TryParse(Settings.maxFactionCountBuffer, out i))
                Settings.maxFactionCount = i;

            foreach (var f in Settings.FactionSettings)
            {
                Widgets.DrawLineHorizontal(0, innerY, canvas.xMax - 36);
                innerY += yBuffer;

                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(0, innerY - 2, 250, Text.LineHeight), $"RFC.{f.FactionDef.defName}".Translate());
                Text.Font = GameFont.Small;
                if (Widgets.ButtonText(new Rect(canvas.xMax - 135, innerY, 100, 28), "Reset".Translate()))
                {
                    f.DefaultValues();
                }
                innerY += Text.LineHeight + 10;

                x = 10;
                Widgets.Label(new Rect(x, innerY, 150, Text.LineHeight), "RFC.CountMinMax".Translate());
                x += 160;
                f.MinCountBuffer = Widgets.TextArea(new Rect(x, innerY, 70, Text.LineHeight), f.MinCountBuffer, false);
                x += 80;
                f.MaxCountBuffer = Widgets.TextArea(new Rect(x, innerY, 70, Text.LineHeight), f.MaxCountBuffer, false);
                innerY += yBuffer + 8;

                CheckboxLabeled(new Rect(10, innerY, 300, Text.LineHeight), "RFC.EnableFactionRandomGoodwill".Translate(), ref f.RandomGoodwill, "RFC.EnableFactionRandomGoodwillToolTip".Translate());
                innerY += yBuffer + 8;

                bool b = f.BoostSettlementCount;
                CheckboxLabeled(new Rect(10, innerY, 300, Text.LineHeight), "RFC.BoostSettlementCount".Translate(), ref b, "RFC.BoostSettlementCountToolTip".Translate());
                f.BoostSettlementCount = b;
                innerY += yBuffer + 8;

                if (int.TryParse(f.MinCountBuffer, out i))
                    f.MinCount = i;
                if (int.TryParse(f.MaxCountBuffer, out i))
                    f.MaxCount = i;

                Widgets.Label(new Rect(10, innerY, 100, Text.LineHeight), "RFC.Density".Translate());
                Text.Font = GameFont.Tiny;
                Widgets.Label(new Rect(105, innerY, canvas.xMax - 101, Text.LineHeight), GetFactionDensityLabel(f.Density));
                innerY += yBuffer + 12;
                f.Density = Widgets.HorizontalSlider(
                    new Rect(10, innerY, canvas.xMax - 36, Text.LineHeight), f.Density, MIN_DENSITY, MAX_DENSITY, true, null, "loose".Translate(), "tight".Translate());
                innerY += doubleYBuffer;
                Text.Font = GameFont.Small;

                Widgets.Label(new Rect(10, innerY, 100, Text.LineHeight), "RFC.Settlements".Translate());
                Text.Font = GameFont.Tiny;
                Widgets.Label(new Rect(105, innerY, canvas.xMax - 101, Text.LineHeight), GetSettlementCountFactorLabel(f.SettlementCountFactor));
                innerY += yBuffer + 12;
                f.SettlementCountFactor = Widgets.HorizontalSlider(
                    new Rect(10, innerY, canvas.xMax - 36, Text.LineHeight), f.SettlementCountFactor, f.MinSettlementsValue, f.MaxSettlementsValue, true, null, "PlanetPopulation_Low".Translate(), "PlanetPopulation_High".Translate());
                innerY += doubleYBuffer;
                Text.Font = GameFont.Small;
            }

            Widgets.EndScrollView();

            x = canvas.x;
            if (isDialogWindow)
                x = canvas.xMax - 135;
            if (Widgets.ButtonText(new Rect(x, canvas.yMax, 100, 30), "Reset".Translate()))
            {
                Settings.DefaultValues();
            }
        }

        public static void CheckboxLabeled(Rect rect, string label, ref bool b, string tooltip)
        {
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }
                TooltipHandler.TipRegion(rect, tooltip);
            }
            Widgets.CheckboxLabeled(rect, label, ref b);
        }

        private static string GetFactionDensityLabel(float factionGrouping)
        {
            if (factionGrouping < 0.5f)
            {
                return "PlanetPopulation_Normal".Translate();
            }
            return "";
        }

        private static string GetSettlementCountFactorLabel(float density)
        {
            if (density > 1.5 && density < 2)
            {
                return "PlanetPopulation_Normal".Translate();
            }
            return "";
        }

        public Controller(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
        }
    }

    public class Settings : ModSettings
    {
        public int minFactionCount = 7;
        public string minFactionCountBuffer = "";
        public int maxFactionCount = 7;
        public string maxFactionCountBuffer = "";
        public bool allowMechanoids = true;
        //public bool randomGoodwill = true;
        public bool dynamicColors = true;
        public bool relationsChangeOverTime = true;
        public List<FactionSettings> FactionSettings = new List<FactionSettings>();

        public int FactionCount
        {
            get
            {
                int min = minFactionCount, max = maxFactionCount, minCount = 0, maxCount = 0;
                foreach (var f in FactionSettings)
                {
                    minCount += f.MinCount;
                    maxCount += f.MaxCount;
                }

                if (minCount > minFactionCount)
                {
                    Log.Message($"Min faction count is less than the sum of all min factions. Using {minCount} for min factions.");
                    min = minCount;
                }
                if (maxCount < minFactionCount)
                {
                    Log.Warning($"Sum of all min factions is greater than min factions. Using {maxCount} for max factions.");
                    max = maxCount;
                }
                if (minCount > maxFactionCount)
                {
                    Log.Warning($"Sum of all min factions is greater than max factions. Using {minCount} for max factions.");
                    max = minCount;
                }

                if (min > max)
                {
                    Log.Warning($"Min faction count is greater than Max faction count. Defaulting to {max} factions.");
                    min = max;
                }
                return Rand.RangeInclusive(min, max);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref minFactionCount, "minFactionCount", 7);
            Scribe_Values.Look(ref maxFactionCount, "maxFactionCount", 7);
            Scribe_Values.Look(ref allowMechanoids, "allowMechanoids", true);
            //Scribe_Values.Look(ref randomGoodwill, "randomGoodwill", true);
            Scribe_Values.Look(ref dynamicColors, "dynamicColors", true);
            Scribe_Values.Look(ref relationsChangeOverTime, "relationsChangeOverTime", true);
            Scribe_Collections.Look(ref FactionSettings, "factionSettings", LookMode.Deep, new object[0]);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.ResetBuffers(false);
                if (FactionSettings == null)
                    FactionSettings = new List<FactionSettings>();
            }
        }

        public void ResetBuffers(bool all = true)
        {
            this.minFactionCountBuffer = this.minFactionCount.ToString();
            this.maxFactionCountBuffer = this.maxFactionCount.ToString();
            if (all)
                foreach (var f in this.FactionSettings)
                    f.ResetBuffers();
        }

        public void ReloadFactions()
        {
            if (FactionSettings == null)
                FactionSettings = new List<FactionSettings>();

            for (int i = FactionSettings.Count - 1; i >= 0; --i)
            {
                if (FactionSettings[i] == null || !FactionSettings[i].LoadFactionDef())
                {
                    Log.Warning($"unable to find faction {FactionSettings[i].FactionDefName}");
                    FactionSettings.RemoveAt(i);
                }
            }

            DefDatabase<FactionDef>.AllDefsListForReading.ForEach(d =>
            {
                if (!d.hidden && !d.isPlayer)
                {
                    bool found = false;
                    foreach (var fs in FactionSettings)
                        if (fs.FactionDef == d)
                        {
                            found = true;
                            break;
                        }
                    if (!found)
                    {
                        Log.Message($"no settings found for faction {d.defName} and default settings will be added.");
                        var s = new FactionSettings(d);
                        s.ResetBuffers();
                        FactionSettings.Add(s);
                    }
                }
            });

            FactionSettings.Sort((l, r) => l.FactionDef.label.CompareTo(r.FactionDef.label));
            this.ResetBuffers(true);
        }

        internal void DefaultValues()
        {
            this.minFactionCount = 7;
            this.maxFactionCount = 7;
            this.allowMechanoids = true;
            this.dynamicColors = true;
            this.relationsChangeOverTime = true;
            this.ResetBuffers(false);
            this.FactionSettings.ForEach(f => f.DefaultValues());
        }
    }

    public class FactionSettings : IExposable
    {
        public const float DEFAULT_DENSITY = 0.01f;
        public const float DEFAULT_SETTLEMENTS = 1.8f;
        public const float SETTLEMENT_BOOST_AMOUNT = 10;

        public FactionDef FactionDef;
        private string factionDefName;
        public float Density;
        public float SettlementCountFactor;
        private bool boostSettlementCount;
        public int MinCount;
        public string MinCountBuffer = "";
        public int MaxCount;
        public string MaxCountBuffer = "";
        public bool RandomGoodwill;

        public string FactionDefName => factionDefName;

        public FactionSettings() { }
        public FactionSettings(FactionDef def) {
            this.FactionDef = def;
            this.factionDefName = def.defName;
            this.DefaultValues();
        }

        public bool BoostSettlementCount
        {
            get => this.boostSettlementCount;
            set
            {
                if (this.boostSettlementCount != value)
                {
                    this.SettlementCountFactor = BoundValue(this.SettlementCountFactor, this.MinSettlementsValue, this.MaxSettlementsValue);

                    this.boostSettlementCount = value;
                    if (this.boostSettlementCount)
                    {
                        this.SettlementCountFactor *= SETTLEMENT_BOOST_AMOUNT;
                    }
                    else
                    {
                        this.SettlementCountFactor /= SETTLEMENT_BOOST_AMOUNT;
                    }
                }
            }
        }

        public void ExposeData()
        {
            try
            {
                Scribe_Values.Look(ref this.factionDefName, "factionDef");
                Scribe_Values.Look(ref this.Density, "density", DEFAULT_DENSITY);
                Scribe_Values.Look(ref this.SettlementCountFactor, "settlementCountFactor", DEFAULT_SETTLEMENTS);
                Scribe_Values.Look(ref this.MinCount, "minCount", 1);
                Scribe_Values.Look(ref this.MaxCount, "maxCount", 1);
                Scribe_Values.Look(ref RandomGoodwill, "randomGoodwill", false);
                Scribe_Values.Look(ref boostSettlementCount, "boostSettlementCount", false);

                if (Density < 0.01f)
                    Density = 0.01f;
                if (SettlementCountFactor < 0.01f)
                    SettlementCountFactor = 0.01f;

                if (Scribe.mode == LoadSaveMode.PostLoadInit)
                {
                    this.ResetBuffers();
                }
            }
            catch
            {
                this.FactionDef = null;
                Log.Warning("failed to load faction, is a mod missing?");
            }
        }

        public bool LoadFactionDef()
        {
            this.FactionDef = null;
            foreach (var f in DefDatabase<FactionDef>.AllDefsListForReading)
            {
                if (f.defName == this.factionDefName)
                {
                    this.FactionDef = f;
                    break;
                }
            }
            this.ResetBuffers();
            return this.FactionDef != null;
        }

        public void ResetBuffers()
        {
            this.MinCountBuffer = this.MinCount.ToString();
            this.MaxCountBuffer = this.MaxCount.ToString();
        }

        internal void DefaultValues()
        {
            this.MinCount = this.FactionDef.requiredCountAtGameStart;
            this.MaxCount = (this.FactionDef.maxCountAtGameStart > 100)
                ? this.FactionDef.requiredCountAtGameStart
                : this.FactionDef.maxCountAtGameStart;
            this.boostSettlementCount = false;
            this.SettlementCountFactor = DEFAULT_SETTLEMENTS;
            this.Density = DEFAULT_DENSITY;
            this.RandomGoodwill = false;
            this.ResetBuffers();
        }

        private float BoundValue(float v, float min, float max)
        {
            if (v < min)
                return min;
            if (v > max)
                return max;
            return v;
        }

        public float MinSettlementsValue
        { 
            get
            {
                float v = Controller.MIN_SETTLEMENTS;
                if (this.boostSettlementCount)
                    v *= SETTLEMENT_BOOST_AMOUNT;
                return v;
            }
        }
        public float MaxSettlementsValue
        {
            get
            {
                float v = Controller.MAX_SETTLEMENTS;
                if (this.boostSettlementCount)
                    v *= SETTLEMENT_BOOST_AMOUNT;
                return v;
            }
        }
    }
}
