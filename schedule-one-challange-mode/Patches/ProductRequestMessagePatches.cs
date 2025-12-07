using HarmonyLib;
using System.Collections.Generic;
using challange_mode.Managers;
using UnityEngine;
using MelonLoader;

#if MONO
using ScheduleOne.Economy;
using ScheduleOne.Product;
using ScheduleOne.ItemFramework;
using ScheduleOne.UI.Handover;
#endif

namespace challange_mode.Patches
{
    /// <summary>
    /// Tracks when customers fail or succeed to find products they want
    /// </summary>
    [HarmonyPatch(typeof(Customer), "GetWeightedRandomProduct")]
    public class TrackProductRequest_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Customer __instance, ref ProductDefinition __result, ref float appeal)
        {
            if (__instance?.NPC?.ID == null)
                return;

            if (__result != null && appeal >= ChallengeConfig.MIN_APPEAL_FOR_REQUEST)
            {
                ProductRequestTracker.ResetFailureCount(__instance.NPC.ID);
                MelonLogger.Msg($"[ProductRequest] {__instance.NPC.fullName} found suitable product (appeal: {appeal:F2})");
            }
            else 
            {
                ProductRequestTracker.RecordFailedRequest(__instance);
                int failureCount = ProductRequestTracker.GetFailureCount(__instance.NPC.ID);
                MelonLogger.Msg($"[ProductRequest] {__instance.NPC.fullName} failed to find product (appeal: {appeal:F2}, failures: {failureCount})");
            }
        }
    }
}

