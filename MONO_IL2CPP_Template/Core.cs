using MelonLoader;
using HarmonyLib;
using System.Reflection;

[assembly: MelonInfo(typeof(challange_mode.Core), "Challenge Mode", "1.0.0", "Dixie", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace challange_mode
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Challenge Mode initialized - success rates reduced by 70%");
            
            // Apply Harmony patches
            var harmony = new HarmonyLib.Harmony("challange_mode");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            LoggerInstance.Msg("Harmony patches applied successfully");
        }
    }
}