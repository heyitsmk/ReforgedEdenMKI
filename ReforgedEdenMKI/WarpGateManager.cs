using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReforgedEdenMKI
{
    public static class WarpGateManager
    {
        private static readonly HashSet<string> LOADED_PLAYFIELDS;
        private static readonly Dictionary<string, List<WarpGate>> PLAYFIELD_WARPGATES;

        static WarpGateManager()
        {
            LOADED_PLAYFIELDS = new HashSet<string>();
            PLAYFIELD_WARPGATES = new Dictionary<string, List<WarpGate>>
            {
                {
                    "Andromeda-Decay [Sun Left]",
                    new List<WarpGate>()
                    {
                        new WarpGate("Eden_BAO_ProgGate1", "Ancient Warp Gate", "Andromeda-Decay [Sun Left]", "Decay-Andromeda [Sun Right]")
                    }
                },
                {
                    "Decay-Andromeda [Sun Right]",
                    new List<WarpGate>()
                    {
                        new WarpGate("Eden_BAO_ProgGate2", "Ancient Warp Gate", "Decay-Andromeda [Sun Right]", "Andromeda-Decay [Sun Left]")
                    }
                }
            };
        }

        internal static void OnGSL(ReforgedEdenMKI mod, GlobalStructureList gsl)
        {
            foreach (var kvp in gsl.globalStructures)
            {
                if (LOADED_PLAYFIELDS.Contains(kvp.Key))
                {
                    foreach (var wg in PLAYFIELD_WARPGATES[kvp.Key])
                    {
                        wg.OnGSL(mod, kvp.Value);
                    }
                }
            }
        }

        internal static void OnPlayerInfo(ReforgedEdenMKI mod, PlayerInfo pi)
        {
            if (LOADED_PLAYFIELDS.Contains(pi.playfield))
            {
                foreach (var wg in PLAYFIELD_WARPGATES[pi.playfield])
                {
                    wg.OnPlayerInfo(mod, pi);
                }
            }
        }
        
        internal static void OnPlayfieldLoaded(string playfield)
        {
            if (PLAYFIELD_WARPGATES.ContainsKey(playfield))
                LOADED_PLAYFIELDS.Add(playfield);
        }

        internal static void OnPlayfieldUnloaded(string playfield)
        {
            if (PLAYFIELD_WARPGATES.ContainsKey(playfield) && LOADED_PLAYFIELDS.Contains(playfield))
                LOADED_PLAYFIELDS.Remove(playfield);
        }

        internal static void OnUpdate(ReforgedEdenMKI mod)
        {
            foreach (var pf in LOADED_PLAYFIELDS)
            {
                foreach (var wg in PLAYFIELD_WARPGATES[pf])
                {
                    if (wg.LastUpdated < DateTime.Now.Ticks - 10000000)
                        wg.Update(mod);
                }
            }
        }
    }
}
