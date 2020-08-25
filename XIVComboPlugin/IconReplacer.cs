using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using Serilog;
using System.Threading.Tasks;
using System.Threading;
using Dalamud.Plugin;
using System.Dynamic;
using System.Text;

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
        private readonly IntPtr playerJob;
        private uint lastJob = 0;

        private readonly IntPtr BuffVTableAddr;
        private float ping;

        private unsafe delegate int* getArray(long* address);

        private bool shutdown;

        public IconReplacer(SigScanner scanner, ClientState clientState, XIVComboConfiguration configuration)
        {
            ping = 0;
            shutdown = false;
            Configuration = configuration;
            this.clientState = clientState;

            Address = new IconReplacerAddressResolver();
            Address.Setup(scanner);

            comboTimer = scanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 80 7E 21 00", 0x178);
            lastComboMove = comboTimer + 0x4;
            /*
            playerLevel = scanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 88 45 EF", 0x4d) + 0x78;
            playerJob = playerLevel - 0xE;
            */
            BuffVTableAddr = scanner.GetStaticAddressFromSig("48 89 05 ?? ?? ?? ?? 88 05 ?? ?? ?? ?? 88 05 ?? ?? ?? ??", 0);

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

        public void AddNoUpdate(uint [] ids)
        {
            foreach (uint id in ids)
            {
                if (!noUpdateIcons.Contains(id))
                    noUpdateIcons.Add(id);
            }
        }

        public void RemoveNoUpdate(uint [] ids)
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
                UpdateBuffAddress();
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
            if (!seenNoUpdate.Contains(actionID)) { 
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
            //var level = Marshal.ReadByte(playerLevel);
            var level = clientState.LocalPlayer.Level;

            // Still spaghetti Need to make a Array for Jobs and Pull Flags based on Job ID

            // ASTROLOGIAN
            if (job == 33)
            {
                // Make cards on the same button as play
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.AstrologianCardsOnDrawFeature))
                    if (actionID == AST.Play)
                    {
                        ASTcombo combo = new ASTcombo();
                        foreach (uint a in combo.Play_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }
            }

            // BLACK MAGE
            if (job == 25)
            {
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackEnochianFeature))
                    if (actionID == BLM.Enochian)
                    {
                        BLMcombo combo = new BLMcombo();
                        foreach (uint a in combo.Enochian_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                // Umbral Soul and Transpose
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackManaFeature))
                    if (actionID == BLM.Transpose)
                    {
                        BLMcombo combo = new BLMcombo();
                        foreach (uint a in combo.Transpose_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                // Ley Lines and BTL
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackLeyLines))
                    if (actionID == BLM.LeyLines)
                    {
                        BLMcombo combo = new BLMcombo();
                        foreach (uint a in combo.LeyLines_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }
            }

            // BARD
            if (job == 23)
            {
                // Replace Wanderer's Minuet with PP when in WM.
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BardWandererPPFeature))
                    if (actionID == BRD.WanderersMinuet)
                    {
                        BRDcombo combo = new BRDcombo();
                        foreach (uint a in combo.WanderersMinuet_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                // Replace HS/BS with SS/RA when procced.
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BardStraightShotUpgradeFeature))
                    if (actionID == BRD.HeavyShot || actionID == BRD.BurstShot)
                    {
                        BRDcombo combo = new BRDcombo();
                        foreach (uint a in combo.HeavyShot_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

            }

            // DANCER
            if (job == 38)
            {
                // AoE GCDs are split into two buttons, because priority matters
                // differently in different single-target moments. Thanks yoship.
                // Replaces each GCD with its procced version.
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerAoeGcdFeature))
                {
                    if (actionID == DNC.Bloodshower)
                    {
                        DNCcombo combo = new DNCcombo();
                        foreach (uint a in combo.Bladeshower_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                    if (actionID == DNC.RisingWindmill)
                    {
                        DNCcombo combo = new DNCcombo();
                        foreach (uint a in combo.Windmill_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }
                }

                // Fan Dance changes into Fan Dance 3 while flourishing.
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerFanDanceCombo))
                {
                    if (actionID == DNC.FanDance1)
                    {
                        DNCcombo combo = new DNCcombo();
                        foreach (uint a in combo.FanDance1_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                    // Fan Dance 2 changes into Fan Dance 3 while flourishing.
                    if (actionID == DNC.FanDance2)
                    {
                        DNCcombo combo = new DNCcombo();
                        foreach (uint a in combo.FanDance2_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }
                }
            }

            // DRAGOON
            if (job == 22)
            {
                // Change Jump/High Jump into Mirage Dive when Dive Ready
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonJumpFeature))
                    if (actionID == DRG.Jump)
                    {
                        DRGcombo combo = new DRGcombo();
                        foreach (uint a in combo.Jump_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                // Change Blood of the Dragon into Stardiver when in Life of the Dragon
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonBOTDFeature))
                    if (actionID == DRG.BOTD)
                    {
                        DRGcombo combo = new DRGcombo();
                        foreach (uint a in combo.BOTD_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }

                    }

                // Replace Coerthan Torment with Coerthan Torment combo chain
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonCoerthanTormentCombo))
                    if (actionID == DRG.CTorment)
                    {
                        DRGcombo combo = new DRGcombo();
                        foreach (uint a in combo.DoomSpike_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }


                // Replace Chaos Thrust with the Chaos Thrust combo chain
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonChaosThrustCombo))
                    if (actionID == DRG.ChaosThrust)
                    {
                        DRGcombo combo = new DRGcombo();
                        foreach (uint a in combo.ChaosThrust_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                // Replace Full Thrust with the Full Thrust combo chain
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonFullThrustCombo))
                    if (actionID == 84)
                    {
                        DRGcombo combo = new DRGcombo();
                        foreach (uint a in combo.FullThrust_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }
            }
            
            //LEFT OFF ON DRK

            // SAMURAI
            if (job == 34)
            {
                // Replaced for Personal Use with Single Target
                // Replace Yukikaze with Yukikaze combo
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiYukikazeCombo))
                    if (actionID == SAM.Yukikaze)
                    {
                        SAMcombo combo = new SAMcombo();
                        foreach (uint a in combo.Single_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                // Replace Gekko with Gekko combo
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiGekkoCombo))
                    if (actionID == SAM.Gekko)
                    {
                        SAMcombo combo = new SAMcombo();
                        foreach (uint a in combo.Gekko_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                // Replace Kasha with Kasha combo
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiKashaCombo))
                    if (actionID == SAM.Kasha)
                    {
                        SAMcombo combo = new SAMcombo();
                        foreach (uint a in combo.Kasha_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                // Replaced for Personal Use with AOE
                // Replace Mangetsu with Mangetsu combo
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiMangetsuCombo))
                    if (actionID == SAM.Mangetsu)
                    {
                        SAMcombo combo = new SAMcombo();
                        foreach (uint a in combo.Aoe_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                // Replace Oka with Oka combo
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiOkaCombo))
                    if (actionID == SAM.Oka)
                    {
                        SAMcombo combo = new SAMcombo();
                        foreach (uint a in combo.Oka_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }

                // Turn Seigan into Third Eye when not procced
                if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiThirdEyeFeature))
                    if (actionID == SAM.Seigan)
                    {
                        SAMcombo combo = new SAMcombo();
                        foreach (uint a in combo.Seigan_Combo(clientState, comboTime, lastMove, level))
                        {
                            if (a != 0)
                                return a;
                        }
                    }
            }

            return iconHook.Original(self, actionID);
        }

        private void UpdateBuffAddress()
        {
            try
            {
                activeBuffArray = FindBuffAddress();
            }
            catch (Exception)
            {
                //Before you're loaded in
                activeBuffArray = IntPtr.Zero;
            }
        }

        private unsafe IntPtr FindBuffAddress()
        {
            var num = Marshal.ReadIntPtr(BuffVTableAddr);
            var step2 = (IntPtr) (Marshal.ReadInt64(num) + 0x280);
            var step3 = Marshal.ReadIntPtr(step2);
            var callback = Marshal.GetDelegateForFunctionPointer<getArray>(step3);
            return (IntPtr) callback((long*) num) + 8;
        }

        private void PopulateDict()
        {
            var dictionary = new ActionDictionary.Ids();
            var vanillaDictionary = dictionary.Vanilla();
            for (var i = 0; i < vanillaDictionary.Length; i++)
            {
                vanillaIds.Add(vanillaDictionary[i]);  
            }
            var customDictionary = dictionary.Custom();
            for (var i = 0; i < customDictionary.Length; i++)
            {
                customIds.Add(customDictionary[i]);
            }
        }
    }
}
