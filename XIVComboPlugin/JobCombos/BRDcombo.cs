using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class BRDcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Wanderer's Minuet with PP when in WM.
                if (actionID == BRD.WanderersMinuet)
                {
                    if (clientState.JobGauges.Get<BRDGauge>().ActiveSong == CurrentSong.WANDERER)
                        return BRD.PitchPerfect;
                    return BRD.WanderersMinuet;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace HS/BS with SS/RA when procced.
                if (actionID == BRD.HeavyShot || actionID == BRD.BurstShot)
                {
                if (SearchBuffArray(122, clientState))
                    {
                        if (level >= 70) return BRD.RefulgentArrow;
                        return BRD.StraightShot;
                    }

                    if (level >= 76) return BRD.BurstShot;
                    return BRD.HeavyShot;
                }
            return iconHook.Original(self, actionID);
        }
    }
}




