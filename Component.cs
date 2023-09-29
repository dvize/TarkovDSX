using System;
using System.IO;
using System.Threading.Tasks;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using HarmonyLib;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;
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

        private float lastSendTime;
        private float sendInterval = 0.05f;  //limit for how often to send udp packets

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
            LoadTomlConfig();
        }

        private void LoadTomlConfig()
        {
            // Load and parse the TOML file
            DocumentSyntax toml = Toml.Parse(File.ReadAllText("dvize.DSX.config.toml"));

            //create toml table from the parsed toml file
            TomlTable config = toml.ToModel() as TomlTable;

            if (config != null)
            {
                TomlTable assaultrifle = (TomlTable)config["assaultrifle"];
                TomlTable fullauto = (TomlTable)assaultrifle["fullauto"];

                double frequency = (double)fullauto["frequency"];

            }

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
            // if laying down do lean left and right with controller tilt
            // if weapon jammed, shake controller to clear?
            // if weapon empty, set trigger to light empty click

            player.OnDamageReceived += Player_OnDamageReceived;             //use this for the damage LED indicator on controller
            player.HandsChangedEvent += Player_HandsChangedEvent;           //use this to detect when weapon has changed to see the type and firemode
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
                bool isBallisticWeapon;

                if (obj.Item is KnifeClass)
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
                    Logger.LogDebug("Weapon.firerate is: " + weapon.FireRate);

                    changeTriggerFromWeaponType(weapon);

                    triggerThresholdLeft = Instruction.TriggerThreshold(Trigger.Left, 150);
                    leftTriggerUpdate = Instruction.GameCube(Trigger.Left);

                }
                else if (!isBallisticWeapon)
                {
                    KnifeClass weapon = obj.Item as KnifeClass;

                    if (weapon.Weight <= 0.3)
                    {
                        Logger.LogDebug("TarkovDSX: Melee Weapon in hands: " + weapon.LocalizedName());

                        rightTriggerUpdate = Instruction.Resistance(Trigger.Right, 0, 3);
                    }
                    else if (weapon.Weight <= 1)
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

        internal static void changeTriggerFromWeaponType(Weapon weapon)
        {
            switch (weapon.WeapClass.ToLower())
            {
                case "assaultrifle":
                    Logger.LogDebug("TarkovDSX: Assault Rifle in hands: " + weapon.LocalizedName());
                    if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else if (weapon.SelectedFireMode == Weapon.EFireMode.fullauto)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    else if (weapon.SelectedFireMode == Weapon.EFireMode.burst)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                case "assaultcarbine": // toz sks 7.62x39
                    Logger.LogDebug("TarkovDSX: Assault Carbine in hands: " + weapon.LocalizedName());
                    if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                    {

                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else if (weapon.SelectedFireMode == Weapon.EFireMode.fullauto)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    else if (weapon.SelectedFireMode == Weapon.EFireMode.burst)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                case "marksmanrifle": //RSASS, mk-18, SR-25, HK G28
                    Logger.LogDebug("TarkovDSX: Marksman Rifle in hands:" + weapon.LocalizedName());
                    if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else if (weapon.SelectedFireMode == Weapon.EFireMode.fullauto)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    else if (weapon.SelectedFireMode == Weapon.EFireMode.burst)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                case "sniperrifle": // mosins , DVL-10, basically any bolt-action
                    Logger.LogDebug("TarkovDSX: Sniper Rifle in hands: " + weapon.LocalizedName());
                    if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.VeryHard(Trigger.Right);
                    }
                    break;
                case "smg":
                    Logger.LogDebug("TarkovDSX: SMG in hands: " + weapon.LocalizedName());
                    if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else if (weapon.SelectedFireMode == Weapon.EFireMode.fullauto)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    else if (weapon.SelectedFireMode == Weapon.EFireMode.burst)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                case "shotgun":
                    Logger.LogDebug("TarkovDSX: Shotgun in hands: " + weapon.LocalizedName());
                    if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else if (weapon.SelectedFireMode == Weapon.EFireMode.doubleaction)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    break;
                case "pistol":
                    Logger.LogDebug("TarkovDSX: Pistol in hands: " + weapon.LocalizedName());
                    if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                case "grenadelauncher":
                    Logger.LogDebug("TarkovDSX: Grenade Launcher in hands: " + weapon.LocalizedName());
                    if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Rigid(Trigger.Right);
                    }
                    break;
                case "machinegun":
                    Logger.LogDebug("TarkovDSX: Machine Gun in hands: " + weapon.LocalizedName());
                    if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                default:
                    Logger.LogDebug("TarkovDSX: Unknown Weapon in hands: " + weapon.LocalizedName());
                    DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 0);
                    DSXComponent.rightTriggerUpdate = Instruction.Normal(Trigger.Right);
                    break;
            }
        }

        internal static void changeTriggerFromWeaponType(Weapon weapon, Weapon.EFireMode firemode)
        {
            switch (weapon.WeapClass.ToLower())
            {
                case "assaultrifle":
                    Logger.LogDebug("TarkovDSX: Assault Rifle in hands: " + weapon.LocalizedName());
                    if (firemode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else if (firemode == Weapon.EFireMode.fullauto)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    else if (firemode == Weapon.EFireMode.burst)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                case "assaultcarbine": // toz sks 7.62x39
                    Logger.LogDebug("TarkovDSX: Assault Carbine in hands: " + weapon.LocalizedName());
                    if (firemode == Weapon.EFireMode.single)
                    {

                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else if (firemode == Weapon.EFireMode.fullauto)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    else if (firemode == Weapon.EFireMode.burst)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                case "marksmanrifle": //RSASS, mk-18, SR-25, HK G28
                    Logger.LogDebug("TarkovDSX: Marksman Rifle in hands:" + weapon.LocalizedName());
                    if (firemode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else if (firemode == Weapon.EFireMode.fullauto)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    else if (firemode == Weapon.EFireMode.burst)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                case "sniperrifle": // mosins , DVL-10, basically any bolt-action
                    Logger.LogDebug("TarkovDSX: Sniper Rifle in hands: " + weapon.LocalizedName());
                    if (firemode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.VeryHard(Trigger.Right);
                    }
                    break;
                case "smg":
                    Logger.LogDebug("TarkovDSX: SMG in hands: " + weapon.LocalizedName());
                    if (firemode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else if (firemode == Weapon.EFireMode.fullauto)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    else if (firemode == Weapon.EFireMode.burst)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                case "shotgun":
                    Logger.LogDebug("TarkovDSX: Shotgun in hands: " + weapon.LocalizedName());
                    if (firemode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else if (firemode == Weapon.EFireMode.doubleaction)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    break;
                case "pistol":
                    Logger.LogDebug("TarkovDSX: Pistol in hands: " + weapon.LocalizedName());
                    if (firemode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                case "grenadelauncher":
                    Logger.LogDebug("TarkovDSX: Grenade Launcher in hands: " + weapon.LocalizedName());
                    if (firemode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Rigid(Trigger.Right);
                    }
                    break;
                case "machinegun":
                    Logger.LogDebug("TarkovDSX: Machine Gun in hands: " + weapon.LocalizedName());
                    if (firemode == Weapon.EFireMode.single)
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                    }
                    else
                    {
                        DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                        DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                    }
                    break;
                default:
                    Logger.LogDebug("TarkovDSX: Unknown Weapon in hands: " + weapon.LocalizedName());
                    DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 0);
                    DSXComponent.rightTriggerUpdate = Instruction.Normal(Trigger.Right);
                    break;
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

        internal static int CalculateFrequency(int fireRate)
        {
            //doesn't seem like a full auto fire above 15 frequency
            return (int)((15.0 / maxFireRate) * fireRate);
        }









    }
}
