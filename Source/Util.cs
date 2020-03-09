using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FactionControl
{
    class Util
    {
        private readonly static Dictionary<Faction, int> FactionMapCenter = new Dictionary<Faction, int>();
        private readonly static Dictionary<Faction, FactionSettings> FactionSettingsLookup = new Dictionary<Faction, FactionSettings>();
        private readonly static Dictionary<Faction, double> FactionSprawl = new Dictionary<Faction, double>();

        private static void Clear()
        {
            FactionMapCenter.Clear();
            FactionSettingsLookup.Clear();
            FactionSprawl.Clear();
        }

        public static void GenerateFactionsIntoWorld()
        {
            Clear();
            List<GeneratedFactions> gf = GenerateFactions();

            /*foreach (var g in gf)
            {
                Log.Message(
                    $"{g.Settings.FactionDef}:" +
                    $"\n    Count: {g.Factions.Count}" +
                    $"\n    Min Count: {g.Settings.MinCount}" +
                    $"\n    Max Count: {g.Settings.MaxCount}" +
                    $"\n    Density: {g.Settings.Density}" +
                    $"\n    Settlement Count: {g.Settings.SettlementCountFactor}");
            }
            Log.Message(
                $"Faction Map Center Count: {FactionMapCenter.Count}" +
                $"\nFind.WorldGrid.TilesCount:{Find.WorldGrid.TilesCount}" +
                $"\nFind.World.info.overallPopulation.GetScaleFactor:{Find.World.info.overallPopulation.GetScaleFactor()}");*/

            if (FactionMapCenter.Count == 0)
            {
                Log.Warning("No factions generated.");
                return;
            }

            float scaled = (float)Find.WorldGrid.TilesCount * 0.00001f * Find.World.info.overallPopulation.GetScaleFactor();
            /*if (FactionMapCenter.Count > 5)
            {
                scaled /= (FactionMapCenter.Count / 5);
            }
            else
            {
                scaled /= FactionMapCenter.Count;
            }*/

            foreach (var g in gf)
            {
                FloatRange settlementsPer100kTiles = new FloatRange(75f * g.Settings.SettlementCountFactor * 0.01f, 85f * g.Settings.SettlementCountFactor * 0.01f);
                foreach (var f in g.Factions)
                {
                    int toPlace = GenMath.RoundRandom(scaled * settlementsPer100kTiles.RandomInRange);
                    for (int i = 0; i < toPlace; ++i)
                    {
                        Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                        settlement.SetFaction(f);
                        settlement.Tile = RandomSettlementTileFor(f);
                        settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
                        Find.WorldObjects.Add(settlement);
                    }
                }
            }
            Clear();
        }

        private static double GetFactionSprawl(Faction f, FactionSettings s)
        {
            if (!FactionSprawl.TryGetValue(f, out double sprawl))
            {
                double sqrtTiles = Math.Sqrt(Find.WorldGrid.TilesCount);
                //double sqrtFactionCount = Math.Sqrt(FactionSettingsLookup.Count);
                sprawl = sqrtTiles / (5 * s.Density);
                if (sprawl < 1)
                    sprawl = 1.001f;
                FactionSprawl[f] = sprawl;
            }
            return sprawl;
        }

        private static int RandomSettlementTileFor(Faction faction)
        {
            int result = TileFinder.RandomSettlementTileFor(faction, false, x =>
            {
                Tile tile = Find.WorldGrid[x];
                if (!tile.biome.canBuildBase || !tile.biome.implemented || tile.hilliness == Hilliness.Impassable)
                {
                    return false;
                }
                if (faction == null || faction.def.hidden.Equals(true) || faction.def.isPlayer.Equals(true))
                {
                    return true;
                }
                else if (!FactionMapCenter.TryGetValue(faction, out int center) || 
                         !FactionSettingsLookup.TryGetValue(faction, out FactionSettings s) ||
                         Find.WorldGrid.ApproxDistanceInTiles(center, x) > GetFactionSprawl(faction, s))
                {
                    return false;
                }
                return true;
            });
            if (faction == null || faction.def.hidden.Equals(true) || faction.def.isPlayer.Equals(true))
            {
                return result;
            }
            else if (result != 0)
                return result;
            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }

        private static List<GeneratedFactions> GenerateFactions()
        {
            List<GeneratedFactions> generated = new List<GeneratedFactions>(Controller.Settings.FactionSettings.Count);
            int total = 0;
            foreach (var fs in Controller.Settings.FactionSettings)
            {
                var counter = new GeneratedFactions(fs);
                for (int i = 0; i < fs.MinCount; ++i)
                {
                    Faction faction = FactionGenerator.NewGeneratedFaction(fs.FactionDef);
                    Find.FactionManager.Add(faction);
                    counter.Factions.Add(faction);
                    FactionSettingsLookup.Add(faction, fs);
                    if (!fs.FactionDef.hidden)
                        ++total;
                }
                generated.Add(counter);
            }

            int factionCount = Controller.Settings.FactionCount;
            while (total < factionCount)
            {
                GeneratedFactions gf = generated.Where((GeneratedFactions g) => g.Settings.FactionDef.canMakeRandomly && g.Factions.Count < g.Settings.MaxCount).RandomElement();
                Faction faction = FactionGenerator.NewGeneratedFaction(gf.Settings.FactionDef);
                Find.FactionManager.Add(faction);
                gf.Factions.Add(faction);
                FactionSettingsLookup.Add(faction, gf.Settings);
                if (!gf.Settings.FactionDef.hidden)
                    ++total;
            }

            // Find where each faction will be on the map
            double minFactionSeparation = Math.Sqrt(Find.WorldGrid.TilesCount) / (Math.Sqrt(total) * 2);
            foreach (var g in generated)
            {
                foreach (var f in g.Factions)
                {
                    int center = TileFinder.RandomSettlementTileFor(f, false, tile => {
                        foreach (var c in FactionMapCenter.Values)
                        {
                            if (Find.WorldGrid.ApproxDistanceInTiles(c, tile) < minFactionSeparation)
                            {
                                return false;
                            }
                        }
                        return true;
                    });
                    if (center == 0)
                    {
                        Log.Warning($"unable to find place to put {f.Name} of faction {f.def.defName}");
                        Find.FactionManager.Remove(f);
                    }
                    else // Success
                    { 
                        FactionMapCenter.Add(f, center);
                    }
                }
            }

            return generated;
        }

        private class GeneratedFactions
        {
            public readonly FactionSettings Settings;
            public readonly List<Faction> Factions = new List<Faction>();
            public GeneratedFactions(FactionSettings fs) { this.Settings = fs; }
        }
    }
}
