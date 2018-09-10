using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FactionControl
{
    /*internal class CustomFaction : IExposable
    {
        public FactionDef FactionDef;
        public float Frequency;
        public bool TreatAsPirate;
        public bool UseHidden;

        public CustomFaction()
        {

        }

        public CustomFaction(FactionDef factionDef, float frequency, bool treadAsPirate, bool useHidden)
        {
            this.FactionDef = factionDef;
            this.Frequency = frequency;
            this.TreatAsPirate = treadAsPirate;
            this.UseHidden = useHidden;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref FactionDef, "faction");
            Scribe_Values.Look(ref Frequency, "frequency");
            Scribe_Values.Look(ref TreatAsPirate, "treatAsPirate");
            Scribe_Values.Look(ref UseHidden, "useHidden");
        }
    }

    internal class FactionUtil
    {
        public static readonly Dictionary<string, CustomFaction> CustomFactions = new Dictionary<string, CustomFaction>();
        private static bool initialized = false;

        public static void Init()
        {
            if (initialized)
                return;

            initialized = true;
            foreach (FactionDef def in DefDatabase<FactionDef>.AllDefs)
            {
                Log.Error("FactionDef: " + def.defName);
                bool alreadyListed = CustomFactions.ContainsKey(def.defName);
                if (alreadyListed.Equals(false))
                {
                    if (def.hidden.Equals(true))
                    {
                        if (def.defName == "Spacer" || def.defName == "SpacerHostile" || def.defName == "Mechanoid" || def.defName == "Insect")
                        {
                            continue;
                        }
                        CustomFactions.Add(def.defName, new CustomFaction(def, 50f, false, true));
                    }
                    else
                    {
                        if (def.defName == "Outlander" || def.defName == "Tribe" || def.defName == "Pirate" || def.isPlayer.Equals(true))
                        {
                            continue;
                        }
                        float frequency;
                        bool useHidden;
                        bool treatAsPirate;
                        if (!def.canMakeRandomly)
                        {
                            frequency = 60f;
                            useHidden = true;
                        }
                        else
                        {
                            frequency = def.requiredCountAtGameStart;
                            useHidden = false;
                        }
                        if (def.maxCountAtGameStart < 50)
                        {
                            treatAsPirate = true;
                        }
                        else
                        {
                            treatAsPirate = false;
                        }
                        CustomFactions.Add(def.defName, new CustomFaction(def, frequency, treatAsPirate, useHidden));
                    }
                }
            }

            SetIncidentLevels();
        }

        public static void SetIncidentLevels()
        {
            foreach (IncidentDef def in DefDatabase<IncidentDef>.AllDefsListForReading)
            {
                if (def.defName == "PoisonShipPartCrash" || def.defName == "PsychicEmanatorShipPartCrash")
                {
                    if (Controller.Settings.allowMechanoids.Equals(true))
                    {
                        def.baseChance = 2.0f;
                    }
                    else
                    {
                        def.baseChance = 0.0f;
                    }
                }
            }
        }
    }*/
}
