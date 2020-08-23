using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class RDMcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Veraero/thunder 2 with Impact when Dualcast is active
                if (actionID == RDM.Veraero2)
                {
                    if (SearchBuffArray(167, clientState) || SearchBuffArray(1249, clientState))
                    {
                        if (level >= 66) return RDM.Impact;
                        return RDM.Scatter;
                    }
                    return RDM.Veraero2;
                }

                if (actionID == RDM.Verthunder2)
                {
                    if (SearchBuffArray(167, clientState) || SearchBuffArray(1249, clientState))
                    {
                        if (level >= 66) return RDM.Impact;
                        return RDM.Scatter;
                    }
                    return RDM.Verthunder2;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Replace Redoublement with Redoublement combo, Enchanted if possible.
                if (actionID == RDM.Redoublement)
                {
                    var gauge = clientState.JobGauges.Get<RDMGauge>();
                    if ((lastMove == RDM.Riposte || lastMove == RDM.ERiposte) && level >= 35)
                    {
                        if (gauge.BlackGauge >= 25 && gauge.WhiteGauge >= 25)
                            return RDM.EZwerchhau;
                        return RDM.Zwerchhau;
                    }

                    if (lastMove == RDM.Zwerchhau && level >= 50)
                    {
                        if (gauge.BlackGauge >= 25 && gauge.WhiteGauge >= 25)
                            return RDM.ERedoublement;
                        return RDM.Redoublement;
                    }

                    if (gauge.BlackGauge >= 30 && gauge.WhiteGauge >= 30)
                        return RDM.ERiposte;
                    return RDM.Riposte;
                }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo3(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {

                if (actionID == RDM.Verstone)
                {
                    if (level >= 80 && (lastMove == RDM.Verflare || lastMove == RDM.Verholy)) return RDM.Scorch;
                    if (SearchBuffArray(1235, clientState)) return RDM.Verstone;
                    if (level < 62) return RDM.Jolt;
                    return RDM.Jolt2;
                }
                if (actionID == RDM.Verfire)
                {
                    if (level >= 80 && (lastMove == RDM.Verflare || lastMove == RDM.Verholy)) return RDM.Scorch;
                    if (SearchBuffArray(1234, clientState)) return RDM.Verfire;
                    if (level < 62) return RDM.Jolt;
                    return RDM.Jolt2;
                }
            return iconHook.Original(self, actionID);
        }
    }
}






