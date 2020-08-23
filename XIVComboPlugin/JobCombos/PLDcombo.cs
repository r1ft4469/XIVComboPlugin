using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class PLDcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Goring Blade with Goring Blade combo
            if (actionID == PLD.GoringBlade)
            {
                if (comboTime > 0)
                {
                    if (lastMove == PLD.FastBlade && level >= 4)
                        return PLD.RiotBlade;
                    if (lastMove == PLD.RiotBlade && level >= 54)
                        return PLD.GoringBlade;
                }

                return PLD.FastBlade;
            }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Royal Authority with Royal Authority combo
            if (actionID == PLD.RoyalAuthority || actionID == PLD.RageOfHalone)
            {
                if (comboTime > 0)
                {
                    if (lastMove == PLD.FastBlade && level >= 4)
                        return PLD.RiotBlade;
                    if (lastMove == PLD.RiotBlade)
                    {
                        if (level >= 60)
                            return PLD.RoyalAuthority;
                        if (level >= 26)
                            return PLD.RageOfHalone;
                    }
                }

                return PLD.FastBlade;
            }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo3(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Prominence with Prominence combo
            if (actionID == PLD.Prominence)
            {
                if (comboTime > 0)
                    if (lastMove == PLD.TotalEclipse && level >= 40)
                        return PLD.Prominence;

                return PLD.TotalEclipse;
            }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo4(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Requiescat with Confiteor when under the effect of Requiescat
            if (actionID == PLD.Requiescat)
            {
                if (SearchBuffArray(1368, clientState) && level >= 80)
                    return PLD.Confiteor;
                return PLD.Requiescat;
            }
            return iconHook.Original(self, actionID);
        }
    }
}
