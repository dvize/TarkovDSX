﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using UnityEngine;

//pragma 
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0007 // Use implicit type

namespace DSX
{
    public class DSXComponent : MonoBehaviour
    {
        private static GameWorld gameWorld;
        private static DualSenseConnection dualSenseConnection;
        internal static Player player;

        private const int maxFireRate = 1000;      // 1000 rounds per minute
        private const int ControllerIndex = 0;
        private float lastSendTime;
        private float sendInterval = 0.03f;  //limit for how often to send udp packets

        internal static Instruction leftTriggerUpdate = new Instruction();
        internal static Instruction rightTriggerUpdate = new Instruction();
        private Instruction rgb = new Instruction();
        private Instruction micLED = new Instruction();
        private Instruction playerLED = new Instruction();
        private Instruction playerLEDNewRevision = new Instruction();
        internal static Instruction triggerThresholdLeft = new Instruction();
        internal static Instruction triggerThresholdRight = new Instruction();
        private Packet leftTrigger = new Packet();
        private Packet rightTrigger = new Packet();
        private Packet otherControllerInstructions = new Packet();

        public static ConfigFile config = new ConfigFile();


        public static ManualLogSource Logger
        {
            get; private set;
        }

        public DSXComponent()
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(DSXComponent));
            }
        }
        public static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.GetOrAddComponent<DSXComponent>();

                player = gameWorld.MainPlayer;

                Logger.LogDebug("TarkovDSX Enabled");
            }
        }

        private void Start()
        {
            StartDSXConnection();
            SetupInitialControls();
            AttachEvents();
            LoadJsonConfig();
            Logger.LogInfo("TarkovDSX: Loaded all start events with no issue");
        }

        private void LoadJsonConfig()
        {
            //grab bepinex plugin path and the dvize.DSX folder
            string path = Path.Combine(BepInEx.Paths.PluginPath, "dvize.DSX", "dvize.DSX.config.json");

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                config = JsonConvert.DeserializeObject<ConfigFile>(json);
            }
            else
            {
                Logger.LogError("Config file not found at path: " + path);
            }

            Logger.LogDebug("TarkovDSX: Deserialized JSON file into ConfigFile");
        }

        private void StartDSXConnection()
        {
            Logger.LogDebug("TarkovDSX: Starting DSX Connection");
            try
            {
                dualSenseConnection = new DualSenseConnection();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }
        private void SetupInitialControls()
        {
            leftTriggerUpdate = Instruction.Normal(Trigger.Left);
            rightTriggerUpdate = Instruction.Normal(Trigger.Right);
            rgb = Instruction.RGBUpdate(0, 0, 0);
            micLED = Instruction.MicLED(MicLEDMode.Off);
            playerLED = Instruction.PlayerLED(false, false, false, false, false);
            playerLEDNewRevision = Instruction.PlayerLEDNewRevision(PlayerLEDNewRevision.AllOff);
            triggerThresholdLeft = Instruction.TriggerThreshold(Trigger.Left, 0);
            triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 0);

            //add { rgb, playerLED} to the othercontrollerinstructions packet
            leftTrigger.instructions = new Instruction[] { leftTriggerUpdate, triggerThresholdLeft };
            rightTrigger.instructions = new Instruction[] { rightTriggerUpdate, triggerThresholdRight };
            otherControllerInstructions.instructions = new Instruction[] { micLED, playerLEDNewRevision, rgb, playerLED };

            config = new ConfigFile();
        }
        private void Update()
        {
            if (dualSenseConnection != null)
            {
                if (Time.time - lastSendTime >= sendInterval)
                {
                    // Update the last send time
                    lastSendTime = Time.time;

                    // Assign instructions and send via UDP
                    leftTrigger.instructions = new Instruction[] { leftTriggerUpdate, triggerThresholdLeft };
                    rightTrigger.instructions = new Instruction[] { rightTriggerUpdate, triggerThresholdRight };
                    otherControllerInstructions.instructions = new Instruction[] { micLED, playerLEDNewRevision, rgb, playerLED };

                    dualSenseConnection.Send(leftTrigger);
                    dualSenseConnection.Send(rightTrigger);
                    dualSenseConnection.Send(otherControllerInstructions);
                }
            }

        }
        private void AttachEvents()
        {
            // if laying down do lean left and right with controller tilt
            // if weapon jammed, shake controller to clear?

            player.OnDamageReceived += Player_OnDamageReceived;             //use this for the damage LED indicator on controller
            player.HandsChangedEvent += Player_HandsChangedEvent;           //use this to detect when weapon has changed to see the type and firemode

        }

        private void Player_HandsChangedEvent(IHandsController controller)
        {
            //Logger.LogDebug("TarkovDSX: Hands Changed");

            //check to see the weapon type if its a gun or grenade or melee
            if (player.IsWeaponOrKnifeInHands)
            {
                bool isBallisticWeapon;

                if (controller.Item is KnifeClass)
                {
                    isBallisticWeapon = false;
                }
                else
                {
                    //Logger.LogDebug("TarkovDSX: Shooting Weapon in hands");
                    isBallisticWeapon = true;
                }

                if (isBallisticWeapon)
                {
                    Weapon weapon = controller.Item as Weapon;
                    Logger.LogDebug("Weapon.firerate is: " + weapon.FireRate);
                    Logger.LogDebug("TarkovDSX: Weapon FireMode is " + weapon.SelectedFireMode.ToString().ToLower());

                    checkEmptyChamber(weapon);
                    checkMalfunction(weapon);
                    changeTriggerFromWeaponType(weapon);

                    //set aim in with scope for whatever side trigger is set in config.other
                    foreach (var scope in config.other)
                    {
                        if (scope.name.ToLower() == "scope")
                        {
                            foreach (var fireMode in scope.fireModes)
                            {
                                if (fireMode.mode == weapon.SelectedFireMode.ToString().ToLower())
                                {
                                    if (fireMode.trigger.ToLower() == "left")
                                    {
                                        triggerThresholdLeft = ReturnScopeThresholdInstruction(weapon);
                                        leftTriggerUpdate = ReturnScopeInstruction(weapon);
                                    }
                                    else
                                    {
                                        triggerThresholdRight = ReturnScopeThresholdInstruction(weapon);
                                        rightTriggerUpdate = ReturnScopeInstruction(weapon);
                                    }
                                }
                            }
                        }
                    }
                }

                else if (!isBallisticWeapon)
                {
                    KnifeClass weapon = controller.Item as KnifeClass;
                    //Logger.LogDebug("TarkovDSX: Melee Weapon in hands: " + weapon.LocalizedName());
                    if (weapon.Weight <= 0.4)
                    {
                        leftTriggerUpdate = Instruction.Resistance(Trigger.Left, 0, 3);
                        rightTriggerUpdate = Instruction.Resistance(Trigger.Right, 0, 3);
                    }
                    else if (weapon.Weight <= 1)
                    {
                        leftTriggerUpdate = Instruction.Resistance(Trigger.Left, 0, 6);
                        rightTriggerUpdate = Instruction.Resistance(Trigger.Right, 0, 6);
                    }
                    else
                    {
                        leftTriggerUpdate = Instruction.Resistance(Trigger.Left, 0, 9);
                        rightTriggerUpdate = Instruction.Resistance(Trigger.Right, 0, 9);
                    }
                }

            }
        }

        private async void Player_OnDamageReceived(float damage, EBodyPart part, EDamageType type, float absorbed, MaterialType special)
        {
            //Logger.LogDebug("TarkovDSX: Damage Received");
            //flashing hit indicator

            // Create the first RGB update
            Packet tempupdate = new Packet();
            tempupdate.instructions = new Instruction[] { Instruction.RGBUpdate(255, 0, 0) };

            dualSenseConnection.Send(tempupdate);

            // Wait for a short duration
            await Task.Delay(20);

            // Create the second RGB update
            tempupdate.instructions[0] = Instruction.RGBUpdate(0, 0, 0);
            dualSenseConnection.Send(tempupdate);
        }

        internal static void checkMalfunction(Weapon weapon)
        {
            //check if malfunction
            if (weapon.MalfState.State != Weapon.EMalfunctionState.None)
            {
                Logger.LogDebug("TarkovDSX: FirearmController OnAddAmmoInChamber: Malfunction still there");

                var side = DSXComponent.readConfigTriggerSide(weapon);

                if (side == Trigger.Left)
                {
                    DSXComponent.triggerThresholdLeft = Instruction.TriggerThreshold(Trigger.Left, 50);
                    DSXComponent.leftTriggerUpdate = Instruction.VerySoft(Trigger.Left);
                }
                else
                {
                    DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 50);
                    DSXComponent.rightTriggerUpdate = Instruction.VerySoft(Trigger.Right);
                }

                return;
            }
        }

        internal static void checkEmptyChamber(Weapon weapon)
        {

            if (weapon.GetMagazineSlot() != null && !(weapon.ChamberAmmoCount == 0 && weapon.GetCurrentMagazineCount() == 0))
            {
                return;
            }

            // Early return if weapon doesn't have a magazine slot but the chamber isn't empty
            if (weapon.GetMagazineSlot() == null && weapon.ChamberAmmoCount != 0)
            {
                return;
            }

            Logger.LogDebug("TarkovDSX: FirearmController OnAddAmmoInChamber: Empty Chamber Trigger Setting");
            var side = readConfigTriggerSide(weapon);
            var triggerUpdateInstruction = Instruction.VerySoft(side);
            var triggerThresholdInstruction = Instruction.TriggerThreshold(side, 50);

            if (side == Trigger.Left)
            {
                triggerThresholdLeft = triggerThresholdInstruction;
                leftTriggerUpdate = triggerUpdateInstruction;
            }
            else
            {
                triggerThresholdRight = triggerThresholdInstruction;
                rightTriggerUpdate = triggerUpdateInstruction;
            }

        }


        #region ToolMethods
        //use this method when switching weapons.
        internal static void changeTriggerFromWeaponType(Weapon weapon)
        {
            //Logger.LogDebug("TarkovDSX: " + weapon.LocalizedName());

            var side = readConfigTriggerSide(weapon);

            if (side == Trigger.Left)
            {
                triggerThresholdLeft = ReturnThresholdInstruction(weapon);
                leftTriggerUpdate = ReturnTriggerInstruction(weapon);
            }
            else
            {
                triggerThresholdRight = ReturnThresholdInstruction(weapon);
                rightTriggerUpdate = ReturnTriggerInstruction(weapon);
            }

        }

        //use this overloaded method only when changing to a future firemode
        internal static void changeTriggerFromWeaponType(Weapon weapon, Weapon.EFireMode firemode)
        {
            var side = readConfigTriggerSide(weapon);

            if (side == Trigger.Left)
            {
                triggerThresholdLeft = ReturnThresholdInstruction(weapon);
                leftTriggerUpdate = ReturnTriggerInstruction(weapon, firemode);
            }
            else
            {
                triggerThresholdRight = ReturnThresholdInstruction(weapon);
                rightTriggerUpdate = ReturnTriggerInstruction(weapon, firemode);
            }
        }

        internal static Trigger readConfigTriggerSide(Weapon weapon)
        {
            //loop through the config to find the weapon and firemode then return the firemode.trigger
            foreach (WeaponConfig weaponConfig in config.weapons)
            {
                if (weaponConfig.name == weapon.WeapClass.ToLower())
                {
                    foreach (FireModeConfig fireMode in weaponConfig.fireModes)
                    {
                        if (fireMode.mode == weapon.SelectedFireMode.ToString().ToLower())
                        {
                            if (fireMode.trigger.ToLower() == "left")
                            {
                                return Trigger.Left;
                            }
                            else if (fireMode.trigger.ToLower() == "right")
                            {
                                return Trigger.Right;
                            }
                        }
                    }
                    break;
                }
            }

            Logger.LogDebug("TarkovDSX: (ReadConfigTriggerSide) Could not find weapon/firemode in config, defaulting to right trigger");
            Logger.LogDebug("TarkovDSX: The weapon is: " + weapon.WeapClass.ToLower() + " and the firemode is: " + weapon.SelectedFireMode.ToString().ToLower());
            return Trigger.Right;
        }
        internal static int CalculateFrequency(int fireRate)
        {
            //doesn't seem like a full auto fire above 15 frequency
            return (int)((15.0 / maxFireRate) * fireRate);
        }
        internal static Trigger ConvertToTrigger(string trigger)
        {
            if (trigger.ToLower() == "left")
            {
                return Trigger.Left;
            }
            else
            {
                return Trigger.Right;
            }
        }

        //use this method when switching weapons.
        internal static Instruction ReturnTriggerInstruction(Weapon weapon)
        {
            //look at config[0] to loop through the weaponconfig

            foreach (WeaponConfig weaponConfig in config.weapons)
            {
                if (weaponConfig.name == weapon.WeapClass.ToLower())
                {
                    //now loop to find the firemode
                    foreach (FireModeConfig fireMode in weaponConfig.fireModes)
                    {
                        if (fireMode.mode == weapon.SelectedFireMode.ToString().ToLower())
                        {
                            switch (fireMode.instructionType.ToLower())
                            {
                                case "normal":
                                    return Instruction.Normal(ConvertToTrigger(fireMode.trigger));
                                case "gamecube":
                                    return Instruction.GameCube(ConvertToTrigger(fireMode.trigger));
                                case "verysoft":
                                    return Instruction.VerySoft(ConvertToTrigger(fireMode.trigger));
                                case "soft":
                                    return Instruction.Soft(ConvertToTrigger(fireMode.trigger));
                                case "hard":
                                    return Instruction.Hard(ConvertToTrigger(fireMode.trigger));
                                case "veryhard":
                                    return Instruction.VeryHard(ConvertToTrigger(fireMode.trigger));
                                case "hardest":
                                    return Instruction.Hardest(ConvertToTrigger(fireMode.trigger));
                                case "rigid":
                                    return Instruction.Rigid(ConvertToTrigger(fireMode.trigger));
                                case "vibratetrigger":
                                    return Instruction.VibrateTrigger(ConvertToTrigger(fireMode.trigger), (int)fireMode.frequency);
                                case "choppy":
                                    return Instruction.Choppy(ConvertToTrigger(fireMode.trigger));
                                case "medium":
                                    return Instruction.Medium(ConvertToTrigger(fireMode.trigger));
                                case "vibrateTriggerPulse":
                                    return Instruction.VibrateTriggerPulse(ConvertToTrigger(fireMode.trigger));
                                case "customtriggervaluemode":
                                    return Instruction.CustomTriggerValueMode(ConvertToTrigger(fireMode.trigger),
                                        (CustomTriggerValueMode)fireMode.custTriggerValueMode,
                                        (int)fireMode.force1,
                                        (int)fireMode.force2,
                                        (int)fireMode.force3,
                                        (int)fireMode.force4,
                                        (int)fireMode.force5,
                                        (int)fireMode.force6,
                                        (int)fireMode.force7);
                                case "resistance":
                                    return Instruction.Resistance(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.force);
                                case "bow":
                                    return Instruction.Bow(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end, (int)fireMode.force, (int)fireMode.snapforce);
                                case "galloping":
                                    return Instruction.Galloping(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end, (int)fireMode.firstFoot,
                                        (int)fireMode.secondFoot, (int)fireMode.frequency);
                                case "semiautomaticgun":
                                    return Instruction.SemiAutomaticGun(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end, (int)fireMode.force);
                                case "automaticgun":
                                    return Instruction.AutomaticGun(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.strength, (int)fireMode.frequency);
                                case "machine":
                                    return Instruction.Machine(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end,
                                        (int)fireMode.strengthA, (int)fireMode.strengthB, (int)fireMode.frequency, (float)fireMode.period);
                                default:
                                    //default to normal
                                    return Instruction.Normal(ConvertToTrigger(fireMode.trigger));
                            }


                        }
                    }
                    break;
                }
            }

            return Instruction.Normal(Trigger.Right);
        }

        //use this overloaded method only when changing to a future firemode
        internal static Instruction ReturnTriggerInstruction(Weapon weapon, Weapon.EFireMode eFireMode)
        {
            foreach (WeaponConfig weaponConfig in config.weapons)
            {
                if (weaponConfig.name == weapon.WeapClass.ToLower())
                {
                    //now loop to find the firemode
                    foreach (FireModeConfig fireMode in weaponConfig.fireModes)
                    {
                        if (fireMode.mode == eFireMode.ToString().ToLower())
                        {
                            switch (fireMode.instructionType.ToLower())
                            {
                                case "normal":
                                    return Instruction.Normal(ConvertToTrigger(fireMode.trigger));
                                case "gamecube":
                                    return Instruction.GameCube(ConvertToTrigger(fireMode.trigger));
                                case "verysoft":
                                    return Instruction.VerySoft(ConvertToTrigger(fireMode.trigger));
                                case "soft":
                                    return Instruction.Soft(ConvertToTrigger(fireMode.trigger));
                                case "hard":
                                    return Instruction.Hard(ConvertToTrigger(fireMode.trigger));
                                case "veryhard":
                                    return Instruction.VeryHard(ConvertToTrigger(fireMode.trigger));
                                case "hardest":
                                    return Instruction.Hardest(ConvertToTrigger(fireMode.trigger));
                                case "rigid":
                                    return Instruction.Rigid(ConvertToTrigger(fireMode.trigger));
                                case "vibratetrigger":
                                    return Instruction.VibrateTrigger(ConvertToTrigger(fireMode.trigger), (int)fireMode.frequency);
                                case "choppy":
                                    return Instruction.Choppy(ConvertToTrigger(fireMode.trigger));
                                case "medium":
                                    return Instruction.Medium(ConvertToTrigger(fireMode.trigger));
                                case "vibrateTriggerPulse":
                                    return Instruction.VibrateTriggerPulse(ConvertToTrigger(fireMode.trigger));
                                case "customtriggervaluemode":
                                    return Instruction.CustomTriggerValueMode(ConvertToTrigger(fireMode.trigger),
                                        (CustomTriggerValueMode)fireMode.custTriggerValueMode,
                                        (int)fireMode.force1,
                                        (int)fireMode.force2,
                                        (int)fireMode.force3,
                                        (int)fireMode.force4,
                                        (int)fireMode.force5,
                                        (int)fireMode.force6,
                                        (int)fireMode.force7);
                                case "resistance":
                                    return Instruction.Resistance(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.force);
                                case "bow":
                                    return Instruction.Bow(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end, (int)fireMode.force, (int)fireMode.snapforce);
                                case "galloping":
                                    return Instruction.Galloping(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end, (int)fireMode.firstFoot,
                                        (int)fireMode.secondFoot, (int)fireMode.frequency);
                                case "semiautomaticgun":
                                    return Instruction.SemiAutomaticGun(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end, (int)fireMode.force);
                                case "automaticgun":
                                    return Instruction.AutomaticGun(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.strength, (int)fireMode.frequency);
                                case "machine":
                                    return Instruction.Machine(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end,
                                        (int)fireMode.strengthA, (int)fireMode.strengthB, (int)fireMode.frequency, (float)fireMode.period);
                                default:
                                    //default to normal
                                    return Instruction.Normal(ConvertToTrigger(fireMode.trigger));
                            }


                        }
                    }
                    break;
                }
            }

            return Instruction.Normal(Trigger.Right);
        }
        internal static Instruction ReturnScopeInstruction(Weapon weapon)
        {

            foreach (ScopeConfig scope in config.other)
            {
                if (scope.name.ToLower() == "scope")
                {
                    //now loop to find the firemode
                    foreach (var fireMode in scope.fireModes)
                    {
                        if (fireMode.mode == weapon.SelectedFireMode.ToString().ToLower())
                        {
                            switch (fireMode.instructionType.ToLower())
                            {
                                case "normal":
                                    return Instruction.Normal(ConvertToTrigger(fireMode.trigger));
                                case "gamecube":
                                    return Instruction.GameCube(ConvertToTrigger(fireMode.trigger));
                                case "verysoft":
                                    return Instruction.VerySoft(ConvertToTrigger(fireMode.trigger));
                                case "soft":
                                    return Instruction.Soft(ConvertToTrigger(fireMode.trigger));
                                case "hard":
                                    return Instruction.Hard(ConvertToTrigger(fireMode.trigger));
                                case "veryhard":
                                    return Instruction.VeryHard(ConvertToTrigger(fireMode.trigger));
                                case "hardest":
                                    return Instruction.Hardest(ConvertToTrigger(fireMode.trigger));
                                case "rigid":
                                    return Instruction.Rigid(ConvertToTrigger(fireMode.trigger));
                                case "vibratetrigger":
                                    return Instruction.VibrateTrigger(ConvertToTrigger(fireMode.trigger), (int)fireMode.frequency);
                                case "choppy":
                                    return Instruction.Choppy(ConvertToTrigger(fireMode.trigger));
                                case "medium":
                                    return Instruction.Medium(ConvertToTrigger(fireMode.trigger));
                                case "vibrateTriggerPulse":
                                    return Instruction.VibrateTriggerPulse(ConvertToTrigger(fireMode.trigger));
                                case "customtriggervaluemode":
                                    return Instruction.CustomTriggerValueMode(ConvertToTrigger(fireMode.trigger),
                                        (CustomTriggerValueMode)fireMode.custTriggerValueMode,
                                        (int)fireMode.force1,
                                        (int)fireMode.force2,
                                        (int)fireMode.force3,
                                        (int)fireMode.force4,
                                        (int)fireMode.force5,
                                        (int)fireMode.force6,
                                        (int)fireMode.force7);
                                case "resistance":
                                    return Instruction.Resistance(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.force);
                                case "bow":
                                    return Instruction.Bow(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end, (int)fireMode.force, (int)fireMode.snapforce);
                                case "galloping":
                                    return Instruction.Galloping(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end, (int)fireMode.firstFoot,
                                        (int)fireMode.secondFoot, (int)fireMode.frequency);
                                case "semiautomaticgun":
                                    return Instruction.SemiAutomaticGun(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end, (int)fireMode.force);
                                case "automaticgun":
                                    return Instruction.AutomaticGun(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.strength, (int)fireMode.frequency);
                                case "machine":
                                    return Instruction.Machine(ConvertToTrigger(fireMode.trigger), (int)fireMode.start, (int)fireMode.end,
                                        (int)fireMode.strengthA, (int)fireMode.strengthB, (int)fireMode.frequency, (float)fireMode.period);
                                default:
                                    //default to normal
                                    return Instruction.Normal(ConvertToTrigger(fireMode.trigger));
                            }


                        }
                    }
                    break;
                }
            }

            return Instruction.Normal(Trigger.Right);
        }
        internal static Instruction ReturnThresholdInstruction(Weapon weapon)
        {
            foreach (WeaponConfig weaponConfig in config.weapons)
            {
                if (weaponConfig.name == weapon.WeapClass.ToLower())
                {
                    //now loop to find the firemode
                    foreach (FireModeConfig fireMode in weaponConfig.fireModes)
                    {
                        if (fireMode.mode == weapon.SelectedFireMode.ToString().ToLower())
                        {
                            return Instruction.TriggerThreshold(ConvertToTrigger(fireMode.trigger), fireMode.threshold);
                        }
                    }
                    break;
                }
            }

            return Instruction.TriggerThreshold(Trigger.Invalid, 0);
        }

        internal static Instruction ReturnScopeThresholdInstruction(Weapon weapon)
        {
            foreach (ScopeConfig scopeConfig in config.other)
            {
                if (scopeConfig.name == "scope")
                {
                    //now loop to find the firemode
                    foreach (FireModeConfig fireMode in scopeConfig.fireModes)
                    {
                        if (fireMode.mode == weapon.SelectedFireMode.ToString().ToLower())
                        {
                            return Instruction.TriggerThreshold(ConvertToTrigger(fireMode.trigger), fireMode.threshold);
                        }
                    }
                    break;
                }
            }

            return Instruction.TriggerThreshold(Trigger.Invalid, 0);
        }

        #endregion

        #region JsonFileDeserializedClasses
        public class FireModeConfig
        {
            public string mode;
            public int threshold;
            public string trigger;
            public string instructionType;
            public int? start;
            public int? end;
            public int? force;
            public int? snapforce;
            public int? strength;
            public int? frequency;
            public int? custTriggerValueMode;
            public int? force1;
            public int? force2;
            public int? force3;
            public int? force4;
            public int? force5;
            public int? force6;
            public int? force7;
            public int? firstFoot;
            public int? secondFoot;
            public int? strengthA;
            public int? strengthB;
            public float? period;
        }
        public class WeaponConfig
        {
            public string name;
            public List<FireModeConfig> fireModes;
        }
        public class ScopeConfig
        {
            public string name;
            public List<FireModeConfig> fireModes;
        }
        public class ConfigFile
        {
            public List<WeaponConfig> weapons;
            public List<ScopeConfig> other;
        }
        #endregion

    }
}
