using HarmonyLib;
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
        internal static Dictionary<FactionDef, FactionDensity> FDs = new Dictionary<FactionDef, FactionDensity>();
        internal static Dictionary<FactionDef, int> FirstSettlementLocation = new Dictionary<FactionDef, int>();
        [HarmonyPriority(Priority.First)]
        public static void Prefix()
        {
            FDs.Clear();
            FirstSettlementLocation.Clear();
            foreach (var fd in Settings.FactionDensities)
            {
                if (fd.Enabled)
                {
                    var def = DefDatabase<FactionDef>.GetNamed(fd.FactionDefName, false);
                    if (def != null)
                        FDs[def] = fd;
                    }
            }
        }
        [HarmonyPriority(Priority.First)]
        public static void Postfix()
        {
            FDs.Clear();
            FirstSettlementLocation.Clear();
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
            if (__result != 0 && faction != null &&
                WorldGenerator_Generate.FirstSettlementLocation.ContainsKey(faction.def) == false)
            {
                WorldGenerator_Generate.FirstSettlementLocation[faction.def] = __result;
            }
        }

        [HarmonyPatch(typeof(TileFinder), "IsValidTileForNewSettlement")]
        public static class TileFinder_IsValidTileForNewSettlement
        {
            static void Postfix(ref bool __result, int tile)
            {
                Faction f = TileFinder_RandomSettlementTileFor.Faction;
                if (f != null &&
                    !f.IsPlayer && !f.Hidden &&
                    WorldGenerator_Generate.FDs.TryGetValue(f.def, out FactionDensity fd) && fd.Enabled && 
                    WorldGenerator_Generate.FirstSettlementLocation.TryGetValue(f.def, out int center))
                {
                    var dist = Find.WorldGrid.ApproxDistanceInTiles(tile, center);
                    __result = dist < fd.Density;
                }
            }
        }
    }
}
