using HarmonyLib;
using System.Collections.Generic;
using MelonLoader;

#if MONO
using ScheduleOne.Economy;
using ScheduleOne.Product;
#endif

namespace challange_mode.Patches
{
    /// <summary>
    /// Debug patch to understand why some customers find products and others don't
    /// </summary>
    [HarmonyPatch(typeof(Customer), "GetWeightedRandomProduct")]
    public class GetWeightedRandomProduct_Debug_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(Customer __instance, Dealer dealer)
        {
            // Get the list of products they're considering
            var orderableProducts = dealer != null 
                ? dealer.GetOrderableProducts() 
                : ProductManager.ListedProducts;

            MelonLogger.Msg($"[ProductSearch] {__instance.NPC.fullName} searching with {orderableProducts.Count} available products");
            
            if (orderableProducts.Count == 0)
            {
                MelonLogger.Msg($"[ProductSearch] {__instance.NPC.fullName} → NO PRODUCTS IN LIST");
            }
        }
    }
}

