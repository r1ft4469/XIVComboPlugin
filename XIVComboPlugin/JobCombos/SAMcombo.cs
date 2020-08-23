using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using System.Runtime.InteropServices;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class SAMcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Yukikaze with Yukikaze combo
                if (actionID == SAM.Yukikaze)
                {
                    if (SearchBuffArray(1233, clientState))
                        return SAM.Yukikaze;
                    if (comboTime > 0)
                        if (lastMove == SAM.Hakaze && level >= 50)
                            return SAM.Yukikaze;
                    return SAM.Hakaze;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Gekko with Gekko combo
                if (actionID == SAM.Gekko)
                {
                    if (SearchBuffArray(1233, clientState))
                        return SAM.Gekko;
                    if (comboTime > 0)
                    {
                        if (lastMove == SAM.Hakaze && level >= 4)
                            return SAM.Jinpu;
                        if (lastMove == SAM.Jinpu && level >= 30)
                            return SAM.Gekko;
                    }

                    return SAM.Hakaze;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo3(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Kasha with Kasha combo
                if (actionID == SAM.Kasha)
                {
                    if (SearchBuffArray(1233, clientState))
                        return SAM.Kasha;
                    if (comboTime > 0)
                    {
                        if (lastMove == SAM.Hakaze && level >= 18)
                            return SAM.Shifu;
                        if (lastMove == SAM.Shifu && level >= 40)
                            return SAM.Kasha;
                    }

                    return SAM.Hakaze;
                }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo4(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Mangetsu with Mangetsu combo
                if (actionID == SAM.Mangetsu)
                {
                    if (SearchBuffArray(1233, clientState))
                        return SAM.Mangetsu;
                    if (comboTime > 0)
                        if (lastMove == SAM.Fuga && level >= 35)
                            return SAM.Mangetsu;
                    return SAM.Fuga;
                }

            return iconHook.Original(self, actionID);
        }
        public static ulong Combo5(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Oka with Oka combo
                if (actionID == SAM.Oka)
                {
                    if (SearchBuffArray(1233, clientState))
                        return SAM.Oka;
                    if (comboTime > 0)
                        if (lastMove == SAM.Fuga && level >= 45)
                            return SAM.Oka;
                    return SAM.Fuga;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo6(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Turn Seigan into Third Eye when not procced
                if (actionID == SAM.Seigan)
                {
                    if (SearchBuffArray(1252, clientState)) return SAM.Seigan;
                    return SAM.ThirdEye;
                }
            return iconHook.Original(self, actionID);
        }
    }
}