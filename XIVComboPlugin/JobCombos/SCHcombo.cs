using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class SCHcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Change Fey Blessing into Consolation when Seraph is out.
            if (actionID == SCH.FeyBless)
            {
                if (clientState.JobGauges.Get<SCHGauge>().SeraphTimer > 0) return SCH.Consolation;
                return SCH.FeyBless;
            }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Change Energy Drain into Aetherflow when you have no more Aetherflow stacks.
                if (actionID == SCH.EnergyDrain)
                {
                    if (clientState.JobGauges.Get<SCHGauge>().NumAetherflowStacks == 0) return SCH.Aetherflow;
                    return SCH.EnergyDrain;
                }
            return iconHook.Original(self, actionID);
        }
    }
}


