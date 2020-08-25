using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using System;
using Dalamud.Plugin;
using System.Linq;
using System.Runtime.ExceptionServices;
using ImGuiNET;

namespace XIVComboPlugin.JobActions
{
    public static class DNC
    {
        public const uint
            Bladeshower = 15994,
            Bloodshower = 15996,
            Windmill = 15993,
            RisingWindmill = 15995,
            FanDance1 = 16007,
            FanDance3 = 16009,
            FanDance2 = 16008;
    }

    public class DNCcombo
    {

        public uint[] Bladeshower_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Bloodshower_Conditional(clientState, comboTime, lastMove, level),
                DNC.Bladeshower
            };
        }
        public uint[] Windmill_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Windmill_Conditional(clientState, comboTime, lastMove, level),
                DNC.Windmill
            };
        }
        public uint[] FanDance2_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                FanDance3_Conditional(clientState, comboTime, lastMove, level),
                DNC.FanDance2
            };
        }
        public uint[] FanDance1_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                FanDance3_Conditional(clientState, comboTime, lastMove, level),
                DNC.FanDance1
            };
        }
        private uint FanDance3_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1820, clientState))
                return DNC.FanDance3;
            return 0;
        }
        private uint Bloodshower_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1817, clientState))
                return DNC.Bloodshower;
            return 0;
        }
        private uint Windmill_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1816, clientState))
                return DNC.RisingWindmill;
            return 0;
        }
    }
}
