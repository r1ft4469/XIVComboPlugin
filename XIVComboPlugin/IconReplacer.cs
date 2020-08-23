using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using Serilog;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Structs;
using XIVComboPlugin.JobCombos;
using Dalamud.Plugin;
using XIVComboPlugin.Configuration;

namespace XIVComboPlugin
{
    public class IconReplacer
    {
        public delegate ulong OnCheckIsIconReplaceableDelegate(uint actionID);

        public delegate ulong OnGetIconDelegate(byte param1, uint param2);

        private IntPtr activeBuffArray = IntPtr.Zero;

        private readonly IconReplacerAddressResolver Address;
        private readonly Hook<OnCheckIsIconReplaceableDelegate> checkerHook;
        private readonly ClientState clientState;

        private readonly IntPtr comboTimer;

        private readonly XIVComboConfiguration Configuration;

        private readonly HashSet<uint> customIds;
        private readonly HashSet<uint> vanillaIds;
        private HashSet<uint> noUpdateIcons;
        private HashSet<uint> seenNoUpdate;

        private readonly Hook<OnGetIconDelegate> iconHook;
        private readonly IntPtr lastComboMove;
        private readonly IntPtr playerLevel;
        private uint lastJob = 0;

        private unsafe delegate int* getArray(long* address);

        private bool shutdown;

        public IconReplacer(SigScanner scanner, ClientState clientState, XIVComboConfiguration configuration)
        {
            shutdown = false;
            Configuration = configuration;
            this.clientState = clientState;

            Address = new IconReplacerAddressResolver();
            Address.Setup(scanner);

            comboTimer = scanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 80 7E 21 00", 0x178);
            lastComboMove = comboTimer + 0x4;

            customIds = new HashSet<uint>();
            vanillaIds = new HashSet<uint>();
            noUpdateIcons = new HashSet<uint>();
            seenNoUpdate = new HashSet<uint>();

            PopulateDict();

            Log.Verbose("===== H O T B A R S =====");
            Log.Verbose("IsIconReplaceable address {IsIconReplaceable}", Address.IsIconReplaceable);
            Log.Verbose("GetIcon address {GetIcon}", Address.GetIcon);
            Log.Verbose("ComboTimer address {ComboTimer}", comboTimer);
            Log.Verbose("LastComboMove address {LastComboMove}", lastComboMove);
            Log.Verbose("PlayerLevel address {PlayerLevel}", playerLevel);

            iconHook = new Hook<OnGetIconDelegate>(Address.GetIcon, new OnGetIconDelegate(GetIconDetour), this);
            checkerHook = new Hook<OnCheckIsIconReplaceableDelegate>(Address.IsIconReplaceable,
                new OnCheckIsIconReplaceableDelegate(CheckIsIconReplaceableDetour), this);

            Task.Run(() =>
            {
                BuffTask();
            });
        }

        public void Enable()
        {
            iconHook.Enable();
            checkerHook.Enable();
        }

        public void Dispose()
        {
            shutdown = true;
            iconHook.Dispose();
            checkerHook.Dispose();
        }

        public void AddNoUpdate(uint[] ids)
        {
            foreach (uint id in ids)
            {
                if (!noUpdateIcons.Contains(id))
                    noUpdateIcons.Add(id);
            }
        }

        public void RemoveNoUpdate(uint[] ids)
        {
            foreach (uint id in ids)
            {
                if (noUpdateIcons.Contains(id))
                    noUpdateIcons.Remove(id);
                if (seenNoUpdate.Contains(id))
                    seenNoUpdate.Remove(id);
            }
        }

        private async void BuffTask()
        {
            while (!shutdown)
            {
                await Task.Delay(1000);
            }
        }

        // I hate this function. This is the dumbest function to exist in the game. Just return 1.
        // Determines which abilities are allowed to have their icons updated.
        private ulong CheckIsIconReplaceableDetour(uint actionID)
        {
            if (!noUpdateIcons.Contains(actionID))
            {
                return 1;
            }
            if (!seenNoUpdate.Contains(actionID))
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        ///     Replace an ability with another ability
        ///     actionID is the original ability to be "used"
        ///     Return either actionID (itself) or a new Action table ID as the
        ///     ability to take its place.
        ///     I tend to make the "combo chain" button be the last move in the combo
        ///     For example, Souleater combo on DRK happens by dragging Souleater
        ///     onto your bar and mashing it.
        /// </summary>
        private ulong GetIconDetour(byte self, uint actionID)
        {
            if (clientState.LocalPlayer == null) return iconHook.Original(self, actionID);
            var job = clientState.LocalPlayer.ClassJob.Id;
            if (lastJob != job)
            {
                lastJob = job;
                seenNoUpdate.Clear();
            }
            // TODO: More jobs, level checking for everything.
            if (noUpdateIcons.Contains(actionID) && !seenNoUpdate.Contains(actionID))
            {
                seenNoUpdate.Add(actionID);
                return actionID;
            }
            if (vanillaIds.Contains(actionID)) return iconHook.Original(self, actionID);
            if (!customIds.Contains(actionID)) return actionID;
            if (activeBuffArray == IntPtr.Zero) return iconHook.Original(self, actionID);

            // Don't clutter the spaghetti any worse than it already is.
            var lastMove = Marshal.ReadInt32(lastComboMove);
            var comboTime = Marshal.PtrToStructure<float>(comboTimer);
            var level = clientState.LocalPlayer.Level;

            // DRAGOON
            if (job == DRG.ClassID)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonJumpFeature))
                    DRGcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonBOTDFeature))
                    DRGcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonCoerthanTormentCombo))
                    DRGcombo.Combo3(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonChaosThrustCombo))
                    DRGcombo.Combo4(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonFullThrustCombo))
                    DRGcombo.Combo5(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // DARK KNIGHT
            if (job == DRK.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DarkSouleaterCombo))
                    DRKcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DarkStalwartSoulCombo))
                    DRKcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // PALADIN
            if (job == PLD.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.PaladinGoringBladeCombo))
                    PLDcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.PaladinRoyalAuthorityCombo))
                    PLDcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.PaladinProminenceCombo))
                    PLDcombo.Combo3(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.PaladinRequiescatCombo))
                    PLDcombo.Combo4(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // WARRIOR
            if (job == WAR.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WarriorStormsPathCombo))
                    WARcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WarriorStormsEyeCombo))
                    WARcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WarriorMythrilTempestCombo))
                    WARcombo.Combo3(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // SAMURAI
            if (job == SAM.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiYukikazeCombo))
                    SAMcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiGekkoCombo))
                    SAMcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiKashaCombo))
                    SAMcombo.Combo3(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiMangetsuCombo))
                    SAMcombo.Combo4(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiOkaCombo))
                    SAMcombo.Combo5(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiThirdEyeFeature))
                    SAMcombo.Combo6(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // NINJA
            if (job == NIN.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.NinjaArmorCrushCombo))
                    NINcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.NinjaAeolianEdgeCombo))
                    NINcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.NinjaHakkeMujinsatsuCombo))
                    NINcombo.Combo3(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.NinjaAssassinateFeature))
                    NINcombo.Combo4(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // GUNBREAKER
            if (job == GNB.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerSolidBarrelCombo))
                    GNBcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerGnashingFangCont))
                    GNBcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerGnashingFangCombo))
                    GNBcombo.Combo3(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerDemonSlaughterCombo))
                    GNBcombo.Combo4(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // MACHINIST
            if (job == MCH.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.MachinistMainCombo))
                    MCHcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.MachinistOverheatFeature))
                    MCHcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.MachinistSpreadShotFeature))
                    MCHcombo.Combo3(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // BLACK MAGE
            if (job == BLM.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackEnochianFeature))
                    BLMcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackManaFeature))
                    BLMcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackLeyLines))
                    BLMcombo.Combo3(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // ASTROLOGIAN
            if (job == AST.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.AstrologianCardsOnDrawFeature))
                    ASTcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // SUMMONER
            if (job == SMN.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerDemiCombo))
                    SMNcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerBoPCombo))
                    SMNcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerEDFesterCombo))
                    SMNcombo.Combo3(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerESPainflareCombo))
                    SMNcombo.Combo4(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // SCHOLAR
            if (job == SCH.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ScholarSeraphConsolationFeature))
                    SCHcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ScholarEnergyDrainFeature))
                    SCHcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // DANCER
            if (job == DNC.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerAoeGcdFeature))
                    DNCcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerFanDanceCombo))
                    DNCcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // WHM
            if (job == WHM.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WhiteMageSolaceMiseryFeature))
                    WHMcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WhiteMageRaptureMiseryFeature))
                    WHMcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // BARD
            if (job == BRD.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BardWandererPPFeature))
                    BRDcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BardStraightShotUpgradeFeature))
                    BRDcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // MONK
            if (job == MNK.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.MnkAoECombo))
                    MNKcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            // RED MAGE
            if (job == RDM.ClassId)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageAoECombo))
                    RDMcombo.Combo1(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageMeleeCombo))
                    RDMcombo.Combo2(self, actionID, comboTime, level, lastMove, iconHook, clientState);
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageVerprocCombo))
                    RDMcombo.Combo3(self, actionID, comboTime, level, lastMove, iconHook, clientState);
            }

            return iconHook.Original(self, actionID);
        }

        public static bool SearchTargetBuffArray(short needle, ClientState clientState, float refresh = 0, bool global = false)
        {
            StatusEffect[] a;
            StatusEffect[] allStatusArray = Array.FindAll(clientState.Targets.CurrentTarget.statusEffects,
                            EffectId => EffectId.EffectId == needle);
            StatusEffect[] ownStatusArray = Array.FindAll(allStatusArray,
                OwnerId => OwnerId.OwnerId == clientState.LocalPlayer.ActorId);
            if (global) { a = allStatusArray; } else { a = ownStatusArray; }
            if (clientState.Targets.CurrentTarget.statusEffects == null) return false;
            if (Array.Find(a, Duration => Duration.Duration > refresh).Duration > refresh)
                return true;
            return false;
        }

        public static bool SearchBuffArray(short needle, ClientState clientState, float refresh = 0, bool global = false)
        {
            StatusEffect[] a;
            StatusEffect[] allStatusArray = Array.FindAll(clientState.LocalPlayer.statusEffects,
                            EffectId => EffectId.EffectId == needle);
            StatusEffect[] ownStatusArray = Array.FindAll(allStatusArray,
                OwnerId => OwnerId.OwnerId == clientState.LocalPlayer.ActorId);
            if (global) { a = allStatusArray; } else { a = ownStatusArray; }
            if (a == null) return false;
            if (Array.Find(a, Duration => Duration.Duration > refresh).Duration > refresh)
                return true;
            return false;
        }

        public void PopulateDict()
        {
            customIds.Add(16477);
            customIds.Add(88);
            customIds.Add(84);
            customIds.Add(7491);
            customIds.Add(3632);
            customIds.Add(16468);
            customIds.Add(3538);
            customIds.Add(7483);
            customIds.Add(3539);
            customIds.Add(16457);
            customIds.Add(42);
            customIds.Add(7477);
            customIds.Add(45);
            customIds.Add(16462);
            customIds.Add(7497);
            customIds.Add(7480);
            customIds.Add(7481);
            customIds.Add(7482);
            customIds.Add(7541);
            customIds.Add(7494);
            customIds.Add(7494);
            customIds.Add(8831);
            customIds.Add(7605);
            customIds.Add(9087);
            customIds.Add(7499);
            customIds.Add(7502);
            customIds.Add(7867);
            customIds.Add(7484);
            customIds.Add(7485);
            customIds.Add(3563);
            customIds.Add(2255);
            customIds.Add(16488);
            customIds.Add(16145);
            customIds.Add(16150);
            customIds.Add(16149);
            customIds.Add(7413);
            customIds.Add(2870);
            customIds.Add(3575);
            customIds.Add(149);
            customIds.Add(17055);
            customIds.Add(3582);
            customIds.Add(3581);
            customIds.Add(163);
            customIds.Add(181);
            customIds.Add(3578);
            customIds.Add(16543);
            customIds.Add(167);
            customIds.Add(15994);
            customIds.Add(15993);
            customIds.Add(16007);
            customIds.Add(16008);
            customIds.Add(16531);
            customIds.Add(16534);
            customIds.Add(3559);
            customIds.Add(97);
            customIds.Add(16525);
            customIds.Add(16524);
            customIds.Add(7516);
            customIds.Add(3566);
            customIds.Add(92);
            customIds.Add(3553);
            customIds.Add(2873);
            customIds.Add(3579);
            customIds.Add(17209);
            customIds.Add(7501);
            customIds.Add(21);
            customIds.Add(DNC.Bloodshower);
            customIds.Add(DNC.RisingWindmill);
            customIds.Add(RDM.Verstone);
            customIds.Add(RDM.Verfire);
            customIds.Add(MNK.Rockbreaker);
            customIds.Add(BLM.LeyLines);
            customIds.Add(PLD.Requiescat);
            vanillaIds.Add(0x3e75);
            vanillaIds.Add(0x3e76);
            vanillaIds.Add(0x3e77);
            vanillaIds.Add(0x3e78);
            vanillaIds.Add(0x3e7d);
            vanillaIds.Add(0x3e7e);
            vanillaIds.Add(0x3e86);
            vanillaIds.Add(0x3f10);
            vanillaIds.Add(0x3f25);
            vanillaIds.Add(0x3f1b);
            vanillaIds.Add(0x3f1c);
            vanillaIds.Add(0x3f1d);
            vanillaIds.Add(0x3f1e);
            vanillaIds.Add(0x451f);
            vanillaIds.Add(0x42ff);
            vanillaIds.Add(0x4300);
            vanillaIds.Add(0x49d4);
            vanillaIds.Add(0x49d5);
            vanillaIds.Add(0x49e9);
            vanillaIds.Add(0x49ea);
            vanillaIds.Add(0x49f4);
            vanillaIds.Add(0x49f7);
            vanillaIds.Add(0x49f9);
            vanillaIds.Add(0x4a06);
            vanillaIds.Add(0x4a31);
            vanillaIds.Add(0x4a32);
            vanillaIds.Add(0x4a35);
            vanillaIds.Add(0x4792);
            vanillaIds.Add(0x452f);
            vanillaIds.Add(0x453f);
            vanillaIds.Add(0x454c);
            vanillaIds.Add(0x455c);
            vanillaIds.Add(0x455d);
            vanillaIds.Add(0x4561);
            vanillaIds.Add(0x4565);
            vanillaIds.Add(0x4566);
            vanillaIds.Add(0x45a0);
            vanillaIds.Add(0x45c8);
            vanillaIds.Add(0x45c9);
            vanillaIds.Add(0x45cd);
            vanillaIds.Add(0x4197);
            vanillaIds.Add(0x4199);
            vanillaIds.Add(0x419b);
            vanillaIds.Add(0x419d);
            vanillaIds.Add(0x419f);
            vanillaIds.Add(0x4198);
            vanillaIds.Add(0x419a);
            vanillaIds.Add(0x419c);
            vanillaIds.Add(0x419e);
            vanillaIds.Add(0x41a0);
            vanillaIds.Add(0x41a1);
            vanillaIds.Add(0x41a2);
            vanillaIds.Add(0x41a3);
            vanillaIds.Add(0x417e);
            vanillaIds.Add(0x404f);
            vanillaIds.Add(0x4051);
            vanillaIds.Add(0x4052);
            vanillaIds.Add(0x4055);
            vanillaIds.Add(0x4053);
            vanillaIds.Add(0x4056);
            vanillaIds.Add(0x405e);
            vanillaIds.Add(0x405f);
            vanillaIds.Add(0x4063);
            vanillaIds.Add(0x406f);
            vanillaIds.Add(0x4074);
            vanillaIds.Add(0x4075);
            vanillaIds.Add(0x4076);
            vanillaIds.Add(0x407d);
            vanillaIds.Add(0x407f);
            vanillaIds.Add(0x4083);
            vanillaIds.Add(0x4080);
            vanillaIds.Add(0x4081);
            vanillaIds.Add(0x4082);
            vanillaIds.Add(0x4084);
            vanillaIds.Add(0x408e);
            vanillaIds.Add(0x4091);
            vanillaIds.Add(0x4092);
            vanillaIds.Add(0x4094);
            vanillaIds.Add(0x4095);
            vanillaIds.Add(0x409c);
            vanillaIds.Add(0x409d);
            vanillaIds.Add(0x40aa);
            vanillaIds.Add(0x40ab);
            vanillaIds.Add(0x40ad);
            vanillaIds.Add(0x40ae);
            vanillaIds.Add(0x272b);
            vanillaIds.Add(0x222a);
            vanillaIds.Add(0x222d);
            vanillaIds.Add(0x222e);
            vanillaIds.Add(0x223b);
            vanillaIds.Add(0x2265);
            vanillaIds.Add(0x2267);
            vanillaIds.Add(0x2268);
            vanillaIds.Add(0x2269);
            vanillaIds.Add(0x2274);
            vanillaIds.Add(0x2290);
            vanillaIds.Add(0x2291);
            vanillaIds.Add(0x2292);
            vanillaIds.Add(0x229c);
            vanillaIds.Add(0x229e);
            vanillaIds.Add(0x22a8);
            vanillaIds.Add(0x22b3);
            vanillaIds.Add(0x22b5);
            vanillaIds.Add(0x22b7);
            vanillaIds.Add(0x22d1);
            vanillaIds.Add(0x4575);
            vanillaIds.Add(0x2335);
            vanillaIds.Add(0x1ebb);
            vanillaIds.Add(0x1cdd);
            vanillaIds.Add(0x1cee);
            vanillaIds.Add(0x1cef);
            vanillaIds.Add(0x1cf1);
            vanillaIds.Add(0x1cf3);
            vanillaIds.Add(0x1cf4);
            vanillaIds.Add(0x1cf7);
            vanillaIds.Add(0x1cfc);
            vanillaIds.Add(0x1d17);
            vanillaIds.Add(0x1d00);
            vanillaIds.Add(0x1d01);
            vanillaIds.Add(0x1d05);
            vanillaIds.Add(0x1d07);
            vanillaIds.Add(0x1d0b);
            vanillaIds.Add(0x1d0d);
            vanillaIds.Add(0x1d0f);
            vanillaIds.Add(0x1d12);
            vanillaIds.Add(0x1d13);
            vanillaIds.Add(0x1d4f);
            vanillaIds.Add(0x1d64);
            vanillaIds.Add(0x1d50);
            vanillaIds.Add(0x1d58);
            vanillaIds.Add(0x1d59);
            vanillaIds.Add(0x1d51);
            vanillaIds.Add(0x1d53);
            vanillaIds.Add(0x1d66);
            vanillaIds.Add(0x1d55);
            vanillaIds.Add(0xdda);
            vanillaIds.Add(0xddd);
            vanillaIds.Add(0xdde);
            vanillaIds.Add(0xde3);
            vanillaIds.Add(0xdf0);
            vanillaIds.Add(0xe00);
            vanillaIds.Add(0xe0b);
            vanillaIds.Add(0xe0c);
            vanillaIds.Add(0xe0e);
            vanillaIds.Add(0xe0f);
            vanillaIds.Add(0xe11);
            vanillaIds.Add(0xe18);
            vanillaIds.Add(0xfed);
            vanillaIds.Add(0xff7);
            vanillaIds.Add(0xffb);
            vanillaIds.Add(0xfe9);
            vanillaIds.Add(0xb30);
            vanillaIds.Add(0x12e);
            vanillaIds.Add(0x8d3);
            vanillaIds.Add(0x8d4);
            vanillaIds.Add(0x8d5);
            vanillaIds.Add(0x8d7);
            vanillaIds.Add(0xb32);
            vanillaIds.Add(0xb34);
            vanillaIds.Add(0xb38);
            vanillaIds.Add(0xb3e);
            vanillaIds.Add(0x12d);
            vanillaIds.Add(0x26);
            vanillaIds.Add(0x31);
            vanillaIds.Add(0x33);
            vanillaIds.Add(0x4b);
            vanillaIds.Add(0x62);
            vanillaIds.Add(0x64);
            vanillaIds.Add(0x71);
            vanillaIds.Add(0x77);
            vanillaIds.Add(0x7f);
            vanillaIds.Add(0x79);
            vanillaIds.Add(0x84);
            vanillaIds.Add(0x90);
            vanillaIds.Add(0x99);
            vanillaIds.Add(0xa4);
            vanillaIds.Add(0xb2);
            vanillaIds.Add(0xa8);
            vanillaIds.Add(0xac);
            vanillaIds.Add(0xb8);
            vanillaIds.Add(0xe2);
            vanillaIds.Add(0x10f);
            vanillaIds.Add(0xf3);
            vanillaIds.Add(0x10e);
            vanillaIds.Add(0x110);
            vanillaIds.Add(0x111);
        }

    }
}
