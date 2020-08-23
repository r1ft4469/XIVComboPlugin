using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class MNKcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
                if (actionID == MNK.Rockbreaker)
                {
                    if (SearchBuffArray(110, clientState)) return MNK.Rockbreaker;
                    if (SearchBuffArray(107, clientState)) return MNK.AOTD;
                    if (SearchBuffArray(108, clientState)) return MNK.FourPointFury;
                    if (SearchBuffArray(109, clientState)) return MNK.Rockbreaker;
                    return MNK.AOTD;
                }
            return iconHook.Original(self, actionID);
        }
    }
}