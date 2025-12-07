using MelonLoader;
using HarmonyLib;
using System.Reflection;
using challange_mode.Managers;

[assembly: MelonInfo(typeof(challange_mode.Core), "Challenge Mode", "1.0.0", "Dixie", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace challange_mode
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Challenge Mode initialized");
            
            var harmony = new HarmonyLib.Harmony("challange_mode");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            LoggerInstance.Msg("Harmony patches applied successfully");

            ProductRequestTracker.LoadData();
            LoggerInstance.Msg("Loaded product request tracking data");
        }

        public override void OnApplicationQuit()
        {
            ProductRequestTracker.SaveData();
            LoggerInstance.Msg("Saved product request tracking data");
        }
    }
}