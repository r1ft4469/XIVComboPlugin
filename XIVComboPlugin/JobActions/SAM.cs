using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using System;
using Dalamud.Plugin;
using System.Linq;
using System.Runtime.ExceptionServices;
using ImGuiNET;
using Dalamud.Game.ClientState.Actors;
using System.Runtime.InteropServices;

namespace XIVComboPlugin.JobActions
{
    public static class SAM
    {
        public const uint
            Yukikaze = 7480,
            Hakaze = 7477,
            Gekko = 7481,
            Jinpu = 7478,
            Kasha = 7482,
            Shifu = 7479,
            Mangetsu = 7484,
            Fuga = 7483,
            Oka = 7485,
            Seigan = 7501,
            ThirdEye = 7498,
            // EXTRA IDS Personal Use
            MercifulEyes = 7502,
            Enpi = 7486,
            MidareSetsugekka = 7487,
            TenkaGoken = 7488,
            Higanbana = 7489,
            HissatsuShinten = 7490,
            HissatsuKyuten = 7491,
            HissatsuGyoten = 7492,
            HissatsuYaten = 7493,
            MeikyoShisui = 7499,
            Ikishoten = 16482,
            HissatsuSenei = 16481,
            HissatsuKaiten = 7494;

        public static bool
            ComboInitialized,
            Ikishoten_Cooldown,
            HissatsuSenei_Cooldown,
            MeikyoShisui_Cooldown;

        public static DateTime
            Ikishoten_CooldownTime,
            HissatsuSenei_CooldownTime,
            MeikyoShisui_CooldownTime;

        public static Action
            MercifulEyesAction,
            OkaAction,
            MangetsuAction,
            HissatsuGyotenAction,
            HissatsuYatenAction,
            YukikazeAction;

        public static SAMcombo
            MercifulEyesCombo,
            OkaCombo,
            MangetsuCombo,
            HissatsuGyotenCombo,
            HissatsuYatenCombo,
            YukikazeCombo;


    }

    public class SAMcombo
    {
        public uint[] Seigan_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Seigan_Conditional(clientState, comboTime, lastMove, level),
                SAM.ThirdEye
            };
        }
        public uint[] Yukikaze_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Yukikaze_Conditional(clientState, comboTime, lastMove, level),
                SAM.Hakaze
            };
        }
        public uint[] Oka_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Oka_Conditional(clientState, comboTime, lastMove, level),
                SAM.Fuga
            };
        }
        public uint[] Mangetsu_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Mangetsu_Conditional(clientState, comboTime, lastMove, level),
                SAM.Fuga
            };
        }
        public uint[] Kasha_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Kasha_Conditional(clientState, comboTime, lastMove, level),
                Shifu_Conditional(clientState, comboTime, lastMove, level),
                SAM.Hakaze
            };
        }
        public uint[] Gekko_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                Gekko_Conditional(clientState, comboTime, lastMove, level),
                Jinpu_Conditional(clientState, comboTime, lastMove, level),
                SAM.Hakaze
            };
        }

        private uint Seigan_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1252, clientState))
            {
                if (clientState.JobGauges.Get<SAMGauge>().Kenki >= 15 && level >= 66)
                    return SAM.Seigan;
            }
            return 0;
        }
        private uint Oka_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1233, clientState))
                return SAM.Oka;
            if (comboTime > 0)
            {
                if (lastMove == SAM.Fuga && level >= 35)
                    return SAM.Oka;
            }
            return 0;
        }
        private uint Mangetsu_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1233, clientState))
                return SAM.Mangetsu;
            if (comboTime > 0)
            {
                if (lastMove == SAM.Fuga && level >= 35)
                    return SAM.Mangetsu;
            }
            return 0;
        }
        private uint Kasha_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1233, clientState))
                return SAM.Kasha;
            if (comboTime > 0)
            {
                if (lastMove == SAM.Shifu && level >= 40)
                    return SAM.Kasha;
            }
            return 0;
        }
        private uint Shifu_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (comboTime > 0)
            {
                if (lastMove == SAM.Hakaze && level >= 18)
                    return SAM.Shifu;
            }
            return 0;
        }
        private uint Jinpu_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (comboTime > 0)
            {
                if (lastMove == SAM.Hakaze && level >= 4)
                    return SAM.Jinpu;
            }
            return 0;
        }
        private uint Gekko_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1233, clientState))
                return SAM.Gekko;
            if (comboTime > 0)
            {
                if (lastMove == SAM.Jinpu && level >= 30)
                    return SAM.Gekko;
            }
            return 0;
        }
        private uint Yukikaze_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1233, clientState))
                return SAM.Yukikaze;
            if (comboTime > 0)
            {
                if (lastMove == SAM.Hakaze && level >= 50)
                    return SAM.Yukikaze;
            }
            return 0;
        }


        // EXTRA Combos and Conditionals for Personal Use
        public uint[] Single_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
                return new uint[]
                {
                MeikyoShisui_Conditional(clientState, SAM.YukikazeAction, comboTime, lastMove, level),
                Seigan_Conditional(clientState, comboTime, lastMove, level),
                Ikishoten_Conditional(clientState, SAM.YukikazeAction, comboTime, lastMove, level),
                Senei_Conditional(clientState, SAM.YukikazeAction, comboTime, lastMove, level),
                HissatsuShinten_Conditional(clientState, comboTime, lastMove, level),
                MidareSetsugekka_Conditional(clientState, comboTime, lastMove, level),
                Higanbana_Conditional(clientState, comboTime, lastMove, level),
                Ka_Conditional(clientState, comboTime, lastMove, level),
                Getsu_Conditional(clientState, comboTime, lastMove, level),
                Setsu_Conditional(clientState, comboTime, lastMove, level),
                Kasha_Conditional(clientState, comboTime, lastMove, level),
                Gekko_Conditional(clientState, comboTime, lastMove, level),
                Shifu_Conditional(clientState, comboTime, lastMove, level),
                Jinpu_Conditional(clientState, comboTime, lastMove, level),
                Yukikaze_Conditional(clientState, comboTime, lastMove, level),
                SAM.Hakaze
                };
        }
        public uint[] Aoe_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                MeikyoShisuiAOE_Conditional(clientState, SAM.MangetsuAction, comboTime, lastMove, level),
                Ikishoten_Conditional(clientState, SAM.MangetsuAction, comboTime, lastMove, level),
                HissatsuKyuten_Conditional(clientState, comboTime, lastMove, level),
                TenkaGoken_Conditional(clientState, comboTime, lastMove, level),
                KaAOE_Conditional(clientState, comboTime, lastMove, level),
                GetsuAOE_Conditional(clientState, comboTime, lastMove, level),
                Oka_Conditional(clientState, comboTime, lastMove, level),
                Mangetsu_Conditional(clientState, comboTime, lastMove, level),
                SAM.Fuga
            };
        }
        public uint[] MercifulEyes_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                MercifulEyes_Conditional(clientState, comboTime, lastMove, level),
                SAM.ThirdEye
            };
        }
        public uint[] Disenguage_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                HissatsuYaten_Conditional(clientState, comboTime, lastMove, level),
                SAM.Enpi
            };
        }
        public uint[] Enguage_Combo(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            return new uint[] {
                HissatsuGyoten_Conditional(clientState, comboTime, lastMove, level),
                SAM.Enpi
            };
        }
        private uint HissatsuGyoten_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (clientState.Targets.CurrentTarget != null)
            {
                if (clientState.Targets.CurrentTarget.YalmDistanceX > 5)
                {
                    if (clientState.JobGauges.Get<SAMGauge>().Kenki >= 10 && level >= 54)
                        return SAM.HissatsuGyoten;
                }
            }
            return 0;
        }
        private uint HissatsuYaten_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (clientState.Targets.CurrentTarget != null)
            {
                if (clientState.Targets.CurrentTarget.YalmDistanceX < 5)
                {
                    if (clientState.JobGauges.Get<SAMGauge>().Kenki >= 10 && level >= 56)
                        return SAM.HissatsuYaten;
                }
            }
            return 0;
        }
        private uint MercifulEyes_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (buffArray.SearchPlayer(1252, clientState))
            {
                if (clientState.LocalPlayer.CurrentHp < clientState.LocalPlayer.MaxHp && level >= 58)
                    return SAM.MercifulEyes;
            }
            return 0;
        }
        private uint Senei_Conditional(ClientState clientState, Action action, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (level >= 72)
            {
                if (action.Last() == SAM.HissatsuSenei)
                {
                    SAM.HissatsuSenei_CooldownTime = IconReplacer.currentTime;
                    SAM.HissatsuSenei_Cooldown = true;
                }
                if (SAM.HissatsuSenei_Cooldown == false)
                {
                    if (clientState.JobGauges.Get<SAMGauge>().Kenki >= 50 && action.Current() == SAM.Ikishoten || clientState.JobGauges.Get<SAMGauge>().Kenki >= 50 && action.Current() == SAM.HissatsuSenei || clientState.JobGauges.Get<SAMGauge>().Kenki >= 50 && SAM.YukikazeAction.Current() == SAM.HissatsuShinten)
                    {
                        return SAM.HissatsuSenei;
                    }
                }
                if (SAM.HissatsuSenei_Cooldown == true && IconReplacer.currentTime.Subtract(SAM.HissatsuSenei_CooldownTime).TotalMilliseconds > 120000)
                {
                    SAM.HissatsuSenei_Cooldown = false;
                }
            }
            return 0;
        }
        private uint Ikishoten_Conditional(ClientState clientState, Action action, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (level >= 68)
            {
                if (action.Last() == SAM.Ikishoten)
                {
                    SAM.Ikishoten_CooldownTime = IconReplacer.currentTime;
                    SAM.Ikishoten_Cooldown = true;
                }
                if (SAM.Ikishoten_Cooldown == false)
                {
                    if (clientState.JobGauges.Get<SAMGauge>().Kenki < 50 && action.Last() == SAM.HissatsuKaiten || clientState.JobGauges.Get<SAMGauge>().Kenki < 50 && action.Current() == SAM.Ikishoten)
                        return SAM.Ikishoten;
                }
                if (SAM.Ikishoten_Cooldown == true && IconReplacer.currentTime.Subtract(SAM.Ikishoten_CooldownTime).TotalMilliseconds > 60000)
                {
                    SAM.Ikishoten_Cooldown = false;
                }
            }
            return 0;
        }
        private uint MeikyoShisui_Conditional(ClientState clientState, Action action, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (action.Last() == SAM.MeikyoShisui)
            {
                SAM.MeikyoShisui_CooldownTime = IconReplacer.currentTime;
                SAM.MeikyoShisui_Cooldown = true;
            }
            if (SAM.MeikyoShisui_Cooldown == false)
            {
                if (buffArray.SearchPlayer(1298, clientState, 0, 10) && buffArray.SearchPlayer(1299, clientState, 0, 10) && buffArray.SearchTarget(1228, clientState, 0, 15))
                {
                    if (!buffArray.SearchPlayer(1233, clientState) && action.Current() == SAM.Hakaze || !buffArray.SearchPlayer(1233, clientState) && action.Current() == SAM.MeikyoShisui)

                        return SAM.MeikyoShisui;
                }
            }
            if (SAM.MeikyoShisui_Cooldown == true && IconReplacer.currentTime.Subtract(SAM.MeikyoShisui_CooldownTime).TotalMilliseconds > 55000)
            {
                SAM.MeikyoShisui_Cooldown = false;
            }
            if (buffArray.SearchPlayer(1233, clientState))
            {
                if (!clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.SETSU) && level >= 50)
                    return SAM.Yukikaze;
                if (!clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) && level >= 30)
                    return SAM.Gekko;
                if (!clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA) && level >= 40)
                    return SAM.Kasha;
                if (clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA) && clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) && clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.SETSU) && level >= 30)
                {
                    if (clientState.JobGauges.Get<SAMGauge>().Kenki >= 20 && level >= 52)
                    {
                        if (!buffArray.SearchPlayer(1229, clientState))
                            return SAM.HissatsuKaiten;
                    }
                    return SAM.MidareSetsugekka;
                }
            }
            return 0;
        }
        private uint MeikyoShisuiAOE_Conditional(ClientState clientState, Action action, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (action.Last() == SAM.MeikyoShisui)
            {
                SAM.MeikyoShisui_CooldownTime = IconReplacer.currentTime;
                SAM.MeikyoShisui_Cooldown = true;
            }
            if (SAM.MeikyoShisui_Cooldown == false)
            {
                if (!buffArray.SearchPlayer(1233, clientState) && action.Current() == SAM.Fuga || !buffArray.SearchPlayer(1233, clientState) && action.Current() == SAM.MeikyoShisui)
                    return SAM.MeikyoShisui;
            }
            if (SAM.MeikyoShisui_Cooldown == true && IconReplacer.currentTime.Subtract(SAM.MeikyoShisui_CooldownTime).TotalMilliseconds > 55000)
            {
                SAM.MeikyoShisui_Cooldown = false;
            }
            if (buffArray.SearchPlayer(1233, clientState))
            {
                if (!clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA) && level >= 40)
                    return SAM.Oka;
                if (!clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) && level >= 30)
                    return SAM.Mangetsu;
                if (clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA) && clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) && level >= 30)
                {
                    if (clientState.JobGauges.Get<SAMGauge>().Kenki >= 20 && level >= 52)
                    {
                        if (!buffArray.SearchPlayer(1229, clientState))
                            return SAM.HissatsuKaiten;
                    }
                    return SAM.TenkaGoken;
                }
            }
            return 0;
        }
        private uint TenkaGoken_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA) && clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) && level >= 30)
            {
                if (clientState.JobGauges.Get<SAMGauge>().Kenki >= 20 && level >= 52)
                {
                    if (!buffArray.SearchPlayer(1229, clientState))
                        return SAM.HissatsuKaiten;
                }
                return SAM.TenkaGoken;
            }
            return 0;
        }
        private uint MidareSetsugekka_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA) && clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) && clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.SETSU) && level >= 30)
            {
                if (clientState.JobGauges.Get<SAMGauge>().Kenki >= 20 && level >= 52)
                {
                    if (!buffArray.SearchPlayer(1229, clientState))
                        return SAM.HissatsuKaiten;
                }
                return SAM.MidareSetsugekka;
            }
            return 0;
        }
        private uint KaAOE_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (!clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA) && level >= 35)
            {
                if (comboTime > 0)
                {
                    if (lastMove == SAM.Fuga && level >= 35)
                        return SAM.Oka;
                }
            }
            return 0;
        }
        private uint HissatsuShinten_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (clientState.JobGauges.Get<SAMGauge>().Kenki > 40 && level >= 62)
            {
                return SAM.HissatsuShinten;
            }
            return 0;
        }
        private uint HissatsuKyuten_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (clientState.JobGauges.Get<SAMGauge>().Kenki > 40 && level >= 62)
            {
                return SAM.HissatsuKyuten;
            }
            return 0;
        }
        private uint GetsuAOE_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (!clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) && level >= 35)
            {
                if (comboTime > 0)
                {
                    if (lastMove == SAM.Fuga && level >= 35)
                        return SAM.Mangetsu;
                }
            }
            return 0;
        }
        private uint Ka_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (!clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA) && level >= 40)
            {
                if (!buffArray.SearchPlayer(1298, clientState, 0, 15))
                {
                    return 0;
                }
                else
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == SAM.Hakaze && level >= 18)
                            return SAM.Shifu;
                    }
                }
            }
            return 0;
        }
        private uint Getsu_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (!clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) && level >= 30)
            {
                if (!buffArray.SearchPlayer(1299, clientState, 0, 15))
                {
                    return 0;
                }
                else
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == SAM.Hakaze && level >= 4)
                            return SAM.Jinpu;
                    }
                }
            }
            return 0;
        }
        private uint Higanbana_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            if (comboTime > 0)
            {
                var buffArray = new BuffArray();
                if (buffArray.SearchPlayer(1298, clientState, 0, 10) && buffArray.SearchPlayer(1299, clientState, 0, 10) && !buffArray.SearchTarget(1228, clientState, 0, 15))
                {
                    if (clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA) || clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) || clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.SETSU))
                    {
                        if (clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) && clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA))
                            return 0;
                        if (clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) && clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.SETSU))
                            return 0;
                        if (clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA) && clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.SETSU))
                            return 0;
                        if (clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.KA) && clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.GETSU) && clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.SETSU))
                            return 0;
                        if (clientState.JobGauges.Get<SAMGauge>().Kenki >= 20 && level >= 52)
                        {
                            if (!buffArray.SearchPlayer(1229, clientState))
                                return SAM.HissatsuKaiten;
                        }
                        if (level >= 30)
                            return SAM.Higanbana;
                    }
                }
            }
            return 0;
        }
        private uint Setsu_Conditional(ClientState clientState, float comboTime = 0, int lastMove = 0, int level = 0)
        {
            var buffArray = new BuffArray();
            if (!clientState.JobGauges.Get<SAMGauge>().Sen.HasFlag(Sen.SETSU) && level >= 50)
            {
                if (!buffArray.SearchPlayer(1299, clientState, 0, 15))
                {
                    return 0;
                }
                else
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == SAM.Hakaze && level >= 4)
                            return SAM.Yukikaze;
                    }
                }
            }
            return 0;
        }

        public void IntSkills()
        {
            SAM.YukikazeCombo = new SAMcombo();
            SAM.YukikazeAction = new Action();
            SAM.HissatsuYatenCombo = new SAMcombo();
            SAM.HissatsuYatenAction = new Action();
            SAM.HissatsuGyotenCombo = new SAMcombo();
            SAM.HissatsuGyotenAction = new Action();
            SAM.MangetsuCombo = new SAMcombo();
            SAM.MangetsuAction = new Action();
            SAM.OkaCombo = new SAMcombo();
            SAM.OkaAction = new Action();
            SAM.MercifulEyesCombo = new SAMcombo();
            SAM.MercifulEyesAction = new Action();
        }
    }
}
