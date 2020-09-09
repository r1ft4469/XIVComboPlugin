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
            DangerZone = 16144,
            SonicBreak = 16153,
            NoMercy = 16138,
            BowShock = 16159,
            BurstStrike = 16162;


        public const short
            BuffReadyToRip = 1842,
            BuffReadyToTear = 1843,
            BuffReadyToGouge = 1844;
        public const byte
            LevelContinuation = 70;
        public static bool
            NoMercy_Cooldown,
            SonicBreak_Cooldown,
            DangerZone_Cooldown,
            BowShock_Cooldown,
            ComboInitialized,
            GnashingFang_Cooldown;
            
        public static DateTime
            NoMercy_CooldownTime,
            SonicBreak_CooldownTime,
            DangerZone_CooldownTime,
            BowShock_CooldownTime,
            GnashingFang_CooldownTime;

        public static Action
            DemonSlaughterAction,
            WickedTalonAction,
            SolidBarrelAction;

        public static GNBcombo
            DemonSlaughterlCombo,
            WickedTalonCombo,
            SolidBarrelCombo;
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
        public uint[] WickedTalon_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
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
                NoMercy_Conditional(clientState, GNB.SolidBarrelAction, comboTime, lastMove, level),
                GnashingFang_Conditional(clientState, comboTime, lastMove, level),
                SonicBreak_Conditional(clientState, comboTime, lastMove, level),
                BowShock_Conditional(clientState, comboTime, lastMove, level),
                DangerZone_Conditional(clientState, GNB.SolidBarrelAction, comboTime, lastMove, level),
                WickedTalon_Conditional(clientState, comboTime, lastMove, level),
                BurstStrike_Conditional(clientState, comboTime, lastMove, level),
                SolidBarrel_Conditional(clientState, comboTime, lastMove, level),
                GNB.KeenEdge
            };
        }
        public uint[] AOE_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0, uint actionID = 0)
        {
            return new uint[] {
                NoMercy_Conditional(clientState, GNB.DemonSlaughterAction, comboTime, lastMove, level),
                BowShock_Conditional(clientState, comboTime, lastMove, level),
                BurstStrike_Conditional(clientState, comboTime, lastMove, level),
                DemonSlaughter_Conditional(clientState, comboTime, lastMove, level),
                GNB.DemonSlice
            };
        }

        private uint BowShock_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (level >= 62)
            {
                if (GNB.BowShock_Cooldown == false)
                {
                    if (!buffArray.SearchTarget(1838, clientState))
                    {
                        return GNB.BowShock;
                    }
                    else
                    {
                        GNB.BowShock_CooldownTime = IconReplacer.currentTime;
                        GNB.BowShock_Cooldown = true;
                    }
                }
                if (GNB.BowShock_Cooldown == true && IconReplacer.currentTime.Subtract(GNB.BowShock_CooldownTime).TotalMilliseconds > 58500)
                {
                    GNB.BowShock_Cooldown = false;
                }
            }
            return 0;
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
            }
            return 0;
        }
        private uint SonicBreak_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (level >= 54)
            {
                if (GNB.SonicBreak_Cooldown == false)
                {
                    if (!buffArray.SearchTarget(1837, clientState))
                    {
                        return GNB.SonicBreak;
                    }
                    else
                    {
                        GNB.SonicBreak_CooldownTime = IconReplacer.currentTime;
                        GNB.SonicBreak_Cooldown = true;
                    }
                }
                if (GNB.SonicBreak_Cooldown == true && IconReplacer.currentTime.Subtract(GNB.SonicBreak_CooldownTime).TotalMilliseconds > 58500)
                {
                    GNB.SonicBreak_Cooldown = false;
                }
            }
            return 0;
        }
        private uint DangerZone_Conditional(ClientState clientState, Action action, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (level >= 18)
            {
                PluginLog.Log(action.Last().ToString());
                if (action.Last() == GNB.DangerZone)
                {
                    GNB.DangerZone_CooldownTime = IconReplacer.currentTime;
                    GNB.DangerZone_Cooldown = true;
                }
                if (GNB.DangerZone_Cooldown == false)
                {
                    if (IconReplacer.currentTime.Subtract(action.Time()).TotalMilliseconds < 1000)
                        return GNB.DangerZone;
                }
                if (GNB.DangerZone_Cooldown == true && IconReplacer.currentTime.Subtract(GNB.DangerZone_CooldownTime).TotalMilliseconds > 30000)
                {
                    GNB.DangerZone_Cooldown = false;
                }
            }
            return 0;
        }
        private uint NoMercy_Conditional(ClientState clientState, Action action, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (action.Last() == GNB.NoMercy)
            {
                GNB.NoMercy_CooldownTime = IconReplacer.currentTime;
                GNB.NoMercy_Cooldown = true;
            }
            if (level >= 2)
            {
                if (GNB.NoMercy_Cooldown == false)
                {
                    if (!buffArray.SearchPlayer(1831, clientState))
                    {
                        if (IconReplacer.currentTime.Subtract(action.Time()).TotalMilliseconds < 1000)
                            return GNB.NoMercy;
                    }
                }
                if (GNB.NoMercy_Cooldown == true && IconReplacer.currentTime.Subtract(GNB.NoMercy_CooldownTime).TotalMilliseconds > 60000)
                {
                    GNB.NoMercy_Cooldown = false;
                }
            }
            return 0;
        }

        private uint GnashingFang_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (level >= 60)
            {
                if (GNB.SolidBarrelAction.Last() == GNB.GnashingFang)
                {
                    GNB.GnashingFang_CooldownTime = IconReplacer.currentTime;
                    GNB.GnashingFang_Cooldown = true;
                }
                if (clientState.JobGauges.Get<GNBGauge>().NumAmmo > 0 && GNB.GnashingFang_Cooldown == false)
                {
                    return GNB.GnashingFang;
                }
                if (GNB.GnashingFang_Cooldown == true && IconReplacer.currentTime.Subtract(GNB.GnashingFang_CooldownTime).TotalMilliseconds > 30000)
                {
                    GNB.GnashingFang_Cooldown = false;
                }
            }
            return 0;
        }

        private uint BurstStrike_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
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

        public void IntSkills()
        {
            GNB.SolidBarrelCombo = new GNBcombo();
            GNB.SolidBarrelAction = new Action();
            GNB.DemonSlaughterlCombo = new GNBcombo();
            GNB.DemonSlaughterAction = new Action();
            GNB.WickedTalonCombo = new GNBcombo();
            GNB.WickedTalonAction = new Action();
        }
    }
}
