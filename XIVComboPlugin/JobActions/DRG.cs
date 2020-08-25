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
    public static class DRG
    {
        public const uint
            Jump = 92,
            HighJump = 16478,
            MirageDive = 7399,
            BOTD = 3553,
            Stardiver = 16480,
            CTorment = 16477,
            DoomSpike = 86,
            SonicThrust = 7397,
            ChaosThrust = 88,
            RaidenThrust = 16479,
            TrueThrust = 75,
            Disembowel = 87,
            FangAndClaw = 3554,
            WheelingThrust = 3556,
            FullThrust = 84,
            VorpalThrust = 78;
    }

    public class DRGcombo
    {
        public uint[] Jump_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                MirageDive_Conditional(clientState, comboTime, lastMove, level),
                HighJump_Conditional(clientState, comboTime, lastMove, level),
                DRG.Jump
            };
        }
        public uint[] BOTD_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Stardiver_Conditional(clientState, comboTime, lastMove, level),
                DRG.BOTD
            };
        }
        public uint[] DoomSpike_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                SonicThrust_Conditional(clientState, comboTime, lastMove, level),
                CTorment_Conditional(clientState, comboTime, lastMove, level),
                DRG.DoomSpike
            };
        }
        public uint[] ChaosThrust_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Disembowel_Conditional(clientState, comboTime, lastMove, level),
                ChaosThrust_Conditional(clientState, comboTime, lastMove, level),
                FangAndClaw_Conditional(clientState, comboTime, lastMove, level),
                WheelingThrust_Conditional(clientState, comboTime, lastMove, level),
                RaidenThrust_Conditional(clientState, comboTime, lastMove, level),
                DRG.TrueThrust
            };
        }
        public uint[] FullThrust_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                VorpalThrust_Conditional(clientState, comboTime, lastMove, level),
                FullThrust_Conditional(clientState, comboTime, lastMove, level),
                FangAndClaw_Conditional(clientState, comboTime, lastMove, level),
                WheelingThrust_Conditional(clientState, comboTime, lastMove, level),
                RaidenThrust_Conditional(clientState, comboTime, lastMove, level),
                DRG.TrueThrust
            };
        }
        private uint VorpalThrust_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (comboTime > 0)
            {
                if ((lastMove == DRG.TrueThrust || lastMove == DRG.RaidenThrust) && level >= 4)
                    return DRG.VorpalThrust;
            }
            return 0;
        }
        private uint FullThrust_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (comboTime > 0)
            {
                if (lastMove == DRG.VorpalThrust && level >= 26)
                    return DRG.FullThrust;
            }
            return 0;
        }
        private uint Disembowel_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (comboTime > 0)
            {
                if ((lastMove == DRG.TrueThrust || lastMove == DRG.RaidenThrust) && level >= 18)
                    return DRG.Disembowel;
            }
            return 0;
        }
        private uint ChaosThrust_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (comboTime > 0)
            {
                if (lastMove == DRG.Disembowel && level >= 50)
                    return DRG.ChaosThrust;
            }
            return 0;
        }
        private uint FangAndClaw_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(802, clientState) && level >= 56)
                return DRG.FangAndClaw;
            return 0;
        }
        private uint WheelingThrust_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(803, clientState) && level >= 58)
                return DRG.WheelingThrust;
            return 0;
        }
        private uint RaidenThrust_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1863, clientState) && level >= 76)
                return DRG.RaidenThrust;
            return 0;
        }
        private uint CTorment_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (lastMove == DRG.SonicThrust && level >= 72)
                return DRG.CTorment;
            return 0;
        }
        private uint SonicThrust_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (lastMove == DRG.DoomSpike && level >= 62)
                return DRG.SonicThrust;
            return 0;
        }
        private uint Stardiver_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (level >= 80)
                if (clientState.JobGauges.Get<DRGGauge>().BOTDState == BOTDState.LOTD)
                    return DRG.Stardiver;
            return 0;
        }
        private uint HighJump_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (level >= 74)
                return DRG.MirageDive;
            return 0;
        }
        private uint MirageDive_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1243, clientState))
                return DRG.MirageDive;
            return 0;
        }
    }
}
