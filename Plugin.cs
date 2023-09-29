using System.Reflection;
using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using EFT;

namespace DSX
{
    [BepInPlugin("com.dvize.TarkovDSX", "dvize.TarkovDSX", "1.0.0")]
    public class DSXPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> PluginEnabled;
        public static ConfigEntry<int> TriggerThreshold;

        private void Awake()
        {
            PluginEnabled = Config.Bind(
                "Main Settings",
                "Plugin on/off",
                true,
                "");

            new NewGamePatch().Enable();
            new ChangeFireModePatch().Enable();
            new AddAmmoInChamberPatch().Enable();
            new RemoveAmmoInChamberPatch().Enable();
        }

    }

    //re-initializes each new game
    internal class NewGamePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));

        [PatchPrefix]
        public static void PatchPrefix()
        {
            DSXComponent.Enable();
        }
    }
}
