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
    public static class BRD
    {
        public const uint
            WanderersMinuet = 3559,
            PitchPerfect = 7404,
            HeavyShot = 97,
            BurstShot = 16495,
            StraightShot = 98,
            RefulgentArrow = 7409;
    }

    public class BRDcombo
    {
        public uint[] WanderersMinuet_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                WanderersMinuet_Conditional(clientState, comboTime, lastMove, level),
                BRD.WanderersMinuet
            };
        }

        public uint[] HeavyShot_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                StraightShot_Conditional(clientState, comboTime, lastMove, level),
                BurstShot_Conditional(clientState, comboTime, lastMove, level),
                BRD.HeavyShot
            };
        }

        private uint BurstShot_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (level >= 76) return BRD.BurstShot;
            return 0;
        }
        private uint StraightShot_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(122, clientState))
            {
                if (level >= 70) return BRD.RefulgentArrow;
                return BRD.StraightShot;
            }
            return 0;
        }
        private uint WanderersMinuet_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (clientState.JobGauges.Get<BRDGauge>().ActiveSong == CurrentSong.WANDERER)
                return BRD.PitchPerfect;
            return 0;
        }

    }
}
