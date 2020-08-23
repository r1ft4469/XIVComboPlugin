using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class MCHcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
         // Replace Clean Shot with Heated Clean Shot combo
         // Or with Heat Blast when overheated.
         // For some reason the shots use their unheated IDs as combo moves
                if (actionID == MCH.CleanShot || actionID == MCH.HeatedCleanShot)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == MCH.SplitShot)
                        {
                            if (level >= 60)
                                return MCH.HeatedSlugshot;
                            if (level >= 2)
                                return MCH.SlugShot;
                        }

                        if (lastMove == MCH.SlugShot)
                        {
                            if (level >= 64)
                                return MCH.HeatedCleanShot;
                            if (level >= 26)
                                return MCH.CleanShot;
                        }
                    }

                    if (level >= 54)
                        return MCH.HeatedSplitShot;
                    return MCH.SplitShot;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Hypercharge with Heat Blast when overheated
                if (actionID == MCH.Hypercharge)
                {
                    var gauge = clientState.JobGauges.Get<MCHGauge>();
                    if (gauge.IsOverheated() && level >= 35)
                        return MCH.HeatBlast;
                    return MCH.Hypercharge;
                }

            return iconHook.Original(self, actionID);
        }
        public static ulong Combo3(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Spread Shot with Auto Crossbow when overheated.
                if (actionID == MCH.SpreadShot)
                {
                    if (clientState.JobGauges.Get<MCHGauge>().IsOverheated() && level >= 52)
                        return MCH.AutoCrossbow;
                    return MCH.SpreadShot;
                }
            return iconHook.Original(self, actionID);
        }
    }
}







