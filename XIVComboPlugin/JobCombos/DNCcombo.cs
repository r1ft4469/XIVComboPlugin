using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class DNCcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // AoE GCDs are split into two buttons, because priority matters
            // differently in different single-target moments. Thanks yoship.
            // Replaces each GCD with its procced version.
                if (actionID == DNC.Bloodshower)
                {
                    if (SearchBuffArray(1817, clientState))
                        return DNC.Bloodshower;
                    return DNC.Bladeshower;
                }

                if (actionID == DNC.RisingWindmill)
                {
                    if (SearchBuffArray(1816, clientState))
                        return DNC.RisingWindmill;
                    return DNC.Windmill;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Fan Dance changes into Fan Dance 3 while flourishing.
            if (actionID == DNC.FanDance1)
            {
                if (SearchBuffArray(1820, clientState))
                    return DNC.FanDance3;
                return DNC.FanDance1;
            }

            // Fan Dance 2 changes into Fan Dance 3 while flourishing.
            if (actionID == DNC.FanDance2)
            {
                if (SearchBuffArray(1820, clientState))
                    return DNC.FanDance3;
                return DNC.FanDance2;
            }
            return iconHook.Original(self, actionID);
        }
    }
}