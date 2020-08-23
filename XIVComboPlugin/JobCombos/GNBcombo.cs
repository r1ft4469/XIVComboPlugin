using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class GNBcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Solid Barrel with Solid Barrel combo
                if (actionID == GNB.SolidBarrel)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == GNB.KeenEdge && level >= 4)
                            return GNB.BrutalShell;
                        if (lastMove == GNB.BrutalShell && level >= 26)
                            return GNB.SolidBarrel;
                    }

                    return GNB.KeenEdge;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Wicked Talon with Gnashing Fang combo Continuation
                if (actionID == GNB.WickedTalon)
                {
                        if (level >= GNB.LevelContinuation)
                        {
                            if (SearchBuffArray(GNB.BuffReadyToRip, clientState))
                                return GNB.JugularRip;
                            if (SearchBuffArray(GNB.BuffReadyToTear, clientState))
                                return GNB.AbdomenTear;
                            if (SearchBuffArray(GNB.BuffReadyToGouge, clientState))
                                return GNB.EyeGouge;
                        }
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo3(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Wicked Talon with Gnashing Fang combo
            if (actionID == GNB.WickedTalon)
            {
                var ammoComboState = clientState.JobGauges.Get<GNBGauge>().AmmoComboStepNumber;
                switch (ammoComboState)
                {
                    case 1:
                        return GNB.SavageClaw;
                    case 2:
                        return GNB.WickedTalon;
                    default:
                        return GNB.GnashingFang;
                }
            }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo4(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Demon Slaughter with Demon Slaughter combo
                if (actionID == GNB.DemonSlaughter)
                {
                    if (comboTime > 0)
                        if (lastMove == GNB.DemonSlice && level >= 40)
                            return GNB.DemonSlaughter;
                    return GNB.DemonSlice;
                }
            return iconHook.Original(self, actionID);
        }
    }
}






