using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReforgedEdenMKI
{
    internal class WarpGate
    {
        internal static readonly ushort WARPGATE_PLAYERINFO_ID = 61003;
        internal static readonly ushort WARPGATE_GSI = 61006;

        private static readonly long WARPGATE_RETRY_DELAY = 120L * 10000000L;
        private static readonly long WARPGATE_COOLDOWN = 300L * 10000000L;

        private int _entityId;        
        private long _lastWarp;
        private string _name;
        private HashSet<int> _nearbyEntities;
        private HashSet<int> _nearbyPlayers;
        private PVector3 _position;
        private string _prefabName;
        private string _sourcePlayfield;
        private string _targetPlayfield;

        internal int ActivatingEntityId { get; private set; }
        internal long CountdownStarted { get; private set; }
        internal long LastRejected { get; private set; }
        internal long LastUpdated { get; private set; }
        internal WarpGateState State { get; private set; }       
        
        internal WarpGate(string prefabName, string name, string sourcePlayfield, string targetPlayfield)
        {
            _entityId = 0;
            _lastWarp = 0;
            _name = name;
            _nearbyEntities = new HashSet<int>();
            _nearbyPlayers = new HashSet<int>();
            _position = new PVector3(0, 0, 0);
            _prefabName = prefabName;
            _sourcePlayfield = sourcePlayfield;
            _targetPlayfield = targetPlayfield;
            CountdownStarted = 0;
            LastRejected = 0;
            LastUpdated = 0;
            State = WarpGateState.Disabled;
        }

        internal void Update(ReforgedEdenMKI mod)
        {
            LastUpdated = DateTime.Now.Ticks;
            mod.LegacyAPI.Game_Request(CmdId.Request_GlobalStructure_Update, WARPGATE_GSI, new PString(_sourcePlayfield));
            var pids = mod.GameAPI.Application.GetPlayerEntityIds();
            var localPids = new List<int>();
            foreach (var pid in pids)
            {
                var pi = mod.GameAPI.Application.GetPlayerDataFor(pid);
                if (pi != null && pi.Value.PlayfieldName == _sourcePlayfield)
                {
                    mod.LegacyAPI.Game_Request(CmdId.Request_Player_Info, WARPGATE_PLAYERINFO_ID, new Id(pid));
                    localPids.Add(pid);
                }
            }
            if (State == WarpGateState.Cooldown && _lastWarp < DateTime.Now.Ticks - WARPGATE_COOLDOWN)
                State = WarpGateState.Enabled;
            if (State == WarpGateState.WarpCountdownInitiated)
            {
                MessagePlayerList(mod, localPids, "All vessels and players within 500m of the warp gate will be teleported in 90 seconds...");
                State = WarpGateState.WarpCountdown90;
            }
            if (State == WarpGateState.WarpCountdown90 && CountdownStarted < DateTime.Now.Ticks - (30L * 10000000L))
            {
                MessagePlayerList(mod, localPids, "All vessels and players within 500m of the warp gate will be teleported in 60 seconds...");
                State = WarpGateState.WarpCountdown60;
            }
            if (State == WarpGateState.WarpCountdown60 && CountdownStarted < DateTime.Now.Ticks - (60L * 10000000L))
            {
                MessagePlayerList(mod, localPids, "All vessels and players within 500m of the warp gate will be teleported in 30 seconds...");
                State = WarpGateState.WarpCountdown30;
            }
            if (State == WarpGateState.WarpCountdown30 && CountdownStarted < DateTime.Now.Ticks - (90L * 10000000L))
            {
                MessagePlayerList(mod, localPids, "Teleporting...");
                State = WarpGateState.Cooldown;
                WarpManager.WarpEntitiesAsync(mod, _nearbyPlayers.ToList(), _nearbyEntities.ToList(), _targetPlayfield);
            }
        }

        internal void OnGSL(ReforgedEdenMKI mod, List<GlobalStructureInfo> gsl)
        {
            foreach (var gsi in gsl)
            {
                if (_entityId == 0 && gsi.name == _name && gsi.coreType == 4)
                {
                    _entityId = gsi.id;
                    _position = gsi.pos;
                    mod.GameAPI.Log($"WarpGate - Discovered warpgate with id: {_entityId} powered: {gsi.powered}");
                }
                if (_entityId == gsi.id && gsi.powered && State == WarpGateState.Disabled)
                    State = WarpGateState.Enabled;
                if (_entityId != 0 && gsi.id != _entityId && (gsi.type == 3 || gsi.type == 4 || gsi.type == 5) && gsi.pos.Minus(_position).Magnitude() < 500)
                    _nearbyEntities.Add(gsi.id);
                else
                    _nearbyEntities.Remove(gsi.id);
            }
        }

        internal void OnPlayerInfo(ReforgedEdenMKI mod, PlayerInfo pi)
        {
            float distance = pi.pos.Minus(_position).Magnitude();
            if (distance < 500)
                _nearbyPlayers.Add(pi.entityId);
            else
                _nearbyPlayers.Remove(pi.entityId);
            if (State == WarpGateState.Enabled && LastRejected < DateTime.Now.Ticks - WARPGATE_RETRY_DELAY)
            {                                
                if (distance < 50)
                {
                    mod.GameAPI.Log($"WarpGate - Sending activation dialog to player {pi.playerName}");
                    State = WarpGateState.PendingDialogResponse;
                    SendActivationQuery(mod, pi.entityId);
                }
            }
        }

        private void MessagePlayerList(ReforgedEdenMKI mod, List<int> pids, string message)
        {
            foreach (var pid in pids)
            {
                mod.GameAPI.Application.SendChatMessage(new Eleon.MessageData()
                {
                    Channel = Eleon.MsgChannel.SinglePlayer,
                    RecipientEntityId = pid,
                    SenderType = Eleon.SenderType.ServerPrio,
                    Text = message
                });
            }
        }

        private void SendActivationQuery(ReforgedEdenMKI mod, int entityId)
        {
            ActivatingEntityId = entityId;
            mod.GameAPI.Application.ShowDialogBox(entityId, new DialogConfig() 
            {
                BodyText = "The ancient device pulses with gravitational energy. As the gate draws near your vision swims and you feel a strange presence brush against your consciousness.",
                ButtonTexts = new string[]
                {
                    "Give in to the sensation.",
                    "Resist the alien influence."
                },
                ButtonIdxForEnter = 0,
                ButtonIdxForEsc = 1,
                TitleText = "Ancient Warp Gate"
            }, 
            (ix, lid, ic, pid, cv) => 
            { 
                if (ix == 0)
                {
                    CountdownStarted = DateTime.Now.Ticks;
                    if (State == WarpGateState.PendingDialogResponse)
                        State = WarpGateState.WarpCountdownInitiated;
                }    
                else
                {
                    LastRejected = DateTime.Now.Ticks;
                    if (State == WarpGateState.PendingDialogResponse)
                        State = WarpGateState.Enabled;
                }
            }, 0);

        }
    }
}
