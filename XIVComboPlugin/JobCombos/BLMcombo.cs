using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using static XIVComboPlugin.IconReplacer;

namespace XIVComboPlugin.JobCombos
{
    static class BLMcombo
    {
        public static ulong Combo1(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Enochian changes to B4 or F4 depending on stance.
                if (actionID == BLM.Enochian)
                {
                    var gauge = clientState.JobGauges.Get<BLMGauge>();
                    if (gauge.IsEnoActive())
                    {
                        if (gauge.InUmbralIce() && level >= 58)
                            return BLM.Blizzard4;
                        if (level >= 60)
                            return BLM.Fire4;
                    }

                    return BLM.Enochian;
                }
            return iconHook.Original(self, actionID);
        }

        public static ulong Combo2(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Umbral Soul and Transpose
                if (actionID == BLM.Transpose)
                {
                    var gauge = clientState.JobGauges.Get<BLMGauge>();
                    if (gauge.InUmbralIce() && gauge.IsEnoActive() && level >= 76)
                        return BLM.UmbralSoul;
                    return BLM.Transpose;
                }
            return iconHook.Original(self, actionID);
        }
        public static ulong Combo3(byte self, uint actionID, float comboTime, byte level, int lastMove, Hook<OnGetIconDelegate> iconHook, ClientState clientState)
        {
            // Ley Lines and BTL
                if (actionID == BLM.LeyLines)
                {
                    if (SearchBuffArray(737, clientState) && level >= 62)
                        return BLM.BTL;
                    return BLM.LeyLines;
                }
            return iconHook.Original(self, actionID);
        }
    }
}






