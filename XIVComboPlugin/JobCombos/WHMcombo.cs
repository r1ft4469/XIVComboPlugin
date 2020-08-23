using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class WHMcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Solace with Misery when full blood lily
                if (actionID == WHM.Solace)
                {
                    if (clientState.JobGauges.Get<WHMGauge>().NumBloodLily == 3)
                        return WHM.Misery;
                    return WHM.Solace;
                }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Solace with Misery when full blood lily
                if (actionID == WHM.Rapture)
                {
                    if (clientState.JobGauges.Get<WHMGauge>().NumBloodLily == 3)
                        return WHM.Misery;
                    return WHM.Rapture;
                }

            return iconHook.Original(self, actionID);
        }
    }
}



