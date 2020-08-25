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
    public static class BLM
    {
        public const uint
            Enochian = 3575,
            Blizzard4 = 3576,
            Fire4 = 3577,
            Transpose = 149,
            UmbralSoul = 16506,
            LeyLines = 3573,
            BTL = 7419;
    }

    public class BLMcombo
    {
        public uint[] Enochian_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Enochian_Conditional(clientState, comboTime, lastMove, level),
                BLM.Enochian
            };
        }
        public uint[] Transpose_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Transpose_Conditional(clientState, comboTime, lastMove, level),
                BLM.Transpose
            };
        }
        public uint[] LeyLines_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                LeyLines_Conditional(clientState, comboTime, lastMove, level),
                BLM.LeyLines
            };
        }
        private uint LeyLines_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(737, clientState) && level >= 62)
                return BLM.BTL;
            return 0;
        }
        private uint Transpose_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var gauge = clientState.JobGauges.Get<BLMGauge>();
            if (gauge.InUmbralIce() && gauge.IsEnoActive() && level >= 76)
                return BLM.UmbralSoul;
            return 0;
        }
        private uint Enochian_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var gauge = clientState.JobGauges.Get<BLMGauge>();
            if (gauge.IsEnoActive())
            {
                if (gauge.InUmbralIce() && level >= 58)
                    return BLM.Blizzard4;
                if (level >= 60)
                    return BLM.Fire4;
            }
            return 0;
        }

    }
}
