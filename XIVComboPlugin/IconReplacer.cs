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

        public static DateTime
            currentTime;

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
            currentTime = DateTime.Now;
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

            // Made a Switch
            switch (job)
            {
                // SAMURAI
                case 34:
                    {
                        if (SAM.ComboInitialized == false)
                        {
                            new SAMcombo().IntSkills();
                            SAM.ComboInitialized = true;
                        }
                        // Replaced for Personal Use with Single Target
                        // Replace Yukikaze with Yukikaze combo
                        if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiYukikazeCombo) && actionID == SAM.Yukikaze)
                            return SAM.YukikazeAction.Parse(SAM.YukikazeCombo.Single_Combo(clientState, comboTime, lastMove, level), actionID);
                        // Replace Gekko with Gekko combo
                        if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiGekkoCombo) && actionID == SAM.HissatsuYaten)
                            return SAM.HissatsuYatenAction.Parse(SAM.HissatsuYatenCombo.Disenguage_Combo(clientState, comboTime, lastMove, level), actionID);
                        // Replace Kasha with Kasha combo
                        if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiKashaCombo) && actionID == SAM.HissatsuGyoten)
                            return SAM.HissatsuGyotenAction.Parse(SAM.HissatsuGyotenCombo.Enguage_Combo(clientState, comboTime, lastMove, level), actionID);
                        // Replaced for Personal Use with AOE
                        // Replace Mangetsu with Mangetsu combo
                        if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiMangetsuCombo) && actionID == SAM.Mangetsu)
                            return SAM.MangetsuAction.Parse(SAM.MangetsuCombo.Aoe_Combo(clientState, comboTime, lastMove, level), actionID);
                        // Replace Oka with Oka combo
                        if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiOkaCombo) && actionID == SAM.Oka)
                            return SAM.OkaAction.Parse(SAM.OkaCombo.Oka_Combo(clientState, comboTime, lastMove, level), actionID);
                        // Turn Seigan into Third Eye when not procced
                        if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiThirdEyeFeature) && actionID == SAM.MercifulEyes)
                            return SAM.MercifulEyesAction.Parse(SAM.MercifulEyesCombo.MercifulEyes_Combo(clientState, comboTime, lastMove, level), actionID);
                        return iconHook.Original(self, actionID);
                    }
                // GUNBREAKER WIP
                case 37:
                    {
                        if (GNB.ComboInitialized == false)
                        {
                            new GNBcombo().IntSkills();
                            GNB.ComboInitialized = true;
                        }
                        if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerSolidBarrelCombo) && actionID == GNB.SolidBarrel)
                            return GNB.SolidBarrelAction.Parse(GNB.SolidBarrelCombo.ST_Combo(clientState, comboTime, lastMove, level), actionID);
                        if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerDemonSlaughterCombo) && actionID == GNB.DemonSlaughter)
                            return GNB.DemonSlaughterAction.Parse(GNB.DemonSlaughterlCombo.AOE_Combo(clientState, comboTime, lastMove, level), actionID);
                        if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerGnashingFangCombo) && actionID == GNB.WickedTalon)
                            return GNB.WickedTalonAction.Parse(GNB.DemonSlaughterlCombo.WickedTalon_Combo(clientState, comboTime, lastMove, level), actionID);
                        return iconHook.Original(self, actionID);
                    }
                default:
                    return iconHook.Original(self, actionID);
            }
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

    public class Action
    {
        private uint
            CurrentAction,
            LastAction;

        private DateTime ActionTime;

        public uint Parse(uint[] ActionList, uint actionID)
        {
            foreach (uint a in ActionList)
                if (a != 0)
                {
                    if (CurrentAction != a)
                    {
                        LastAction = CurrentAction;
                        ActionTime = IconReplacer.currentTime;
                        CurrentAction = a;
                    }
                    return a;
                }
            return actionID;
        }
        public uint Last()
        {
            return LastAction;
        }
        public uint Current()
        {
            return CurrentAction;
        }
        public DateTime Time()
        {
            return ActionTime;
        }
    }
}