using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;

namespace DSX
{

    internal class ChangeFireModePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController.GClass1496), "ChangeFireMode");
        }

        [PatchPostfix]
        private static void Postfix(Player ___player_0, ref Weapon.EFireMode fireMode, Weapon ___weapon_0)
        {
            if (___player_0.IsYourPlayer)
            {
                var weapon = ___weapon_0;

                Logger.LogDebug("TarkovDSX: Change Firemode Weapon in hands: " + fireMode.ToString());

                //same logic as from change hands
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
                        else if(weapon.SelectedFireMode == Weapon.EFireMode.fullauto)
                        {
                            DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                            DSXComponent.rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, DSXComponent.CalculateFrequency(weapon.FireRate));
                        }
                        else if(weapon.SelectedFireMode == Weapon.EFireMode.burst)
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
                        else if(weapon.SelectedFireMode == Weapon.EFireMode.fullauto)
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

                /* DSXComponent.triggerThresholdLeft = Instruction.TriggerThreshold(Trigger.Left, 150);
                 DSXComponent.leftTriggerUpdate = Instruction.GameCube(Trigger.Left);*/

            }

        }
    }


}
