using System.Reflection;
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
            return AccessTools.Method(typeof(Player.FirearmController.GClass1608), nameof(Player.FirearmController.GClass1608.ChangeFireMode));
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
            return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.IEventsConsumerOnAddAmmoInChamber));
        }

        [PatchPostfix]
        private static void Postfix(Player.FirearmController __instance, Player ____player)
        {
            if (____player.IsYourPlayer)
            {
                Logger.LogDebug("TarkovDSX: FirearmController OnAddAmmoInChamber");

                var weapon = __instance.Item;

                DSXComponent.checkMalfunction(weapon);

                //Logger.LogDebug("TarkovDSX: FirearmController OnAddAmmoInChamber: Reset Trigger as Ammo Added to Chamber");
                DSXComponent.changeTriggerFromWeaponType(weapon);
                return;
            }

        }
    }

    internal class RemoveAmmoInChamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.IEventsConsumerOnDelAmmoChamber));
        }

        [PatchPostfix]
        private static void Postfix(Player.FirearmController __instance, Player ____player)
        {
            if (____player.IsYourPlayer)
            {
                // Logger.LogDebug("TarkovDSX: FirearmController RemoveAmmoFromChamber");
                var weapon = __instance.Item;

                DSXComponent.checkMalfunction(weapon);
                DSXComponent.checkEmptyChamber(weapon);
            }

        }
    }

}
