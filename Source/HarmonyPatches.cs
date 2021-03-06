﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace FactionControl
{

    [StaticConstructorOnStartup]
    internal static class Main
    {
        public static List<CustomFaction> CustomFactions = new List<CustomFaction>();

        static Main()
        {
            var harmony = new Harmony("com.rimworld.mod.factioncontrol");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            LongEventHandler.QueueLongEvent(new Action(Init), "LibraryStartup", false, null);
        }

        private static void Init()
        {
            Settings_ModdedFactions.VerifyCustomFactions();

            List<CustomFaction> loaded = new List<CustomFaction>();
            foreach (FactionDef def in DefDatabase<FactionDef>.AllDefs)
            {
                if (def.isPlayer)
                    continue;

                switch (def.defName)
                {
                    case "Ancients":
                    case "AncientsHostile":
                    case "Mechanoid":
                    case "Insect":
                    case "OutlanderCivil":
                    case "OutlanderRough":
                    case "TribeCivil":
                    case "TribeRough":
                    case "TribeSavage":
                    case "Pirate":
                    case "Empire":
                        continue;
                    default:
                        CustomFaction cf = new CustomFaction
                        {
                            FactionDef = def,
                            RequiredCountDefault = def.requiredCountAtGameStart,
                            RequiredCount = def.requiredCountAtGameStart,
                            MaxCountAtStart = def.maxCountAtGameStart
                        };

                        bool contains = false;
                        foreach (CustomFaction f in Main.CustomFactions)
                        {
                            if (f.FactionDef == def)
                            {
                                f.MaxCountAtStart = def.maxCountAtGameStart;
                                f.RequiredCountDefault = def.requiredCountAtGameStart;
                                if (f.RequiredCount == -1)
                                    f.RequiredCount = def.requiredCountAtGameStart;
                                contains = true;
                                break;
                            }
                        }
                        loaded.Add(cf);
                        if (!contains)
                            Main.CustomFactions.Add(cf);
                        break;
                }
            }

            for (int i = CustomFactions.Count - 1; i >= 0; --i)
            {
                if (!loaded.Contains(CustomFactions[i]))
                {
                    CustomFactions.RemoveAt(i);
                }
            }
            SetIncidents.SetIncidentLevels();

            loaded.Clear();
            loaded = null;
        }
    }


    [HarmonyPatch(typeof(Page_SelectScenario), "BeginScenarioConfiguration")]
    static class Patch_Page_SelectScenario_BeginScenarioConfiguration
    {
        static void Prefix()
        {
            SetIncidents.SetIncidentLevels();
        }
    }

    [HarmonyPatch(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow")]
    static class Patch_SavedGameLoaderNow_LoadGameFromSaveFileNow
    {
        static void Prefix()
        {
            SetIncidents.SetIncidentLevels();
        }
        static void Postfix()
        {
            if (Current.Game?.World != null)
                Controller.UpdateSettingsForMapSize(Current.Game.World.factionManager.AllFactions.Count());
        }
    }

    [HarmonyPatch(typeof(LoadedModManager), "GetSettingsFilename", null)]
    public static class LoadedModManager_GetSettingsFilename
    {
        private static void Prefix(string modIdentifier, string modHandleName, ref string __result)
        {
            if (modHandleName.Contains("Controller_ModdedFactions"))
            {
                modHandleName = "Controller_ModdedFactions";
            }
            __result = Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename(string.Format("Mod_{0}_{1}.xml", modIdentifier, modHandleName)));
        }
    }

    public static class SetIncidents
    {
        public static void SetIncidentLevels()
        {
            foreach (IncidentDef def in DefDatabase<IncidentDef>.AllDefsListForReading)
            {
                switch (def.defName)
                {
                    case "DefoliatorShipPartCrash":
                    case "PsychicEmanatorShipPartCrash":
                        if (Controller.Settings.allowMechanoids)
                        {
                            def.baseChance = 2.0f;
                        }
                        else
                        {
                            def.baseChance = 0.0f;
                        }
                        break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "FactionCanBeGroupSource", null)]
    public static class IncidentWorker_RaidEnemy_FactionCanBeGroupSource
    {
        public static bool Prefix(Faction f, ref bool __result)
        {
            if (f == Faction.OfMechanoids)
            {
                int hostileCount = 0;
                List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
                for (int i = 0; i < allFactionsListForReading.Count; i++)
                {
                    if ((allFactionsListForReading[i] != Faction.OfMechanoids) && !allFactionsListForReading[i].def.hidden && allFactionsListForReading[i].HostileTo(Faction.OfPlayer))
                    {
                        hostileCount++;
                    }
                }
                if (hostileCount < 1)
                {
                    __result = true;
                    return false;
                }
                if (Controller.Settings.allowMechanoids.Equals(false) && hostileCount > 0)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FactionGenerator), "GenerateFactionsIntoWorld", null)]
    public static class FactionGenerator_GenerateFactionsIntoWorld
    {
        public static bool Prefix()
        {
            int num = 0;
            int actualFactionCount = 0;
            Controller.factionCenters.Clear();

            foreach (CustomFaction cf in Main.CustomFactions)
            {
                cf.FactionDef.requiredCountAtGameStart = (int)cf.RequiredCount;
                if (cf.RequiredCount == 0)
                {
                    cf.FactionDef.maxCountAtGameStart = 0;
                }
                else
                {
                    cf.FactionDef.maxCountAtGameStart = (int)cf.MaxCountAtStart;
                }
            }

            foreach (FactionDef def in DefDatabase<FactionDef>.AllDefs)
            {
                if (def.isPlayer)
                    continue;

                switch (def.defName)
                {
                    case "OutlanderCivil":
                        SetFactionCount(def, (int)Controller_FactionOptions.Settings.outlanderCivilMin);
                        break;
                    case "OutlanderRough":
                        SetFactionCount(def, (int)Controller_FactionOptions.Settings.outlanderHostileMin);
                        break;
                    case "TribeCivil":
                        SetFactionCount(def, (int)Controller_FactionOptions.Settings.tribalCivilMin);
                        break;
                    case "TribeRough":
                        SetFactionCount(def, (int)Controller_FactionOptions.Settings.tribalHostileMin);
                        break;
                    case "TribeSavage":
                        SetFactionCount(def, (int)Controller_FactionOptions.Settings.tribalSavageMin);
                        break;
                    case "Empire":
                        SetFactionCount(def, (int)Controller_FactionOptions.Settings.empireMin);
                        break;
                    case "Pirate":
                        SetFactionCount(def, (int)Controller_FactionOptions.Settings.pirateMin);
                        break;
                }

                actualFactionCount += def.requiredCountAtGameStart;
                for (int i = 0; i < def.requiredCountAtGameStart; i++)
                {
                    Faction faction = FactionGenerator.NewGeneratedFaction(def);
                    Find.FactionManager.Add(faction);
                    if (!def.hidden)
                    {
                        num++;
                    }
                }
            }

            if (Controller_FactionOptions.Settings.MinFactionCount == 0 &&
                Main.CustomFactions.Count == 0)
            {
                /*Log.Error("Faction Control: No factions were selected. To prevent the game from going into an infinite loop a tribe was added.");
                FactionDef def = DefDatabase<FactionDef>.GetNamed("TribeCivil");
                def.requiredCountAtGameStart = 1;
                Controller.maxFactionSprawl = 1;
                Faction faction = FactionGenerator.NewGeneratedFaction(def);
                Find.FactionManager.Add(faction);
                actualFactionCount = 1;*/
                return false;
            }

            Controller.UpdateSettingsForMapSize(actualFactionCount);

            while (num < (int)Controller_FactionOptions.Settings.factionCount)
            {
                FactionDef facDef = (from fa in DefDatabase<FactionDef>.AllDefs
                                     where fa.canMakeRandomly && Find.FactionManager.AllFactions.Count((Faction f) => f.def == fa) < fa.maxCountAtGameStart
                                     select fa).RandomElement<FactionDef>();
                Faction faction2 = FactionGenerator.NewGeneratedFaction(facDef);
                Find.World.factionManager.Add(faction2);
                num++;
            }
            float tilesCount = (float)Find.WorldGrid.TilesCount / 100000f;
            float minBP100K = 75f * Controller.Settings.factionDensity;
            float maxBP100K = 85f * Controller.Settings.factionDensity;
            FloatRange factionBasesPer100kTiles = new FloatRange(minBP100K, maxBP100K);
            int maxCount = 0;
            int count = GenMath.RoundRandom(tilesCount * factionBasesPer100kTiles.RandomInRange);
            count -= Find.WorldObjects.SettlementBases.Count;
            for (int j = 0; j < count && maxCount < 2000; j++)
            {
                Faction faction2 = (
                  from x in Find.World.factionManager.AllFactionsListForReading
                  where !x.def.isPlayer && !x.def.hidden
                  select x).RandomElementByWeight<Faction>((Faction x) => x.def.settlementGenerationWeight);
                Settlement factionBase = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                factionBase.SetFaction(faction2);
                factionBase.Tile = TileFinder.RandomSettlementTileFor(faction2, false, null);
                if (factionBase.Tile < 1)
                {
                    j--;
                }
                else
                {
                    factionBase.Name = SettlementNameGenerator.GenerateSettlementName(factionBase);
                    Find.WorldObjects.Add(factionBase);
                }
                ++maxCount;
            }
            return false;
        }

        private static void UpdateDef(FactionDef def, int requiredCount)
        {
            def.requiredCountAtGameStart = requiredCount;
            if (requiredCount < 1)
            {
                def.maxCountAtGameStart = 0;
            }
            else
            {
                def.maxCountAtGameStart = requiredCount;
            }
        }

        private static void SetFactionCount(FactionDef def, int required)
        {
            def.requiredCountAtGameStart = required;
            if (def.requiredCountAtGameStart == 0)
                def.maxCountAtGameStart = 0;
            else
                def.maxCountAtGameStart = 100;
        }
    }

    [HarmonyPatch(typeof(TileFinder), "RandomSettlementTileFor", null)]
    public static class TileFinder_RandomSettlementTileFor
    {
        public static bool Prefix(Faction faction, ref int __result, bool mustBeAutoChoosable = false, Predicate<int> extraValidator = null)
        {
            int num;
            for (int i = 0; i < 2500; i++)
            {
                if ((
                from _ in Enumerable.Range(0, 100)
                select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight<int>((int x) => {
                    Tile item = Find.WorldGrid[x];
                    if (!item.biome.canBuildBase || !item.biome.implemented || item.hilliness == Hilliness.Impassable)
                    {
                        return 0f;
                    }
                    if (mustBeAutoChoosable && !item.biome.canAutoChoose)
                    {
                        return 0f;
                    }
                    if (extraValidator != null && !extraValidator(x))
                    {
                        return 0f;
                    }
                    return item.biome.settlementSelectionWeight;
                }, out num))
                {
                    if (TileFinder.IsValidTileForNewSettlement(num, null))
                    {
                        if (faction == null || faction.def.hidden.Equals(true) || faction.def.isPlayer.Equals(true))
                        {
                            __result = num;
                            return false;
                        }
                        else if (Controller.factionCenters.ContainsKey(faction))
                        {
                            double sprawl = Controller.maxFactionSprawl;
                            if (faction.def.defName.Equals("Pirate"))
                                sprawl = Controller.pirateSprawl;

                            float test = Find.WorldGrid.ApproxDistanceInTiles(Controller.factionCenters[faction], num);
                            if (faction.def.maxCountAtGameStart == (faction.def.requiredCountAtGameStart * 2))
                            {
                                if (test < (sprawl * 3))
                                {
                                    __result = num;
                                    return false;
                                }
                            }
                            else
                            {
                                if (test < sprawl)
                                {
                                    __result = num;
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            bool locationOK = true;
                            foreach (KeyValuePair<Faction, int> factionCenter in Controller.factionCenters)
                            {
                                float test = Find.WorldGrid.ApproxDistanceInTiles(factionCenter.Value, num);
                                if (test < Controller.minFactionSeparation)
                                {
                                    locationOK = false;
                                }
                            }
                            if (locationOK.Equals(true))
                            {
                                __result = num;
                                Controller.factionCenters.Add(faction, num);
                                return false;
                            }
                        }
                    }
                }
            }
            Log.Warning(string.Concat("Failed to find faction base tile for ", faction));
            if (Controller.failureCount.ContainsKey(faction))
            {
                Controller.failureCount[faction]++;
                if (Controller.failureCount[faction] == 10)
                {
                    Controller.failureCount.Remove(faction);
                    if (Controller.factionCenters.ContainsKey(faction))
                    {
                        Controller.factionCenters.Remove(faction);
                        Log.Warning("  Relocating faction center.");
                    }
                }
            }
            else
            {
                Log.Warning("  Retrying.");
                Controller.failureCount.Add(faction, 1);
            }
            __result = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(Faction))]
    [HarmonyPatch("Color", MethodType.Getter)]
    [HarmonyPriority(Priority.Last)]
    public static class Patch_Faction_get_Color
    {
        public static void Postfix(Faction __instance, ref Color __result)
        {
            if (!Controller.Settings.dynamicColors ||
                __instance.def.isPlayer)
            {
                return;
            }

            float goodwill = GoodWillToColor(__instance.GoodwillWith(Faction.OfPlayer));

            if (__instance.HostileTo(Faction.OfPlayer))
            {
                __result = new Color(0.75f, goodwill, goodwill);
            }
            else
            {
                switch (__instance.def.defName)
                {
                    case "TribeCivil":
                        __result = new Color(1f, 1f, goodwill);
                        break;
                    case "TribeRough":
                        __result = new Color(goodwill, 1f, goodwill);
                        break;
                    case "OutlanderCivil":
                        __result = new Color(goodwill, goodwill, 1f);
                        break;
                    case "OutlanderRough":
                        __result = new Color(0.5f, goodwill, 1f);
                        break;
                    default:
                        return;
                }
            }
        }

        private static float GoodWillToColor(int goodwill)
        {
            float v = Math.Abs(goodwill) * 0.01f;
            //if (v > .65f)
            //    v = .65f;
            if (v > 1f)
                v = 1f;
            else if (v < 0.35f)
                v = 0.35f;
            v = 1 - v;
            return v;
        }
    }

    [HarmonyPatch(typeof(FactionGenerator), "EnsureRequiredEnemies")]
    public static class FactionGenerator_EnsureRequiredEnemies
    {
        public static void Prefix(Faction player)
        {
            if (!Controller.Settings.randomGoodwill)
                return;

            foreach (Faction f in Find.FactionManager.AllFactions)
            {
                switch (f.def.defName)
                {
                    case "OutlanderCivil":
                    case "OutlanderRough":
                    case "TribeCivil":
                    case "TribeRough":
                        int change;
                        if (f.HostileTo(Faction.OfPlayer))
                            change = Rand.RangeInclusive(-55, 25);
                        else
                            change = Rand.RangeInclusive(-25, 55);

                        FactionRelationKind orig = f.RelationKindWith(Faction.OfPlayer);
                        f.TryAffectGoodwillWith(Faction.OfPlayer, change);

                        if (orig != FactionRelationKind.Hostile &&
                            f.GoodwillWith(Faction.OfPlayer) < -10)
                        {
                            f.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile);
                        }
                        else if (orig == FactionRelationKind.Hostile &&
                                 f.GoodwillWith(Faction.OfPlayer) >= 0)
                        {
                            f.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Neutral);
                        }
                        break;
                }
            }
            /*foreach (FactionDef allDef in DefDatabase<FactionDef>.AllDefs)
            {
                if (!allDef.mustStartOneEnemy || !Find.World.factionManager.AllFactions.Any<Faction>((Faction f) => f.def == allDef) || Find.World.factionManager.AllFactions.Any<Faction>((Faction f) => (f.def != allDef ? false : f.HostileTo(player))))
                {
                    continue;
                }
                Faction faction = (
                from f in Find.World.factionManager.AllFactions
                where f.def == allDef
                select f).RandomElement<Faction>();
                int num = -faction.GoodwillWith(player) + 100;
                num = (int)(num * Rand.Range(0.51f, 0.99f));
                int randomInRange = DiplomacyTuning.ForcedStartingEnemyGoodwillRange.RandomInRange;
                int goodwillChange = randomInRange - num;
                faction.TryAffectGoodwillWith(player, goodwillChange, false, false, null, null);
                faction.TrySetRelationKind(player, FactionRelationKind.Hostile, false, null, null);
            }
            int hostileCount = 0;
            int friendlyCount = 0;
            List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
            for (int i = 0; i < allFactionsListForReading.Count; i++)
            {
                if (allFactionsListForReading[i].def.hidden || allFactionsListForReading[i].def.isPlayer) { continue; }
                if (allFactionsListForReading[i].GoodwillWith(player) > -80f) { allFactionsListForReading[i].TrySetNotHostileTo(player, false); }
                if (allFactionsListForReading[i].HostileTo(player))
                {
                    hostileCount++;
                }
                else
                {
                    friendlyCount++;
                }
            }
            int hostileTarget = (int)(Find.World.factionManager.AllFactions.Count() / 3) + Rand.Range(-1, 2);
            if ((hostileTarget - hostileCount) > friendlyCount) { hostileTarget = friendlyCount + hostileCount; }
            for (int i = hostileCount; i < hostileTarget; i++)
            {
                Faction faction = (
                from f in Find.World.factionManager.AllFactions
                where (f.HostileTo(player) ? false : f.def.isPlayer ? false : !f.def.hidden)
                select f).RandomElement<Faction>();
                int num = -faction.GoodwillWith(player) + 100;
                num = (int)(num * Rand.Range(0.51f, 0.99f));
                int randomInRange = DiplomacyTuning.ForcedStartingEnemyGoodwillRange.RandomInRange;
                int goodwillChange = randomInRange - num;
                faction.TryAffectGoodwillWith(player, goodwillChange, false, false, null, null);
                faction.TrySetRelationKind(player, FactionRelationKind.Hostile, false, null, null);
            }
            return false;*/
        }
    }

    [HarmonyPatch(typeof(Faction), "TryAffectGoodwillWith")]
    public static class Patch_Faction_TryAffectGoodwillWith
    {
        static bool Prefix(Faction __instance, ref bool __result, Faction other, int goodwillChange, bool canSendMessage, bool canSendHostilityLetter, string reason, GlobalTargetInfo? lookTarget)
        {
            if (!Controller.Settings.relationsChangeOverTime &&
                other == Faction.OfPlayer &&
                goodwillChange < 0 &&
                !canSendMessage && !canSendHostilityLetter && reason == null && lookTarget == null)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LetterStack), "ReceiveLetter", typeof(string), typeof(string), typeof(LetterDef), typeof(string))]
    public static class Patch_LetterStack_ReceiveLetter
    {
        static bool Prefix(string label, string text, LetterDef textLetterDef, string debugInfo)
        {
            if (!Controller.Settings.relationsChangeOverTime && 
                textLetterDef == LetterDefOf.NegativeEvent && 
                label == "LetterLabelFactionBaseProximity".Translate())
            {
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(Page_CreateWorldParams), "DoWindowContents")]
    public static class Patch_Page_CreateWorldParams_DoWindowContents
    {
        static void Postfix(Rect rect)
        {
            float y = rect.y + rect.height - 78f;
            Text.Font = GameFont.Small;
            string label = "RFC.FactionControlName".Translate();
            if (Widgets.ButtonText(new Rect(0, y, 150, 32), label))
            {
                OpenSettingsWindow();
            }
        }

        public static void OpenSettingsWindow()
        {
            Find.WindowStack.TryRemove(typeof(EditWindow_Log));
            if (!Find.WindowStack.TryRemove(typeof(SettingsDialogWindow)))
            {
                Find.WindowStack.Add(new SettingsDialogWindow());
            }
        }
    }
}
