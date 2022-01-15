using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionControl
{

    [StaticConstructorOnStartup]
    internal static class Main
    {
        internal static bool IsConfigMapsLoaded;
        internal static bool IsRandomGoodwillLoaded;
        static Main()
        {
            var harmony = new Harmony("com.rimworld.mod.factioncontrol");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            IsConfigMapsLoaded = ModLister.GetActiveModWithIdentifier("configurablemaps.kv.rw") != null;
            IsRandomGoodwillLoaded = ModLister.GetActiveModWithIdentifier("randomgoodwill.kv.rw") != null;
        }
    }

    [HarmonyPatch(typeof(Page_SelectScenario), nameof(Page_SelectScenario.BeginScenarioConfiguration))]
    static class Patch_Page_SelectScenario_BeginScenarioConfiguration
    {
        [HarmonyPriority(Priority.First)]
        static void Prefix()
        {
            if (Settings.OverrideFactionMaxCount)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var d in DefDatabase<FactionDef>.AllDefs)
                {
                    if (d.maxConfigurableAtWorldCreation <= 0 && !d.hidden && !d.isPlayer && d.defName != "PColony")
                    {
                        sb.Append($"-{d.defName}\n");
                        d.maxConfigurableAtWorldCreation = 100;
                    }
                }
                if (sb.Length > 0)
                {
                    Log.Message("[Faction Control] Overriding 'maxConfigurableAtWorldCreation' for factions:\n" + sb.ToString());
                }
            }
        }
    }

    [HarmonyPatch(typeof(Page_CreateWorldParams), nameof(Page_CreateWorldParams.DoWindowContents))]
    public static class Patch_Page_CreateWorldParams_DoWindowContents
    {
        static void Postfix(Rect rect)
        {
            float y = rect.y + rect.height - 78f;
            Text.Font = GameFont.Small;
            string label = "RFC.FactionControlName".Translate();
            float x = 0f;
            if (Main.IsConfigMapsLoaded)
                x += 170;
            if (Main.IsRandomGoodwillLoaded)
                x += 170;
            if (Widgets.ButtonText(new Rect(x, y, 150, 32), label))
            {
                if (!Find.WindowStack.TryRemove(typeof(SettingsWindow)))
                {
                    Find.WindowStack.Add(new SettingsWindow());
                }
            }
        }
    }

    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GenerateWorld))]
    public class WorldGenerator_Generate
    {
        internal static bool IsGeneratingWorld = false;
        internal static Dictionary<FactionDef, FactionDensity> FDs = new Dictionary<FactionDef, FactionDensity>();
        internal static Dictionary<string, int> FirstSettlementLocation = new Dictionary<string, int>();
        private static Dictionary<FactionDef, int> MaxAtWorldCreate = new Dictionary<FactionDef, int>();

        [HarmonyPriority(Priority.First)]
        public static void Prefix()
        {
            // make sure the tile finder doesn't use outdated data until we tell it to
            TileFinder_RandomSettlementTileFor.FirstSettlementLocations = null;
            TileFinder_IsValidTileForNewSettlement.FirstSettlementLocations = null;
            TileFinder_IsValidTileForNewSettlement.FDs = null;
            // actual prep stuff
            IsGeneratingWorld = true;

            FDs.Clear();
            FirstSettlementLocation.Clear();
            MaxAtWorldCreate.Clear();
            foreach (var fd in Settings.FactionDensities)
            {
                if (fd.Enabled)
                {
                    var def = DefDatabase<FactionDef>.GetNamed(fd.FactionDefName, false);
                    if (def != null)
                        FDs[def] = fd;
                    }
            }
            if (Settings.DisableFactionLimit)
            {
                DefDatabase<FactionDef>.AllDefs.Do(d =>
                {
                    MaxAtWorldCreate[d] = d.maxConfigurableAtWorldCreation;
                    d.maxConfigurableAtWorldCreation = 1000;
                });
            }
            // tell tile finder to use our data - this is done before any of the tile finder function is called
            TileFinder_RandomSettlementTileFor.FirstSettlementLocations = FirstSettlementLocation;
            TileFinder_IsValidTileForNewSettlement.FirstSettlementLocations = FirstSettlementLocation;
            TileFinder_IsValidTileForNewSettlement.FDs = FDs;
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix()
        {
            // reset max at world creation
            foreach (var kv in MaxAtWorldCreate)
                kv.Key.maxConfigurableAtWorldCreation = kv.Value;
            MaxAtWorldCreate.Clear();
        }

        public static void Finalizer()
        {
            IsGeneratingWorld = false;
        }
    }

    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GenerateFromScribe))]
    public static class WorldGenerator_GenerateFromScribe
    {
        internal static bool IsGeneratingWorld = false;
        internal static Dictionary<FactionDef, FactionDensity> FDs = new Dictionary<FactionDef, FactionDensity>();
        internal static Dictionary<string, int> FirstSettlementLocation = new Dictionary<string, int>();
        private static Dictionary<FactionDef, int> MaxAtWorldCreate = new Dictionary<FactionDef, int>();
        internal static HashSet<Settlement> AddedSettlements = new HashSet<Settlement>();

        [HarmonyPriority(Priority.First)]
        public static void Prefix()
        {
            // make sure the tile finder doesn't use outdated data until we tell it to
            TileFinder_RandomSettlementTileFor.FirstSettlementLocations = null;
            TileFinder_IsValidTileForNewSettlement.FirstSettlementLocations = null;
            TileFinder_IsValidTileForNewSettlement.FDs = null;
            // actual prep stuff
            IsGeneratingWorld = true;

            FDs.Clear();
            FirstSettlementLocation.Clear();
            MaxAtWorldCreate.Clear();
            AddedSettlements.Clear();
            foreach (var fd in Settings.FactionDensities)
            {
                if (fd.Enabled)
                {
                    var def = DefDatabase<FactionDef>.GetNamed(fd.FactionDefName, false);
                    if (def != null)
                        FDs[def] = fd;
                }
            }
            if (Settings.DisableFactionLimit)
            {
                DefDatabase<FactionDef>.AllDefs.Do(d =>
                {
                    MaxAtWorldCreate[d] = d.maxConfigurableAtWorldCreation;
                    d.maxConfigurableAtWorldCreation = 1000;
                });
            }
            // tile finder won't get new data until cross references are resolved near the end of save load, so we are not telling it to use our data, yet
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix()
        {
            // reset max at world creation
            foreach (var kv in MaxAtWorldCreate)
                kv.Key.maxConfigurableAtWorldCreation = kv.Value;
            MaxAtWorldCreate.Clear();
        }
    }

    // generating without world data is probably similar, haven't seen it happen though
    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GenerateWithoutWorldData))]
    public static class WorldGenerator_GenerateWithoutWorldData
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix() => WorldGenerator_GenerateFromScribe.Prefix();

        [HarmonyPriority(Priority.First)]
        public static void Postfix() => WorldGenerator_GenerateFromScribe.Postfix();
    }

    [HarmonyPatch(typeof(CrossRefHandler), nameof(CrossRefHandler.ResolveAllCrossReferences))]
    public static class CrossRefHandler_ResolveAllCrossReferences
    {
        public static void Postfix()
        {
            if (!WorldGenerator_GenerateFromScribe.IsGeneratingWorld) return;
            // tell tile finder to use our data - this is done before we can re-obtain settlement tiles
            TileFinder_RandomSettlementTileFor.FirstSettlementLocations = WorldGenerator_GenerateFromScribe.FirstSettlementLocation;
            TileFinder_IsValidTileForNewSettlement.FirstSettlementLocations = WorldGenerator_GenerateFromScribe.FirstSettlementLocation;
            TileFinder_IsValidTileForNewSettlement.FDs = WorldGenerator_GenerateFromScribe.FDs;

            // set 'first' settlements
            foreach (var s in Find.WorldObjects.Settlements)
            {
                if (!WorldGenerator_GenerateFromScribe.AddedSettlements.Contains(s) && s.Faction != null && s.Faction.Name != null
                    && !WorldGenerator_GenerateFromScribe.FirstSettlementLocation.ContainsKey(s.Faction.Name))
                    WorldGenerator_GenerateFromScribe.FirstSettlementLocation[s.Faction.Name] = s.Tile;
            }
            // now randomize settlement positions based on mod logic
            foreach (var s in WorldGenerator_GenerateFromScribe.AddedSettlements)
            {
                s.Tile = s.Faction != null ? TileFinder.RandomSettlementTileFor(s.Faction) : TileFinder.RandomStartingTile();
            }

            // now we can clear stuff we won't use until a save is loaded the next time
            WorldGenerator_GenerateFromScribe.AddedSettlements.Clear();
            WorldGenerator_GenerateFromScribe.IsGeneratingWorld = false;
        }
    }

    [HarmonyPatch(typeof(WorldObjectsHolder), nameof(WorldObjectsHolder.Add))]
    public static class WorldObjectsHolder_Add
    {
        public static void Postfix(WorldObject o)
        {
            if (WorldGenerator_GenerateFromScribe.IsGeneratingWorld && o is Settlement s)
                WorldGenerator_GenerateFromScribe.AddedSettlements.Add(s);
        }
    }

    [HarmonyPatch(typeof(WorldObjectsHolder), nameof(WorldObjectsHolder.Remove))]
    public static class WorldObjectHolder_Remove
    {
        public static void Postfix(WorldObject o)
        {
            if (WorldGenerator_GenerateFromScribe.IsGeneratingWorld && o is Settlement s)
                WorldGenerator_GenerateFromScribe.AddedSettlements.Remove(s);
        }
    }

    [HarmonyPatch(typeof(TileFinder), nameof(TileFinder.RandomSettlementTileFor))]
    public static class TileFinder_RandomSettlementTileFor
    {
        internal static Faction Faction;
        internal static Dictionary<string, int> FirstSettlementLocations;

        [HarmonyPriority(Priority.First)]
        static void Prefix(Faction faction)
        {
            Faction = faction;
        }

        [HarmonyPriority(Priority.First)]
        static void Postfix(ref int __result, Faction faction, bool mustBeAutoChoosable, Predicate<int> extraValidator)
        {
            if (FirstSettlementLocations == null) return;

            try
            {
                if (faction != null && faction.Name != null && FirstSettlementLocations != null &&
                    FirstSettlementLocations.ContainsKey(faction.Name) == false)
                {
                    if (Settings.CenterPointEnabled && FirstSettlementLocations.Count == 0 &&
                        (Settings.GroupDistance.MinEnabled || Settings.GroupDistance.MaxEnabled))
                    {
                        __result = Settings.CenterPoint;
                    }
                    FirstSettlementLocations[faction.Name] = __result;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(TileFinder), nameof(TileFinder.IsValidTileForNewSettlement))]
    public static class TileFinder_IsValidTileForNewSettlement
    {
        internal static Dictionary<string, int> FirstSettlementLocations;
        internal static Dictionary<FactionDef, FactionDensity> FDs;

        static void Postfix(ref bool __result, ref int tile)
        {
            if (!__result || FirstSettlementLocations == null || FDs == null) return;

            if (tile == 0)
            {
                //Log.Message($"- could not place settlement on tile {tile}");
                __result = false;
                return;
            }

            Faction f = TileFinder_RandomSettlementTileFor.Faction;
            if (f != null &&
                !f.IsPlayer && !f.Hidden &&
                FDs.TryGetValue(f.def, out FactionDensity fd) && fd.Enabled)
            {
                if (FirstSettlementLocations.TryGetValue(f.Name, out int center))
                {
                    var dist = Find.WorldGrid.ApproxDistanceInTiles(tile, center);
                    __result = dist < fd.Density;
                }
                else // First settlement
                {
                    if (Settings.GroupDistance.MinEnabled)
                    {
                        bool ok = true;
                        foreach (var kv in FirstSettlementLocations)
                        {
                            var dist = Find.WorldGrid.ApproxDistanceInTiles(tile, kv.Value);
                            ok = dist > Settings.GroupDistance.MinDistance;
                            if (!ok)
                                break;
                        }
                        __result = ok;
                    }
                    if (Settings.GroupDistance.MaxEnabled)
                    {
                        bool ok = true;
                        foreach (var kv in FirstSettlementLocations)
                        {
                            var dist = Find.WorldGrid.ApproxDistanceInTiles(tile, kv.Value);
                            ok = dist < Settings.GroupDistance.MaxDistance;
                            if (!ok)
                                break;
                        }
                        __result = ok;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(FactionGenerator), nameof(FactionGenerator.GenerateFactionsIntoWorld))]
    public class Patch_FactionGenerator_GenerateFactionsIntoWorld
    {
        [HarmonyPriority(Priority.First)]
        static void Postfix()
        {
            if (!WorldGenerator_Generate.IsGeneratingWorld)
                return;
            HashSet<Settlement> settlements = new HashSet<Settlement>();
            bool first = true;
            foreach (var s in Find.WorldObjects.Settlements)
            {
                if (s.Tile == 0)
                {
                    if (first)
                    {
                        first = false;
                        continue;
                    }
                    settlements.Add(s);
                }
            }
            foreach (var s in settlements)
                Find.WorldObjects.Remove(s);
        }
    }


    [HarmonyPatch(typeof(WorldFactionsUIUtility), nameof(WorldFactionsUIUtility.DoRow))]
    public class Patch_WorldFactionsUIUtility_DoRow
    {
        [HarmonyPriority(Priority.High)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> il = instructions.ToList();
            bool found = false;
            for (int i = 0; i < il.Count; ++i)
            {
                if (il[i].opcode == OpCodes.Ldc_I4_S && !found)
                {
                    found = true;
                    il[i].opcode = OpCodes.Call;
                    il[i].operand =
                                typeof(Patch_WorldFactionsUIUtility_DoRow).GetMethod(
                                nameof(Patch_WorldFactionsUIUtility_DoRow.GetMaxSettlements), BindingFlags.Static | BindingFlags.Public);
                }
                yield return il[i];
            }
            if (!found)
            {
                Log.Error("Faction Control could not inject itself properly. This is due to other mods modifying the same code this mod needs to modify.");
            }
        }

        public static int GetMaxSettlements()
        {
            if (Settings.DisableFactionLimit)
                return 1000;
            return 12;
        }
    }
}
