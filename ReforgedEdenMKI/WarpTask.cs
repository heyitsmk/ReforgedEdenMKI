using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReforgedEdenMKI
{
    internal class WarpTask
    {
        internal static readonly ushort WARPTASK_PLAYER_CHANGEPF = 61001;
        internal static readonly ushort WARPTASK_ENTITY_CHANGEPF = 61002;

        internal int EntityId { get; set; }
        internal bool IsPlayer { get; set; }
        internal string Playfield { get; set; }
        internal PVector3 Position { get; set; }
        internal PVector3 Rotation { get; set; }

        internal void Warp(ModGameAPI api)
        {
            if (IsPlayer)
            {
                api.Game_Request(
                    CmdId.Request_Player_ChangePlayerfield,
                    WARPTASK_PLAYER_CHANGEPF,
                    new IdPlayfieldPositionRotation(EntityId, Playfield, Position, Rotation));
            }
            else
            {
                api.Game_Request(
                    CmdId.Request_Entity_ChangePlayfield,
                    WARPTASK_ENTITY_CHANGEPF,
                    new IdPlayfieldPositionRotation(EntityId, Playfield, Position, Rotation));
            }
        }
    }
}
