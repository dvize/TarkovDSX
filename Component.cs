﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using UnityEngine;

namespace DSX
{
    public class DSXComponent : MonoBehaviour
    {
        private static GameWorld gameWorld;
        private static DualSenseConnection dualSenseConnection;
        private static Player player;
        private const int maxFireRate = 1000;      // 1000 rounds per minute
        private const int ControllerIndex = 0;

        private Instruction leftTriggerUpdate = new Instruction();
        private Instruction rightTriggerUpdate = new Instruction();
        private Instruction rgb = new Instruction();
        private Instruction micLED = new Instruction();
        private Instruction playerLED = new Instruction();
        private Instruction playerLEDNewRevision = new Instruction();
        private Instruction triggerThresholdLeft = new Instruction();
        private Instruction triggerThresholdRight = new Instruction();
        private Packet leftTrigger = new Packet();
        private Packet rightTrigger = new Packet();
        private Packet otherControllerInstructions = new Packet();

        private float lastSendTime;
        private float sendInterval = 0.05f;

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
        }

        private void AttachEvents()
        {
            // need to check weapon in hand, on firemode switch, if aim down sights, if grenade pin pulled, on damage received flash colors?
            // also what do i need to attach to use controller gyro for lean or blindfire shooting
            // if laying down do lean left and right with controller tilt
            // if weapon jammed, shake controller to clear?
           
            player.OnDamageReceived += Player_OnDamageReceived;
            player.HandsChangedEvent += Player_HandsChangedEvent;
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
        private void Player_HandsChangedEvent(GInterface109 obj)
        {
            Logger.LogDebug("TarkovDSX: Hands Changed");
            //check to see the weapon type if its a gun or grenade or melee
            if (player.IsWeaponOrKnifeInHands)
            {
                bool isBallisticWeapon = false; 

                if(obj.Item is KnifeClass)
                {
                    isBallisticWeapon = false;
                }
                else
                {
                    Logger.LogDebug("TarkovDSX: Shooting Weapon in hands");
                    isBallisticWeapon = true;
                }
                
                if (isBallisticWeapon)
                {
                    Weapon weapon = obj.Item as Weapon;
                    string weapclass = weapon.WeapClass.ToLower();
                    string firemode = weapon.SelectedFireMode.ToString().ToLower();
                   

                    Logger.LogDebug("Weapon.WeapClass is: " + weapclass);
                    Logger.LogDebug("Weapon.SelectedFireMode is: " + firemode);
                    Logger.LogDebug("Weapon.firerate is: " + weapon.FireRate);

                    /*switch (weapclass)
                    {
                        case "assaultrifle":
                            if(firemode == "fullauto")
                            {
                                Logger.LogDebug("TarkovDSX: Full Auto Assault Rifle in hands: " + weapon.LocalizedName());
                                triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                                rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, CalculateFrequency(weapon.FireRate));

                            }
                            else if(firemode == "semiauto")
                            {
                                Logger.LogDebug("TarkovDSX: Semi Auto Assault Rifle in hands: " + weapon.LocalizedName());
                                triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 130);
                                rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                            }
                            break;
                    }*/

                    if (firemode == "fullauto")
                    {
                        Logger.LogDebug("TarkovDSX: Full Auto Assault Rifle in hands: " + weapon.LocalizedName());

                        //triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        var frequency = CalculateFrequency(weapon.FireRate);
                        Logger.LogDebug("TarkovDSX: Frequency is: " + frequency);
                        rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 6, 8, frequency);
                    }

                    triggerThresholdLeft = Instruction.TriggerThreshold(Trigger.Left, 150);
                    leftTriggerUpdate = Instruction.GameCube(Trigger.Left);
                    
                }
                else if (!isBallisticWeapon)
                {
                    KnifeClass weapon = obj.Item as KnifeClass;

                    if(weapon.Weight < 0.3)
                    {
                        Logger.LogDebug("TarkovDSX: Melee Weapon in hands: " + weapon.LocalizedName());
                        
                        rightTriggerUpdate = Instruction.Resistance(Trigger.Right, 0, 3);
                    }
                    else if (weapon.Weight < 1)
                    {
                        Logger.LogDebug("TarkovDSX: Melee Weapon in hands: " + weapon.LocalizedName());
                        rightTriggerUpdate = Instruction.Resistance(Trigger.Right, 0, 6);
                    }
                    else
                    {
                        Logger.LogDebug("TarkovDSX: Melee Weapon in hands: " + weapon.LocalizedName());
                        rightTriggerUpdate = Instruction.Resistance(Trigger.Right, 0, 9);
                    }
                    leftTriggerUpdate = Instruction.Normal(Trigger.Left);
                }
                
            }
        }
        private async void Player_OnDamageReceived(float damage, EBodyPart part, EDamageType type, float absorbed, MaterialType special)
        {
            Logger.LogDebug("TarkovDSX: Damage Received");
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

        private int CalculateFrequency(int fireRate)
        {
            //doesn't seem like a full auto fire above 15 frequency
            return (int)((15.0 / maxFireRate) * fireRate);
        }
        

        

        




    }
}
