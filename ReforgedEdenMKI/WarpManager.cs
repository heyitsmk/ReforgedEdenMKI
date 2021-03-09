using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReforgedEdenMKI
{
    internal static class WarpManager
    {
        internal static readonly ushort WARPMANAGER_LOAD_PF = 61005;

        private static readonly List<WarpTask> _warpTasks = new List<WarpTask>();

        internal static void WarpEntitiesAsync(ReforgedEdenMKI mod, List<int> playerIds, List<int> entityIds, string playfield)
        {
            mod.GameAPI.Log($"WarpManager - Requesting warp to {playfield} for {playerIds.Count + entityIds.Count} entities");
            List<PVector3> previousLocations = new List<PVector3>();
            Random r = new Random();
            float spread = (entityIds.Count < 10) ? 500 : 2000;
            int i = 0;
            while (i < playerIds.Count)
            {
                PVector3 newLocation = new PVector3
                {
                    x = (float)((r.NextDouble() - .5) * 2) * spread,
                    y = (float)((r.NextDouble() - .5) * 2) * spread,
                    z = (float)((r.NextDouble() - .5) * 2) * spread
                };

                if (!previousLocations.Any(loc => loc.Minus(newLocation).Magnitude() < 50))
                {
                    previousLocations.Add(newLocation);
                    _warpTasks.Add(new WarpTask() 
                    { 
                        EntityId = playerIds[i], 
                        IsPlayer = true, 
                        Playfield = playfield, 
                        Position = newLocation, 
                        Rotation = new PVector3() 
                    });
                    i++;
                }
            }

            i = 0;
            while (i < entityIds.Count)
            {
                PVector3 newLocation = new PVector3
                {
                    x = (float)((r.NextDouble() - .5) * 2) * spread,
                    y = (float)((r.NextDouble() - .5) * 2) * spread,
                    z = (float)((r.NextDouble() - .5) * 2) * spread
                };

                if (!previousLocations.Any(loc => loc.Minus(newLocation).Magnitude() < 50))
                {
                    previousLocations.Add(newLocation);
                    _warpTasks.Add(new WarpTask()
                    {
                        EntityId = entityIds[i],
                        IsPlayer = false,
                        Playfield = playfield,
                        Position = newLocation,
                        Rotation = new PVector3()
                    });
                    i++;
                }
            }
            mod.GameAPI.Log($"WarpManager - Enqueued {_warpTasks.Count} warp tasks for {playfield}");

            mod.LegacyAPI.Game_Request(CmdId.Request_Load_Playfield, WARPMANAGER_LOAD_PF, new PlayfieldLoad() { playfield = playfield });
            mod.GameAPI.Log($"WarpManager - Requesting playfield {playfield} load");
        }

        internal static void OnPlayfieldLoaded(ReforgedEdenMKI mod, string playfield)
        {
            mod.GameAPI.Log($"WarpManager - Playfield {playfield} loaded");
            var warpMe = _warpTasks.Where(wt => wt.Playfield == playfield).ToList();
            mod.GameAPI.Log($"WarpManager - There are {warpMe.Count} pending warps for this playfield");
            foreach (var wt in warpMe)
            {
                mod.GameAPI.Log($"WarpManager - Warping entity {wt.EntityId} to playfield {wt.Playfield}");
                wt.Warp(mod.LegacyAPI);
                _warpTasks.Remove(wt);
            }
        }
    }
}
