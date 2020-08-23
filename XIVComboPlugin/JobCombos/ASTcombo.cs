using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class ASTcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Make cards on the same button as play
                if (actionID == AST.Play)
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
            return iconHook.Original(self, actionID);
        }
    }
}

