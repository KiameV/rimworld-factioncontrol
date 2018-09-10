using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;

namespace FactionControl
{

    [StaticConstructorOnStartup]
    internal static class Main
    {
        public static List<string> factionsModdedNames = new List<string>();
        public static List<string> factionsModdedLabels = new List<string>();
        public static List<float> factionsModdedFreq = new List<float>();
        public static List<bool> factionsModdedTreatAsPirate = new List<bool>();
        public static List<bool> factionsModdedUseHidden = new List<bool>();
        private static bool initialized = false;

        static Main()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.rimworld.mod.factioncontrol");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            //LongEventHandler.QueueLongEvent(new Action(RFC_Initializer.Setup), "LibraryStartup", false, null);
        }
        public static void Setup()
        {
            if (initialized)
                return;

            initialized = true;
            foreach (FactionDef allDef in DefDatabase<FactionDef>.AllDefs)
            {
                bool alreadyListed = false;
                for (int i = 0; i < factionsModdedNames.Count; i++)
                {
                    if (allDef.defName == factionsModdedNames[i])
                    {
                        alreadyListed = true;
                        break;
                    }
                }
                if (alreadyListed.Equals(false))
                {
                    if (allDef.hidden.Equals(true))
                    {
                        if (allDef.defName == "Spacer" || allDef.defName == "SpacerHostile" || allDef.defName == "Mechanoid" || allDef.defName == "Insect")
                        {
                            continue;
                        }
                        factionsModdedFreq.Add(50);
                        factionsModdedUseHidden.Add(true);
                        factionsModdedTreatAsPirate.Add(false);
                        factionsModdedNames.Add(allDef.defName);
                        factionsModdedLabels.Add(allDef.label);
                    }
                    else
                    {
                        if (allDef.defName == "Outlander" || allDef.defName == "Tribe" || allDef.defName == "Pirate" || allDef.isPlayer.Equals(true))
                        {
                            continue;
                        }
                        if (allDef.canMakeRandomly.Equals(false))
                        {
                            factionsModdedFreq.Add(60);
                            factionsModdedUseHidden.Add(true);
                        }
                        else
                        {
                            factionsModdedFreq.Add(allDef.requiredCountAtGameStart);
                            factionsModdedUseHidden.Add(false);
                        }
                        if (allDef.maxCountAtGameStart < 50)
                        {
                            factionsModdedTreatAsPirate.Add(true);
                        }
                        else
                        {
                            factionsModdedTreatAsPirate.Add(false);
                        }
                        factionsModdedNames.Add(allDef.defName);
                        factionsModdedLabels.Add(allDef.label);
                    }
                }
            }
            for (int i = 0; i < factionsModdedNames.Count; i++)
            {
                bool activeFaction = false;
                foreach (FactionDef allDef in DefDatabase<FactionDef>.AllDefs)
                {
                    if (allDef.defName == factionsModdedNames[i])
                    {
                        activeFaction = true;
                        break;
                    }
                }
                if (activeFaction.Equals(false))
                {
                    factionsModdedNames.RemoveAt(i);
                    factionsModdedLabels.RemoveAt(i);
                    factionsModdedFreq.RemoveAt(i);
                    factionsModdedTreatAsPirate.RemoveAt(i);
                    factionsModdedUseHidden.RemoveAt(i);
                    i--;
                }
            }
            SetIncidents.SetIncidentLevels();
        }
    }

    

    [HarmonyPatch(typeof(LoadedModManager), "GetSettingsFilename", null)]
    public static class LoadedModManager_GetSettingsFilename
    {
        private static bool Prefix(string modIdentifier, string modHandleName, ref string __result)
        {
            if (modHandleName.Contains("Controller_ModdedFactions"))
            {
                modHandleName = "Controller_ModdedFactions";
            }
            __result = Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename(string.Format("Mod_{0}_{1}.xml", modIdentifier, modHandleName)));
            return false;
        }
    }

    public static class SetIncidents
    {
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
            Main.Setup();
            int num = 0;
            int actualFactionCount = 0;
            Controller.factionCenters.Clear();
            foreach (FactionDef allDef in DefDatabase<FactionDef>.AllDefs)
            {
                if (allDef.defName == "Outlander")
                {
                    allDef.requiredCountAtGameStart = (int)Controller.Settings.outlanderMin;
                    if (allDef.requiredCountAtGameStart < 1)
                    {
                        allDef.maxCountAtGameStart = 0;
                    }
                    else
                    {
                        allDef.maxCountAtGameStart = 100;
                    }
                }
                else if (allDef.defName == "Tribe")
                {
                    allDef.requiredCountAtGameStart = (int)Controller.Settings.tribalMin;
                    if (allDef.requiredCountAtGameStart < 1)
                    {
                        allDef.maxCountAtGameStart = 0;
                    }
                    else
                    {
                        allDef.maxCountAtGameStart = 100;
                    }
                }
                else if (allDef.defName == "Pirate")
                {
                    allDef.requiredCountAtGameStart = (int)Controller.Settings.pirateMin;
                    allDef.maxCountAtGameStart = allDef.requiredCountAtGameStart * 2;
                }
                else if (allDef.defName == "Spacer" || allDef.defName == "SpacerHostile") { }
                else if (allDef.defName == "Mechanoid" || allDef.defName == "Insect") { }
                else if (allDef.isPlayer) { }
                else
                {
                    int i = 0;
                    for (i = 0; i < Main.factionsModdedNames.Count; i++)
                    {
                        if (allDef.defName == Main.factionsModdedNames[i])
                        {
                            break;
                        }
                    }
                    if (Main.factionsModdedFreq[i] > 45)
                    {
                        if (Main.factionsModdedUseHidden[i].Equals(true))
                        {
                            allDef.requiredCountAtGameStart = 1;
                        }
                        else
                        {
                            allDef.requiredCountAtGameStart = 0;
                        }
                    }
                    else
                    {
                        allDef.requiredCountAtGameStart = (int)Main.factionsModdedFreq[i];
                    }
                    if (allDef.requiredCountAtGameStart < 1)
                    {
                        allDef.maxCountAtGameStart = 0;
                    }
                    else
                    {
                        allDef.maxCountAtGameStart = 100;
                    }
                    if (Main.factionsModdedTreatAsPirate[i].Equals(true))
                    {
                        allDef.maxCountAtGameStart = allDef.requiredCountAtGameStart * 2;
                    }
                }
                actualFactionCount += allDef.requiredCountAtGameStart;
                Controller.minFactionSeparation = Math.Sqrt(Find.WorldGrid.TilesCount) / (Math.Sqrt(actualFactionCount) * 2);
                if (Controller.Settings.factionGrouping < 1)
                {
                    Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount);
                }
                else if (Controller.Settings.factionGrouping < 2)
                {
                    Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount) / (Math.Sqrt(actualFactionCount) * 1.5);
                }
                else if (Controller.Settings.factionGrouping < 3)
                {
                    Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount) / (Math.Sqrt(actualFactionCount) * 2.25);
                }
                else
                {
                    Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount) / (Math.Sqrt(actualFactionCount) * 3);
                }
                for (int i = 0; i < allDef.requiredCountAtGameStart; i++)
                {
                    Faction faction = FactionGenerator.NewGeneratedFaction(allDef);
                    Find.FactionManager.Add(faction);
                    if (!allDef.hidden)
                    {
                        num++;
                    }
                }
            }
            while (num < (int)Controller.Settings.factionCount)
            {
                FactionDef factionDef = (
                  from fa in DefDatabase<FactionDef>.AllDefs
                  where (!fa.canMakeRandomly ? false : Find.FactionManager.AllFactions.Count<Faction>((Faction f) => f.def == fa) < fa.maxCountAtGameStart)
                  select fa).RandomElement<FactionDef>();
                if (factionDef == null) { break; }
                Faction faction1 = FactionGenerator.NewGeneratedFaction(factionDef);
                Find.World.factionManager.Add(faction1);
                num++;
            }
            float tilesCount = (float)Find.WorldGrid.TilesCount / 100000f;
            float minBP100K = 75f;
            float maxBP100K = 85f;
            if (Controller.Settings.factionDensity < 1)
            {
                minBP100K = minBP100K * 0.25f;
                maxBP100K = maxBP100K * 0.25f;
            }
            else if (Controller.Settings.factionDensity < 2)
            {
                minBP100K = minBP100K * 0.5f;
                maxBP100K = maxBP100K * 0.5f;
            }
            else if (Controller.Settings.factionDensity < 3) { }
            else if (Controller.Settings.factionDensity < 4)
            {
                minBP100K = minBP100K * 2;
                maxBP100K = maxBP100K * 2;
            }
            else if (Controller.Settings.factionDensity < 5)
            {
                minBP100K = minBP100K * 4;
                maxBP100K = maxBP100K * 4;
            }
            else
            {
                minBP100K = minBP100K * 8;
                maxBP100K = maxBP100K * 8;
            }
            FloatRange factionBasesPer100kTiles = new FloatRange(minBP100K, maxBP100K);
            int maxCount = 0;
            int count = GenMath.RoundRandom(tilesCount * factionBasesPer100kTiles.RandomInRange);
            count -= Find.WorldObjects.SettlementBases.Count;
            for (int j = 0; j < count && maxCount < 200; j++)
            {
                Faction faction2 = (
                  from x in Find.World.factionManager.AllFactionsListForReading
                  where (x.def.isPlayer ? false : !x.def.hidden)
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
    }

    [HarmonyPatch(typeof(TileFinder), "RandomSettlementTileFor", null)]
    public static class TileFinder_RandomFactionBaseTileFor
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
                            float test = Find.WorldGrid.ApproxDistanceInTiles(Controller.factionCenters[faction], num);
                            if (faction.def.maxCountAtGameStart == (faction.def.requiredCountAtGameStart * 2))
                            {
                                if (test < (Controller.maxFactionSprawl * 3))
                                {
                                    __result = num;
                                    return false;
                                }
                            }
                            else
                            {
                                if (test < Controller.maxFactionSprawl)
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

    [HarmonyPatch(typeof(FactionGenerator), "EnsureRequiredEnemies", null)]
    public static class FactionGenerator_EnsureRequiredEnemies
    {
        public static bool Prefix(Faction player)
        {
            foreach (FactionDef allDef in DefDatabase<FactionDef>.AllDefs)
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
            return false;
        }
    }

}
