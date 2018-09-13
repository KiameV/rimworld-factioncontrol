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
        public static List<CustomFaction> CustomFactions = new List<CustomFaction>();

        static Main()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.rimworld.mod.factioncontrol");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            LongEventHandler.QueueLongEvent(new Action(Init), "LibraryStartup", false, null);
        }
        private static void Init()
        {
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
                    case "Pirate":
                        continue;
                    default:
                        CustomFaction cf = new CustomFaction
                        {
                            FactionDef = def
                        };
                        if (def.hidden.Equals(true))
                        {
                            cf.Frequency = 50;
                            cf.TreatAsPirate = false;
                            cf.UseHidden = true;
                        }
                        else
                        {
                            if (!def.canMakeRandomly)
                            {
                                cf.Frequency = 60;
                                cf.UseHidden = true;
                            }
                            else
                            {
                                cf.Frequency = def.requiredCountAtGameStart;
                                cf.UseHidden = false;
                            }

                            if (def.maxCountAtGameStart < 50)
                            {
                                cf.TreatAsPirate = true;
                            }
                            else
                            {
                                cf.TreatAsPirate = false;
                            }
                        }

                        if (!CustomFactions.Contains(cf))
                            CustomFactions.Add(cf);

                        loaded.Add(cf);
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
            int num = 0;
            int actualFactionCount = 0;
            Controller.factionCenters.Clear();

            foreach (CustomFaction cf in Main.CustomFactions)
            {
                if (cf.FactionDef.defName.Equals(cf.FactionDef.defName))
                {
                    int requiredCount = (int)cf.Frequency;
                    if (requiredCount > 45)
                    {
                        if (cf.UseHidden)
                        {
                            cf.FactionDef.requiredCountAtGameStart = 1;
                        }
                        else
                        {
                            cf.FactionDef.requiredCountAtGameStart = 0;
                        }
                    }

                    if (cf.TreatAsPirate)
                    {
                        cf.FactionDef.maxCountAtGameStart = cf.FactionDef.requiredCountAtGameStart * 2;
                    }
                }
            }

            foreach (FactionDef def in DefDatabase<FactionDef>.AllDefs)
            {
                if (def.isPlayer)
                    continue;

                switch (def.defName)
                {
                    case "OutlanderCivil":
                        UpdateDef(def, (int)Controller_FactionOptions.Settings.outlanderCivilMin);
                        break;
                    case "OutlanderRough":
                        UpdateDef(def, (int)Controller_FactionOptions.Settings.outlanderHostileMin);
                        break;
                    case "TribeCivil":
                        UpdateDef(def, (int)Controller_FactionOptions.Settings.tribalCivilMin);
                        break;
                    case "TribeRough":
                        UpdateDef(def, (int)Controller_FactionOptions.Settings.tribalHostileMin);
                        break;
                    case "Pirate":
                        def.requiredCountAtGameStart = (int)Controller_FactionOptions.Settings.pirateMin;
                        def.maxCountAtGameStart = def.requiredCountAtGameStart * 2;
                        break;
                }

                actualFactionCount += def.requiredCountAtGameStart;
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

            if (Controller_FactionOptions.Settings.outlanderCivilMin == 0 &&
                Controller_FactionOptions.Settings.outlanderHostileMin == 0 &&
                Controller_FactionOptions.Settings.tribalCivilMin == 0 &&
                Controller_FactionOptions.Settings.tribalHostileMin == 0 &&
                Main.CustomFactions.Count == 0)
            {
                Log.Error("Faction Control: No factions were selected. To prevent the game from going into an infinite loop a tribe was added.");
                FactionDef def = DefDatabase<FactionDef>.GetNamed("TribeCivil");
                def.requiredCountAtGameStart = 1;
                Controller.maxFactionSprawl = 1;
                Faction faction = FactionGenerator.NewGeneratedFaction(def);
                Find.FactionManager.Add(faction);
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
            for (int j = 0; j < count && maxCount < 250; j++)
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

        private static void UpdateDef(FactionDef def, int requiredCount)
        {
            def.requiredCountAtGameStart = requiredCount;
            if (def.requiredCountAtGameStart < 1)
            {
                def.maxCountAtGameStart = 0;
            }
            else
            {
                def.maxCountAtGameStart = 100;
            }
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
        public static void Postfix(Faction player)
        {
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
                            change = Rand.RangeInclusive(-55, 35);
                        else
                            change = Rand.RangeInclusive(-35, 55);

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
}
