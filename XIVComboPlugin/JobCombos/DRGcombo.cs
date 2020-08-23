using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class DRGcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Change Jump/High Jump into Mirage Dive when Dive Ready
            if (actionID == DRG.Jump)
            {
                if (SearchBuffArray(1243, clientState))
                    return DRG.MirageDive;
                if (level >= 74)
                    return DRG.HighJump;
                return DRG.Jump;
            }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Change Blood of the Dragon into Stardiver when in Life of the Dragon
            if (actionID == DRG.BOTD)
            {
                if (level >= 80)
                    if (clientState.JobGauges.Get<DRGGauge>().BOTDState == BOTDState.LOTD)
                        return DRG.Stardiver;
                return DRG.BOTD;

            }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo3(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Coerthan Torment with Coerthan Torment combo chain
            if (actionID == DRG.CTorment)
            {
                if (comboTime > 0)
                {
                    if (lastMove == DRG.DoomSpike && level >= 62)
                        return DRG.SonicThrust;
                    if (lastMove == DRG.SonicThrust && level >= 72)
                        return DRG.CTorment;
                }

                return DRG.DoomSpike;
            }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo4(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Chaos Thrust with the Chaos Thrust combo chain
            if (actionID == DRG.ChaosThrust)
            {
                if (comboTime > 0)
                {
                    if ((lastMove == DRG.TrueThrust || lastMove == DRG.RaidenThrust)
                        && level >= 18)
                        return DRG.Disembowel;
                    if (lastMove == DRG.Disembowel && level >= 50)
                        return DRG.ChaosThrust;
                }
                if (SearchBuffArray(802, clientState) && level >= 56)
                    return DRG.FangAndClaw;
                if (SearchBuffArray(803, clientState) && level >= 58)
                    return DRG.WheelingThrust;
                if (SearchBuffArray(1863, clientState) && level >= 76)
                    return DRG.RaidenThrust;

                return DRG.TrueThrust;
            }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo5(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Full Thrust with the Full Thrust combo chain
            if (actionID == 84)
            {
                if (comboTime > 0)
                {
                    if ((lastMove == DRG.TrueThrust || lastMove == DRG.RaidenThrust)
                        && level >= 4)
                        return DRG.VorpalThrust;
                    if (lastMove == DRG.VorpalThrust && level >= 26)
                        return DRG.FullThrust;
                }
                if (SearchBuffArray(802, clientState) && level >= 56)
                    return DRG.FangAndClaw;
                if (SearchBuffArray(803, clientState) && level >= 58)
                    return DRG.WheelingThrust;
                if (SearchBuffArray(1863, clientState) && level >= 76)
                    return DRG.RaidenThrust;

                return DRG.TrueThrust;
            }
            return iconHook.Original(self, actionID);
        }
    }
}
