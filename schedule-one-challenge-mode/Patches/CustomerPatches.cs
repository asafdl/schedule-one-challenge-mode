using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using challenge_mode;
using MelonLoader;

#if MONO
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
#endif

namespace challenge_mode.Patches
{
    public static class CustomerBehaviorHelpers
    {
        public static float CalculateDrugAffinityBonus(CustomerData customerData, EDrugType drugType)
        {
            if (customerData?.DefaultAffinityData?.ProductAffinities == null)
                return 0f;

            float affinity = customerData.DefaultAffinityData.GetAffinity(drugType);
            return affinity * ChallengeConfig.DRUG_AFFINITY_MAX_IMPACT;
        }

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
    }

    [HarmonyPatch(typeof(Customer), nameof(Customer.GetProductEnjoyment))]
    public class GetProductEnjoyment_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Customer __instance, ProductDefinition product, EQuality quality, ref float __result)
        {
            var customerData = __instance.CustomerData;
            if (customerData == null)
                return;

            float originalResult = __result;
            float drugTypeBonus = CustomerBehaviorHelpers.CalculateDrugAffinityBonus(customerData, product.DrugType);
            float effectBonus = CustomerBehaviorHelpers.CalculateEffectMatchBonus(customerData, product);

            __result += drugTypeBonus + effectBonus;
            __result = Mathf.Clamp01(__result);

            // MelonLogger.Msg($"[Enjoyment] {__instance.NPC.fullName} → {product.Name} ({quality}): " +
            //                $"Base={originalResult:F3}, Drug={drugTypeBonus:+0.000;-0.000;+0.000}, " +
            //                $"Effects={effectBonus:+0.000;-0.000;+0.000}, Final={__result:F3}");
        }
    }

    [HarmonyPatch(typeof(Customer), nameof(Customer.GetOfferSuccessChance))]
    public class GetOfferSuccessChance_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Customer __instance, List<ItemInstance> items, ref float __result)
        {
            if (items == null || items.Count == 0)
                return;

            float avgEnjoyment = CustomerBehaviorHelpers.CalculateAverageEnjoyment(__instance, items);
            float difficultyMultiplier = ChallengeConfig.GetSuccessMultiplier(avgEnjoyment);
            float standardsPenalty = ChallengeConfig.GetStandardsPenalty((int)__instance.CustomerData.Standards);

            __result *= difficultyMultiplier * standardsPenalty;
        }
    }

    [HarmonyPatch(typeof(Customer), "GetWeightedRandomProduct")]
    public class GetWeightedRandomProduct_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref ProductDefinition __result, ref float appeal)
        {
            if (__result != null && appeal < ChallengeConfig.MIN_APPEAL_FOR_REQUEST)
            {
                __result = null;
                appeal = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(Customer), "EvaluateCounteroffer")]
    public class EvaluateCounteroffer_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Customer __instance, ProductDefinition product, int quantity, float price, ref bool __result)
        {
            if (!__result)
                return;

            MelonLogger.Msg($"[Counteroffer] {__instance.NPC.fullName}: {product.Name} x{quantity} @ ${price:F0}");

            float enjoyment = __instance.GetProductEnjoyment(
                product, 
                __instance.CustomerData.Standards.GetCorrespondingQuality());

            if (enjoyment < ChallengeConfig.COUNTEROFFER_MIN_ENJOYMENT)
            {
                MelonLogger.Msg($"[Counteroffer] REJECTED - Enjoyment too low ({enjoyment:F3} < {ChallengeConfig.COUNTEROFFER_MIN_ENJOYMENT:F3})");
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

                float roll = UnityEngine.Random.value;
                MelonLogger.Msg($"[Counteroffer] Mediocre enjoyment ({enjoyment:F3}) - reject chance: {rejectChance:F2} ({(rejectChance*100):F0}%), roll: {roll:F3}");

                if (roll < rejectChance)
                {
                    MelonLogger.Msg($"[Counteroffer] REJECTED - Failed probability check");
                    __result = false;
                }
                else
                {
                    MelonLogger.Msg($"[Counteroffer] ACCEPTED - Passed probability check");
                }
            }
            else
            {
                MelonLogger.Msg($"[Counteroffer] ACCEPTED - Good enjoyment ({enjoyment:F3})");
            }
        }
    }
}

