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

            // Debug: Log what we're actually seeing
            if (__result != null)
            {
                var quality = __instance.CustomerData.Standards.GetCorrespondingQuality();
                float enjoyment = __instance.GetProductEnjoyment(__result, quality);
                float priceRatio = __result.Price / __result.MarketValue;
                
                MelonLogger.Msg($"[ProductSearch] {__instance.NPC.fullName} considering {__result.Name}: " +
                               $"enjoyment={enjoyment:F3}, price={__result.Price:F0}, " +
                               $"market={__result.MarketValue:F0}, ratio={priceRatio:F2}, appeal={appeal:F3}");
            }

            // Appeal = enjoyment + price_factor, can range from -1 to 2+
            bool foundGoodProduct = __result != null && appeal >= ChallengeConfig.MIN_APPEAL_FOR_SUCCESS;

            if (foundGoodProduct)
            {
                ProductRequestTracker.ResetFailureCount(__instance.NPC.ID);
            }
            else 
            {
                ProductRequestTracker.RecordFailedRequest(__instance);
                int failureCount = ProductRequestTracker.GetFailureCount(__instance.NPC.ID);
                string reason = __result == null ? "no products available" : $"low appeal ({appeal:F3})";
                MelonLogger.Msg($"[ProductRequest] {__instance.NPC.fullName} FAILED: {reason}, failures: {failureCount}");
            }
        }
    }
}

