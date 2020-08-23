using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class SMNcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // DWT changes. 
            // Now contains DWT, Deathflare, Summon Bahamut, Enkindle Bahamut, FBT, and Enkindle Phoenix.
            // What a monster of a button.
            /*
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerDwtCombo))
                if (actionID == 3581)
                {
                    var gauge = clientState.JobGauges.Get<SMNGauge>();
                    if (gauge.TimerRemaining > 0)
                    {
                        if (gauge.ReturnSummon > 0)
                        {
                            if (gauge.IsPhoenixReady()) return 16516;
                            return 7429;
                        }

                        if (level >= 60) return 3582;
                    }
                    else
                    {
                        if (gauge.IsBahamutReady()) return 7427;
                        if (gauge.IsPhoenixReady())
                        {
                            if (level == 80) return 16549;
                            return 16513;
                        }

                        return 3581;
                    }
                }
                */
                // Replace Deathflare with demi enkindles
                if (actionID == SMN.Deathflare)
                {
                    var gauge = clientState.JobGauges.Get<SMNGauge>();
                    if (gauge.IsPhoenixReady())
                        return SMN.EnkindlePhoenix;
                    if (gauge.TimerRemaining > 0 && gauge.ReturnSummon != SummonPet.NONE)
                        return SMN.EnkindleBahamut;
                    return SMN.Deathflare;
                }

                //Replace DWT with demi summons
                if (actionID == SMN.DWT)
                {
                    var gauge = clientState.JobGauges.Get<SMNGauge>();
                    if (gauge.IsBahamutReady())
                        return SMN.SummonBahamut;
                    if (gauge.IsPhoenixReady() ||
                        gauge.TimerRemaining > 0 && gauge.ReturnSummon != SummonPet.NONE)
                    {
                        if (level >= 80)
                            return SMN.FBTHigh;
                        return SMN.FBTLow;
                    }
                    return SMN.DWT;
                }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Ruin 1 now upgrades to Brand of Purgatory in addition to Ruin 3 and Fountain of Fire
                if (actionID == SMN.Ruin1 || actionID == SMN.Ruin3)
                {
                    var gauge = clientState.JobGauges.Get<SMNGauge>();
                    if (gauge.TimerRemaining > 0)
                        if (gauge.IsPhoenixReady())
                        {
                            if (SearchBuffArray(1867, clientState))
                                return SMN.BrandOfPurgatory;
                            return SMN.FountainOfFire;
                        }

                    if (level >= 54)
                        return SMN.Ruin3;
                    return SMN.Ruin1;
                }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo3(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Change Fester into Energy Drain
                if (actionID == SMN.Fester)
                {
                    if (!clientState.JobGauges.Get<SMNGauge>().HasAetherflowStacks())
                        return SMN.EnergyDrain;
                    return SMN.Fester;
                }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo4(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            //Change Painflare into Energy Syphon
                if (actionID == SMN.Painflare)
                {
                    if (!clientState.JobGauges.Get<SMNGauge>().HasAetherflowStacks())
                        return SMN.EnergySyphon;
                    if (level >= 52)
                        return SMN.Painflare;
                    return SMN.EnergySyphon;
                }
            return iconHook.Original(self, actionID);
        }
    }
}








