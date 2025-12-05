using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

#if MONO
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
#endif

namespace challange_mode.Patches
{
    [HarmonyPatch(typeof(Customer), nameof(Customer.GetProductEnjoyment))]
    public class GetProductEnjoyment_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Customer __instance, ProductDefinition product, ref float __result)
        {
            // Original weights: drug type 0.3, properties 0.4, quality 0.3
            // We're increasing importance of drug type and property matches
            
            // Get customer preferences
            var customerData = __instance.CustomerData;
            if (customerData == null || customerData.DefaultAffinityData == null) return;

            // Calculate drug type match bonus/penalty based on affinity
            float drugTypeBonus = 0f;
            var affinityData = customerData.DefaultAffinityData;
            if (affinityData.ProductAffinities != null && affinityData.ProductAffinities.Count > 0)
            {
                float affinity = affinityData.GetAffinity(product.DrugType);
                // Affinity ranges from -1 to 1, scale it to larger impact
                drugTypeBonus = affinity * 0.3f; // Up to ±30% modifier
            }

            // Calculate property match bonus/penalty
            float propertyBonus = 0f;
            if (customerData.PreferredProperties != null && customerData.PreferredProperties.Count > 0 && product.Properties != null)
            {
                int matchingProps = 0;
                foreach (var prefProp in customerData.PreferredProperties)
                {
                    if (product.Properties.Contains(prefProp))
                        matchingProps++;
                }
                
                float matchRatio = (float)matchingProps / customerData.PreferredProperties.Count;
                // Stronger weighting: bonus for good matches, penalty for poor matches
                propertyBonus = (matchRatio - 0.5f) * 0.4f; // Up to ±20% modifier
            }

            // Apply modifiers
            __result += drugTypeBonus + propertyBonus;
            
            // Clamp to valid range
            __result = UnityEngine.Mathf.Clamp01(__result);
        }
    }

    [HarmonyPatch(typeof(Customer), nameof(Customer.GetOfferSuccessChance))]
    public class GetOfferSuccessChance_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Customer __instance, List<ItemInstance> items, ref float __result)
        {
            // Apply harsh penalty for poor product matches
            // If average enjoyment is low (< 0.5), slash success rate dramatically
            
            if (items == null || items.Count == 0) return;

            // Calculate average enjoyment across all offered products
            float totalEnjoyment = 0f;
            int productCount = 0;

            foreach (var item in items)
            {
                // Cast to ProductItemInstance to access Quality and Definition
                if (item is ProductItemInstance productItem)
                {
                    var product = productItem.Definition as ProductDefinition;
                    if (product != null)
                    {
                        var quality = productItem.Quality;
                        float enjoyment = __instance.GetProductEnjoyment(product, quality);
                        totalEnjoyment += enjoyment;
                        productCount++;
                    }
                }
            }

            if (productCount == 0) return;

            float avgEnjoyment = totalEnjoyment / productCount;

            // Harsh multiplier based on enjoyment:
            // < 0.3 enjoyment: 0.1x success (90% reduction)
            // 0.3-0.5 enjoyment: 0.3x success (70% reduction)
            // 0.5-0.7 enjoyment: 0.6x success (40% reduction)
            // > 0.7 enjoyment: 1.0x (no change)
            
            float multiplier;
            if (avgEnjoyment < 0.3f)
                multiplier = 0.1f;
            else if (avgEnjoyment < 0.5f)
                multiplier = 0.3f;
            else if (avgEnjoyment < 0.7f)
                multiplier = 0.6f;
            else
                multiplier = 1.0f;

            __result *= multiplier;
        }
    }

    [HarmonyPatch(typeof(Customer), "EvaluateCounteroffer")]
    public class EvaluateCounteroffer_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Customer __instance, ProductDefinition product, int quantity, float price, ref bool __result)
        {
            // If already rejected, don't override
            if (!__result) return;

            // Apply stricter counteroffer requirements
            
            // 1. Stricter price limit: reject if > 2x average spend (original was 3x)
            float adjustedWeeklySpend = __instance.CustomerData.GetAdjustedWeeklySpend(__instance.NPC.RelationData.RelationDelta / 5f);
            var orderDays = __instance.CustomerData.GetOrderDays(__instance.CurrentAddiction, __instance.NPC.RelationData.RelationDelta / 5f);
            float avgOrderSpend = adjustedWeeklySpend / (float)orderDays.Count;
            
            if (price >= avgOrderSpend * 2f)
            {
                __result = false;
                return;
            }

            // 2. Minimum enjoyment threshold: reject if < 40%
            float enjoyment = __instance.GetProductEnjoyment(product, __instance.CustomerData.Standards.GetCorrespondingQuality());
            
            if (enjoyment < 0.4f)
            {
                __result = false;
                return;
            }

            // 3. Apply additional difficulty multiplier based on enjoyment
            // If enjoyment is mediocre (0.4-0.6), add 50% chance to reject anyway
            if (enjoyment < 0.6f)
            {
                float rejectChance = UnityEngine.Mathf.Lerp(0.5f, 0f, (enjoyment - 0.4f) / 0.2f);
                if (UnityEngine.Random.value < rejectChance)
                {
                    __result = false;
                }
            }
        }
    }
}

