using HarmonyLib;
using challange_mode.Managers;

#if MONO
using ScheduleOne.Persistence;
#endif

namespace challange_mode.Patches
{
    /// <summary>
    /// Hook into game save to persist our tracking data
    /// </summary>
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Save), new System.Type[] { typeof(string) })]
    public class Save_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            // Save our tracking data whenever the game saves
            ProductRequestTracker.SaveData();
        }
    }

    /// <summary>
    /// Hook into game load to restore our tracking data
    /// </summary>
    [HarmonyPatch(typeof(LoadManager), nameof(LoadManager.StartGame))]
    public class StartGame_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            // Load our tracking data whenever the game loads
            ProductRequestTracker.LoadData();
        }
    }
}

