using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using System;
using Dalamud.Plugin;
using System.Linq;
using System.Runtime.ExceptionServices;
using ImGuiNET;

namespace XIVComboPlugin.JobActions
{
    public static class GNB
    {
        public const uint
            SolidBarrel = 16145,
            KeenEdge = 16137,
            BrutalShell = 16139,
            WickedTalon = 16150,
            GnashingFang = 16146,
            SavageClaw = 16147,
            DemonSlaughter = 16149,
            DemonSlice = 16141,
            Continuation = 16155,
            JugularRip = 16156,
            AbdomenTear = 16157,
            EyeGouge = 16158,
            BurstStrike = 16162;


        public const short
            BuffReadyToRip = 1842,
            BuffReadyToTear = 1843,
            BuffReadyToGouge = 1844;
        public const byte
            LevelContinuation = 70;
        public static bool
            GnashingFang_Cooldown;
        public static DateTime GnashingFang_CooldownTime;
    }

    public static class Wi
    {
        
    }

    public class GNBcombo
    {
        public uint[] SolidBarrel_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                SolidBarrel_Conditional(clientState, comboTime, lastMove, level),
                GNB.KeenEdge
            };
        }
        public uint[] WickedTalon_Combo(ClientState clientState, uint actionID, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                WickedTalon_Conditional(clientState, comboTime, lastMove, level),
                GNB.GnashingFang
            };
        }
        public uint[] DemonSlaughter_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                DemonSlaughter_Conditional(clientState, comboTime, lastMove, level),
                GNB.DemonSlice
            };
        }
        public uint[] ST_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0, uint actionID = 0)
        {
            return new uint[] {
                Ammo_Conditional(clientState, comboTime, lastMove, level),
                SolidBarrel_Conditional(clientState, comboTime, lastMove, level),
                GNB.KeenEdge
            };
        }
        public uint[] AOE_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0, uint actionID = 0)
        {
            return new uint[] {
                Ammo_Conditional(clientState, comboTime, lastMove, level),
                SolidBarrel_Conditional(clientState, comboTime, lastMove, level),
                GNB.KeenEdge
            };
        }


        private uint DemonSlaughter_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (comboTime > 0)
                if (lastMove == GNB.DemonSlice && level >= 40)
                    return GNB.DemonSlaughter;
            return 0;
        }
        private uint WickedTalon_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
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

        private uint Ammo_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (level >= 60)
            {
                if (IconReplacer.LastAction == GNB.GnashingFang)
                {
                    GNB.GnashingFang_CooldownTime = DateTime.Now;
                    GNB.GnashingFang_Cooldown = true;
                }
                if (clientState.JobGauges.Get<GNBGauge>().NumAmmo > 0 && GNB.GnashingFang_Cooldown == false)
                {
                    return GNB.GnashingFang;
                }
                if (GNB.GnashingFang_Cooldown == true && DateTime.Now.Subtract(GNB.GnashingFang_CooldownTime).TotalMilliseconds > 30000)
                {
                    GNB.GnashingFang_Cooldown = false;
                }
                var ammoComboState = clientState.JobGauges.Get<GNBGauge>().AmmoComboStepNumber;
                switch (ammoComboState)
                {
                    case 1:
                        return GNB.SavageClaw;
                    case 2:
                        return GNB.WickedTalon;
                }
            }
            if (level >= 30)
            {
                if (clientState.JobGauges.Get<GNBGauge>().NumAmmo > 0)
                {
                    return GNB.BurstStrike;
                }
            }
            return 0;
        }
        private uint SolidBarrel_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (comboTime > 0)
            {
                if (lastMove == GNB.KeenEdge && level >= 4)
                    return GNB.BrutalShell;
                if (lastMove == GNB.BrutalShell && level >= 26)
                    return GNB.SolidBarrel;
            }
            return 0;
        }
    }
}
