using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class DRKcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Souleater with Souleater combo chain
                if (actionID == DRK.Souleater)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == DRK.HardSlash && level >= 2)
                            return DRK.SyphonStrike;
                        if (lastMove == DRK.SyphonStrike && level >= 26)
                            return DRK.Souleater;
                    }

                    return DRK.HardSlash;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Stalwart Soul with Stalwart Soul combo chain
                if (actionID == DRK.StalwartSoul)
                {
                    if (comboTime > 0)
                        if (lastMove == DRK.Unleash && level >= 72)
                            return DRK.StalwartSoul;

                    return DRK.Unleash;
                }
            return iconHook.Original(self, actionID);
        }
    }
}
