using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class WARcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Storm's Path with Storm's Path combo
                if (actionID == WAR.StormsPath)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == WAR.HeavySwing && level >= 4)
                            return WAR.Maim;
                        if (lastMove == WAR.Maim && level >= 26)
                            return WAR.StormsPath;
                    }

                    return 31;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Storm's Eye with Storm's Eye combo
                if (actionID == WAR.StormsEye)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == WAR.HeavySwing && level >= 4)
                            return WAR.Maim;
                        if (lastMove == WAR.Maim && level >= 50)
                            return WAR.StormsEye;
                    }

                    return WAR.HeavySwing;
                }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo3(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Mythril Tempest with Mythril Tempest combo
                if (actionID == WAR.MythrilTempest)
                {
                    if (comboTime > 0)
                        if (lastMove == WAR.Overpower && level >= 40)
                            return WAR.MythrilTempest;
                    return WAR.Overpower;
                }
            return iconHook.Original(self, actionID);
        }
    }
}