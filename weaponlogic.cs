using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSX;
using EFT.InventoryLogic;

namespace dvize.DSX
{
    /*internal class weaponlogic
    {
        switch (weapon.WeapClass.ToLower())
        {
            case "assaultrifle":
                Logger.LogDebug("TarkovDSX: Assault Rifle in hands: " + weapon.LocalizedName());
                if (weapon.SelectedFireMode == Weapon.EFireMode.semiauto)
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                }
                else if (weapon.SelectedFireMode == Weapon.EFireMode.fullauto)
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, CalculateFrequency(weapon.FireRate));
                }
                break;
            case "assaultcarbine": // toz sks 7.62x39
                Logger.LogDebug("TarkovDSX: Assault Carbine in hands: " + weapon.LocalizedName());
                if (weapon.SelectedFireMode == Weapon.EFireMode.semiauto)
                {

                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                }
                else
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, CalculateFrequency(weapon.FireRate));
                }
                break;
            case "marksmanrifle": //RSASS, mk-18, SR-25, HK G28
                Logger.LogDebug("TarkovDSX: Marksman Rifle in hands:" + weapon.LocalizedName());
                if (weapon.SelectedFireMode == Weapon.EFireMode.semiauto)
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                }
                else
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, CalculateFrequency(weapon.FireRate));
                }
                break;
            case "sniperrifle": // mosins , DVL-10, basically any bolt-action
                Logger.LogDebug("TarkovDSX: Sniper Rifle in hands: " + weapon.LocalizedName());
                if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.VeryHard(Trigger.Right);
                }
                else if (weapon.SelectedFireMode == Weapon.EFireMode.semiauto)
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.VeryHard(Trigger.Right);
                }
                break;
            case "smg":
                Logger.LogDebug("TarkovDSX: SMG in hands: " + weapon.LocalizedName());
                if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                }
                else
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, CalculateFrequency(weapon.FireRate));
                }
                break;
            case "shotgun":
                Logger.LogDebug("TarkovDSX: Shotgun in hands: " + weapon.LocalizedName());
                if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                }
                break;
            case "pistol":
                Logger.LogDebug("TarkovDSX: Pistol in hands: " + weapon.LocalizedName());
                if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                }
                else
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, CalculateFrequency(weapon.FireRate));
                }
                break;
            case "grenadelauncher":
                Logger.LogDebug("TarkovDSX: Grenade Launcher in hands: " + weapon.LocalizedName());
                if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.Rigid(Trigger.Right);
                }
                break;
            case "machinegun":
                Logger.LogDebug("TarkovDSX: Machine Gun in hands: " + weapon.LocalizedName());
                if (weapon.SelectedFireMode == Weapon.EFireMode.single)
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.Hard(Trigger.Right);
                }
                else
                {
                    triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 80);
                    rightTriggerUpdate = Instruction.AutomaticGun(Trigger.Right, 0, 8, CalculateFrequency(weapon.FireRate));
                }
                break;
            default:
                Logger.LogDebug("TarkovDSX: Unknown Weapon in hands: " + weapon.LocalizedName());
                triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 0);
                rightTriggerUpdate = Instruction.Normal(Trigger.Right);
                break;

            }
    */
}
