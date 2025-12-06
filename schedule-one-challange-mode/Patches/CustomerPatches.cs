using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if MONO
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
#endif

namespace challange_mode.Patches
{
    /// <summary>
    /// Configuration constants for Challenge Mode difficulty settings
    /// </summary>
    public static class ChallengeConfig
    {
        public const float DRUG_AFFINITY_MAX_IMPACT = 0.3f;
        
        public const float EFFECT_MATCH_NONE = -0.35f;
        public const float EFFECT_MATCH_ONE = 0.0f;
        public const float EFFECT_MATCH_TWO = 0.15f;
        public const float EFFECT_MATCH_THREE = 0.25f;
        
        public const float ENJOYMENT_CRITICAL_LOW = 0.25f;
        public const float ENJOYMENT_LOW = 0.45f;
        public const float ENJOYMENT_MEDIUM = 0.65f;
        
        public const float MULTIPLIER_CRITICAL_LOW = 0.1f;
        public const float MULTIPLIER_LOW = 0.3f;
        public const float MULTIPLIER_MEDIUM = 0.6f;
        public const float MULTIPLIER_HIGH = 1.0f;
        
        public const float COUNTEROFFER_PRICE_LIMIT_MULTIPLIER = 2.0f;
        public const float COUNTEROFFER_MIN_ENJOYMENT = 0.4f;
        public const float COUNTEROFFER_MEDIOCRE_THRESHOLD = 0.6f;
        public const float COUNTEROFFER_MAX_REJECT_CHANCE = 0.5f;
    }

    /// <summary>
    /// Helper utilities for customer behavior calculations
    /// </summary>
    public static class CustomerBehaviorHelpers
    {
        /// <summary>
        /// Calculates drug type affinity bonus based on customer preferences
        /// </summary>
        public static float CalculateDrugAffinityBonus(CustomerData customerData, EDrugType drugType)
        {
            if (customerData?.DefaultAffinityData?.ProductAffinities == null)
                return 0f;

            float affinity = customerData.DefaultAffinityData.GetAffinity(drugType);
            return affinity * ChallengeConfig.DRUG_AFFINITY_MAX_IMPACT;
        }

        /// <summary>
        /// Calculates effect matching bonus based on preferred effects.
        /// Uses discrete difficulty tiers based on exact match count.
        /// </summary>
        public static float CalculateEffectMatchBonus(CustomerData customerData, ProductDefinition product)
        {
            if (customerData?.PreferredProperties == null || 
                customerData.PreferredProperties.Count == 0)
                return 0f;

            if (product?.Properties == null)
                return ChallengeConfig.EFFECT_MATCH_NONE;

            int matchingEffects = customerData.PreferredProperties.Count(prefProp => 
                product.Properties.Contains(prefProp));

            return matchingEffects switch
            {
                0 => ChallengeConfig.EFFECT_MATCH_NONE,
                1 => ChallengeConfig.EFFECT_MATCH_ONE,
                2 => ChallengeConfig.EFFECT_MATCH_TWO,
                _ => ChallengeConfig.EFFECT_MATCH_THREE
            };
        }

        /// <summary>
        /// Calculates average enjoyment across multiple product items
        /// </summary>
        public static float CalculateAverageEnjoyment(Customer customer, List<ItemInstance> items)
        {
            if (items == null || items.Count == 0)
                return 0f;

            float totalEnjoyment = 0f;
            int productCount = 0;

            foreach (var item in items)
            {
                if (item is ProductItemInstance productItem)
                {
                    var product = productItem.Definition as ProductDefinition;
                    if (product != null)
                    {
                        float enjoyment = customer.GetProductEnjoyment(product, productItem.Quality);
                        totalEnjoyment += enjoyment;
                        productCount++;
                    }
                }
            }

            return productCount > 0 ? totalEnjoyment / productCount : 0f;
        }


        /// <summary>
        /// Calculates average order spend for a customer
        /// </summary>
        public static float GetAverageOrderSpend(Customer customer)
        {
            float normalizedRelation = customer.NPC.RelationData.RelationDelta / 5f;
            float adjustedWeeklySpend = customer.CustomerData.GetAdjustedWeeklySpend(normalizedRelation);
            var orderDays = customer.CustomerData.GetOrderDays(customer.CurrentAddiction, normalizedRelation);
            
            return adjustedWeeklySpend / orderDays.Count;
        }
    }

    /// <summary>
    /// Modifies product enjoyment calculation to emphasize drug type and effect preferences
    /// </summary>
    [HarmonyPatch(typeof(Customer), nameof(Customer.GetProductEnjoyment))]
    public class GetProductEnjoyment_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Customer __instance, ProductDefinition product, ref float __result)
        {
            var customerData = __instance.CustomerData;
            if (customerData == null)
                return;

            float drugTypeBonus = CustomerBehaviorHelpers.CalculateDrugAffinityBonus(customerData, product.DrugType);
            float effectBonus = CustomerBehaviorHelpers.CalculateEffectMatchBonus(customerData, product);

            __result += drugTypeBonus + effectBonus;
            __result = Mathf.Clamp01(__result);
        }
    }

    /// <summary>
    /// Applies harsh penalties to success chance based on product enjoyment
    /// </summary>
    [HarmonyPatch(typeof(Customer), nameof(Customer.GetOfferSuccessChance))]
    public class GetOfferSuccessChance_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Customer __instance, List<ItemInstance> items, ref float __result)
        {
            if (items == null || items.Count == 0)
                return;

            float avgEnjoyment = CustomerBehaviorHelpers.CalculateAverageEnjoyment(__instance, items);
            float multiplier = CustomerBehaviorHelpers.GetSuccessMultiplier(avgEnjoyment);

            __result *= multiplier;
        }
    }

    /// <summary>
    /// Applies stricter requirements for counter-offer acceptance
    /// </summary>
    [HarmonyPatch(typeof(Customer), "EvaluateCounteroffer")]
    public class EvaluateCounteroffer_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Customer __instance, ProductDefinition product, int quantity, float price, ref bool __result)
        {
            if (!__result)
                return;

            float avgOrderSpend = CustomerBehaviorHelpers.GetAverageOrderSpend(__instance);
            if (price >= avgOrderSpend * ChallengeConfig.COUNTEROFFER_PRICE_LIMIT_MULTIPLIER)
            {
                __result = false;
                return;
            }

            float enjoyment = __instance.GetProductEnjoyment(
                product, 
                __instance.CustomerData.Standards.GetCorrespondingQuality());

            if (enjoyment < ChallengeConfig.COUNTEROFFER_MIN_ENJOYMENT)
            {
                __result = false;
                return;
            }

            if (enjoyment < ChallengeConfig.COUNTEROFFER_MEDIOCRE_THRESHOLD)
            {
                float enjoymentRange = ChallengeConfig.COUNTEROFFER_MEDIOCRE_THRESHOLD - 
                                      ChallengeConfig.COUNTEROFFER_MIN_ENJOYMENT;
                float enjoymentAboveMin = enjoyment - ChallengeConfig.COUNTEROFFER_MIN_ENJOYMENT;
                float rejectChance = Mathf.Lerp(
                    ChallengeConfig.COUNTEROFFER_MAX_REJECT_CHANCE, 
                    0f, 
                    enjoymentAboveMin / enjoymentRange);

                if (UnityEngine.Random.value < rejectChance)
                    __result = false;
            }
        }
    }
}

