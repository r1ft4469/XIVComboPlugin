using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class NINcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Armor Crush with Armor Crush combo
                if (actionID == NIN.ArmorCrush)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == NIN.SpinningEdge && level >= 4)
                            return NIN.GustSlash;
                        if (lastMove == NIN.GustSlash && level >= 54)
                            return NIN.ArmorCrush;
                    }

                    return NIN.SpinningEdge;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Aeolian Edge with Aeolian Edge combo
                if (actionID == NIN.AeolianEdge)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == NIN.SpinningEdge && level >= 4)
                            return NIN.GustSlash;
                        if (lastMove == NIN.GustSlash && level >= 26)
                            return NIN.AeolianEdge;
                    }

                    return NIN.SpinningEdge;
                }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo3(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Hakke Mujinsatsu with Hakke Mujinsatsu combo
                if (actionID == NIN.HakkeM)
                {
                    if (comboTime > 0)
                        if (lastMove == NIN.DeathBlossom && level >= 52)
                            return NIN.HakkeM;
                    return NIN.DeathBlossom;
                }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo4(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            //Replace Dream Within a Dream with Assassinate when Assassinate Ready
                if (actionID == NIN.DWAD)
                {
                    if (SearchBuffArray(1955, clientState)) return NIN.Assassinate;
                    return NIN.DWAD;
                }
            return iconHook.Original(self, actionID);
        }
    }
}








