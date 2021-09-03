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

    [HarmonyPatch(typeof(Page_SelectScenario), "BeginScenarioConfiguration")]
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
                    if (d.maxConfigurableAtWorldCreation <= 0 && !d.hidden && !d.isPlayer)
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

    [HarmonyPatch(typeof(Page_CreateWorldParams), "DoWindowContents")]
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

    [HarmonyPatch(typeof(WorldGenerator), "GenerateWorld")]
    public class WorldGenerator_Generate
    {
        internal static bool IsGeneratingWorld = false;
        internal static Dictionary<FactionDef, FactionDensity> FDs = new Dictionary<FactionDef, FactionDensity>();
        internal static Dictionary<string, int> FirstSettlementLocation = new Dictionary<string, int>();
        private static Dictionary<FactionDef, int> MaxAtWorldCreate = new Dictionary<FactionDef, int>();
        internal static HashSet<int> SettlementLocaitons = new HashSet<int>();
        internal static StringBuilder sb = new StringBuilder();

        [HarmonyPriority(Priority.First)]
        public static void Prefix()
        {
            IsGeneratingWorld = true;

            sb.Clear();
            SettlementLocaitons.Clear();
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
        }
        [HarmonyPriority(Priority.First)]
        public static void Postfix()
        {
            SettlementLocaitons.Clear();
            FDs.Clear();
            FirstSettlementLocation.Clear();
            if (sb.Length > 0)
            {
                Log.Message("[Faction Control] The following messages were made durring world generation:\n"+sb.ToString());
                sb.Clear();
            }
            foreach (var kv in MaxAtWorldCreate)
                kv.Key.maxConfigurableAtWorldCreation = kv.Value;
            MaxAtWorldCreate.Clear();
        }

        public static void Finalizer()
        {
            IsGeneratingWorld = false;
        }
    }

    [HarmonyPatch(typeof(TileFinder), "RandomSettlementTileFor")]
    public static class TileFinder_RandomSettlementTileFor
    {
        internal static Faction Faction;

        [HarmonyPriority(Priority.First)]
        static void Prefix(Faction faction)
        {
            Faction = faction;
        }

        [HarmonyPriority(Priority.First)]
        static void Postfix(ref int __result, Faction faction, bool mustBeAutoChoosable, Predicate<int> extraValidator)
        {
            if (!WorldGenerator_Generate.IsGeneratingWorld)
                return;
            try
            {
                if (faction != null && faction.Name != null && WorldGenerator_Generate.FirstSettlementLocation != null &&
                    WorldGenerator_Generate.FirstSettlementLocation.ContainsKey(faction.Name) == false)
                {
                    if (Settings.CenterPointEnabled && WorldGenerator_Generate.FirstSettlementLocation.Count == 0 &&
                        (Settings.GroupDistance.MinEnabled || Settings.GroupDistance.MaxEnabled))
                    {
                        __result = Settings.CenterPoint;
                    }
                    WorldGenerator_Generate.FirstSettlementLocation[faction.Name] = __result;
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(TileFinder), "IsValidTileForNewSettlement")]
    public static class TileFinder_IsValidTileForNewSettlement
    {
        static void Postfix(ref bool __result, ref int tile)
        {
            if (!WorldGenerator_Generate.IsGeneratingWorld || !__result)
                return;

            if (tile == 0 ||
                WorldGenerator_Generate.SettlementLocaitons.Contains(tile))
            {
                //Log.Message($"- could not place settlement on tile {tile}");
                __result = false;
                return;
            }

            Faction f = TileFinder_RandomSettlementTileFor.Faction;
            if (f != null &&
                !f.IsPlayer && !f.Hidden &&
                WorldGenerator_Generate.FDs.TryGetValue(f.def, out FactionDensity fd) && fd.Enabled)
            {
                if (WorldGenerator_Generate.FirstSettlementLocation.TryGetValue(f.Name, out int center))
                {
                    var dist = Find.WorldGrid.ApproxDistanceInTiles(tile, center);
                    __result = dist < fd.Density;
                }
                else // First settlement
                {
                    if (Settings.GroupDistance.MinEnabled)
                    {
                        bool ok = true;
                        foreach (var kv in WorldGenerator_Generate.FirstSettlementLocation)
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
                        foreach (var kv in WorldGenerator_Generate.FirstSettlementLocation)
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
            if (__result)
                WorldGenerator_Generate.SettlementLocaitons.Add(tile);
        }
    }

    [HarmonyPatch(typeof(FactionGenerator), "GenerateFactionsIntoWorld")]
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


    [HarmonyPatch(typeof(WorldFactionsUIUtility), "DoRow")]
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
