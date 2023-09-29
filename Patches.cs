﻿using System.Reflection;
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
                DSXComponent.changeTriggerFromWeaponType(weapon, fireMode);

            }

        }
    }

    internal class AddAmmoInChamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), "method_31");
        }

        [PatchPostfix]
        private static void Postfix(Player.FirearmController __instance, Player ____player)
        {
            if (____player.IsYourPlayer)
            {
                Logger.LogDebug("TarkovDSX: FirearmController OnAddAmmoInChamber");

                var weapon = __instance.Item;

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
                }

                //Logger.LogDebug("TarkovDSX: FirearmController OnAddAmmoInChamber: Reset Trigger as Ammo Added to Chamber");
                DSXComponent.changeTriggerFromWeaponType(weapon);

            }

        }
    }

    internal class RemoveAmmoInChamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), "method_30");
        }

        [PatchPostfix]
        private static void Postfix(Player.FirearmController __instance, Player ____player)
        {
            if (____player.IsYourPlayer)
            {
               // Logger.LogDebug("TarkovDSX: FirearmController RemoveAmmoFromChamber");
                var weapon = __instance.Item;

                //need check if mag is empty as well
                if (weapon.GetCurrentMagazineCount() == 0)
                {
                    Logger.LogDebug("TarkovDSX: FirearmController OnAddAmmoInChamber: Out of Ammo Trigger Setting");
                    DSXComponent.triggerThresholdRight = Instruction.TriggerThreshold(Trigger.Right, 50);
                    DSXComponent.rightTriggerUpdate = Instruction.VerySoft(Trigger.Right);
                }

            }

        }
    }
}
