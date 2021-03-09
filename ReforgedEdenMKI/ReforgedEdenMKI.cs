using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eleon;
using Eleon.Modding;

namespace ReforgedEdenMKI
{
    public class ReforgedEdenMKI : IMod, ModInterface
    {
        internal ModGameAPI LegacyAPI { get; private set; }
        internal IModApi GameAPI { get; private set; }

        public void Init(IModApi modApi)
        {
            GameAPI = modApi;

            GameAPI.Log("Reforged Eden MKI - Initialized");

            GameAPI.Application.Update += Application_Update;
            GameAPI.Application.ChatMessageSent += Application_ChatMessageSent;            
        }

        private void Application_ChatMessageSent(MessageData chatMsgData)
        {
            if (chatMsgData.Text.ToLower().Contains("!mods"))
            {
                GameAPI.Application.SendChatMessage(new MessageData()
                {
                    RecipientEntityId = chatMsgData.SenderEntityId,
                    Text = "Reforged Eden MKI - v0.1",
                    Channel = Eleon.MsgChannel.SinglePlayer,
                    SenderType = Eleon.SenderType.System
                });
            }
        }

        private void Application_Update()
        {
            try
            {
                WarpGateManager.OnUpdate(this);
            }
            catch (Exception ex)
            {
                GameAPI.Log($"Exception - {ex.Message}");
            }
        }

        public void Shutdown()
        {
            GameAPI.Log("Shut Down");
        }

        public void Game_Start(ModGameAPI dediAPI)
        {
            LegacyAPI = dediAPI;
        }

        public void Game_Update()
        {
        }

        public void Game_Exit()
        {
        }

        public void Game_Event(CmdId eventId, ushort seqNr, object data)
        {
            switch (eventId)
            {
                case CmdId.Event_GlobalStructure_List:
                    if (seqNr == WarpGate.WARPGATE_GSI)
                        WarpGateManager.OnGSL(this, (GlobalStructureList)data);
                    break;
                case CmdId.Event_Playfield_Loaded:                    
                    WarpGateManager.OnPlayfieldLoaded((data as PlayfieldLoad).playfield);                    
                    WarpManager.OnPlayfieldLoaded(this, (data as PlayfieldLoad).playfield);
                    break;
                case CmdId.Event_Playfield_Unloaded:
                    WarpGateManager.OnPlayfieldUnloaded((data as PlayfieldLoad).playfield);
                    break;
                case CmdId.Event_Player_Info:
                    if (seqNr == WarpGate.WARPGATE_PLAYERINFO_ID)
                        WarpGateManager.OnPlayerInfo(this, (PlayerInfo)data);
                    break;
            }
        }
    }
}