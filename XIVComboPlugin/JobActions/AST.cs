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
    public static class AST
    {
        public const uint
            Play = 17055,
            Draw = 3590,
            Balance = 4401,
            Bole = 4404,
            Arrow = 4402,
            Spear = 4403,
            Ewer = 4405,
            Spire = 4406;
    }

    public class ASTcombo
    {
        public uint[] Play_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Play_Conditional(clientState, comboTime, lastMove, level),
            };
        }

        private uint Play_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var gauge = clientState.JobGauges.Get<ASTGauge>();
            switch (gauge.DrawnCard())
            {
                case CardType.BALANCE:
                    return AST.Balance;
                case CardType.BOLE:
                    return AST.Bole;
                case CardType.ARROW:
                    return AST.Arrow;
                case CardType.SPEAR:
                    return AST.Spear;
                case CardType.EWER:
                    return AST.Ewer;
                case CardType.SPIRE:
                    return AST.Spire;
                /*
                case CardType.LORD:
                    return 7444;
                case CardType.LADY:
                    return 7445;
                */
                default:
                    return AST.Draw;
            }
        }
    }
}
